Imports System.ComponentModel
Imports System.IO

Public Class FontPreviewWindow
    Private ReadOnly stagingDirectory As String

    Public Sub New(entry As SupplementalCacheEntry, payload As Byte())
        If entry Is Nothing Then Throw New ArgumentNullException(NameOf(entry))
        If payload Is Nothing OrElse payload.Length = 0 Then Throw New InvalidDataException("The cached font payload is empty.")
        InitializeComponent()

        stagingDirectory = Path.Combine(Path.GetTempPath(), "RBXAssetExtractor", "FontPreviews", Guid.NewGuid().ToString("N"))
        Try
            Directory.CreateDirectory(stagingDirectory)
            Dim fontPath = Path.Combine(stagingDirectory, "preview" & entry.Extension)
            File.WriteAllBytes(fontPath, payload)

            Dim folderUri As New Uri(stagingDirectory.TrimEnd(Path.DirectorySeparatorChar) & Path.DirectorySeparatorChar, UriKind.Absolute)
            Dim families = Fonts.GetFontFamilies(folderUri).ToList()
            If families.Count = 0 Then Throw New InvalidDataException("Windows Presentation Foundation could not load a font family from this file.")

            Dim previewFamily = families(0)
            Dim familyName = previewFamily.Source
            Dim familyMarker = familyName.LastIndexOf("#"c)
            If familyMarker >= 0 AndAlso familyMarker + 1 < familyName.Length Then familyName = familyName.Substring(familyMarker + 1)
            Title = $"Font Preview · {entry.FriendlyName}"
            TitleText.Text = familyName
            DetailsText.Text = $"{entry.TypeLabel}  •  {AppServices.FormatBytes(entry.Size)}  •  {AppServices.SafePrefix(entry.Hash, 16)}"
            LargeSample.FontFamily = previewFamily
            MediumSample.FontFamily = previewFamily
            SmallSample.FontFamily = previewFamily
            AlphabetSample.FontFamily = previewFamily
            SymbolSample.FontFamily = previewFamily
        Catch
            CleanupStagingDirectory()
            Throw
        End Try
    End Sub

    Private Sub CloseButton_Click(sender As Object, e As RoutedEventArgs)
        Close()
    End Sub

    Protected Overrides Sub OnClosing(e As CancelEventArgs)
        MyBase.OnClosing(e)
        Dispatcher.BeginInvoke(Sub() CleanupStagingDirectory(), System.Windows.Threading.DispatcherPriority.ApplicationIdle)
    End Sub

    Private Sub CleanupStagingDirectory()
        Try
            If Directory.Exists(stagingDirectory) Then Directory.Delete(stagingDirectory, recursive:=True)
        Catch
        End Try
    End Sub
End Class