﻿<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class About
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(About))
        Me.lblAppName = New System.Windows.Forms.Label
        Me.lblAppAuthor = New System.Windows.Forms.Label
        Me.lblAppVersion = New System.Windows.Forms.Label
        Me.btnOK = New System.Windows.Forms.Button
        Me.lblSiteURL = New System.Windows.Forms.LinkLabel
        Me.btnDonate = New System.Windows.Forms.Button
        Me.SuspendLayout()
        '
        'lblAppName
        '
        Me.lblAppName.Font = New System.Drawing.Font("Microsoft Sans Serif", 24.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAppName.Location = New System.Drawing.Point(12, 9)
        Me.lblAppName.Name = "lblAppName"
        Me.lblAppName.Size = New System.Drawing.Size(216, 47)
        Me.lblAppName.TabIndex = 0
        Me.lblAppName.Text = "App Name"
        Me.lblAppName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblAppAuthor
        '
        Me.lblAppAuthor.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, CType((System.Drawing.FontStyle.Bold Or System.Drawing.FontStyle.Italic), System.Drawing.FontStyle), System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAppAuthor.ForeColor = System.Drawing.Color.Green
        Me.lblAppAuthor.Location = New System.Drawing.Point(12, 56)
        Me.lblAppAuthor.Name = "lblAppAuthor"
        Me.lblAppAuthor.Size = New System.Drawing.Size(216, 23)
        Me.lblAppAuthor.TabIndex = 1
        Me.lblAppAuthor.Text = "App Author"
        Me.lblAppAuthor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblAppVersion
        '
        Me.lblAppVersion.Location = New System.Drawing.Point(16, 86)
        Me.lblAppVersion.Name = "lblAppVersion"
        Me.lblAppVersion.Size = New System.Drawing.Size(212, 23)
        Me.lblAppVersion.TabIndex = 2
        Me.lblAppVersion.Text = "App Version"
        Me.lblAppVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'btnOK
        '
        Me.btnOK.Location = New System.Drawing.Point(12, 157)
        Me.btnOK.Name = "btnOK"
        Me.btnOK.Size = New System.Drawing.Size(75, 23)
        Me.btnOK.TabIndex = 3
        Me.btnOK.Text = "OK"
        Me.btnOK.UseVisualStyleBackColor = True
        '
        'lblSiteURL
        '
        Me.lblSiteURL.Location = New System.Drawing.Point(12, 119)
        Me.lblSiteURL.Name = "lblSiteURL"
        Me.lblSiteURL.Size = New System.Drawing.Size(216, 23)
        Me.lblSiteURL.TabIndex = 4
        Me.lblSiteURL.TabStop = True
        Me.lblSiteURL.Text = "SiteURL"
        Me.lblSiteURL.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'btnDonate
        '
        Me.btnDonate.AutoSize = True
        Me.btnDonate.BackColor = System.Drawing.SystemColors.Control
        Me.btnDonate.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None
        Me.btnDonate.Image = CType(resources.GetObject("btnDonate.Image"), System.Drawing.Image)
        Me.btnDonate.Location = New System.Drawing.Point(153, 157)
        Me.btnDonate.Name = "btnDonate"
        Me.btnDonate.Size = New System.Drawing.Size(80, 27)
        Me.btnDonate.TabIndex = 5
        Me.btnDonate.Text = "Donate"
        Me.btnDonate.UseVisualStyleBackColor = False
        '
        'About
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(240, 192)
        Me.Controls.Add(Me.btnDonate)
        Me.Controls.Add(Me.lblSiteURL)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.lblAppVersion)
        Me.Controls.Add(Me.lblAppAuthor)
        Me.Controls.Add(Me.lblAppName)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "About"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "About"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lblAppName As System.Windows.Forms.Label
    Friend WithEvents lblAppAuthor As System.Windows.Forms.Label
    Friend WithEvents lblAppVersion As System.Windows.Forms.Label
    Friend WithEvents btnOK As System.Windows.Forms.Button
    Friend WithEvents lblSiteURL As System.Windows.Forms.LinkLabel
    Friend WithEvents btnDonate As System.Windows.Forms.Button
End Class
