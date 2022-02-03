#!/bin/bash
# This script is intended for use with semantic-release prepare step with
# the exec plugin.

while getopts v:r:f: flag
do
    case "${flag}" in
        v) version=${OPTARG};;
        r) release_notes=${OPTARG};;
        f) feed_type=${OPTARG};;
    esac
done

# Update the Version property in Directory.Build.props:
sed -E -i 's|<Version>.*</Version>|<Version>'"$version"'</Version>|g' Directory.Build.props

# Update the version in readme and docs files with <span class="_version"> elements:
sed -E -i 's|<span class="_version">.*</span>|<span class="_version">'"$version"'</span>|g' README.md docs/*.md

# Set text to display whether the release is public feed or private feed.
sed -E -i 's|<span class="_release-feed-type">.*</span>|<span class="_release-feed-type">'"$feed_type"'</span>|g' README.md docs/*.md

# Set text to display whethet the release is public feed or private feed.
perl -i -0777 -pe 's|<!--_release-notes-->.*<!--_release-notes-->|<!--_release-notes-->\n'"${release_notes}"'\n<!--_release-notes-->|s' README.md docs/*.md