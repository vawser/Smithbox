#!/bin/sh
# Starts Xvfb directly (xvfb-run hangs in the container), then Smithbox. Arguments go to Smithbox (e.g. --remote-role Docker).
CFG="$HOME/.local/share/Smithbox/Configuration/Configuration.json"
if [ ! -f "$CFG" ]; then
    # SDL reports no usable DPI under Xvfb, which would give a font size of 0
    mkdir -p "$(dirname "$CFG")"
    echo '{"Interface_Scale_by_DPI": false}' > "$CFG"
fi
Xvfb :99 -screen 0 1920x1080x24 -dpi 96 -nolisten tcp 2>/dev/null &
sleep 2
exec ./Smithbox.Vulkan "$@"
