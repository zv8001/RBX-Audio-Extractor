Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading
Imports System.Windows.Threading

Class Application
    Private Shared ReadOnly CrashLogLock As New Object()
    Private Shared fatalErrorReported As Integer

    Protected Overrides Sub OnStartup(e As StartupEventArgs)
        AddHandler DispatcherUnhandledException, AddressOf OnDispatcherUnhandledException
        AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf OnDomainUnhandledException
        AddHandler TaskScheduler.UnobservedTaskException, AddressOf OnUnobservedTaskException

        MyBase.OnStartup(e)
    End Sub

    Private Sub SingleLineTextBox_PreviewMouseWheel(sender As Object, e As MouseWheelEventArgs)
        Dim textBox = TryCast(sender, TextBox)
        If textBox IsNot Nothing Then textBox.ScrollToLine(0)
        e.Handled = True
    End Sub

    Private Sub SingleLineTextBox_TextChanged(sender As Object, e As TextChangedEventArgs)
        Dim textBox = TryCast(sender, TextBox)
        If textBox IsNot Nothing Then textBox.ScrollToLine(0)
    End Sub

    Private Shared Sub OnDispatcherUnhandledException(sender As Object, e As DispatcherUnhandledExceptionEventArgs)
        ReportFatalError("WPF dispatcher", e.Exception)
        e.Handled = False
    End Sub

    Private Shared Sub OnDomainUnhandledException(sender As Object, e As UnhandledExceptionEventArgs)
        Dim exception = TryCast(e.ExceptionObject, Exception)
        If exception Is Nothing Then
            exception = New Exception(Convert.ToString(e.ExceptionObject))
        End If

        ReportFatalError("application domain", exception)
    End Sub

    Private Shared Sub OnUnobservedTaskException(sender As Object, e As UnobservedTaskExceptionEventArgs)
        WriteCrashLog("unobserved background task", e.Exception)
        e.SetObserved()
    End Sub

    Private Shared Sub ReportFatalError(source As String, exception As Exception)
        If Interlocked.Exchange(fatalErrorReported, 1) <> 0 Then Return

        Dim logPath = WriteCrashLog(source, exception)
        Dim message As New StringBuilder()
        message.AppendLine("RBX Asset Extractor encountered an unexpected error and must close.")
        message.AppendLine()
        message.AppendLine(exception.Message)

        If Not String.IsNullOrWhiteSpace(logPath) Then
            message.AppendLine()
            message.AppendLine("A crash report was saved to:")
            message.AppendLine(logPath)
        End If

        NativeMethods.MessageBoxW(
            IntPtr.Zero,
            message.ToString(),
            "RBX Asset Extractor - Fatal Error",
            NativeMethods.ErrorIcon Or NativeMethods.SetForeground)
    End Sub

    Private Shared Function WriteCrashLog(source As String, exception As Exception) As String
        Try
            Dim logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RBXAssetExtractor",
                "Logs")
            Directory.CreateDirectory(logDirectory)

            Dim logPath = Path.Combine(
                logDirectory,
                $"crash-{DateTime.Now:yyyyMMdd-HHmmss-fff}.log")

            Dim report As New StringBuilder()
            report.AppendLine("RBX Asset Extractor crash report")
            report.AppendLine($"Timestamp: {DateTimeOffset.Now:O}")
            report.AppendLine($"Version: {AppServices.CurrentVersion}")
            report.AppendLine($"Source: {source}")
            report.AppendLine($"Operating system: {RuntimeInformation.OSDescription}")
            report.AppendLine($"Process architecture: {RuntimeInformation.ProcessArchitecture}")
            report.AppendLine($".NET runtime: {RuntimeInformation.FrameworkDescription}")
            report.AppendLine()
            report.AppendLine(exception.ToString())

            SyncLock CrashLogLock
                File.WriteAllText(logPath, report.ToString(), Encoding.UTF8)
            End SyncLock

            Return logPath
        Catch
            Return String.Empty
        End Try
    End Function

    Private NotInheritable Class NativeMethods
        Friend Const ErrorIcon As UInteger = &H10UI
        Friend Const SetForeground As UInteger = &H10000UI

        <DllImport("user32.dll", CharSet:=CharSet.Unicode, EntryPoint:="MessageBoxW")>
        Friend Shared Function MessageBoxW(
            owner As IntPtr,
            text As String,
            caption As String,
            messageType As UInteger) As Integer
        End Function
    End Class
End Class
