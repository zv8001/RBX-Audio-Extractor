Imports System
Imports System.Diagnostics
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json
Imports System.Text.RegularExpressions

' Update signing helper. Run from the repository root:
'   dotnet run --project tools/UpdateSigner -- genkey
'   dotnet run --project tools/UpdateSigner -- sign   <path-to-RBXAssetExtractor.exe>
'   dotnet run --project tools/UpdateSigner -- verify <path-to-RBXAssetExtractor.exe> <publicKeyBase64>
Module Program
    Private ReadOnly PrivateKeyFile As String = Path.Combine(Directory.GetCurrentDirectory(), "tools", "update-private-key.txt")

    Sub Main(args As String())
        Try
            If args.Length = 0 Then
                PrintUsage()
                Return
            End If
            Select Case args(0).ToLowerInvariant()
                Case "genkey"
                    GenerateKey()
                Case "sign"
                    If args.Length < 2 Then Throw New ArgumentException("'sign' needs the path to the built executable.")
                    SignFile(args(1))
                Case "verify"
                    If args.Length < 3 Then Throw New ArgumentException("'verify' needs <path-to-exe> <publicKeyBase64>.")
                    VerifyFile(args(1), args(2))
                Case Else
                    PrintUsage()
                    Environment.Exit(1)
            End Select
        Catch ex As Exception
            Console.Error.WriteLine("Error: " & ex.Message)
            Environment.Exit(1)
        End Try
    End Sub

    Private Sub PrintUsage()
        Console.WriteLine("Run from the repository root:")
        Console.WriteLine("  dotnet run --project tools/UpdateSigner -- genkey")
        Console.WriteLine("  dotnet run --project tools/UpdateSigner -- sign   <path-to-RBXAssetExtractor.exe>")
        Console.WriteLine("  dotnet run --project tools/UpdateSigner -- verify <path-to-RBXAssetExtractor.exe> <publicKeyBase64>")
    End Sub

    Private Sub GenerateKey()
        Using ec = ECDsa.Create(ECCurve.NamedCurves.nistP256)
            Dim privateKey = Convert.ToBase64String(ec.ExportPkcs8PrivateKey())
            Dim publicKey = Convert.ToBase64String(ec.ExportSubjectPublicKeyInfo())

            Directory.CreateDirectory(Path.GetDirectoryName(PrivateKeyFile))
            File.WriteAllText(PrivateKeyFile, privateKey)

            Console.WriteLine($"Private key written to: {PrivateKeyFile}")
            Console.WriteLine("  Keep it OFFLINE and secret. It is gitignored. Never commit or share it.")
            Console.WriteLine()
            Console.WriteLine("PUBLIC key - paste into RBX-Audio-Extractor2/Services/UpdateVerifier.vb (PublicKeyBase64):")
            Console.WriteLine(publicKey)
        End Using
    End Sub

    Private Sub SignFile(exePath As String)
        If Not File.Exists(exePath) Then Throw New FileNotFoundException("Executable not found.", exePath)
        If Not File.Exists(PrivateKeyFile) Then Throw New FileNotFoundException($"Private key not found at {PrivateKeyFile}. Run 'genkey' first.")

        Dim folder = Path.GetDirectoryName(Path.GetFullPath(exePath))
        Dim payload = File.ReadAllBytes(exePath)
        Dim version = ReadVersion(exePath)
        Dim hash = Sha256Hex(payload)

        ' The manifest binds version + exe hash; signing it authenticates both as one unit, so a
        ' server cannot lie about the version or pair a fake version with a different executable.
        Dim manifestJson = $"{{""version"":""{version}"",""sha256"":""{hash}""}}"
        Dim manifestBytes = Encoding.UTF8.GetBytes(manifestJson)

        Using ec = ECDsa.Create()
            Dim bytesRead As Integer
            ec.ImportPkcs8PrivateKey(Convert.FromBase64String(File.ReadAllText(PrivateKeyFile).Trim()), bytesRead)

            Dim manifestSig = Convert.ToBase64String(ec.SignData(manifestBytes, HashAlgorithmName.SHA256))
            Dim exeSig = Convert.ToBase64String(ec.SignData(payload, HashAlgorithmName.SHA256))

            File.WriteAllBytes(Path.Combine(folder, "update.json"), manifestBytes)
            File.WriteAllText(Path.Combine(folder, "update.json.sig"), manifestSig)
            File.WriteAllText(exePath & ".sig", exeSig)
            File.WriteAllText(Path.Combine(folder, "v.txt"), version)
        End Using

        Console.WriteLine($"Signed version {version}")
        Console.WriteLine($"  sha256: {hash}")
        Console.WriteLine("Wrote into the exe folder:")
        Console.WriteLine("  update.json                 (signed manifest: version + hash)")
        Console.WriteLine("  update.json.sig             (manifest signature)")
        Console.WriteLine("  RBXAssetExtractor.exe.sig   (legacy per-exe signature, for older clients)")
        Console.WriteLine("  v.txt                       (version string, for older clients)")
        Console.WriteLine("Upload these together with RBXAssetExtractor.exe to the update host.")
    End Sub

    ' Mirrors the app's install-time check: manifest signature must verify, and the exe's hash must
    ' match the hash inside the signed manifest.
    Private Sub VerifyFile(exePath As String, publicKeyBase64 As String)
        If Not File.Exists(exePath) Then Throw New FileNotFoundException("Executable not found.", exePath)
        Dim folder = Path.GetDirectoryName(Path.GetFullPath(exePath))
        Dim manifestPath = Path.Combine(folder, "update.json")
        Dim manifestSigPath = Path.Combine(folder, "update.json.sig")
        If Not File.Exists(manifestPath) Then Throw New FileNotFoundException("Manifest not found.", manifestPath)
        If Not File.Exists(manifestSigPath) Then Throw New FileNotFoundException("Manifest signature not found.", manifestSigPath)

        Dim manifestBytes = File.ReadAllBytes(manifestPath)
        Dim manifestSig = Convert.FromBase64String(File.ReadAllText(manifestSigPath).Trim())
        Dim payload = File.ReadAllBytes(exePath)

        Using ec = ECDsa.Create()
            Dim bytesRead As Integer
            ec.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKeyBase64), bytesRead)
            If Not ec.VerifyData(manifestBytes, manifestSig, HashAlgorithmName.SHA256) Then
                Console.WriteLine("INVALID: manifest signature does NOT match the public key - do not ship.")
                Environment.Exit(2)
            End If
        End Using

        Using doc = JsonDocument.Parse(manifestBytes)
            Dim version = doc.RootElement.GetProperty("version").GetString()
            Dim expectedHash = doc.RootElement.GetProperty("sha256").GetString()
            Dim actualHash = Sha256Hex(payload)
            Console.WriteLine($"manifest version: {version}")
            If String.Equals(expectedHash, actualHash, StringComparison.OrdinalIgnoreCase) Then
                Console.WriteLine("VALID: manifest signature OK and exe hash matches.")
            Else
                Console.WriteLine($"INVALID: exe hash {actualHash} does not match manifest {expectedHash} - do not ship.")
                Environment.Exit(2)
            End If
        End Using
    End Sub

    Private Function Sha256Hex(bytes As Byte()) As String
        Return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant()
    End Function

    Private Function ReadVersion(exePath As String) As String
        Dim info = FileVersionInfo.GetVersionInfo(exePath)
        Dim raw = If(String.IsNullOrWhiteSpace(info.ProductVersion), info.FileVersion, info.ProductVersion)
        Dim match = Regex.Match(If(raw, String.Empty), "\d+(?:\.\d+){1,3}")
        If Not match.Success Then Throw New InvalidDataException($"Could not read a version number from '{exePath}'.")
        Return match.Value
    End Function
End Module
