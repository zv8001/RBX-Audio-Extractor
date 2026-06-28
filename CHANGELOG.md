# Changelog

All notable changes to RBX Asset Extractor are documented in this file.

## [2.0.1] - 2026-06-27

### Fixed

- Embedded native Magick.NET, SQLite, and SNI libraries into the published single-file executable.
- Removed the startup DLL downloader and its dependency on the hosted Netlify files.
- Native libraries are now extracted locally by the .NET bundle loader without an internet connection.
## [2.0.0] - 2026-06-27

### Performance

- Replaced the slow Full Game Audio and Full Game Images extraction loops with direct, read-only SQLite cache scanning.
- Scan only short payload headers while discovering files instead of loading every complete asset.
- Added support for assets stored inline in `rbx-storage.db` and externally in the `rbx-storage` directory.
- Removed the mass temporary-file dump from full-cache audio and image workflows.
- Assets are now loaded only when previewed, played, or exported.
- Added background scanning and exporting to keep the interface responsive.
- Added batched list updates and throttled progress reporting.
- Bulk exports reuse database connections and stream external payloads with larger buffers.

### Audio

- Added direct Roblox cache detection for OGG Vorbis and MP3 audio.
- Added MP3 playback through the existing NAudio audio system.
- Cached audio now streams from memory without temporary copies.
- Added format-aware filenames, extensions, and save dialogs.
- Added direct selected-file and bulk audio export.
- Added scan and export status reporting to Full Game Audio.

### Images

- Separated Full Game Images scanning from Full Game Audio.
- Added direct detection for PNG, JPEG, BMP, and WebP images.
- Added direct image preview from cached payload bytes.
- Added stale-preview protection when rapidly changing selections.
- Added native-format export and optional PNG conversion.
- Added direct selected-file and bulk image export.
- Added scan and export status reporting to Full Game Images.

### Roblox thumbnails

- Added a permanent **Thumbnails** tab.
- Added parsing for Roblox HTTP-cache records stored in database category 11.
- Added extraction for cached avatar images, avatar headshots, asset thumbnails, and game thumbnails.
- Added PNG and WebP thumbnail support.
- Use meaningful Roblox cache request names for exported thumbnails when available.
- Added thumbnail preview, selected export, bulk export, and progress reporting.
- A real-cache verification found 1,199 thumbnails: 1,192 WebP and 7 PNG.

### Mesh extraction

- Added a permanent **Meshes** tab.
- Added direct Roblox cache mesh discovery.
- Added support for Roblox mesh versions 1 through 5.
- Added support for Draco-compressed version 7 meshes.
- Added Wavefront OBJ export with vertices, normals, UV coordinates, and triangle faces.
- Added selected and bulk mesh export.
- Unsupported mesh versions are marked clearly instead of failing silently.
- A real-cache verification found 4,169 exportable meshes.

### Mesh preview

- Added an interactive mesh preview window.
- Double-clicking a supported mesh opens its preview.
- Added mouse rotation and mouse-wheel zoom.
- Added automatic model centering and scaling.
- Added wireframe rendering and vertex/triangle information.
- Mesh decoding and preview loading run asynchronously.

### Models and textures

- Added a permanent **RBXM & KTX** tab.
- Added detection and export for binary RBXM, XML RBXM, KTX, and KTX2 files.
- Added filtering between RBXM and KTX results.
- Added selected and bulk export.
- Bulk exports can separate RBXM and KTX files into their own directories.

### Fonts

- Added a permanent **Fonts** tab.
- Added detection and export for TrueType (`.ttf`) and OpenType (`.otf`) fonts.
- Added selected and bulk font export.
- A real-cache verification found 92 TTF and 5 OTF files.

### Metadata

- Added a permanent **Metadata** tab.
- Added detection and export for JSON, texture-pack XML, and HLS/M3U8 playlists.
- Added an integrated read-only text preview.
- Added selected and bulk metadata export.
- A real-cache verification found 43 JSON records, 611 texture-pack XML records, and 10 playlists.

### Cache clearing safety

- Added an initial confirmation before deleting the Roblox local asset cache.
- Added an optional prompt to force-close running Roblox processes.
- Added a second explicit warning before force-closing anything.
- Warn that Roblox Studio will close and unsaved work may be lost.
- Added recursive child-process termination for Roblox processes.
- Added logging for successfully closed and failed processes.
- Prevent multiple cache-clear operations from running simultaneously.

### Interface

- Added Meshes, RBXM & KTX, Thumbnails, Fonts, and Metadata as real WinForms Designer tabs.
- All new tabs and their controls are visible and editable in Visual Studio's Designer.
- No new tabs are dynamically constructed at runtime.
- Added permanent status labels, progress bars, lists, previews, and scan/export controls.
- Added control anchoring for resizing.

### Project modernization

- Pinned the project to .NET SDK 8.0.422 using `global.json`.
- Updated Magick.NET from 14.9.0 to 14.14.0.
- Replaced the local `DracoMeshDecoder.dll` reference with the `Openize.Drako` NuGet package.
- Verified the current NuGet dependency graph reports no known vulnerable packages.
- Verified both Release and Visual Studio design-time builds.

### New source files

- `RBXAssetExtractor/RobloxCacheAssetExtractor.vb`
- `RBXAssetExtractor/RobloxMeshExtractor.vb`
- `RBXAssetExtractor/MeshPreviewForm.vb`
- `RBXAssetExtractor/RobloxSupplementalCacheExtractor.vb`

### Known warnings

- TradeWright.TabControlExtra and VedooControls still target older .NET Framework versions.
- Windows Media Player COM references continue to produce compatibility warnings.
- One pre-existing VB iteration-variable lambda warning remains.

## [1.2.4]

- Updated dependencies to address previously reported package vulnerabilities.