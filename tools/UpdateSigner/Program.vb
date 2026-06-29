Imports System
Imports System.IO
Imports System.Security.Cryptography

' Update signing helper. Run from the repository root:
'   dotnet run --project tools/UpdateSigner -- genkey
'   dotnet run --project tools/UpdateSigner -- sign <path-to-RBXAssetExtractor.exe>
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
        Console.WriteLine("  dotnet run --project tools/UpdateSigner -- sign <path-to-RBXAssetExtractor.exe>")
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

        Dim privateKey = File.ReadAllText(PrivateKeyFile).Trim()
        Dim payload = File.ReadAllBytes(exePath)

        Using ec = ECDsa.Create()
            Dim bytesRead As Integer
            ec.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKey), bytesRead)
            Dim signature = Convert.ToBase64String(ec.SignData(payload, HashAlgorithmName.SHA256))
            Dim signaturePath = exePath & ".sig"
            File.WriteAllText(signaturePath, signature)

            Console.WriteLine($"Signature written to: {signaturePath}")
            Console.WriteLine("Upload both files (same folder) to the update host:")
            Console.WriteLine("  RBXAssetExtractor.exe")
            Console.WriteLine("  RBXAssetExtractor.exe.sig")
        End Using
    End Sub

    ' Mirrors UpdateVerifier.Verify in the app: checks <exe> against <exe>.sig using the public key.
    Private Sub VerifyFile(exePath As String, publicKeyBase64 As String)
        If Not File.Exists(exePath) Then Throw New FileNotFoundException("Executable not found.", exePath)
        Dim signaturePath = exePath & ".sig"
        If Not File.Exists(signaturePath) Then Throw New FileNotFoundException("Signature not found.", signaturePath)

        Dim payload = File.ReadAllBytes(exePath)
        Dim signature = Convert.FromBase64String(File.ReadAllText(signaturePath).Trim())

        Using ec = ECDsa.Create()
            Dim bytesRead As Integer
            ec.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKeyBase64), bytesRead)
            Dim ok = ec.VerifyData(payload, signature, HashAlgorithmName.SHA256)
            Console.WriteLine(If(ok, "VALID: signature matches the public key.", "INVALID: signature does NOT match — do not ship."))
            If Not ok Then Environment.Exit(2)
        End Using
    End Sub
End Module
