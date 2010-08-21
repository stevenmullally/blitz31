Option Explicit On

Public Class Options
    Private saveOptions As Boolean

    ''' <summary>
    ''' Handles FormClosing for the Options form.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Options_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If saveOptions Then
            GameTable.Players(1).Name = txtPlayer1Name.Text
            GameTable.Players(2).Name = txtPlayer2Name.Text
            GameTable.Players(3).Name = txtPlayer3Name.Text
            GameTable.Players(4).Name = txtPlayer4Name.Text
            GameTable.UpdateOnStart = chkStartupUpdate.Checked
            GameTable.SaveSettings()
            GameTable.UpdateScores(GameTable.GameActive)
        End If
    End Sub

    ''' <summary>
    ''' Handles Load for the Options form.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Options_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        txtPlayer1Name.Text = GameTable.Players(1).Name
        txtPlayer2Name.Text = GameTable.Players(2).Name
        txtPlayer3Name.Text = GameTable.Players(3).Name
        txtPlayer4Name.Text = GameTable.Players(4).Name
        chkStartupUpdate.Checked = GameTable.UpdateOnStart
        saveOptions = False
    End Sub

    ''' <summary>
    ''' Flags saveOptions if all data is acceptable.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks>Checks each player name for an invalid name, IE, field empty.</remarks>
    Private Sub btnOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOk.Click
        Dim errText As String = "You must enter in a player name."

        If txtPlayer1Name.Text.Length = 0 Then
            MsgBox(errText, MsgBoxStyle.Exclamation + MsgBoxStyle.OkOnly, "Invalid Player Name")
            txtPlayer1Name.Focus()
            Exit Sub
        End If

        If txtPlayer2Name.Text.Length = 0 Then
            MsgBox(errText, MsgBoxStyle.Exclamation + MsgBoxStyle.OkOnly, "Invalid Player Name")
            txtPlayer2Name.Focus()
            Exit Sub
        End If

        If txtPlayer3Name.Text.Length = 0 Then
            MsgBox(errText, MsgBoxStyle.Exclamation + MsgBoxStyle.OkOnly, "Invalid Player Name")
            txtPlayer3Name.Focus()
            Exit Sub
        End If

        If txtPlayer4Name.Text.Length = 0 Then
            MsgBox(errText, MsgBoxStyle.Exclamation + MsgBoxStyle.OkOnly, "Invalid Player Name")
            txtPlayer4Name.Focus()
            Exit Sub
        End If

        saveOptions = True
        Me.Close()
    End Sub

    ''' <summary>
    ''' Closes form without saving changes.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        Me.Close()
    End Sub

    ''' <summary>
    ''' Resets values to default settings.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub btnDefault_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDefault.Click
        txtPlayer1Name.Text = "Player 1"
        txtPlayer2Name.Text = "Player 2"
        txtPlayer3Name.Text = "Player 3"
        txtPlayer4Name.Text = "Player 4"
    End Sub
End Class