Imports System.Collections.ObjectModel
Imports System.Diagnostics
Imports System.IO
Imports System.Reflection
Imports System.Windows.Threading

Public Module AppServices
    Public Const ProjectWebsiteUrl As String = "https://rbx-asset-extractor.vexthatprotogen.com/"
    Public Event StatusChanged(message As String, progress As Double, indeterminate As Boolean)
    Public Event AssetCountChanged(category As String, count As Integer)
    Public Event LogAdded(message As String)
    Public Event CacheCleared()
    Public Event ApplicationDataCleared()

    Public ReadOnly Property Activity As New ObservableCollection(Of String)()

    Public ReadOnly Property CurrentVersion As String
        Get
            Dim attribute = Assembly.GetExecutingAssembly().GetCustomAttribute(Of AssemblyInformationalVersionAttribute)()
            Dim value = If(attribute?.InformationalVersion, Assembly.GetExecutingAssembly().GetName().Version?.ToString())
            If String.IsNullOrWhiteSpace(value) Then Return "unknown"
            Dim metadataIndex = value.IndexOf("+"c)
            If metadataIndex >= 0 Then value = value.Substring(0, metadataIndex)
            Return value
        End Get
    End Property

    Public ReadOnly Property RobloxLocalPath As String
        Get
            Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox")
        End Get
    End Property

    Public ReadOnly Property DatabasePath As String
        Get
            Return Path.Combine(RobloxLocalPath, "rbx-storage.db")
        End Get
    End Property

    Public ReadOnly Property PayloadPath As String
        Get
            Return Path.Combine(RobloxLocalPath, "rbx-storage")
        End Get
    End Property

    Public Sub Report(message As String, Optional progress As Double = 0, Optional indeterminate As Boolean = False)
        Dispatch(Sub()
                     RaiseEvent StatusChanged(message, Math.Max(0, Math.Min(100, progress)), indeterminate)
                     AddLog(message)
                 End Sub)
    End Sub

    Public Sub SetCount(category As String, count As Integer)
        Dispatch(Sub() RaiseEvent AssetCountChanged(category, count))
    End Sub

    Public Sub AddLog(message As String)
        Dim line = $"[{Date.Now:HH:mm:ss}] {message}"
        Dispatch(Sub()
                     Activity.Add(line)
                     While Activity.Count > 1000
                         Activity.RemoveAt(0)
                     End While
                     RaiseEvent LogAdded(line)
                 End Sub)
    End Sub

    Public Sub NotifyApplicationDataCleared()
        RaiseEvent ApplicationDataCleared()
    End Sub

    Public Sub NotifyCacheCleared()
        Dispatch(Sub() RaiseEvent CacheCleared())
    End Sub

    Public Sub OpenPath(path As String)
        If String.IsNullOrWhiteSpace(path) Then Return
        Process.Start(New ProcessStartInfo(path) With {.UseShellExecute = True})
    End Sub

    Public Function FormatBytes(value As Long) As String
        If value < 1024 Then Return $"{value:N0} B"
        If value < 1024L * 1024L Then Return $"{value / 1024.0:N1} KB"
        If value < 1024L * 1024L * 1024L Then Return $"{value / (1024.0 * 1024.0):N1} MB"
        Return $"{value / (1024.0 * 1024.0 * 1024.0):N2} GB"
    End Function

    Public Function SafePrefix(value As String, Optional length As Integer = 12) As String
        If String.IsNullOrEmpty(value) Then Return "unknown"
        Return value.Substring(0, Math.Min(length, value.Length))
    End Function

    Private Sub Dispatch(action As Action)
        Dim dispatcher = Application.Current?.Dispatcher
        If dispatcher Is Nothing OrElse dispatcher.CheckAccess() Then
            action()
        Else
            dispatcher.BeginInvoke(action, DispatcherPriority.Background)
        End If
    End Sub
End Module