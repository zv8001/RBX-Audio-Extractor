''' <summary>
''' Signed description of a published release. The signature over the serialized manifest binds the
''' version and the executable's SHA-256 together, so the update channel can authenticate both.
''' </summary>
Public NotInheritable Class UpdateManifest
    Public Property Version As String
    Public Property Sha256 As String
End Class
