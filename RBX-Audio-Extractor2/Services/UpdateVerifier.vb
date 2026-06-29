Imports System.Security.Cryptography

''' <summary>
''' Verifies downloaded update binaries against an offline-held signing key. The PRIVATE key never
''' ships and never touches the server; only the PUBLIC key below is embedded in the app. A binary
''' is trusted only if it was signed by the matching private key, so a compromised download server
''' cannot push a malicious update (it can serve any bytes, but cannot forge a valid signature).
''' See tools/README.md for how to generate the keypair and sign a release.
''' </summary>
Public NotInheritable Class UpdateVerifier
    ' One-time: run tools/generate-update-key.ps1, keep the PRIVATE key offline, and paste the
    ' PUBLIC key (Base64 SubjectPublicKeyInfo, ECDSA P-256) here. Until this is set, updates are
    ' refused (fail-closed) rather than installed unverified.
    Public Const PublicKeyBase64 As String = "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEkqNcD9FWcpxo+DMp1UQ1rb+BUU6AZomhczetP9m+3l4euuQkF/VxlLJaNV59oojw0soY4BlcppGtguT9mZvLUA=="

    Private Sub New()
    End Sub

    Public Shared ReadOnly Property IsConfigured As Boolean
        Get
            Return Not String.IsNullOrWhiteSpace(PublicKeyBase64) AndAlso Not PublicKeyBase64.StartsWith("REPLACE_WITH", StringComparison.Ordinal)
        End Get
    End Property

    ''' <summary>
    ''' Returns True only if <paramref name="payload"/> matches the detached Base64 ECDSA/SHA-256
    ''' <paramref name="signatureBase64"/> under the embedded public key. Throws if no key is embedded.
    ''' </summary>
    Public Shared Function Verify(payload As Byte(), signatureBase64 As String) As Boolean
        If Not IsConfigured Then Throw New InvalidOperationException("Update verification is not configured: this build has no signing public key embedded.")
        If payload Is Nothing OrElse payload.Length = 0 Then Return False
        If String.IsNullOrWhiteSpace(signatureBase64) Then Return False

        Dim signature As Byte()
        Try
            signature = Convert.FromBase64String(signatureBase64.Trim())
        Catch
            Return False
        End Try

        Try
            Using key = ECDsa.Create()
                key.ImportSubjectPublicKeyInfo(Convert.FromBase64String(PublicKeyBase64), Nothing)
                Return key.VerifyData(payload, signature, HashAlgorithmName.SHA256)
            End Using
        Catch
            Return False
        End Try
    End Function
End Class
