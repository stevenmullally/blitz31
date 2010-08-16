<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Main
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
        Me.btnOpenFile = New System.Windows.Forms.Button
        Me.txtFile = New System.Windows.Forms.TextBox
        Me.ofdInput = New System.Windows.Forms.OpenFileDialog
        Me.btnHash = New System.Windows.Forms.Button
        Me.txtMD5 = New System.Windows.Forms.TextBox
        Me.Label1 = New System.Windows.Forms.Label
        Me.Label2 = New System.Windows.Forms.Label
        Me.txtSHA = New System.Windows.Forms.TextBox
        Me.SuspendLayout()
        '
        'btnOpenFile
        '
        Me.btnOpenFile.Location = New System.Drawing.Point(12, 12)
        Me.btnOpenFile.Name = "btnOpenFile"
        Me.btnOpenFile.Size = New System.Drawing.Size(75, 23)
        Me.btnOpenFile.TabIndex = 0
        Me.btnOpenFile.Text = "Open"
        Me.btnOpenFile.UseVisualStyleBackColor = True
        '
        'txtFile
        '
        Me.txtFile.Location = New System.Drawing.Point(93, 12)
        Me.txtFile.Name = "txtFile"
        Me.txtFile.Size = New System.Drawing.Size(502, 20)
        Me.txtFile.TabIndex = 1
        '
        'ofdInput
        '
        Me.ofdInput.InitialDirectory = "C:\"
        Me.ofdInput.RestoreDirectory = True
        '
        'btnHash
        '
        Me.btnHash.Location = New System.Drawing.Point(12, 89)
        Me.btnHash.Name = "btnHash"
        Me.btnHash.Size = New System.Drawing.Size(75, 23)
        Me.btnHash.TabIndex = 2
        Me.btnHash.Text = "Hash"
        Me.btnHash.UseVisualStyleBackColor = True
        '
        'txtMD5
        '
        Me.txtMD5.Location = New System.Drawing.Point(93, 91)
        Me.txtMD5.Name = "txtMD5"
        Me.txtMD5.Size = New System.Drawing.Size(502, 20)
        Me.txtMD5.TabIndex = 3
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(90, 75)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(30, 13)
        Me.Label1.TabIndex = 4
        Me.Label1.Text = "MD5"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(90, 114)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(35, 13)
        Me.Label2.TabIndex = 5
        Me.Label2.Text = "SHA1"
        '
        'txtSHA
        '
        Me.txtSHA.Location = New System.Drawing.Point(93, 130)
        Me.txtSHA.Name = "txtSHA"
        Me.txtSHA.Size = New System.Drawing.Size(502, 20)
        Me.txtSHA.TabIndex = 6
        '
        'Main
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(607, 162)
        Me.Controls.Add(Me.txtSHA)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtMD5)
        Me.Controls.Add(Me.btnHash)
        Me.Controls.Add(Me.txtFile)
        Me.Controls.Add(Me.btnOpenFile)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.Name = "Main"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "MD5 / SHA Hasher"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents btnOpenFile As System.Windows.Forms.Button
    Friend WithEvents txtFile As System.Windows.Forms.TextBox
    Friend WithEvents ofdInput As System.Windows.Forms.OpenFileDialog
    Friend WithEvents btnHash As System.Windows.Forms.Button
    Friend WithEvents txtMD5 As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents txtSHA As System.Windows.Forms.TextBox

End Class
