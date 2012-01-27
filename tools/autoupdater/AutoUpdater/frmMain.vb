Option Explicit On
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text

Public Class frmMain
    Private Sub frmMain_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        OutputBox("Press Update button to begin")
    End Sub

    Private Sub OutputBox(ByVal textinput As String)
        txtOutput.Text += textinput & vbCrLf
        txtOutput.Refresh()
    End Sub

    Private Sub btnUpdate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnUpdate.Click
        Dim updateListR As String = "http://www.ryanskeldon.com/software/blitz/updatelist.txt"
        Dim updateListL As String = Application.StartupPath & "\updatelist.txt"

        btnUpdate.Enabled = False
        txtOutput.Text = ""

        Dim PingOut As New Net.NetworkInformation.Ping
        Dim PingIn As Net.NetworkInformation.PingReply

        Try
            PingIn = PingOut.Send("www.psykad.com", 3000)
        Catch ex As Exception
            OutputBox("Unable to connect to update server. Check your internet connection.")
            Exit Sub
        End Try

        Try
            If PingIn.Status = Net.NetworkInformation.IPStatus.Success Then
                Dim wc As New Net.WebClient
                Dim localFile As String
                Dim remoteFile As String
                Dim fileHash As String

                OutputBox("Download update list...")
                If File.Exists(updateListL) Then
                    Kill(updateListL)
                End If

                wc.DownloadFile(updateListR, updateListL)

                Dim fileStream As New StreamReader(updateListL)
                Dim buffer As String
                Dim buffer2() As String

                With fileStream
                    Do Until .EndOfStream
                        buffer = .ReadLine

                        If Mid(buffer, 1, 4) = "file" Then
                            buffer = Mid(buffer, 6, Len(buffer))
                            buffer2 = Split(buffer, ",")

                            remoteFile = buffer2(0)
                            localFile = Application.StartupPath & "\" & buffer2(1)
                            fileHash = buffer2(2)

                            If File.Exists(localFile) Then
                                If getFileSHA(localFile) <> fileHash Then
                                    Kill(localFile)
                                    OutputBox("Downloading " & remoteFile)
                                    wc.DownloadFile(remoteFile, localFile)
                                End If
                            Else
                                OutputBox("Downloading " & remoteFile)
                                wc.DownloadFile(remoteFile, localFile)
                            End If
                        End If
                    Loop

                    .Close()
                End With

                OutputBox("Update complete.")

                Dim msgResponse As MsgBoxResult

                msgResponse = MsgBox("Would you like to run the updated program?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, Application.ProductName)

                If msgResponse = MsgBoxResult.Yes Then
                    System.Diagnostics.Process.Start(Application.StartupPath & "\Blitz.exe")
                    Application.Exit()
                End If
            Else
                OutputBox("Unable to perform update")
            End If
        Catch ex As Exception
            OutputBox(ex.Message)
        End Try

        btnUpdate.Enabled = True
    End Sub

    Private Function getFileSHA(ByVal filePath As String) As String
        Dim shaProvider As SHA1CryptoServiceProvider = New SHA1CryptoServiceProvider
        Dim fStream As FileStream = New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192)

        shaProvider.ComputeHash(fStream)
        fStream.Close()

        Dim hash As Byte() = shaProvider.Hash
        Dim buff As StringBuilder = New StringBuilder
        Dim hashByte As Byte

        For Each hashByte In hash
            buff.Append(String.Format("{0:X2}", hashByte))
        Next

        Return buff.ToString
    End Function
End Class
