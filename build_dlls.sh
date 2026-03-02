#!/bin/bash
# build_dlls.sh - Build all projects, including GodotUtils and Visualize extras.
# Usage: Double-click to run from desktop or run from terminal

# Open a new terminal if not already running in one (KDE Konsole)
if [ -z "$KONSOLE_VERSION" ] && [ -x "$(command -v konsole)" ]; then
	konsole -e "$0" internal_run "$@" &
	exit 0
fi

if [ "$1" = "internal_run" ]; then
	shift
fi

echo "Building all projects, including extras (GodotUtils, Visualize)..."
if dotnet build -p:BuildExtras=true "$@"; then
	echo -e "\nBuild complete! DLLs have been copied to Framework/Libraries."
else
	echo -e "\nBuild failed. See errors above."
fi

echo -e "\nPress Enter to close this window."
read _
