'    Blitz, a simple draw and discard game, suitable for players of all ages.
'    Copyright (C) 2009-2012  Ryan Skeldon <psykad@gmail.com>
'
'    This program is free software; you can redistribute it and/or modify
'    it under the terms of the GNU General Public License as published by
'    the Free Software Foundation; either version 2 of the License, or
'    (at your option) any later version.
'
'    This program is distributed in the hope that it will be useful,
'    but WITHOUT ANY WARRANTY; without even the implied warranty of
'    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    GNU General Public License for more details.
'
'    You should have received a copy of the GNU General Public License
'    along with this program; if not, write to the Free Software
'    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
Public Class frmAbout
    Private siteURL As String = "http://blitz31.googlecode.com/"
    Private donateURL As String = "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=9XXRRN9KPWEFQ"

    Private Sub frmAbout_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim versionInfo As FileVersionInfo = FileVersionInfo.GetVersionInfo(Application.ExecutablePath)

        txtLicense.Text = Blitz.My.Resources.COPYING
        txtLicense.SelectionStart = 0
        lblAppVersion.Text = "Version " & versionInfo.ProductVersion
    End Sub

    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOK.Click
        Me.Close()
    End Sub

    Private Sub btnDonate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDonate.Click
        Process.Start(donateURL)
    End Sub

    Private Sub lblWebSite_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblWebSite.Click
        Process.Start(siteURL)
    End Sub

    Private Sub lblWebSite_MouseEnter(ByVal sender As Object, ByVal e As System.EventArgs) Handles lblWebSite.MouseEnter
        Me.Cursor = Cursors.Hand
    End Sub

    Private Sub lblWebSite_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles lblWebSite.MouseLeave
        Me.Cursor = Cursors.Arrow
    End Sub
End Class
