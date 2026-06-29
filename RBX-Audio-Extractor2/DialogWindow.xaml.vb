Imports System.Media

Public Class DialogWindow
    Private dialogResultValue As MessageBoxResult = MessageBoxResult.None
    Private ReadOnly buttonsValue As MessageBoxButton
    Private ReadOnly defaultValue As MessageBoxResult
    Private ReadOnly iconValue As MessageBoxImage
    Private closingFromButton As Boolean

    Public Sub New(title As String, message As String, buttons As MessageBoxButton, icon As MessageBoxImage, defaultResult As MessageBoxResult)
        InitializeComponent()
        Me.Title = title
        TitleText.Text = title
        MessageText.Text = message
        buttonsValue = buttons
        defaultValue = defaultResult
        iconValue = icon
        ConfigureIcon(icon)
        ConfigureButtons(buttons)
    End Sub

    Protected Overrides Sub OnContentRendered(e As EventArgs)
        MyBase.OnContentRendered(e)
        PlayDialogSound()
    End Sub

    Private Sub PlayDialogSound()
        Select Case iconValue
            Case MessageBoxImage.Error
                SystemSounds.Hand.Play()
            Case MessageBoxImage.Warning
                SystemSounds.Exclamation.Play()
            Case MessageBoxImage.Question
                SystemSounds.Question.Play()
            Case MessageBoxImage.Information
                SystemSounds.Asterisk.Play()
            Case Else
                SystemSounds.Beep.Play()
        End Select
    End Sub
    Public ReadOnly Property SelectedResult As MessageBoxResult
        Get
            Return dialogResultValue
        End Get
    End Property

    Private Sub ConfigureIcon(icon As MessageBoxImage)
        Select Case icon
            Case MessageBoxImage.Error
                IconText.Text = "×"
                IconBadge.Background = CType(FindResource("DangerBrush"), Brush)
            Case MessageBoxImage.Question
                IconText.Text = "?"
                IconBadge.Background = CType(FindResource("BlueBrush"), Brush)
            Case MessageBoxImage.Warning
                IconText.Text = "!"
                IconBadge.Background = CType(FindResource("WarningBrush"), Brush)
                IconText.Foreground = New SolidColorBrush(Color.FromRgb(&H18, &H18, &H1E))
            Case Else
                IconText.Text = "i"
                IconBadge.Background = CType(FindResource("AccentBrush"), Brush)
        End Select
    End Sub

    Private Sub ConfigureButtons(buttons As MessageBoxButton)
        Select Case buttons
            Case MessageBoxButton.OK
                AddDialogButton("OK", MessageBoxResult.OK, True, True)
            Case MessageBoxButton.OKCancel
                AddDialogButton("Cancel", MessageBoxResult.Cancel, False, True)
                AddDialogButton("OK", MessageBoxResult.OK, True, False)
            Case MessageBoxButton.YesNo
                AddDialogButton("No", MessageBoxResult.No, False, True)
                AddDialogButton("Yes", MessageBoxResult.Yes, True, False)
            Case MessageBoxButton.YesNoCancel
                AddDialogButton("Cancel", MessageBoxResult.Cancel, False, True)
                AddDialogButton("No", MessageBoxResult.No, False, False)
                AddDialogButton("Yes", MessageBoxResult.Yes, True, False)
        End Select
    End Sub

    Private Sub AddDialogButton(label As String, result As MessageBoxResult, primary As Boolean, cancel As Boolean)
        Dim button As New Button With {
            .Content = label,
            .Tag = result,
            .MinWidth = 92,
            .Margin = New Thickness(8, 0, 0, 0),
            .IsDefault = result = defaultValue OrElse (defaultValue = MessageBoxResult.None AndAlso primary),
            .IsCancel = cancel
        }
        button.Style = CType(FindResource(If(primary, "PrimaryButton", "SecondaryButton")), Style)
        AddHandler button.Click, AddressOf DialogButton_Click
        ButtonPanel.Children.Add(button)
    End Sub

    Private Sub DialogButton_Click(sender As Object, e As RoutedEventArgs)
        Dim button = DirectCast(sender, Button)
        dialogResultValue = DirectCast(button.Tag, MessageBoxResult)
        closingFromButton = True
        Close()
    End Sub

    Private Sub CloseButton_Click(sender As Object, e As RoutedEventArgs)
        dialogResultValue = CloseResult()
        closingFromButton = True
        Close()
    End Sub

    Private Function CloseResult() As MessageBoxResult
        Select Case buttonsValue
            Case MessageBoxButton.OK : Return MessageBoxResult.OK
            Case MessageBoxButton.OKCancel, MessageBoxButton.YesNoCancel : Return MessageBoxResult.Cancel
            Case MessageBoxButton.YesNo : Return MessageBoxResult.No
            Case Else : Return MessageBoxResult.None
        End Select
    End Function

    Private Sub Header_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
        If e.ChangedButton = MouseButton.Left Then DragMove()
    End Sub

    Private Sub DialogWindow_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If e.Key <> Key.Escape Then Return
        dialogResultValue = CloseResult()
        closingFromButton = True
        Close()
        e.Handled = True
    End Sub

    Protected Overrides Sub OnClosing(e As ComponentModel.CancelEventArgs)
        If Not closingFromButton Then dialogResultValue = CloseResult()
        MyBase.OnClosing(e)
    End Sub
End Class

Public NotInheritable Class AppDialog
    Private Sub New()
    End Sub

    Public Shared Function Show(message As String,
                                title As String,
                                Optional buttons As MessageBoxButton = MessageBoxButton.OK,
                                Optional icon As MessageBoxImage = MessageBoxImage.None,
                                Optional defaultResult As MessageBoxResult = MessageBoxResult.None,
                                Optional owner As Window = Nothing) As MessageBoxResult
        Dim dialog As New DialogWindow(title, message, buttons, icon, defaultResult)
        Dim actualOwner = If(owner, Application.Current?.Windows.OfType(Of Window)().FirstOrDefault(Function(window) window.IsActive AndAlso Not TypeOf window Is DialogWindow))
        If actualOwner IsNot Nothing AndAlso actualOwner.IsLoaded Then
            dialog.Owner = actualOwner
        Else
            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen
        End If
        dialog.ShowDialog()
        Return dialog.SelectedResult
    End Function
End Class