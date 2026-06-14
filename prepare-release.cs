#!/usr/bin/env -S dotnet --
// Requires .NET 10+  -  run with: dotnet run prepare-release.cs
// Or make executable and run directly: chmod +x prepare-release.cs && ./prepare-release.cs
//
// Usage: prepare-release.cs
//        Run from the repository root.
//
// What this script does:
//    1. Find the most recent semantic-version git tag (v1.2.3)
//    2. Collect every commit since that tag
//    3. Parse commits against the Conventional Commits spec
//    4. Calculate the version bump: MAJOR / MINOR / PATCH / none
//    5. Build formatted release notes and prepend them to CHANGELOG.md
//    6. Update <Version> in src/Directory.Build.props
//    7. Update <span class="_version"> elements in README.md and docs/*.md
//    8. Update <!--_release-notes--> blocks in docs/*.md (full release notes)
//    9. Update <!--_release-notes--> block in README.md (release notes without section heading)
//   10. Prepend release notes to docs/History.md
//
// Conventional Commits and version bump rules:
//   MAJOR bump  - any commit with `!` after the type, e.g. `feat!:` or `fix(scope)!:`
//                 OR one or more `BREAKING CHANGE:`/`BREAKING-CHANGE:` footer tokens
//   MINOR bump  - `feat:` commits (new features, no breaking change)
//   PATCH bump  - `fix:` or `perf:` commits (bug fixes, performance improvements)
//   NO bump     - `docs:`, `style:`, `refactor:`, `test:`, `chore:`, `ci:`, `build:`, `revert:`
//
// Only the highest applicable bump level across all commits is applied.
// Non-conventional commits (free-form messages) are skipped entirely.

using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

// ============================================================================
// ENTRY POINT  (top-level statements must appear before type declarations)
// ============================================================================

try
{
// Verify we are inside a git repository before doing anything.
string repoRoot = Directory.GetCurrentDirectory();
if (!Directory.Exists(Path.Combine(repoRoot, ".git")))
    throw new InvalidOperationException("No .git directory found. Run this script from the repository root.");

Console.WriteLine("Finbuckle.MultiTenant - Release Preparation");
Console.WriteLine();

// --- Step 1: Find the latest semantic-version tag ---
var (latestTag, currentVersion) = FindLatestTag();
Console.WriteLine();

// --- Step 2: Collect raw commits since that tag ---
var rawCommits = CollectCommits(latestTag);
Console.WriteLine();

if (rawCommits.Count == 0)
{
    Console.WriteLine("No new commits since the last release. Nothing to do.");
    return;
}

// --- Step 3: Parse commits using the Conventional Commits spec ---
var conventionalCommits = ParseCommits(rawCommits);
Console.WriteLine();

// --- Step 4: Calculate the version bump ---
var (bumpLevel, newVersion) = DetermineVersionBump(currentVersion!, conventionalCommits);
Console.WriteLine();

if (bumpLevel == BumpLevel.None)
{
    Console.WriteLine("No feat/fix/perf/breaking commits - no version bump required.");
    return;
}

// Validate that the new major version matches the current branch (e.g. "8.x" or "9.x")
ValidateBranchMajorVersion(newVersion);

// Resolve the GitHub repo URL from the git remote (used for links in release notes)
string repoUrl = GetRepoUrl();

// --- Step 5: Build the release notes Markdown ---
string releaseNotes = BuildReleaseNotes(newVersion, currentVersion!, conventionalCommits, repoUrl);

Console.WriteLine("Release notes:");
Console.WriteLine(new string('-', 60));
Console.Write(releaseNotes);
Console.WriteLine(new string('-', 60));
Console.WriteLine();

// --- Step 6: Prepend the notes to CHANGELOG.md ---
PrependToChangelog(releaseNotes, repoRoot);

// --- Step 7: Update <Version> in src/Directory.Build.props ---
UpdateBuildPropsVersion(newVersion, repoRoot);

// --- Step 8: Update _version spans in README.md and docs/*.md ---
UpdateVersionSpans(newVersion, repoRoot);

// --- Step 9 & 10: Update <!--_release-notes--> blocks ---
UpdateReleaseNotesBlocks(releaseNotes, repoRoot);

// --- Step 11: Insert release notes into <!--_history--> block in docs/History.md ---
UpdateHistoryBlock(releaseNotes, repoRoot);

Console.WriteLine();
Console.WriteLine($"Previous version : {currentVersion!.ToTag()}");
Console.WriteLine($"New version      : {newVersion.ToTag()}");

// Emit the new version as a GitHub Actions output variable when running in CI.
string? githubOutput = Environment.GetEnvironmentVariable("GITHUB_OUTPUT");
if (githubOutput is not null)
{
    // Write release notes to a temp file so the workflow can pass them to gh release create.
    string notesFile = Path.Combine(Path.GetTempPath(), "release-notes.md");
    File.WriteAllText(notesFile, releaseNotes);

    File.AppendAllText(githubOutput, $"new_version={newVersion.ToTag()}\n");
    File.AppendAllText(githubOutput, $"release_notes_file={notesFile}\n");
    File.AppendAllText(githubOutput, $"bump_level={bumpLevel}\n");
}
}
catch (Exception ex)
{
    Console.Error.WriteLine($"ERROR: {ex.Message}");
    Environment.Exit(1);
}

