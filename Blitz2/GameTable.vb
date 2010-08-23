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
Imports System.Threading
Imports Blitz2.Objects
Imports Blitz2.Objects.Card

Public Class GameTable
#Region "Game Fields"
    Private deck(51) As Card
    Public Player(5) As Player

    Private seed As Integer

    Private gameActive As Boolean
    Private roundActive As Boolean
    Private knockActive As Boolean
    Private blitzActive As Boolean

    Private currentPlayer As Byte
    Private knocker As Byte
    Private winner As Byte
    Private dealer As Byte

    Private discardTop As Byte
    Private discardBottom As Byte
    Private discardCount As Byte
    Private pickupCard As Byte

    Private cardOffsetX As Byte = 16
    Private cardOffsetY As Byte = 30

    Private Const noCard As Byte = 52

    Private takingTurn As Boolean
    Private computerSync As New Object
    Private computerThread As Thread

    Private DEBUG_MODE As Boolean

    Private Enum CardOwners
        Deck = 0
        Discard = 5
        Used = 6
    End Enum
#End Region

#Region "Form Methods"
    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.SetStyle(ControlStyles.DoubleBuffer Or ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint, True)
        Me.UpdateStyles()

#If DEBUG Then
        DEBUG_MODE = True
#End If
    End Sub

    Private Sub GameTable_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            If Not Card.Initialize Then Exit Sub
        Catch ex As Exception
            MsgBox("Unable to load card library.", MsgBoxStyle.Critical + MsgBoxStyle.OkOnly, "Card Library Error")
            Exit Sub
        End Try

        CreatePlayers()
    End Sub

    Private Sub GameTable_Closing() Handles Me.Closing
        SyncLock computerSync
            If takingTurn Then
                Try
                    computerThread.Abort()
                Catch ex As Exception

                End Try
            End If
        End SyncLock
    End Sub

    Private Sub GameTable_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseDown
        If Not gameActive Or Not roundActive Then Exit Sub
        If Player(currentPlayer).Mode <> Objects.Player.Modes.Human Then Exit Sub

        Dim x As Integer = e.X
        Dim y As Integer = e.Y
        Dim card As Byte
        Dim cardOwner As Byte = CardOwners.Deck
        Dim i As Byte

        For i = 0 To UBound(deck)
            deck(i).Invert = False

            If deck(i).Owner = 1 And Player(1).TotalCards = 4 Then
                With Player(1).HandLocation
                    If (x >= .X) And (x <= (.X + Objects.Card.CardWidth)) And _
                       (y >= .Y) And (y <= (.Y + Objects.Card.CardWidth)) Then

                        card = i
                        cardOwner = 1
                    End If
                End With

                With Player(1)
                    .HandLocation = New Point(.HandLocation.X + cardOffsetX, .HandLocation.Y)
                End With
            ElseIf deck(i).Owner = CardOwners.Discard Then
                With Player(CardOwners.Discard).HandLocation
                    If (x >= .X) And (x <= (.X + Objects.Card.CardWidth)) And _
                       (y >= .Y) And (y <= (.Y + Objects.Card.CardWidth)) And _
                       Player(1).TotalCards < 4 Then

                        card = i
                        cardOwner = CardOwners.Discard
                    End If
                End With
            End If
        Next

        If card <> noCard Then
            deck(card).Invert = True

            If cardOwner = 1 Then

            ElseIf cardOwner = CardOwners.Discard Then

            End If
        Else
            If Player(1).TotalCards < 4 Then
                If knockactive Then

                Else

                End If
            Else

            End If

            ResetInverts()
        End If

        Me.Refresh()
    End Sub

    Private Sub GameTable_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint
        Dim i As Byte

        If Not gameActive Then Exit Sub

        SetCardLocations()

        For i = 0 To UBound(deck)
            Select Case deck(i).Owner
                Case 1
                    With Player(1)
                        If .InGame Then
                            PaintCard(e.Graphics, New Point(.HandLocation.X, .HandLocation.Y), i, deck(i).Invert)
                            .HandLocation = New Point(.HandLocation.X + cardOffsetX, .HandLocation.Y)
                        End If
                    End With
                Case 2
                    With Player(2)
                        If .InGame Then
                            If DEBUG_MODE Or Not roundActive Then
                                PaintCard(e.Graphics, New Point(.HandLocation.X, .HandLocation.Y), i, False)
                            Else
                                PaintCard(e.Graphics, New Point(.HandLocation.X, .HandLocation.Y), noCard, False)
                            End If
                            .HandLocation = New Point(.HandLocation.X, .HandLocation.Y + cardOffsetY)
                        End If
                    End With
                Case 3
                    With Player(3)
                        If .InGame Then
                            If DEBUG_MODE Or Not roundActive Then
                                PaintCard(e.Graphics, New Point(.HandLocation.X, .HandLocation.Y), i, False)
                            Else
                                PaintCard(e.Graphics, New Point(.HandLocation.X, .HandLocation.Y), noCard, False)
                            End If
                            .HandLocation = New Point(.HandLocation.X + cardOffsetX, .HandLocation.Y)
                        End If
                    End With
                Case 4
                    With Player(4)
                        If .InGame Then
                            If DEBUG_MODE Or Not roundActive Then
                                PaintCard(e.Graphics, New Point(.HandLocation.X, .HandLocation.Y), i, False)
                            Else
                                PaintCard(e.Graphics, New Point(.HandLocation.X, .HandLocation.Y), noCard, False)
                            End If
                            .HandLocation = New Point(.HandLocation.X, .HandLocation.Y + cardOffsetY)
                        End If
                    End With
            End Select
        Next

        Dim x As Integer = 0
        Dim y As Integer = 0
        Dim n As Integer = 0

        If Not DeckEmpty() Then
            Select Case CardsLeft()
                Case Is > 35 : n = 7
                Case Is > 30 : n = 6
                Case Is > 25 : n = 5
                Case Is > 20 : n = 4
                Case Is > 15 : n = 3
                Case Is > 10 : n = 2
                Case Is > 5 : n = 1
                Case Else : n = 0
            End Select

            For i = 0 To n
                PaintCard(e.Graphics, New Point(Player(CardOwners.Deck).HandLocation.X - x, Player(CardOwners.Deck).HandLocation.Y - y), noCard, False)
                x += 2 : y += 2
            Next
        End If

        Select Case discardCount
            Case Is > 35 : n = 7
            Case Is > 30 : n = 6
            Case Is > 25 : n = 5
            Case Is > 20 : n = 4
            Case Is > 15 : n = 3
            Case Is > 10 : n = 2
            Case Is > 5 : n = 1
            Case Else : n = 0
        End Select

        If discardTop <> noCard Then
            x = 0 : y = 0

            If discardCount > 1 Then
                For i = 0 To n
                    PaintCard(e.Graphics, New Point(Player(CardOwners.Discard).HandLocation.X - x, Player(CardOwners.Discard).HandLocation.Y - y), discardTop, deck(discardTop).Invert)
                    x += 2 : y += 2
                Next
            End If

            PaintCard(e.Graphics, New Point(Player(CardOwners.Discard).HandLocation.X - x, Player(CardOwners.Discard).HandLocation.Y - y), discardTop, deck(discardTop).Invert)
        End If
    End Sub
