<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class GameTable
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(GameTable))
        Me.MainMenu = New System.Windows.Forms.MenuStrip
        Me.FileToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.NewGameToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.OptionsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.ExitToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.HelpToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.CheckForUpdatesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.AboutToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.btnDrawCard = New System.Windows.Forms.Button
        Me.btnDiscard = New System.Windows.Forms.Button
        Me.lblPlayer1 = New System.Windows.Forms.Label
        Me.lblPlayer2 = New System.Windows.Forms.Label
        Me.lblPlayer3 = New System.Windows.Forms.Label
        Me.lblPlayer4 = New System.Windows.Forms.Label
        Me.lblStatus = New System.Windows.Forms.Label
        Me.ScoreBox = New System.Windows.Forms.GroupBox
        Me.lblScore4 = New System.Windows.Forms.Label
        Me.lblScoreName4 = New System.Windows.Forms.Label
        Me.lblScore3 = New System.Windows.Forms.Label
        Me.lblScoreName3 = New System.Windows.Forms.Label
        Me.lblScore2 = New System.Windows.Forms.Label
        Me.lblScoreName2 = New System.Windows.Forms.Label
        Me.lblScore1 = New System.Windows.Forms.Label
        Me.lblScoreName1 = New System.Windows.Forms.Label
        Me.btnNewRound = New System.Windows.Forms.Button
        Me.MainMenu.SuspendLayout()
        Me.ScoreBox.SuspendLayout()
        Me.SuspendLayout()
        '
        'MainMenu
        '
        Me.MainMenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.FileToolStripMenuItem, Me.HelpToolStripMenuItem})
        Me.MainMenu.Location = New System.Drawing.Point(0, 0)
        Me.MainMenu.Name = "MainMenu"
        Me.MainMenu.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional
        Me.MainMenu.Size = New System.Drawing.Size(584, 24)
        Me.MainMenu.TabIndex = 0
        Me.MainMenu.Text = "MenuStrip1"
        '
        'FileToolStripMenuItem
        '
        Me.FileToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.NewGameToolStripMenuItem, Me.OptionsToolStripMenuItem, Me.ExitToolStripMenuItem})
        Me.FileToolStripMenuItem.Name = "FileToolStripMenuItem"
        Me.FileToolStripMenuItem.Size = New System.Drawing.Size(37, 20)
        Me.FileToolStripMenuItem.Text = "File"
        '
        'NewGameToolStripMenuItem
        '
        Me.NewGameToolStripMenuItem.Name = "NewGameToolStripMenuItem"
        Me.NewGameToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F2
        Me.NewGameToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.NewGameToolStripMenuItem.Text = "New Game"
        '
        'OptionsToolStripMenuItem
        '
        Me.OptionsToolStripMenuItem.Name = "OptionsToolStripMenuItem"
        Me.OptionsToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F4
        Me.OptionsToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.OptionsToolStripMenuItem.Text = "Options"
        '
        'ExitToolStripMenuItem
        '
        Me.ExitToolStripMenuItem.Name = "ExitToolStripMenuItem"
        Me.ExitToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.ExitToolStripMenuItem.Text = "Exit"
        '
        'HelpToolStripMenuItem
        '
        Me.HelpToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.CheckForUpdatesToolStripMenuItem, Me.AboutToolStripMenuItem})
        Me.HelpToolStripMenuItem.Name = "HelpToolStripMenuItem"
        Me.HelpToolStripMenuItem.Size = New System.Drawing.Size(44, 20)
        Me.HelpToolStripMenuItem.Text = "Help"
        '
        'CheckForUpdatesToolStripMenuItem
        '
        Me.CheckForUpdatesToolStripMenuItem.Name = "CheckForUpdatesToolStripMenuItem"
        Me.CheckForUpdatesToolStripMenuItem.Size = New System.Drawing.Size(171, 22)
        Me.CheckForUpdatesToolStripMenuItem.Text = "Check for Updates"
        '
        'AboutToolStripMenuItem
        '
        Me.AboutToolStripMenuItem.Name = "AboutToolStripMenuItem"
        Me.AboutToolStripMenuItem.Size = New System.Drawing.Size(171, 22)
        Me.AboutToolStripMenuItem.Text = "About Blitz"
        '
        'btnDrawCard
        '
        Me.btnDrawCard.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.btnDrawCard.Enabled = False
        Me.btnDrawCard.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnDrawCard.Location = New System.Drawing.Point(199, 383)
        Me.btnDrawCard.Name = "btnDrawCard"
        Me.btnDrawCard.Size = New System.Drawing.Size(92, 33)
        Me.btnDrawCard.TabIndex = 1
        Me.btnDrawCard.Text = "Draw Card"
        Me.btnDrawCard.UseVisualStyleBackColor = False
        Me.btnDrawCard.Visible = False
        '
        'btnDiscard
        '
        Me.btnDiscard.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.btnDiscard.Enabled = False
        Me.btnDiscard.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnDiscard.Location = New System.Drawing.Point(310, 383)
        Me.btnDiscard.Name = "btnDiscard"
        Me.btnDiscard.Size = New System.Drawing.Size(92, 33)
        Me.btnDiscard.TabIndex = 2
        Me.btnDiscard.Text = "Select Card"
        Me.btnDiscard.UseVisualStyleBackColor = False
        Me.btnDiscard.Visible = False
        '
        'lblPlayer1
        '
        Me.lblPlayer1.AutoSize = True
        Me.lblPlayer1.Font = New System.Drawing.Font("Traditional Arabic", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblPlayer1.ForeColor = System.Drawing.Color.White
        Me.lblPlayer1.Location = New System.Drawing.Point(533, 24)
        Me.lblPlayer1.Name = "lblPlayer1"
        Me.lblPlayer1.Size = New System.Drawing.Size(52, 24)
        Me.lblPlayer1.TabIndex = 3
        Me.lblPlayer1.Text = "Label1"
        '
        'lblPlayer2
        '
        Me.lblPlayer2.AutoSize = True
        Me.lblPlayer2.Font = New System.Drawing.Font("Traditional Arabic", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblPlayer2.ForeColor = System.Drawing.Color.White
        Me.lblPlayer2.Location = New System.Drawing.Point(533, 42)
        Me.lblPlayer2.Name = "lblPlayer2"
        Me.lblPlayer2.Size = New System.Drawing.Size(52, 24)
        Me.lblPlayer2.TabIndex = 4
        Me.lblPlayer2.Text = "Label2"
        '
        'lblPlayer3
        '
        Me.lblPlayer3.AutoSize = True
        Me.lblPlayer3.Font = New System.Drawing.Font("Traditional Arabic", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblPlayer3.ForeColor = System.Drawing.Color.White
        Me.lblPlayer3.Location = New System.Drawing.Point(532, 62)
        Me.lblPlayer3.Name = "lblPlayer3"
        Me.lblPlayer3.Size = New System.Drawing.Size(52, 24)
        Me.lblPlayer3.TabIndex = 5
        Me.lblPlayer3.Text = "Label3"
        '
        'lblPlayer4
        '
        Me.lblPlayer4.AutoSize = True
        Me.lblPlayer4.Font = New System.Drawing.Font("Traditional Arabic", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblPlayer4.ForeColor = System.Drawing.Color.White
        Me.lblPlayer4.Location = New System.Drawing.Point(532, 80)
        Me.lblPlayer4.Name = "lblPlayer4"
        Me.lblPlayer4.Size = New System.Drawing.Size(52, 24)
        Me.lblPlayer4.TabIndex = 6
        Me.lblPlayer4.Text = "Label4"
        '
        'lblStatus
        '
        Me.lblStatus.Font = New System.Drawing.Font("Traditional Arabic", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblStatus.ForeColor = System.Drawing.Color.White
        Me.lblStatus.Location = New System.Drawing.Point(196, 355)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(203, 23)
        Me.lblStatus.TabIndex = 7
        Me.lblStatus.Text = "Label5"
        Me.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'ScoreBox
        '
        Me.ScoreBox.BackColor = System.Drawing.Color.Green
        Me.ScoreBox.Controls.Add(Me.lblScore4)
        Me.ScoreBox.Controls.Add(Me.lblScoreName4)
        Me.ScoreBox.Controls.Add(Me.lblScore3)
        Me.ScoreBox.Controls.Add(Me.lblScoreName3)
        Me.ScoreBox.Controls.Add(Me.lblScore2)
        Me.ScoreBox.Controls.Add(Me.lblScoreName2)
        Me.ScoreBox.Controls.Add(Me.lblScore1)
        Me.ScoreBox.Controls.Add(Me.lblScoreName1)
        Me.ScoreBox.ForeColor = System.Drawing.Color.White
        Me.ScoreBox.Location = New System.Drawing.Point(12, 37)
        Me.ScoreBox.Name = "ScoreBox"
        Me.ScoreBox.Size = New System.Drawing.Size(118, 151)
        Me.ScoreBox.TabIndex = 8
        Me.ScoreBox.TabStop = False
        Me.ScoreBox.Text = "Scores"
        '
        'lblScore4
        '
        Me.lblScore4.AutoSize = True
        Me.lblScore4.Location = New System.Drawing.Point(16, 127)
        Me.lblScore4.Name = "lblScore4"
        Me.lblScore4.Size = New System.Drawing.Size(39, 13)
        Me.lblScore4.TabIndex = 17
        Me.lblScore4.Text = "Label8"
        '
        'lblScoreName4
        '
        Me.lblScoreName4.AutoSize = True
        Me.lblScoreName4.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblScoreName4.Location = New System.Drawing.Point(6, 113)
        Me.lblScoreName4.Name = "lblScoreName4"
        Me.lblScoreName4.Size = New System.Drawing.Size(39, 13)
        Me.lblScoreName4.TabIndex = 16
        Me.lblScoreName4.Text = "Label7"
        '
        'lblScore3
        '
        Me.lblScore3.AutoSize = True
        Me.lblScore3.Location = New System.Drawing.Point(16, 94)
        Me.lblScore3.Name = "lblScore3"
        Me.lblScore3.Size = New System.Drawing.Size(39, 13)
        Me.lblScore3.TabIndex = 15
        Me.lblScore3.Text = "Label6"
        '
        'lblScoreName3
        '
        Me.lblScoreName3.AutoSize = True
        Me.lblScoreName3.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblScoreName3.Location = New System.Drawing.Point(6, 80)
        Me.lblScoreName3.Name = "lblScoreName3"
        Me.lblScoreName3.Size = New System.Drawing.Size(39, 13)
        Me.lblScoreName3.TabIndex = 14
        Me.lblScoreName3.Text = "Label5"
        '
        'lblScore2
        '
        Me.lblScore2.AutoSize = True
        Me.lblScore2.Location = New System.Drawing.Point(16, 62)
        Me.lblScore2.Name = "lblScore2"
        Me.lblScore2.Size = New System.Drawing.Size(39, 13)
        Me.lblScore2.TabIndex = 13
        Me.lblScore2.Text = "Label4"
        '
        'lblScoreName2
        '
        Me.lblScoreName2.AutoSize = True
        Me.lblScoreName2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblScoreName2.Location = New System.Drawing.Point(6, 48)
        Me.lblScoreName2.Name = "lblScoreName2"
        Me.lblScoreName2.Size = New System.Drawing.Size(39, 13)
        Me.lblScoreName2.TabIndex = 12
        Me.lblScoreName2.Text = "Label3"
        '
        'lblScore1
        '
        Me.lblScore1.AutoSize = True
        Me.lblScore1.Location = New System.Drawing.Point(16, 30)
        Me.lblScore1.Name = "lblScore1"
        Me.lblScore1.Size = New System.Drawing.Size(39, 13)
        Me.lblScore1.TabIndex = 11
        Me.lblScore1.Text = "Label2"
        '
        'lblScoreName1
        '
        Me.lblScoreName1.AutoSize = True
        Me.lblScoreName1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblScoreName1.Location = New System.Drawing.Point(6, 16)
        Me.lblScoreName1.Name = "lblScoreName1"
        Me.lblScoreName1.Size = New System.Drawing.Size(39, 13)
        Me.lblScoreName1.TabIndex = 10
        Me.lblScoreName1.Text = "Label1"
        '
        'btnNewRound
        '
        Me.btnNewRound.Location = New System.Drawing.Point(233, 422)
        Me.btnNewRound.Name = "btnNewRound"
        Me.btnNewRound.Size = New System.Drawing.Size(138, 23)
        Me.btnNewRound.TabIndex = 9
        Me.btnNewRound.Text = "Deal Next Round"
        Me.btnNewRound.UseVisualStyleBackColor = True
        Me.btnNewRound.Visible = False
        '
        'GameTable
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.Green
        Me.ClientSize = New System.Drawing.Size(584, 608)
        Me.Controls.Add(Me.btnNewRound)
        Me.Controls.Add(Me.ScoreBox)
        Me.Controls.Add(Me.lblStatus)
        Me.Controls.Add(Me.lblPlayer4)
        Me.Controls.Add(Me.lblPlayer3)
        Me.Controls.Add(Me.lblPlayer2)
        Me.Controls.Add(Me.lblPlayer1)
        Me.Controls.Add(Me.btnDiscard)
        Me.Controls.Add(Me.btnDrawCard)
        Me.Controls.Add(Me.MainMenu)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MainMenuStrip = Me.MainMenu
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(600, 644)
        Me.MinimumSize = New System.Drawing.Size(600, 644)
        Me.Name = "GameTable"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Blitz"
        Me.MainMenu.ResumeLayout(False)
        Me.MainMenu.PerformLayout()
        Me.ScoreBox.ResumeLayout(False)
        Me.ScoreBox.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents MainMenu As System.Windows.Forms.MenuStrip
    Friend WithEvents FileToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents NewGameToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ExitToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents HelpToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents CheckForUpdatesToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents AboutToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents btnDrawCard As System.Windows.Forms.Button
    Friend WithEvents btnDiscard As System.Windows.Forms.Button
    Friend WithEvents lblPlayer1 As System.Windows.Forms.Label
    Friend WithEvents lblPlayer2 As System.Windows.Forms.Label
    Friend WithEvents lblPlayer3 As System.Windows.Forms.Label
    Friend WithEvents lblPlayer4 As System.Windows.Forms.Label
    Friend WithEvents lblStatus As System.Windows.Forms.Label
    Friend WithEvents ScoreBox As System.Windows.Forms.GroupBox
    Friend WithEvents lblScore2 As System.Windows.Forms.Label
    Friend WithEvents lblScoreName2 As System.Windows.Forms.Label
    Friend WithEvents lblScore1 As System.Windows.Forms.Label
    Friend WithEvents lblScoreName1 As System.Windows.Forms.Label
    Friend WithEvents lblScore4 As System.Windows.Forms.Label
    Friend WithEvents lblScoreName4 As System.Windows.Forms.Label
    Friend WithEvents lblScore3 As System.Windows.Forms.Label
    Friend WithEvents lblScoreName3 As System.Windows.Forms.Label
    Friend WithEvents btnNewRound As System.Windows.Forms.Button
    Friend WithEvents OptionsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem

End Class
