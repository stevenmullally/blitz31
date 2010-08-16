Option Explicit On

Imports System.Security.Cryptography
Imports System.Text
Imports System.IO
Imports System.Threading

Public Class Main
    Private fileMD5 As String
    Private fileSHA1 As String

    Private shaThread As Thread
    Private md5Thread As Thread

    Private Delegate Sub CalcHashDone(ByVal hash As String)

    Private Sub DoCalcSHADone(ByVal hash As String)
        txtSHA.Text = hash
    End Sub

    Private Sub CalcSHADone(ByVal hash As String)
        Dim cb As New CalcHashDone(AddressOf DoCalcSHADone)
        Dim args() As Object = {hash}

        Me.Invoke(cb, args)
    End Sub

    Private Sub DoCalcMD5Done(ByVal hash As String)
        txtMD5.Text = hash
    End Sub

    Private Sub CalcMD5Done(ByVal hash As String)
        Dim cb As New CalcHashDone(AddressOf DoCalcMD5Done)
        Dim args() As Object = {hash}

        Me.Invoke(cb, args)
    End Sub

    Private Sub btnOpenFile_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOpenFile.Click
        ofdInput.ShowDialog()
    End Sub

    Private Sub getFileMD5()
        Dim MD5 As MD5CryptoServiceProvider = New MD5CryptoServiceProvider
        Dim filePath As String = txtFile.Text

        Dim fStream As FileStream = New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192)
        MD5.ComputeHash(fStream)
        fStream.Close()

        Dim hash As Byte() = MD5.Hash
        Dim buff As StringBuilder = New StringBuilder
        Dim hashByte As Byte

        For Each hashByte In hash
            buff.Append(String.Format("{0:X2}", hashByte))
        Next

        CalcMD5Done(buff.ToString)
    End Sub

    Private Sub getFileSHA()
        Dim SHA As SHA1CryptoServiceProvider = New SHA1CryptoServiceProvider
        Dim filePath As String = txtFile.Text

        Dim fStream As FileStream = New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192)
        SHA.ComputeHash(fStream)
        fStream.Close()

        Dim hash As Byte() = SHA.Hash
        Dim buff As StringBuilder = New StringBuilder
        Dim hashByte As Byte

        For Each hashByte In hash
            buff.Append(String.Format("{0:X2}", hashByte))
        Next

        CalcSHADone(buff.ToString)
    End Sub

    Private Sub ofdInput_FileOk(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles ofdInput.FileOk
        If ofdInput.FileName <> Nothing Then
            ofdInput.RestoreDirectory = True
            txtFile.Text = ofdInput.FileName
        End If
    End Sub

    Private Sub btnHash_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnHash.Click
        txtMD5.Text = "Calculating MD5 hash..."
        md5Thread = New Thread(AddressOf getFileMD5)
        md5Thread.Start()

        txtSHA.Text = "Calculating SHA1 hash..."
        shaThread = New Thread(AddressOf getFileSHA)
        shaThread.Start()
    End Sub

    Private Sub MainForm_Closing() Handles Me.Closing
        Try
            md5Thread.Abort()
            shaThread.Abort()
        Catch ex As Exception

        End Try
    End Sub
End Class