#End Region

#Region "Game Methods"
    Private Sub NewGame()
        Dim i As Byte

        Cursor.Current = Cursors.WaitCursor

        SyncLock computerSync
            If takingTurn Then
                Try
                    computerThread.Abort()
                Catch ex As Exception

                End Try
            End If
        End SyncLock

        gameActive = False
        roundActive = False

        For i = 1 To 4
            Me.Player(i).Tokens = 4
            Me.Player(i).InGame = True
        Next

        dealer = 4
        currentPlayer = 1

        gameActive = True
        Cursor.Current = Cursors.Arrow

        NewRound()
    End Sub

    Private Sub NewRound()
        Dim rnd As New Random
        Dim i As Byte

        Cursor.Current = Cursors.WaitCursor

        seed = rnd.Next(0, 65535)

        For i = 0 To 4
            Player(i).CreateNewHand()
            Player(i).Flagged = False
        Next

        roundActive = True

        ResetDeck()
        DealCards()
        Me.Refresh()
        Cursor.Current = Cursors.Arrow
    End Sub
#End Region

#Region "Card Methods"
    Private Sub DealCards()
        Dim i As Byte
        Dim player As Byte = Me.dealer + 1

        For i = 1 To 12
            If player > 4 Then player = 1

            If Me.Player(player).InGame Then
                MoveCard(CardOwners.Deck, player)
                'Me.Refresh()
            End If

            player += 1
        Next

        MoveCard(CardOwners.Deck, CardOwners.Discard)
        discardCount = 1
        discardBottom = noCard
    End Sub

    Private Sub MoveCard(ByVal fromPlayer As Byte, ByVal toPlayer As Byte, Optional ByVal card As Byte = nocard)
        Select Case fromPlayer
            Case CardOwners.Deck
                Dim cardFound As Boolean
                Dim rnd As New Random(seed)

                Do While Not cardFound
                    card = rnd.Next(0, 52)

                    If deck(card).Owner = CardOwners.Deck Then
                        deck(card).Owner = toPlayer

                        Select Case toPlayer
                            Case 1, 2, 3, 4
                                Player(toPlayer).AddCard(card)
                            Case CardOwners.Discard
                                discardTop = card
                        End Select
                        cardFound = True
                    End If
                Loop
            Case CardOwners.Discard
                pickupCard = card
                deck(card).Owner = toPlayer
                Player(toPlayer).AddCard(card)

                discardTop = discardBottom

                If discardTop <> noCard Then
                    deck(discardTop).Owner = CardOwners.Discard
                End If

                discardCount -= 1
                discardBottom = noCard
                ResetInverts()
            Case Else
                If discardTop <> noCard Then
                    discardBottom = discardTop
                    deck(discardBottom).Owner = CardOwners.Used
                End If

                discardTop = card
                deck(card).Owner = CardOwners.Discard
                discardCount += 1

                Player(fromPlayer).RemoveCard(card)
                ResetInverts()
        End Select
    End Sub

    Private Sub ResetDeck()
        Dim i As Byte

        For i = 0 To UBound(deck)
            deck(i) = New Card(i)
        Next

        discardTop = noCard
        discardBottom = noCard
        discardCount = 0
    End Sub

    Private Sub ResetInverts()
        Dim i As Byte

        For i = 0 To UBound(deck)
            deck(i).Invert = False
        Next
    End Sub

    Private Function CardsLeft() As Byte
        Dim i As Byte

        CardsLeft = 52

        For i = 0 To UBound(deck)
            If deck(i).Owner <> CardOwners.Deck Then CardsLeft -= 1
        Next
    End Function

    Private Function DeckEmpty() As Boolean
        Dim i As Byte

        For i = 0 To UBound(deck)
            If deck(i).Owner = CardOwners.Deck Then Return False
        Next

        Return True
    End Function