// ============================================================================
// LOCAL FUNCTIONS
// ============================================================================

// Run a git command and return trimmed stdout.
// Throws an InvalidOperationException on non-zero exit code.
static string Git(string args)
{
    using var proc = new Process();
    proc.StartInfo = new ProcessStartInfo
    {
        FileName               = "git",
        Arguments              = args,
        RedirectStandardOutput = true,
        RedirectStandardError  = true,
        UseShellExecute        = false,
        CreateNoWindow         = true
    };
    proc.Start();
    string stdout = proc.StandardOutput.ReadToEnd();
    string stderr = proc.StandardError.ReadToEnd();
    proc.WaitForExit();

    if (proc.ExitCode != 0)
        throw new InvalidOperationException(
            $"`git {args}` failed (exit {proc.ExitCode}):\n{stderr.Trim()}");

    return stdout.Trim();
}

// Resolve the GitHub HTTPS base URL from the git remote named "origin".
// Handles both SSH (git@github.com:Owner/Repo.git) and HTTPS remote formats.
// Falls back to the Finbuckle repo URL if the remote cannot be read.
static string GetRepoUrl()
{
    try
    {
        string remote = Git("remote get-url origin");
        // Normalize SSH:   git@github.com:Owner/Repo.git  ->  https://github.com/Owner/Repo
        // Normalize HTTPS: https://github.com/Owner/Repo.git  ->  https://github.com/Owner/Repo
        remote = Regex.Replace(remote, @"^git@([^:]+):(.+?)(?:\.git)?$", "https://$1/$2");
        remote = Regex.Replace(remote, @"\.git$", "");
        return remote.TrimEnd('/');
    }
    catch
    {
        return "https://github.com/Finbuckle/Finbuckle.MultiTenant";
    }
}

// Validate that the major component of newVersion is consistent with the current
// git branch. Rules:
//   main   - any version is allowed (new major versions are cut from main)
//   N.x    - the new version's major must equal N
//   other  - error; releases must be made from main or an N.x branch.
static void ValidateBranchMajorVersion(SemanticVersion newVersion)
{
    string branch = Git("rev-parse --abbrev-ref HEAD");

    if (branch == "main")
    {
        // On main, check that no N.x branch already exists for the new major version.
        // If one does, the release should be made from that branch instead.
        string expectedReleaseBranch = $"{newVersion.Major}.x";
        var allBranches = Git("branch --list --all")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(b => b.Trim().TrimStart('*').Trim()
                          .Replace("remotes/origin/", ""))
            .Distinct();

        if (allBranches.Any(b => b == expectedReleaseBranch))
            throw new InvalidOperationException(
                $"Branch \"{expectedReleaseBranch}\" already exists. " +
                $"Release {newVersion.ToTag()} should be made from that branch, not from main.");

        Console.WriteLine($"Branch check passed: on main and \"{expectedReleaseBranch}\" does not yet exist.");
        return;
    }

    var m = Regex.Match(branch, @"^(\d+)\.x$");
    if (!m.Success)
        throw new InvalidOperationException(
            $"Current branch \"{branch}\" is not a valid release branch. " +
            $"Releases must be made from \"main\" or an \"N.x\" branch (e.g. \"8.x\").");

    int branchMajor = int.Parse(m.Groups[1].Value);
    if (branchMajor != newVersion.Major)
        throw new InvalidOperationException(
            $"Version mismatch: calculated version {newVersion.ToTag()} has major version {newVersion.Major}, " +
            $"but the current branch \"{branch}\" expects major version {branchMajor}.");

    Console.WriteLine($"Branch check passed: {branch} is consistent with {newVersion.ToTag()}");
}


