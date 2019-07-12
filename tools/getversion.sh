#!/usr/bin/env bash
set -o nounset
set -o pipefail

RELEASE_TAG=$(git describe --tags --exact-match 2>/dev/null)
set -o errexit
if [ -z "$RELEASE_TAG" ]
then
    WORKING_SUFFIX=$(if git status --porcelain | grep -qE '^(?:[^?][^ ]|[^ ][^?])\s'; then echo "-WIP"; else echo ""; fi)
    BRANCH_PREFIX=$(git describe --dirty --always --tags)
    echo "$BRANCH_PREFIX$WORKING_SUFFIX"
else
    echo "$RELEASE_TAG"
fi
