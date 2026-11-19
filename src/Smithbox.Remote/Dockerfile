# Smithbox with the remote control extension, CPU rendering (Mesa llvmpipe) under Xvfb.
# Build from the repository root (see build.sh):
#   docker build -f src/Smithbox.Remote/Dockerfile --target runtime -t smithbox-remote .
#   docker build -f src/Smithbox.Remote/Dockerfile --target win -o out/win-x64 .     (Windows build with the extension)

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS src
COPY . /sb

FROM src AS build-linux
RUN dotnet build /sb/src/Smithbox.Vulkan/Smithbox.Vulkan.csproj -c Release-linux -v q -nologo \
 && dotnet build /sb/src/Smithbox.Remote/Smithbox.Remote.csproj -c Release-linux -o /remote -v q -nologo \
 && cp /remote/Smithbox.Remote.dll /sb/src/Smithbox.Vulkan/bin/Release-linux/net10.0/linux-x64/

FROM src AS build-win
RUN dotnet build /sb/src/Smithbox.Vulkan/Smithbox.Vulkan.csproj -c Release-win -v q -nologo \
 && dotnet build /sb/src/Smithbox.Remote/Smithbox.Remote.csproj -c Release-win -o /remote -v q -nologo \
 && cp /remote/Smithbox.Remote.dll /sb/src/Smithbox.Vulkan/bin/Release-win/net10.0/win-x64/

FROM scratch AS win
COPY --from=build-win /sb/src/Smithbox.Vulkan/bin/Release-win/net10.0/win-x64/ /

FROM debian:13 AS runtime
ENV DEBIAN_FRONTEND=noninteractive
# Needs Mesa 25 (llvmpipe with descriptor indexing) and glibc >= 2.38 (cimgui.so); Debian 12 has neither
RUN apt-get update \
 && apt-get install -y --no-install-recommends mesa-vulkan-drivers libvulkan1 xvfb libicu76 libgtk-3-0t64 \
    libx11-6 libxext6 libxrandr2 libxcursor1 libxi6 libxss1 libgl1 curl ca-certificates \
 && rm -rf /var/lib/apt/lists/*
COPY --from=build-linux /sb/src/Smithbox.Vulkan/bin/Release-linux/net10.0/linux-x64/ /app/
COPY src/Smithbox.Remote/entrypoint.sh /entrypoint.sh
ENV VK_ICD_FILENAMES=/usr/share/vulkan/icd.d/lvp_icd.json HOME=/home/sb DISPLAY=:99 \
    DOTNET_STARTUP_HOOKS=/app/Smithbox.Remote.dll SMITHBOX_REMOTE_PORT=7790
WORKDIR /app
ENTRYPOINT ["sh", "/entrypoint.sh"]