// Returns (null, 0.0.0) when no matching tag exists.
static (string? tag, SemanticVersion? version) FindLatestTag()
{
    Console.WriteLine("Step 1: Find the latest semantic version tag");
    Console.WriteLine();

    const string pattern = @"^v(\d+)\.(\d+)\.(\d+)$";

    // --sort=-version:refname treats tag names as semver and orders newest first
    var allTags = Git("tag --list --sort=-version:refname")
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries);

    foreach (var raw in allTags)
    {
        var tag = raw.Trim();
        if (Regex.IsMatch(tag, pattern))
        {
            var ver = SemanticVersion.Parse(tag);
            Console.WriteLine($"Latest semver tag: {tag}");
            return (tag, ver);
        }
    }

    Console.WriteLine("No semantic version tags found - treating current version as 0.0.0");
    return (null, new SemanticVersion(0, 0, 0));
}

// STEP 2 - Return every commit between the given tag and HEAD.
// When sinceTag is null, all history is returned.
// Each element is a (fullHash, shortHash, subject, body) tuple.
static List<(string FullHash, string ShortHash, string Subject, string Body)> CollectCommits(string? sinceTag)
{
    string since = sinceTag is null ? "all history" : $"since {sinceTag}";
    Console.WriteLine($"Step 2: Collect commits ({since})");
    Console.WriteLine();

    // %H = full hash, %h = short hash, %s = subject, %b = body
    // %x00 (NUL) separates fields; %x1E (record separator) separates commits.
    string range = sinceTag is null ? "HEAD" : $"{sinceTag}..HEAD";
    string raw   = Git($"log {range} --pretty=format:%H%x00%h%x00%s%x00%b%x1E");

    if (string.IsNullOrWhiteSpace(raw))
    {
        Console.WriteLine("No commits found in range.");
        return [];
    }

    var records = raw.Split('\x1E', StringSplitOptions.RemoveEmptyEntries);
    var commits = new List<(string, string, string, string)>();

    foreach (var record in records)
    {
        var parts = record.TrimStart('\n').Split('\x00');
        if (parts.Length < 3) continue;

        string fullHash  = parts[0].Trim();
        string shortHash = parts[1].Trim();
        string subject   = parts[2].Trim();
        string body      = parts.Length > 3 ? parts[3].Trim() : string.Empty;

        if (!string.IsNullOrWhiteSpace(fullHash) && !string.IsNullOrWhiteSpace(subject))
            commits.Add((fullHash, shortHash, subject, body));
    }

    foreach (var (_, shortHash, subject, _) in commits)
        Console.WriteLine($"[{shortHash}] {subject}");

    Console.WriteLine();
    Console.WriteLine($"{commits.Count} commit(s) found.");
    return commits;
}

