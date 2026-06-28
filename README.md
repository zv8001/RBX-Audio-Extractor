# RBX Asset Extractor

RBX Asset Extractor is a Windows desktop application for finding, previewing, and exporting assets stored in Roblox's local cache. It can recover audio, images, meshes, models, textures, fonts, thumbnails, and metadata without dumping the entire cache into temporary folders.

![RBX Asset Extractor](docs/images/rbx-asset-extractor.png)

## Download

[Download the latest release from GitHub](https://github.com/zv8001/RBX-Audio-Extractor/releases/latest).

Download and run `RBXAssetExtractor.exe`. Official self-contained releases include .NET and the required native SQLite and image libraries, so no separate dependency installation is needed.

RBX Asset Extractor currently supports 64-bit Windows.

## Quick start

1. Run Roblox or Roblox Studio at least once so a local asset cache exists.
2. Open RBX Asset Extractor.
3. Choose **Audio**, **Images**, **Meshes**, **Cache files**, or **More assets**.
4. Select **Scan cache**.
5. Preview an item or export the selected results.

The cache location is detected automatically. Most assets use their Roblox cache key as the filename because the original public asset name is not always stored locally.

## Supported assets

- **Audio:** OGG and MP3 discovery, duration sorting, playback, seeking, and export.
- **Images:** PNG, JPEG, BMP, and WebP preview and export.
- **Meshes:** Roblox mesh detection, interactive 3D preview, and OBJ conversion.
- **Cache files:** RBXM models plus KTX and KTX2 textures.
- **More assets:** Thumbnails, TrueType/OpenType fonts, JSON, XML, and HLS playlist metadata.
- **Bulk export:** Export an individual result or an entire supported category.

## Fast, local cache access

Normal scans open Roblox's SQLite cache in read-only mode. Assets are read only when needed for identification, preview, playback, or export. The application does not copy the entire cache into a temporary extraction folder.

Your cached assets are not uploaded. Internet access is used for version checks, creator messages, project links, and downloading an update when you approve it.

## Cache maintenance

The **Maintenance** page can open the Roblox cache folder or clear it. Clearing the cache is destructive and removes Roblox's locally cached assets, so the application asks for confirmation first.

If Roblox or Studio is locking the cache, the application can offer to close their processes. Read both confirmation dialogs carefully—this can close Roblox Studio and any unsaved work.

## Troubleshooting

### Cache not detected

Launch Roblox or Roblox Studio once, then restart RBX Asset Extractor. The expected database location is:

```text
%LocalAppData%\Roblox\rbx-storage.db
```

### A scan returns no results

Roblox only stores assets that have been downloaded on your computer. Join a game or open a Studio project containing the asset, then scan again.

### Windows warns about the executable

Only download releases from this GitHub repository or the official project updater. Windows may show a reputation warning for applications that are not code-signed.

### An update is available

The application checks the hosted version when it starts. Accepting the custom update prompt downloads the current release over HTTPS and restarts the application.

## Links

- [Project website](http://vexthatprotogen.com/)
- [GitHub repository](https://github.com/zv8001/RBX-Audio-Extractor)
- [Changelog](CHANGELOG.md)

Made by **zv800 / Vex**.