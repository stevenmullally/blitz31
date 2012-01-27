<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmAbout
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
        Me.lblAppName = New System.Windows.Forms.Label()
        Me.lblAppVersion = New System.Windows.Forms.Label()
        Me.txtLicense = New System.Windows.Forms.TextBox()
        Me.btnOK = New System.Windows.Forms.Button()
        Me.btnDonate = New System.Windows.Forms.Button()
        Me.lblWebSite = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'lblAppName
        '
        Me.lblAppName.AutoSize = True
        Me.lblAppName.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAppName.Location = New System.Drawing.Point(12, 9)
        Me.lblAppName.Name = "lblAppName"
        Me.lblAppName.Size = New System.Drawing.Size(44, 20)
        Me.lblAppName.TabIndex = 0
        Me.lblAppName.Text = "Blitz"
        '
        'lblAppVersion
        '
        Me.lblAppVersion.AutoSize = True
        Me.lblAppVersion.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAppVersion.Location = New System.Drawing.Point(12, 29)
        Me.lblAppVersion.Name = "lblAppVersion"
        Me.lblAppVersion.Size = New System.Drawing.Size(116, 20)
        Me.lblAppVersion.TabIndex = 1
        Me.lblAppVersion.Text = "Version X.XX"
        '
        'txtLicense
        '
        Me.txtLicense.Font = New System.Drawing.Font("Courier New", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtLicense.Location = New System.Drawing.Point(12, 83)
        Me.txtLicense.Multiline = True
        Me.txtLicense.Name = "txtLicense"
        Me.txtLicense.ReadOnly = True
        Me.txtLicense.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtLicense.Size = New System.Drawing.Size(562, 185)
        Me.txtLicense.TabIndex = 2
        '
        'btnOK
        '
        Me.btnOK.Location = New System.Drawing.Point(499, 274)
        Me.btnOK.Name = "btnOK"
        Me.btnOK.Size = New System.Drawing.Size(75, 23)
        Me.btnOK.TabIndex = 3
        Me.btnOK.Text = "OK"
        Me.btnOK.UseVisualStyleBackColor = True
        '
        'btnDonate
        '
        Me.btnDonate.Location = New System.Drawing.Point(418, 274)
        Me.btnDonate.Name = "btnDonate"
        Me.btnDonate.Size = New System.Drawing.Size(75, 23)
        Me.btnDonate.TabIndex = 4
        Me.btnDonate.Text = "Donate"
        Me.btnDonate.UseVisualStyleBackColor = True
        '
        'lblWebSite
        '
        Me.lblWebSite.AutoSize = True
        Me.lblWebSite.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, CType((System.Drawing.FontStyle.Bold Or System.Drawing.FontStyle.Underline), System.Drawing.FontStyle), System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblWebSite.ForeColor = System.Drawing.Color.SteelBlue
        Me.lblWebSite.Location = New System.Drawing.Point(12, 49)
        Me.lblWebSite.Name = "lblWebSite"
        Me.lblWebSite.Size = New System.Drawing.Size(82, 20)
        Me.lblWebSite.TabIndex = 5
        Me.lblWebSite.Text = "Web Site"
        '
        'frmAbout
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(586, 310)
        Me.Controls.Add(Me.lblWebSite)
        Me.Controls.Add(Me.btnDonate)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.txtLicense)
        Me.Controls.Add(Me.lblAppVersion)
        Me.Controls.Add(Me.lblAppName)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmAbout"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "About Blitz"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lblAppName As System.Windows.Forms.Label
    Friend WithEvents lblAppVersion As System.Windows.Forms.Label
    Friend WithEvents txtLicense As System.Windows.Forms.TextBox
    Friend WithEvents btnOK As System.Windows.Forms.Button
    Friend WithEvents btnDonate As System.Windows.Forms.Button
    Friend WithEvents lblWebSite As System.Windows.Forms.Label

End Class