// STEP 3 - Parse the raw commit tuples against the Conventional Commits spec:
//   type(scope)!: description
// Non-conventional commits are silently skipped.
// Breaking changes are detected by `!` in the subject or BREAKING CHANGE footers.
static List<ConventionalCommit> ParseCommits(
    List<(string FullHash, string ShortHash, string Subject, string Body)> raw)
{
    Console.WriteLine("Step 3: Parse commits (Conventional Commits spec)");
    Console.WriteLine();

    const string ccPattern        = @"^(?<type>feat|fix|docs|style|refactor|perf|test|chore|ci|build|revert)(?:\((?<scope>[^)]+)\))?(?<breaking>!)?:\s+(?<desc>.+)$";
    const string brkHeaderPattern = @"^(?:BREAKING CHANGE|BREAKING-CHANGE):\s*(?<note>.*)$";

    var parsed  = new List<ConventionalCommit>();
    int skipped = 0;

    foreach (var (fullHash, shortHash, subject, body) in raw)
    {
        var m = Regex.Match(subject, ccPattern);
        if (!m.Success)
        {
            Console.WriteLine($"[skip] [{shortHash}] {subject}");
            skipped++;
            continue;
        }

        string type  = m.Groups["type"].Value;
        string scope = m.Groups["scope"].Value;  // empty string when no scope present
        string desc  = m.Groups["desc"].Value;

        // Breaking if the subject contains `!` before the colon or footer contains BREAKING CHANGE.
        var breakingNotes = new List<string>();
        bool isBreaking   = m.Groups["breaking"].Success;

        var bodyLines = body.Split('\n')
            .Select(l => l.TrimEnd('\r'))
            .ToList();

        for (int i = 0; i < bodyLines.Count; i++)
        {
            var line = bodyLines[i];
            var bm   = Regex.Match(line, brkHeaderPattern);
            if (!bm.Success) continue;

            var noteBuilder = new StringBuilder(bm.Groups["note"].Value.Trim());

            // Conventional Commits allows footer values to continue on following indented lines.
            int j = i + 1;
            while (j < bodyLines.Count && (bodyLines[j].StartsWith(' ') || bodyLines[j].StartsWith('\t')))
            {
                if (noteBuilder.Length > 0)
                    noteBuilder.Append('\n');
                noteBuilder.Append(bodyLines[j].TrimStart());
                j++;
            }

            var note = noteBuilder.ToString().Trim();
            if (!string.IsNullOrEmpty(note))
                breakingNotes.Add(note);

            i = j - 1;
        }

        if (breakingNotes.Count > 0)
            isBreaking = true;

        parsed.Add(new ConventionalCommit(shortHash, fullHash, type, scope, desc, isBreaking, breakingNotes));

        string label = isBreaking ? "[BREAKING]" : $"[{type}]";
        string sc    = string.IsNullOrEmpty(scope) ? "" : $"({scope}) ";
        Console.WriteLine($"{label} [{shortHash}] {sc}{desc}");
    }

    Console.WriteLine();
    Console.WriteLine($"Parsed: {parsed.Count}, Skipped (non-conventional): {skipped}");
    return parsed;
}

// STEP 4 - Walk every parsed commit and determine the highest-priority bump:
//   breaking change  =>  MAJOR  (1.2.3 -> 2.0.0)
//   feat             =>  MINOR  (1.2.3 -> 1.3.0)
//   fix | perf       =>  PATCH  (1.2.3 -> 1.2.4)
//   everything else  =>  NONE
static (BumpLevel level, SemanticVersion newVersion) DetermineVersionBump(
    SemanticVersion current, List<ConventionalCommit> commits)
{
    Console.WriteLine("Step 4: Determine version bump");
    Console.WriteLine();

    BumpLevel bump = BumpLevel.None;

    foreach (var c in commits)
    {
        if (c.IsBreaking)
        {
            // Nothing can exceed a major bump; stop evaluating immediately
            bump = BumpLevel.Major;
            break;
        }

        if (c.Type == "feat" && bump < BumpLevel.Minor)
            bump = BumpLevel.Minor;
        else if (c.Type is "fix" or "perf" && bump < BumpLevel.Patch)
            bump = BumpLevel.Patch;
    }

    var newVersion = current.Bump(bump);

    string label = bump switch
    {
        BumpLevel.Major => "MAJOR - breaking change detected",
        BumpLevel.Minor => "MINOR - new feature(s) detected",
        BumpLevel.Patch => "PATCH - bug fix / perf improvement detected",
        _               => "NONE - no impactful commits found"
    };

    Console.WriteLine($"Bump: {label}");
    Console.WriteLine($"Version: {current} -> {newVersion}");

    return (bump, newVersion);
}

