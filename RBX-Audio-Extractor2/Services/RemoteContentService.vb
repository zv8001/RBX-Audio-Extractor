Imports System.Net.Http

Public Module RemoteContentService
    Public Const BaseUrl As String = "https://rbxextr.vexthatprotogen.com/"
    Private ReadOnly Client As New HttpClient With {.Timeout = TimeSpan.FromSeconds(20)}

    Dim test As String = ""

    Public Async Function GetCreatorMessageAsync() As Task(Of String)
        Return (Await Client.GetStringAsync(BaseUrl & "messagev2.txt")).Trim()
    End Function
End Module