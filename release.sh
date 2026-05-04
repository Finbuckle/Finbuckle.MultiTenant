#!/bin/bash
# This script is intended for use with semantic-release prepare step with
# the exec plugin.

while getopts v:f: flag
do
    case "${flag}" in
        v) version=${OPTARG};;
        f) feed_type=${OPTARG};;
    esac
done

# Update the Version property in Directory.Build.props:
sed -E -i 's|<Version>.*</Version>|<Version>'"${version}"'</Version>|g' src/Directory.Build.props

# Update the version in readme and docs files with <span class="_version"> elements:
sed -E -i 's|<span class="_version">.*</span>|<span class="_version">'"${version}"'</span>|g' README.md docs/*.md

# Set text to display whether the release is public feed or private feed:
sed -E -i 's|<span class="_release-feed-type">.*</span>|<span class="_release-feed-type">'"${feed_type}"'</span>|g' README.md docs/*.md

# Extract the release notes for this version from the changelog:
escaped_version=$(printf '%s' "${version}" | sed 's/\./\\./g')
release_notes=$(perl -0777 -ne 'if (/(## \['"${escaped_version}"'\].*?)(?=## \[|\z)/s) { print $1 }' CHANGELOG.md)
if [ -z "${release_notes}" ]; then
    echo "Error: could not extract release notes for version ${version} from CHANGELOG.md" >&2
    exit 1
fi

# Set text wherever release notes are needed in docs:
perl -i -0777 -pe 's|<!--_release-notes-->.*<!--_release-notes-->|<!--_release-notes-->\n'"${release_notes}"'\n<!--_release-notes-->|s' docs/*.md

# Set text for release notes in readme (header line stripped):
release_notes_no_header=$(echo -e "${release_notes}" | tail -n +2)
perl -i -0777 -pe 's|<!--_release-notes-->.*<!--_release-notes-->|<!--_release-notes-->\n'"${release_notes_no_header}"'\n<!--_release-notes-->|s' README.md

# Copy the new changelog file to the Version History docs page:
history=$(<CHANGELOG.md)
perl -i -0777 -pe 's|<!--_history-->.*<!--_history-->|<!--_history-->\n'"${history}"'\n<!--_history-->|s' docs/History.md