// STEP 5 - Build a Markdown release-notes block matching the existing CHANGELOG.md style:
//   ## [1.2.3](compare_url) (YYYY-MM-DD)
//   ### ⚠ BREAKING CHANGES
//   * description ([#PR](issue_url)) ([abc1234](commit_url))
// Sections with zero commits are omitted.
static string BuildReleaseNotes(
    SemanticVersion newVersion,
    SemanticVersion previousVersion,
    List<ConventionalCommit> commits,
    string repoUrl)
{
    Console.WriteLine("Step 5: Build release notes");
    Console.WriteLine();

    var sb = new StringBuilder();
    string today = DateTime.UtcNow.ToString("yyyy-MM-dd");

    // Version heading with a GitHub compare link and date in parentheses
    string compareUrl = $"{repoUrl}/compare/{previousVersion.ToTag()}...{newVersion.ToTag()}";
    sb.AppendLine($"## [{newVersion}]({compareUrl}) ({today})");
    sb.AppendLine();

    // Formats a single commit as a changelog bullet.
    // Extracts a trailing "(#1234)" PR reference from the description if present and
    // renders it as a proper GitHub issue link before the commit hash link.
    string FormatBullet(ConventionalCommit c)
    {
        // Strip a trailing "(#NNNN)" PR number from the description text
        var prMatch = Regex.Match(c.Description, @"\s*\(#(\d+)\)\s*$");
        string desc       = prMatch.Success ? c.Description[..prMatch.Index].TrimEnd() : c.Description;
        string prLink     = prMatch.Success
            ? $" ([#{prMatch.Groups[1].Value}]({repoUrl}/issues/{prMatch.Groups[1].Value}))"
            : "";
        string commitLink = $"([{c.ShortHash}]({repoUrl}/commit/{c.FullHash}))";
        return $"* {desc}{prLink} ({commitLink})";
    }

    // Appends a section; does nothing when the list is empty.
    // If any commits in the section carry a scope, items are grouped under
    // #### scope subheadings (sorted alphabetically). Unscoped items appear last.
    void Section(string heading, IList<ConventionalCommit> items)
    {
        if (items.Count == 0) return;
        sb.AppendLine($"### {heading}");
        sb.AppendLine();

        bool anyScoped = items.Any(c => !string.IsNullOrEmpty(c.Scope));

        if (!anyScoped)
        {
            // No scopes in this section — flat list.
            foreach (var c in items)
            {
                if (c.BreakingNotes.Count > 0)
                {
                    foreach (var note in c.BreakingNotes)
                        sb.AppendLine($"* {note}");
                }
                else
                    sb.AppendLine(FormatBullet(c));
            }
        }
        else
        {
            // Group by scope: scoped items first (alpha), unscoped items last.
            var scopes = items
                .Where(c => !string.IsNullOrEmpty(c.Scope))
                .Select(c => c.Scope)
                .Distinct()
                .Order();

            foreach (var scope in scopes)
            {
                sb.AppendLine($"#### {scope}");
                sb.AppendLine();
                foreach (var c in items.Where(c => c.Scope == scope))
                {
                    if (c.BreakingNotes.Count > 0)
                    {
                        foreach (var note in c.BreakingNotes)
                            sb.AppendLine($"* {note}");
                    }
                    else
                        sb.AppendLine(FormatBullet(c));
                }
                sb.AppendLine();
            }

            // Unscoped items under a general group
            var unscoped = items.Where(c => string.IsNullOrEmpty(c.Scope)).ToList();
            if (unscoped.Count > 0)
            {
                sb.AppendLine("#### general");
                sb.AppendLine();
                foreach (var c in unscoped)
                {
                    if (c.BreakingNotes.Count > 0)
                    {
                        foreach (var note in c.BreakingNotes)
                            sb.AppendLine($"* {note}");
                    }
                    else
                        sb.AppendLine(FormatBullet(c));
                }
                sb.AppendLine();
            }
        }

        sb.AppendLine();
    }

    // Only include commit types that drive a version change.
    // docs, style, refactor, test, chore, ci, build, revert are intentionally omitted.
    Section("⚠ BREAKING CHANGES", commits.Where(c => c.IsBreaking).ToList());
    Section("Features",            commits.Where(c => c.Type == "feat" && !c.IsBreaking).ToList());
    Section("Bug Fixes",           commits.Where(c => c.Type == "fix"  && !c.IsBreaking).ToList());
    Section("Performance",         commits.Where(c => c.Type == "perf" && !c.IsBreaking).ToList());

    return sb.ToString();
}

