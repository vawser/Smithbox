#!/usr/bin/env bash
# Minimal client for the remote control: remote.sh <command> [key=value ...]
#   remote.sh help                      all commands with their arguments
#   remote.sh camera pos=130,-18,160 lookat=130,-19,176 fov=40
# Target: SMITHBOX_REMOTE_PORT (default 7780) on 127.0.0.1, or SMITHBOX_DOCKER=<container> to run curl inside that
# container (the server listens on 127.0.0.1:7790 there). Paths in arguments are as Smithbox sees them (container paths).
# SMITHBOX_REMOTE_TOKEN is sent as the X-Smithbox-Token header when set.
set -euo pipefail
cmd=${1:?"usage: remote.sh <command> [key=value ...]"}
shift
args=()
for kv in "$@"; do args+=(--data-urlencode "$kv"); done
if [[ -n "${SMITHBOX_REMOTE_TOKEN:-}" ]]; then args+=(-H "X-Smithbox-Token: $SMITHBOX_REMOTE_TOKEN"); fi
if [[ -n "${SMITHBOX_DOCKER:-}" ]]; then
    exec docker exec "$SMITHBOX_DOCKER" curl -s -G --max-time 3600 "${args[@]}" "http://127.0.0.1:7790/$cmd"
fi
exec curl -s -G --max-time 3600 "${args[@]}" "http://127.0.0.1:${SMITHBOX_REMOTE_PORT:-7780}/$cmd"