#End Region

#Region "Player Methods"
    Private Sub CreatePlayers()
        Dim i As Byte

        For i = 0 To UBound(Me.Player)
            Me.Player(i) = New Player(Objects.Player.Modes.Computer)
        Next

        Me.Player(1).Mode = Objects.Player.Modes.Human

        With Me
            .Player(1).Name = "Player 1"
            .Player(2).Name = "Player 2"
            .Player(3).Name = "Player 3"
            .Player(4).Name = "Player 4"

            .Player(CardOwners.Deck).HandLocation = New Point((290 - Card.CardWidth), (300 - (Card.CardHeight / 2)))
            .Player(CardOwners.Discard).HandLocation = New Point(310, (300 - (Card.CardHeight / 2)))
            .Player(1).HandLocationMid = New Point(300, 500)
            .Player(2).HandLocationMid = New Point(100, 300)
            .Player(3).HandLocationMid = New Point(300, 100)
            .Player(4).HandLocationMid = New Point(500, 300)
        End With
    End Sub

    Private Sub SetCardLocations()
        With Player(1)
            .HandLocation = New Point(.HandLocationMid.X - (Card.CardWidth + (cardOffsetX * (.TotalCards - 1))) / 2, _
                                      .HandLocationMid.Y - (Card.CardHeight / 2))
        End With
        With Player(2)
            .HandLocation = New Point(.HandLocationMid.X - (Card.CardWidth / 2), _
                                      .HandLocationMid.Y - (Card.CardHeight + cardOffsetY * (.TotalCards - 1)) / 2)
        End With
        With Player(3)
            .HandLocation = New Point(.HandLocationMid.X - ((Card.CardWidth + cardOffsetX * (.TotalCards - 1)) / 2), _
                                      .HandLocationMid.Y - (Card.CardHeight / 2))
        End With
        With Player(4)
            .HandLocation = New Point(.HandLocationMid.X - (Card.CardWidth / 2), _
                                      .HandLocationMid.Y - (Card.CardHeight + cardOffsetY * (.TotalCards - 1)) / 2)
        End With
    End Sub
#End Region

#Region "Delegates / Callbacks"
    Private Delegate Sub SimpleCallback()

    Private Sub DoRefreshScreen()
        Me.Refresh()
    End Sub

    Private Sub RefreshScreen()
        Dim cb As New simplecallback(AddressOf DoRefreshScreen)

        Try
            Me.Invoke(cb)
        Catch ex As Exception

        End Try
    End Sub
#End Region

#Region "Main Menu Methods"
    Private Sub NewGameToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NewGameToolStripMenuItem.Click
        NewGame()
    End Sub

    Private Sub ExitToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExitToolStripMenuItem.Click
        Me.Close()
    End Sub
#End Region

End Class
