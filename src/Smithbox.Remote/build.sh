#!/usr/bin/env bash
# Builds the Docker image smithbox-remote (Linux, CPU rendering) and a Windows build of Smithbox with the extension in out/win-x64.
set -euo pipefail
cd "$(dirname "$0")/../.."
docker build -f src/Smithbox.Remote/Dockerfile --target runtime -t smithbox-remote .
rm -rf out/win-x64
docker build -f src/Smithbox.Remote/Dockerfile --target win -o out/win-x64 .
echo "Done: image smithbox-remote, Windows build in $(pwd)/out/win-x64"