// STEP 6 - Prepend releaseNotes to CHANGELOG.md, preserving any top-level
// "# Changelog" heading that already exists at the top of the file.
// Creates the file if it does not yet exist.
static void PrependToChangelog(string releaseNotes, string repoRoot)
{
    Console.WriteLine("Step 6: Prepend to CHANGELOG.md");
    Console.WriteLine();

    string path     = Path.Combine(repoRoot, "CHANGELOG.md");
    string existing = File.Exists(path) ? File.ReadAllText(path) : string.Empty;

    // If the file starts with a top-level heading (e.g. "# Changelog"), keep
    // it first and insert the new release notes directly below it.
    var headerMatch = Regex.Match(existing, @"^(#[^#][^\n]*\n+)", RegexOptions.Multiline);
    string newContent = (headerMatch.Success && existing.StartsWith(headerMatch.Value))
        ? headerMatch.Value + releaseNotes + existing[headerMatch.Length..]
        : releaseNotes + existing;

    File.WriteAllText(path, newContent);
    Console.WriteLine($"{path} updated.");
}

// STEP 7 - Replace the <Version>…</Version> element in src/Directory.Build.props
// with the newly calculated version.
static void UpdateBuildPropsVersion(SemanticVersion newVersion, string repoRoot)
{
    Console.WriteLine("Step 7: Update <Version> in src/Directory.Build.props");
    Console.WriteLine();

    string path = Path.Combine(repoRoot, "src", "Directory.Build.props");
    if (!File.Exists(path))
        throw new FileNotFoundException($"Required file not found: {path}");

    string content = File.ReadAllText(path);
    string updated = Regex.Replace(content, @"<Version>.*?</Version>", $"<Version>{newVersion}</Version>");
    File.WriteAllText(path, updated);
    Console.WriteLine($"{path} updated.");
}

// STEP 8 - Replace <span class="_version">…</span> elements with the new
// version string in README.md and every docs/*.md file.
static void UpdateVersionSpans(SemanticVersion newVersion, string repoRoot)
{
    Console.WriteLine("Step 8: Update _version spans");
    Console.WriteLine();

    var files = new List<string>();
    string readme = Path.Combine(repoRoot, "README.md");
    if (File.Exists(readme)) files.Add(readme);

    string docsDir = Path.Combine(repoRoot, "docs");
    if (Directory.Exists(docsDir))
        files.AddRange(Directory.GetFiles(docsDir, "*.md"));

    foreach (var file in files)
    {
        string content = File.ReadAllText(file);
        string updated = Regex.Replace(
            content,
            @"<span class=""_version"">.*?</span>",
            $"""<span class="_version">{newVersion}</span>""");


        File.WriteAllText(file, updated);
        Console.WriteLine($"{file} updated.");
    }
}

// STEP 9 & 10 - Replace <!--_release-notes-->…<!--_release-notes--> delimiters
// in docs/*.md (full release notes) and README.md (notes without the first heading line).
static void UpdateReleaseNotesBlocks(string releaseNotes, string repoRoot)
{
    Console.WriteLine("Step 9 & 10: Update <!--_release-notes--> blocks");
    Console.WriteLine();

    // For README, strip the first line (the "## [version](url) (date)" heading)
    string releaseNotesNoHeader = string.Join('\n',
        releaseNotes.Split('\n').Skip(1)).TrimStart('\r', '\n');

    // Regex that matches the content between the marker comments (dotall / singleline)
    const string markerPattern = @"<!--_release-notes-->.*?<!--_release-notes-->";
    const RegexOptions dotAll  = RegexOptions.Singleline;

    string docsDir = Path.Combine(repoRoot, "docs");
    if (Directory.Exists(docsDir))
    {
        foreach (var file in Directory.GetFiles(docsDir, "*.md"))
        {
            string content = File.ReadAllText(file);
            if (!Regex.IsMatch(content, markerPattern, dotAll)) continue;

            string updated = Regex.Replace(
                content,
                markerPattern,
                $"<!--_release-notes-->\n{releaseNotes}\n<!--_release-notes-->",
                dotAll);
            File.WriteAllText(file, updated);
            Console.WriteLine($"{file} updated.");
        }
    }

    string readme = Path.Combine(repoRoot, "README.md");
    if (File.Exists(readme))
    {
        string content = File.ReadAllText(readme);
        if (Regex.IsMatch(content, markerPattern, dotAll))
        {
            string updated = Regex.Replace(
                content,
                markerPattern,
                $"<!--_release-notes-->\n{releaseNotesNoHeader}\n<!--_release-notes-->",
                dotAll);
            File.WriteAllText(readme, updated);
            Console.WriteLine($"{readme} updated.");
        }
    }
}

