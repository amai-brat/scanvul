#!/usr/bin/env bash

set -xe

cd src/agent
./build.sh

OUTPUT_DIR="$(cd ../../ && pwd)/build"
mkdir -p "$OUTPUT_DIR"

echo "Packaging installers to $OUTPUT_DIR..."

PROJ_DIR="ScanVul.Agent.Installer"

# 1. Package Windows Installer
WIN_PATH=$(find "$PROJ_DIR" -type d -path "*/win-x64/publish" | head -n 1)

if [ -n "$WIN_PATH" ]; then
    echo "Found Windows publish dir: $WIN_PATH"
    pushd "$WIN_PATH" > /dev/null
    zip -r "$OUTPUT_DIR/ScanVul.Agent.Installer-win-x64.zip" .
    popd > /dev/null
else
    echo "Error: Windows publish directory not found via find."
    exit 1
fi

# 2. Package Linux Installer
LINUX_PATH=$(find "$PROJ_DIR" -type d -path "*/linux-x64/publish" | head -n 1)

if [ -n "$LINUX_PATH" ]; then
    echo "Found Linux publish dir: $LINUX_PATH"
    pushd "$LINUX_PATH" > /dev/null
    zip -r "$OUTPUT_DIR/ScanVul.Agent.Installer-linux-x64.zip" .
    popd > /dev/null
else
    echo "Error: Linux publish directory not found via find."
    exit 1
fi

ls -lh "$OUTPUT_DIR"