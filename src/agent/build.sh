#!/usr/bin/env bash

set -xe

# Build agent for both platforms
dotnet publish ScanVul.Agent/ScanVul.Agent.csproj -c Release -r win-x64
dotnet publish ScanVul.Agent/ScanVul.Agent.csproj -c Release -r linux-x64

# Build installer
dotnet publish ScanVul.Agent.Installer/ScanVul.Agent.Installer.csproj -c Release -r win-x64
dotnet publish ScanVul.Agent.Installer/ScanVul.Agent.Installer.csproj -c Release -r linux-x64