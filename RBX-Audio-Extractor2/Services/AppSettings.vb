''' <summary>
''' Application preferences, persisted as key/value rows in the settings table of
''' RBXAssetExtractor.db. Currently holds the optional custom Roblox SQL database path.
''' </summary>
Public NotInheritable Class AppSettings
    Private Const DatabaseOverrideKey As String = "database_path_override"

    Private Shared _current As AppSettings

    Public Property DatabasePathOverride As String

    Public Shared ReadOnly Property Current As AppSettings
        Get
            If _current Is Nothing Then Load()
            Return _current
        End Get
    End Property

    Public Shared Sub Load()
        Dim settings As New AppSettings()
        Try
            settings.DatabasePathOverride = AppDatabase.GetSetting(DatabaseOverrideKey)
        Catch
            settings.DatabasePathOverride = Nothing
        End Try
        _current = settings
    End Sub

    Public Shared Sub Save()
        Try
            AppDatabase.SetSetting(DatabaseOverrideKey, _current.DatabasePathOverride)
        Catch
            ' Settings are best-effort; failing to persist must never crash the app.
        End Try
    End Sub
End Class
