# ![Finbuckle Logo](https://www.finbuckle.com/images/finbuckle-32x32-gh.png) MultiTenant <span class="_version">10.1.2</span>

## About MultiTenant

MultiTenant is an open source multi-tenancy library for modern .NET created and maintained by [Finbuckle LLC](https://www.finbuckle.com).
It enables tenant resolution, per-tenant app behavior, and per-tenant data isolation.

See [https://www.finbuckle.com/Products](https://www.finbuckle.com/Products) for more details and documentation.

**This release supports .NET 11.**

Beginning with MultiTenant v10, major version releases align with .NET major version releases.

New development focuses on the latest MultiTenant release version while critical security and severe bug 
fixes will be released for prior versions which target .NET versions supported by Microsoft.

In general, you should target the version of MultiTenant that matches your .NET version.

## Open Source Support

Your support helps keep the project going and is greatly appreciated!

MultiTenant is primarily supported by its [GitHub sponsors](https://github.com/sponsors/Finbuckle) and [contributors](https://github.com/Finbuckle/Finbuckle.MultiTenant/graphs/contributors).

Additional support is provided by the following organizations:

<p><a href="https://www.digitalocean.com/">
  <img src="https://opensource.nyc3.cdn.digitaloceanspaces.com/attribution/assets/SVG/DO_Logo_horizontal_blue.svg" alt="Digital Ocean logo" height="40">
</a></p>

<p><a href="https://www.github.com/">
  <img src="https://github.githubassets.com/assets/GitHub-Logo-ee398b662d42.png" alt="GitHub logo" height="40">
</a></p>

<p><a href="https://www.jetbrains.com/">
  <img src="https://resources.jetbrains.com/storage/products/company/brand/logos/jetbrains.svg" alt="Jetbrains logo" height="40">
</a></p>

## License

This project uses the [Apache 2.0 license](https://www.apache.org/licenses/LICENSE-2.0). See [LICENSE](../LICENSE) file for
license information.

### Open Source Maintenance Fee

The source code is freely available under the terms of the license. Use of the official NuGet packages in
revenue-generating activities is subject to the [Open Source Maintenance Fee](https://opensourcemaintenancefee.org)
[EULA](../OSMFEULA.txt). See [pricing](https://www.finbuckle.com/Pricing) for details, including free use
for smaller organizations.

Release builds that reference the official packages show warning `FBMT0001` until the EULA is accepted. To accept it,
set the `AcceptFinbuckleEULA` property in your project file or `Directory.Build.props`:

```xml
<PropertyGroup>
    <AcceptFinbuckleEULA>true</AcceptFinbuckleEULA>
</PropertyGroup>
```

It can also be passed on the command line, for example `dotnet build -c Release -p:AcceptFinbuckleEULA=true`. Builds
that treat warnings as errors fail until the property is set.

## Verifying Packages

The official NuGet packages are built and published by the
[NuGet Push Public](https://github.com/Finbuckle/Finbuckle.MultiTenant/blob/main/.github/workflows/nuget-push-public.yml)
workflow, which creates a GitHub
[artifact attestation](https://docs.github.com/en/actions/concepts/security/artifact-attestations) for each `.nupkg`,
`.snupkg`, and library assembly. The attestation proves the file was built from this repository by that workflow. Packages
released before attestations were added can't be verified this way.

Verification uses the [GitHub CLI](https://cli.github.com). Because NuGet.org adds its own repository signature file,
`.signature.p7s`, to every package it publishes, a `.nupkg` downloaded from NuGet.org doesn't match the attested file
until that signature is removed. The assemblies inside the package aren't changed, so they can be verified directly.

### Verify an assembly

After a restore, the package's assemblies are in the local NuGet cache (`~/.nuget/packages` on macOS and Linux,
`%userprofile%\.nuget\packages` on Windows). Replace `<version>` with the package version:

```bash
gh attestation verify ~/.nuget/packages/finbuckle.multitenant/<version>/lib/net11.0/Finbuckle.MultiTenant.dll \
  --repo Finbuckle/Finbuckle.MultiTenant \
  --signer-workflow Finbuckle/Finbuckle.MultiTenant/.github/workflows/nuget-push-public.yml
```

### Verify a package

Download the package from NuGet.org, remove `.signature.p7s` from a copy, then verify the copy:

```bash
curl -L -o Finbuckle.MultiTenant.<version>.nupkg https://www.nuget.org/api/v2/package/Finbuckle.MultiTenant/<version>
zip -d Finbuckle.MultiTenant.<version>.nupkg .signature.p7s
gh attestation verify Finbuckle.MultiTenant.<version>.nupkg \
  --repo Finbuckle/Finbuckle.MultiTenant \
  --signer-workflow Finbuckle/Finbuckle.MultiTenant/.github/workflows/nuget-push-public.yml
```

On Windows, where `zip` isn't available, remove the signature with PowerShell 7 instead:

```powershell
$zip = [System.IO.Compression.ZipFile]::Open("$PWD\Finbuckle.MultiTenant.<version>.nupkg", 'Update')
$zip.GetEntry('.signature.p7s').Delete()
$zip.Dispose()
```

Removing the signature leaves the rest of the package byte-for-byte as it was built. Don't use the modified copy as a
package source, since NuGet can no longer verify its signature.

## .NET Foundation

This project is supported by the [.NET Foundation](https://dotnetfoundation.org).

## Code of Conduct

This project has adopted the code of conduct defined by the Contributor Covenant to clarify expected behavior in our
community. For more information see the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct)
or the [CONTRIBUTING.md](../CONTRIBUTING.md) file.

## Community

Check out the [GitHub repository](https://github.com/Finbuckle/Finbuckle.MultiTenant) to ask a question, make a request,
or peruse the code!

## Building from Source

From the command line clone the git repository, `cd` into the new directory, and compile with `dotnet build`.

```bash
git clone https://github.com/Finbuckle/Finbuckle.MultiTenant.git
cd Finbuckle.MultiTenant
dotnet build
```

## Running Unit Tests

Run the unit tests from the command line with `dotnet test` from the solution directory.

```bash
dotnet test
```
