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

    Private settingsLoaded As Boolean
    Private databaseOverride As String

    Private Sub EnsureSettingsLoaded()
        If settingsLoaded Then Return
        settingsLoaded = True
        Try
            databaseOverride = AppSettings.Current.DatabasePathOverride
        Catch
            databaseOverride = Nothing
        End Try
    End Sub

    ''' <summary>The Roblox install's default LocalAppData\Roblox folder.</summary>
    Public ReadOnly Property DefaultRobloxLocalPath As String
        Get
            Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox")
        End Get
    End Property

    Public ReadOnly Property DefaultDatabasePath As String
        Get
            Return Path.Combine(DefaultRobloxLocalPath, "rbx-storage.db")
        End Get
    End Property

    ''' <summary>True when the scanners are pointed at a user-chosen database instead of the default.</summary>
    Public ReadOnly Property IsUsingCustomDatabase As Boolean
        Get
            EnsureSettingsLoaded()
            Return Not String.IsNullOrWhiteSpace(databaseOverride)
        End Get
    End Property

    ''' <summary>Folder containing the active database; external payloads are resolved relative to it.</summary>
    Public ReadOnly Property RobloxLocalPath As String
        Get
            If Not IsUsingCustomDatabase Then Return DefaultRobloxLocalPath
            Return If(Path.GetDirectoryName(databaseOverride), DefaultRobloxLocalPath)
        End Get
    End Property

    Public ReadOnly Property DatabasePath As String
        Get
            If Not IsUsingCustomDatabase Then Return DefaultDatabasePath
            Return databaseOverride
        End Get
    End Property

    Public ReadOnly Property PayloadPath As String
        Get
            Return Path.Combine(RobloxLocalPath, "rbx-storage")
        End Get
    End Property

    ''' <summary>Point all scanners at a different rbx-storage.db. Persisted across sessions.</summary>
    Public Sub SetDatabasePathOverride(path As String)
        EnsureSettingsLoaded()
        databaseOverride = If(String.IsNullOrWhiteSpace(path), Nothing, path)
        AppSettings.Current.DatabasePathOverride = databaseOverride
        AppSettings.Save()
    End Sub

    ''' <summary>Restore the default Roblox database path.</summary>
    Public Sub ResetDatabasePathOverride()
        SetDatabasePathOverride(Nothing)
    End Sub

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

    Private ReadOnly fitColumnDefinitions As New Dictionary(Of ListView, (Column As GridViewColumn, Weight As Double)())()

    Public Sub FitGridViewColumns(list As ListView, ParamArray definitions As (Column As GridViewColumn, Weight As Double)())
        If list Is Nothing OrElse definitions Is Nothing OrElse definitions.Length = 0 OrElse list.ActualWidth <= 0 Then Return
        ' Remember the layout so we can refit when the vertical scrollbar appears/disappears (which
        ' changes the viewport width without raising the ListView's own SizeChanged).
        If Not fitColumnDefinitions.ContainsKey(list) Then WireScrollBarRefit(list)
        fitColumnDefinitions(list) = definitions
        ' Only reserve room for the vertical scrollbar when it is actually showing; otherwise the
        ' reserved space renders as an empty trailing GridView header segment.
        Dim scrollBarReserve = If(IsVerticalScrollBarVisible(list), SystemParameters.VerticalScrollBarWidth, 0.0)
        Dim available = Math.Max(0, list.ActualWidth - scrollBarReserve - 2)
        Dim totalWeight = definitions.Sum(Function(item) Math.Max(0, item.Weight))
        If available <= 0 OrElse totalWeight <= 0 Then Return
        For Each definition In definitions
            definition.Column.Width = Math.Max(34, available * Math.Max(0, definition.Weight) / totalWeight)
        Next
    End Sub

    Private Sub WireScrollBarRefit(list As ListView)
        ' ScrollViewer.ScrollChanged bubbles up from the ListView's template; ViewportWidthChange is
        ' non-zero exactly when the vertical scrollbar toggles, which is when we must refit.
        list.AddHandler(ScrollViewer.ScrollChangedEvent, New ScrollChangedEventHandler(AddressOf OnListScrollChanged))
    End Sub

    Private Sub OnListScrollChanged(sender As Object, e As ScrollChangedEventArgs)
        If e.ViewportWidthChange = 0 Then Return
        Dim list = TryCast(sender, ListView)
        If list Is Nothing Then Return
        Dim definitions As (Column As GridViewColumn, Weight As Double)() = Nothing
        If fitColumnDefinitions.TryGetValue(list, definitions) Then FitGridViewColumns(list, definitions)
    End Sub

    Private Function IsVerticalScrollBarVisible(root As DependencyObject) As Boolean
        Dim viewer = FindVisualChild(Of ScrollViewer)(root)
        Return viewer IsNot Nothing AndAlso viewer.ComputedVerticalScrollBarVisibility = Visibility.Visible
    End Function

    Private Function FindVisualChild(Of T As DependencyObject)(parent As DependencyObject) As T
        If parent Is Nothing Then Return Nothing
        Dim count = VisualTreeHelper.GetChildrenCount(parent)
        For i = 0 To count - 1
            Dim child = VisualTreeHelper.GetChild(parent, i)
            Dim match = TryCast(child, T)
            If match IsNot Nothing Then Return match
            match = FindVisualChild(Of T)(child)
            If match IsNot Nothing Then Return match
        Next
        Return Nothing
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