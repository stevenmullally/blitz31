'    Blitz, a simple draw and discard game, suitable for players of all ages.
'    Copyright (C) 2009-2010  Ryan Skeldon <psykad@gmail.com>
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
Option Explicit On

Public Class About

#Region "Form Handlers"
    Private appSiteURL As String = "http://blitz31.googlecode.com/"
    Private appDonateURL As String = "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=9XXRRN9KPWEFQ"

    Private Sub About_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim versionInfo As FileVersionInfo = FileVersionInfo.GetVersionInfo(Application.ExecutablePath)

        lblAppName.Text = Application.ProductName
        lblAppAuthor.Text = "by Ryan Skeldon"
        lblAppVersion.Text = "Version " & versionInfo.ProductVersion
        lblSiteURL.Text = appSiteURL
    End Sub

    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOK.Click
        Me.Close()
    End Sub
#End Region

    Private Sub lblSiteURL_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles lblSiteURL.LinkClicked
        Process.Start(appSiteURL)
    End Sub

    Private Sub btnDonate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDonate.Click
        Process.Start(appDonateURL)
    End Sub
End Class