// STEP 11 - Prepend the release notes to docs/History.md, preserving any
// top-level "# …" heading that already exists at the top of the file.
static void UpdateHistoryBlock(string releaseNotes, string repoRoot)
{
    Console.WriteLine("Step 11: Prepend release notes to docs/History.md");
    Console.WriteLine();

    string historyPath = Path.Combine(repoRoot, "docs", "History.md");

    if (!File.Exists(historyPath))
        throw new FileNotFoundException($"Required file not found: {historyPath}");

    string existing = File.ReadAllText(historyPath);

    var headerMatch = Regex.Match(existing, @"^(#[^#][^\n]*\n+)", RegexOptions.Multiline);
    string newContent = (headerMatch.Success && existing.StartsWith(headerMatch.Value))
        ? headerMatch.Value + releaseNotes + existing[headerMatch.Length..]
        : releaseNotes + existing;

    File.WriteAllText(historyPath, newContent);
    Console.WriteLine($"{historyPath} updated.");
}



// ============================================================================
// TYPE DECLARATIONS
// Per the C# spec, standalone type declarations must come AFTER all
// top-level statements (including local function declarations above).
// ============================================================================

/// <summary>The category of semver bump required by a set of commits.</summary>
enum BumpLevel { None, Patch, Minor, Major }

/// <summary>An immutable semantic version triplet.</summary>
record SemanticVersion(int Major, int Minor, int Patch)
{
    /// <summary>Parse a string like "v1.2.3" or "1.2.3".</summary>
    public static SemanticVersion Parse(string tag)
    {
        var m = Regex.Match(tag.TrimStart('v'), @"^(\d+)\.(\d+)\.(\d+)$");
        if (!m.Success)
            throw new FormatException($"Cannot parse '{tag}' as a semantic version.");
        return new(int.Parse(m.Groups[1].Value),
                   int.Parse(m.Groups[2].Value),
                   int.Parse(m.Groups[3].Value));
    }

    /// <summary>Return a new version with the appropriate component incremented.</summary>
    public SemanticVersion Bump(BumpLevel level) => level switch
    {
        BumpLevel.Major => new SemanticVersion(Major + 1, 0, 0),
        BumpLevel.Minor => new SemanticVersion(Major, Minor + 1, 0),
        BumpLevel.Patch => new SemanticVersion(Major, Minor, Patch + 1),
        _               => this
    };

    public override string ToString() => $"{Major}.{Minor}.{Patch}";

    /// <summary>Return the version prefixed with "v" for use as a git tag.</summary>
    public string ToTag() => $"v{this}";
}

/// <summary>A single parsed conventional commit.</summary>
record ConventionalCommit(
    string ShortHash,    // 7-char abbreviated SHA (used as link text)
    string FullHash,     // full SHA (used in the commit URL)
    string Type,         // feat | fix | docs | chore | ...
    string Scope,        // the (scope) part, or empty string if not specified
    string Description,  // the commit subject/description
    bool IsBreaking, // true when `!` in subject or BREAKING CHANGE footer(s)
    IReadOnlyList<string> BreakingNotes // notes from BREAKING CHANGE footer tokens (possibly multiple)
);
