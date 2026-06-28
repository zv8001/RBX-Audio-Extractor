# RBX Asset Extractor — WPF

This folder contains the modern WPF replacement for the legacy WinForms application.
It targets .NET 10 for Windows and keeps all Roblox cache work local to the desktop process.

## Open and run

Open `New/RBX-Audio-Extractor2.slnx` in Visual Studio with the .NET desktop development workload installed.
The solution-local `New/global.json` selects the .NET 10 SDK without changing the .NET 8 SDK used by the legacy project.

```powershell
cd New/RBX-Audio-Extractor2
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

- `Core/` contains the standalone Roblox cache readers and mesh decoders.
- `Services/` contains shared application state, logging, and cache paths.
- `Views/` contains one WPF view per feature area.
- `MainWindow.xaml` is only the application shell and navigation.

The legacy WinForms project remains in the repository for comparison while the WPF replacement is validated.