Imports System.Media

Public Class AssetRenameDialog
    Public Sub New(currentDisplayName As String, currentCustomName As String)
        InitializeComponent()
        NameBox.Text = If(currentCustomName, String.Empty)
        AddHandler ContentRendered, Sub()
                                        SystemSounds.Asterisk.Play()
                                        NameBox.Focus()
                                        NameBox.SelectAll()
                                    End Sub
    End Sub

    Public ReadOnly Property AssetName As String
        Get
            Return NameBox.Text
        End Get
    End Property

    Private Sub SaveButton_Click(sender As Object, e As RoutedEventArgs)
        DialogResult = True
    End Sub

    Private Sub RemoveButton_Click(sender As Object, e As RoutedEventArgs)
        NameBox.Clear()
        DialogResult = True
    End Sub

    Private Sub CancelButton_Click(sender As Object, e As RoutedEventArgs)
        DialogResult = False
    End Sub

    Private Sub Header_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
        If e.ChangedButton = MouseButton.Left Then DragMove()
    End Sub
End Class