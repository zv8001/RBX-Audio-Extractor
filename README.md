# RBX Asset Extractor - WPF

This repository contains the canonical WPF RBX Asset Extractor application.
It targets .NET 10 for Windows and keeps all Roblox cache work local to the desktop process.

## Open and run

Open `RBX-Audio-Extractor2.slnx` in Visual Studio with the .NET desktop development workload installed.
The repository-level `global.json` selects the .NET 10 SDK used by the application.

```powershell
cd RBX-Audio-Extractor2
dotnet build
dotnet run
```

Publish the standalone x64 executable with:

```powershell
dotnet publish -c Release
```

Native dependencies are embedded into the single-file bundle and extracted locally by .NET when the application starts.

## Included workspaces

- Direct cached OGG/MP3 scanning, playback, seeking, volume, and export
- PNG, JPEG, BMP, and WebP preview/export
- Roblox mesh detection, native WPF 3D preview, and OBJ export
- RBXM, KTX, and KTX2 export
- Thumbnail, font, JSON, XML, and playlist extraction
- Read-only SQLite scanning with no mass temporary-file dump
- Confirmed cache maintenance with optional Roblox/Studio process termination
- Update checks, creator messages, activity logs, and HTTPS downloads

## Structure

- `RBX-Audio-Extractor2/Core/` contains the Roblox cache readers and mesh decoders.
- `RBX-Audio-Extractor2/Services/` contains shared application state, logging, and cache paths.
- `RBX-Audio-Extractor2/Views/` contains one WPF view per feature area.
- `RBX-Audio-Extractor2/MainWindow.xaml` contains the application shell and navigation.

The WPF project is the main and sole supported application.