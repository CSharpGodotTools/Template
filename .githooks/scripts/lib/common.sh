#!/usr/bin/env bash

dotnet_cmd() {
    if command -v flatpak-spawn >/dev/null 2>&1; then
        flatpak-spawn --host dotnet "$@"
    else
        dotnet "$@"
    fi
}