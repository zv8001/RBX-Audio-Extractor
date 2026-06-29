# Update signing

The in-app updater ([AboutView](../RBX-Audio-Extractor2/Views/AboutView.xaml.vb)) only installs a
build described by a **signed manifest** (`update.json`) — a small file holding the release version
and the executable's SHA-256, signed by your offline private key. The app verifies the manifest
signature, refuses any version that is not newer than the installed one (anti-rollback), and checks
that the downloaded exe hashes to the value in the signed manifest. A compromised download server
can serve any bytes, but cannot forge a manifest the app accepts, nor replay an older signed one.

This costs nothing: it's a self-managed ECDSA P-256 keypair, no certificate authority involved.
(Code-signing certificates / Authenticode are a *separate*, paid thing whose only extra benefit is
removing the Windows SmartScreen "unknown publisher" warning. Not required for this protection.)

`UpdateSigner` is a tiny .NET console tool — it uses the same crypto APIs as the app, so it works
with just the .NET SDK (no PowerShell 7 needed). **Run all commands from the repository root.**

## One-time setup

```
dotnet run --project tools/UpdateSigner -- genkey
```

- Writes the **private key** to `tools/update-private-key.txt` (gitignored). Keep it offline and
  secret, and back it up. **Anyone with it can sign updates as you.**
- Prints the **public key**. Paste it into
  [`UpdateVerifier.PublicKeyBase64`](../RBX-Audio-Extractor2/Services/UpdateVerifier.vb).
  Until that constant is set, the updater fails closed (refuses to install anything).

## Every release

1. Bump the version in the project, then build/publish the release exe. The signer reads the version
   straight from the exe, so there is no separate version file to hand-edit.
2. Sign it (reads `tools/update-private-key.txt`). Easiest: **double-click `tools/sign-release.cmd`**
   (it signs the default published exe; or drag an exe onto it, or pass a path). Equivalent command:
   ```
   dotnet run --project tools/UpdateSigner -- sign "RBX-Audio-Extractor2/bin/Release/net10.0-windows/win-x64/publish/win-x64/RBXAssetExtractor.exe"
   ```
   This writes four files next to the exe: `update.json` (signed manifest), `update.json.sig`,
   `RBXAssetExtractor.exe.sig` (legacy, for clients released before manifests), and `v.txt` (legacy).
3. (Optional but recommended) verify before uploading, using your public key:
   ```
   dotnet run --project tools/UpdateSigner -- verify "RBX-Audio-Extractor2/bin/Release/net10.0-windows/win-x64/publish/win-x64/RBXAssetExtractor.exe" "<your public key>"
   ```
4. Upload these, side by side, to the update host (`https://rbxextr.vexthatprotogen.com/`):
   - `RBXAssetExtractor.exe`
   - `update.json` and `update.json.sig`  (used by current clients)
   - `RBXAssetExtractor.exe.sig` and `v.txt`  (kept so clients on older builds still update)

## Key rotation / leak

The app verifies with the *embedded* public key, so you can't just switch keys server-side. To
rotate: ship one release **signed by the current key** that contains the **new** public key, then
sign every later release with the new private key. Existing users update to the key-change build
(verified by the old key), and from then on trust the new key.
