﻿'    Blitz, a simple draw and discard game, suitable for players of all ages.
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
Imports Blitz2.Objects.Player

Public Class GameTable
#Region "Game Fields"
    Private deck(51) As Card
    Public Player(5) As Player

    Private seed As Integer = -1

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
    Private deckTopLocation As Point

    Private cardOffsetX As Byte = 16
    Private cardOffsetY As Byte = 30

    Private screenWidth As Integer
    Private screenHeight As Integer

    Private Const noCard As Byte = 52

    Private takingTurn As Boolean
    Private computerObj As New Object
    Private computerThread As Thread

    Private dealingCards As Boolean
    Private dealingObj As New Object
    Private dealCardsThread As Thread

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
        ' Initialize card library.
        Try
            If Not Card.Initialize Then Exit Sub
        Catch ex As Exception
            MsgBox("Unable to initialize cards library.", MsgBoxStyle.Critical + MsgBoxStyle.OkOnly, "Card Library Error")
            Me.Close()
            Exit Sub
        End Try

        CreatePlayers()
    End Sub

    Private Sub GameTable_Closing() Handles Me.Closing
        ' Abort any running threads.
        SyncLock computerObj
            If takingTurn Then
                Try
                    computerThread.Abort()
                Catch ex As Exception

                End Try
            End If
        End SyncLock

        SyncLock dealingObj
            If dealingCards Then
                Try
                    dealCardsThread.Abort()
                Catch ex As Exception

                End Try
            End If
        End SyncLock
    End Sub

    Private Sub GameTable_MouseDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseDoubleClick
        If Not gameActive Or Not roundActive Then Exit Sub

        ' Check if cards are being dealt.
        SyncLock dealingObj
            If dealingCards Then Exit Sub
        End SyncLock

        Dim i As Byte

        ' Find the inverted card and move it.
        For i = 0 To UBound(deck)
            If deck(i).Invert Then
                Select Case deck(i).Owner
                    Case 1
                        If pickupCard = i Then
                            MsgBox("You cannot discard the card you just picked up from the discard pile.", MsgBoxStyle.Exclamation + MsgBoxStyle.OkOnly, "Illegal Move")
                        Else
                            MoveCard(1, CardOwners.Discard, i)
                            RefreshScreen()
                            TurnOver()
                        End If
                    Case CardOwners.Deck, CardOwners.Discard
                        MoveCard(deck(i).Owner, 1, i)
                        RefreshScreen()
                End Select
            End If
        Next
    End Sub

    Private Sub GameTable_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseDown
        If Not gameActive Or Not roundActive Then Exit Sub

        ' Check if cards are being dealt.
        SyncLock dealingObj
            If dealingCards Then Exit Sub
        End SyncLock

        Dim x As Integer = e.X
        Dim y As Integer = e.Y
        Dim card As Byte = noCard
        Dim cardOwner As Byte = CardOwners.Deck
        Dim validSelection As Boolean
        Dim i As Byte

        SetCardLocations()
        ResetInverts()

        ' Check if a valid card was selected.
        For i = 0 To UBound(deck)
            If deck(i).Owner = 1 And Player(1).TotalCards = 4 Then
                With Player(1).HandLocation
                    If (x >= .X) And (x <= (.X + Objects.Card.CardWidth)) And _
                       (y >= .Y) And (y <= (.Y + Objects.Card.CardHeight)) Then

                        card = i
                        cardOwner = 1
                        validSelection = True
                    End If
                End With

                With Player(1)
                    .HandLocation = New Point(.HandLocation.X + cardOffsetX, .HandLocation.Y)
                End With
            ElseIf deck(i).Owner = CardOwners.Discard Then
                With Player(CardOwners.Discard).HandLocation
                    If (x >= .X) And (x <= (.X + Objects.Card.CardWidth)) And _
                       (y >= .Y) And (y <= (.Y + Objects.Card.CardHeight)) And _
                       Player(1).TotalCards < 4 Then

                        card = i
                        cardOwner = CardOwners.Discard
                        validSelection = True
                    End If
                End With
            Else
                If (x >= deckTopLocation.X) And (x <= (deckTopLocation.X + Objects.Card.CardWidth)) And _
                   (y >= deckTopLocation.Y) And (y <= (deckTopLocation.Y + Objects.Card.CardHeight)) And _
                    Player(1).TotalCards < 4 And Not DeckEmpty() Then

                    card = FreeCard()
                    validSelection = True
                    Exit For
                End If
            End If
        Next

        ' Flag the card if it was a valid selection.
        If validSelection Then
            If deck(card).Owner = CardOwners.Deck Then
                Player(CardOwners.Deck).Flagged = True
            End If
            deck(card).Invert = True
        End If

        RefreshScreen()
    End Sub

    Private Sub GameTable_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Resize
        ' Update height/width fields to new form size.
        screenHeight = Me.Size.Height
        screenWidth = Me.Size.Width

        ' Update the card locations with the new parameters.
        If gameActive Then
            UpdateHandLocations()
        End If

        RefreshScreen()
    End Sub

    Private Sub GameTable_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint
        If Not gameActive Then Exit Sub

        SetCardLocations()

        DrawCards(e)
        DrawDeck(e)
        DrawDiscard(e)
    End Sub

    Private Sub DrawCards(ByVal e As PaintEventArgs)
        Dim i As Byte

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
                            .HandLocation = New Point(.HandLocation.X + cardOffsetX, .HandLocation.Y)
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
                            .HandLocation = New Point(.HandLocation.X + cardOffsetX, .HandLocation.Y)
                        End If
                    End With
            End Select
        Next
    End Sub

    Private Sub DrawDeck(ByVal e As PaintEventArgs)
        Dim x As Integer = 0
        Dim y As Integer = 0
        Dim n As Integer = 0

        ' Draw the deck pile.
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
                If i = n Then
                    PaintCard(e.Graphics, New Point(Player(CardOwners.Deck).HandLocation.X - x, Player(CardOwners.Deck).HandLocation.Y - y), noCard, Player(CardOwners.Deck).Flagged)
                Else
                    PaintCard(e.Graphics, New Point(Player(CardOwners.Deck).HandLocation.X - x, Player(CardOwners.Deck).HandLocation.Y - y), noCard, False)
                End If

                x += 1 : y += 1
            Next
        End If
    End Sub

    Private Sub DrawDiscard(ByVal e As PaintEventArgs)
        Dim x As Integer = 0
        Dim y As Integer = 0
        Dim n As Integer = 0

        ' Draw the discard pile.
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
                    PaintCard(e.Graphics, New Point(Player(CardOwners.Discard).HandLocation.X - x, Player(CardOwners.Discard).HandLocation.Y - y), discardTop, False)
                    x += 1 : y += 1
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

        ' Abort any running threads.
        SyncLock computerObj
            If takingTurn Then
                Try
                    computerThread.Abort()
                Catch ex As Exception

                End Try
            End If
        End SyncLock

        SyncLock dealingObj
            If dealingCards Then
                Try
                    dealCardsThread.Abort()
                Catch ex As Exception

                End Try
            End If
        End SyncLock

        ' Reset game/round status.
        gameActive = False
        roundActive = False

        ' Reset player scores.
        For i = 1 To 4
            Me.Player(i).Tokens = 4
            Me.Player(i).InGame = True
        Next

        dealer = 4
        currentPlayer = 1

        UpdateHandLocations()

        gameActive = True

        Cursor.Current = Cursors.Arrow

        SetupNewRound()
    End Sub

    Private Sub SetupNewRound()
        Dim rnd As New Random
        Dim i As Byte

        Cursor.Current = Cursors.WaitCursor

        ' Get the seed for the current round.
        seed = rnd.Next(0, 65535)

        ' Reset player hands.
        For i = 0 To 4
            Player(i).CreateNewHand()
            Player(i).Flagged = False
        Next

        roundActive = True

        ResetDeck()

        ' Deal the cards.
        dealCardsThread = New Thread(AddressOf DealCards)
        dealCardsThread.Start()
    End Sub

    Private Sub DoStartNewRound()
        Dim i As Byte

        Cursor.Current = Cursors.WaitCursor

        For i = 1 To 4
            If HasBlitz(i) And roundActive Then
                blitzActive = True
                roundActive = False
            End If
        Next

        If roundActive Then
            Cursor.Current = Cursors.Arrow

            RefreshScreen()

            TakeTurn()
        Else
            RoundOver()
        End If

        Cursor.Current = Cursors.Arrow
    End Sub

    Private Sub StartNewRound()
        Dim cb As New SimpleCallback(AddressOf DoStartNewRound)

        Try
            Me.Invoke(cb)
        Catch ex As Exception

        End Try
    End Sub

    Private Sub RoundOver()

    End Sub

    Private Sub TakeTurn()
        Cursor.Current = Cursors.WaitCursor

        pickupCard = noCard

        RefreshScreen()

        If DeckEmpty() Then
            RoundOver()
        Else
            If Me.Player(currentPlayer).InGame Then
                If knockActive And currentPlayer = knocker Then
                    RoundOver()
                Else
                    ' Show text under current player showing their turn.
                    Select Case currentPlayer
                        Case 1
                            ' You're turn!
                        Case 2, 3, 4
                            ' Player X's turn.
                    End Select

                    Select Case Me.Player(currentPlayer).Mode
                        Case Modes.Human
                            If knockActive Then
                                ' Disable ability to knock.
                            Else
                                ' Enable ability to knock.
                            End If
                        Case Modes.Computer
                            ' Disable UI.

                            computerThread = New Thread(AddressOf ComputerTurn)
                            computerThread.Start()
                    End Select
                End If
            Else
                TurnOver()
            End If
        End If

        Cursor.Current = Cursors.Arrow
    End Sub

    Private Sub DoTurnOver()
        If knockActive And knocker = currentPlayer Then
            ' Show that the player knocked
        End If

        If HasBlitz(currentPlayer) And roundActive Then
            blitzActive = True
            roundActive = False
        End If

        If DeckEmpty() Or Not roundActive Then
            RoundOver()
        Else
            currentPlayer += 1
            If currentPlayer > 4 Then currentPlayer = 1
            TakeTurn()
        End If
    End Sub

    Private Sub TurnOver()
        Dim cb As New SimpleCallback(AddressOf DoTurnOver)

        Try
            Me.Invoke(cb)
        Catch ex As Exception

        End Try
    End Sub
#End Region

#Region "Card Methods"
    Private Sub DealCards()
        Dim i As Byte
        Dim player As Byte = Me.dealer + 1

        SyncLock dealingObj
            dealingCards = True
        End SyncLock

        For i = 1 To 12
            If player > 4 Then player = 1

            If Me.Player(player).InGame Then
                MoveCard(CardOwners.Deck, player)
                RefreshScreen()
                If Not DEBUG_MODE Then Thread.Sleep(100)
            End If

            player += 1
        Next

        MoveCard(CardOwners.Deck, CardOwners.Discard)
        RefreshScreen()
        discardCount = 1
        discardBottom = noCard

        SyncLock dealingObj
            dealingCards = False
        End SyncLock

        StartNewRound()
    End Sub

    Private Sub MoveCard(ByVal fromPlayer As Byte, ByVal toPlayer As Byte, Optional ByVal card As Byte = noCard)
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
            Case Else
                If discardTop <> noCard Then
                    discardBottom = discardTop
                    deck(discardBottom).Owner = CardOwners.Used
                End If

                discardTop = card
                deck(card).Owner = CardOwners.Discard
                discardCount += 1

                Player(fromPlayer).RemoveCard(card)
        End Select

        ResetInverts()
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

        Player(CardOwners.Deck).Flagged = False
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

    Private Function FreeCard() As Byte
        Dim rnd As New Random
        Dim foundCard As Boolean
        Dim card As Byte = noCard

        FreeCard = noCard

        Do Until foundCard
            card = rnd.Next(0, 52)

            If deck(card).Owner = CardOwners.Deck Then
                FreeCard = card
                foundCard = True
            End If
        Loop

        Return FreeCard
    End Function
#End Region

#Region "Player Methods"
    Private Sub CreatePlayers()
        Dim i As Byte

        For i = 0 To UBound(Me.Player)
            Me.Player(i) = New Player(Modes.Computer)
        Next

        Me.Player(1).Mode = Modes.Human

        With Me
            .Player(1).Name = "Player 1"
            .Player(2).Name = "Player 2"
            .Player(3).Name = "Player 3"
            .Player(4).Name = "Player 4"

            UpdateHandLocations()
        End With
    End Sub

    Private Sub SetCardLocations()
        With Player(CardOwners.Deck)
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
                    x += 2 : y += 2
                Next
            End If

            deckTopLocation = New Point(.HandLocation.X - x, .HandLocation.Y - y)
        End With
        With Player(1)
            .HandLocation = New Point(.HandLocationMid.X - (Card.CardWidth + (cardOffsetX * (.TotalCards - 1))) / 2, _
                                      .HandLocationMid.Y - (Card.CardHeight / 2))
        End With
        With Player(2)
            .HandLocation = New Point(.HandLocationMid.X - (Card.CardWidth + (cardOffsetX * (.TotalCards - 1))) / 2, _
                                      .HandLocationMid.Y - (Card.CardHeight / 2))
        End With
        With Player(3)
            .HandLocation = New Point(.HandLocationMid.X - ((Card.CardWidth + cardOffsetX * (.TotalCards - 1)) / 2), _
                                      .HandLocationMid.Y - (Card.CardHeight / 2))
        End With
        With Player(4)
            .HandLocation = New Point(.HandLocationMid.X - ((Card.CardWidth + cardOffsetX * (.TotalCards - 1)) / 2), _
                                      .HandLocationMid.Y - (Card.CardHeight / 2))
        End With
    End Sub

    Private Sub UpdateHandLocations()
        Player(CardOwners.Deck).HandLocation = New Point(((screenWidth / 2) - 10 - Card.CardWidth), ((screenHeight / 2) - (Card.CardHeight / 2)))
        Player(CardOwners.Discard).HandLocation = New Point((screenWidth / 2) + 10, ((screenHeight / 2) - (Card.CardHeight / 2)))
        Player(1).HandLocationMid = New Point((screenWidth / 2), (screenHeight / 2) + 200)
        Player(2).HandLocationMid = New Point((screenWidth / 2) - 200, (screenHeight / 2))
        Player(3).HandLocationMid = New Point((screenWidth / 2), (screenHeight / 2) - 200)
        Player(4).HandLocationMid = New Point((screenWidth / 2) + 200, (screenHeight / 2))
    End Sub

    Private Function HasBlitz(ByVal player As Byte) As Boolean
        If PlayerScore(player) = 31 Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Function PlayerScore(ByVal player As Byte) As Byte
        Dim masterSuit As Byte = GetMasterSuit(player)
        Dim score As Byte
        Dim i As Byte

        With Me.Player(player)
            For i = 0 To 2
                If .Hand(i).Suit = masterSuit Then
                    score += .Hand(i).Value
                End If
            Next
        End With

        Return score
    End Function

    Private Function GetMasterSuit(ByVal player As Byte) As Byte
        Dim masterSuit As Byte
        Dim highestCard As Byte
        Dim suits(3) As Byte
        Dim sumA As Byte = 0
        Dim sumB As Byte = 0
        Dim i As Byte

        With Me.Player(player)
            For i = 0 To 2
                suits(.Hand(i).Suit) += 1
                .Hand(i).Flagged = False
            Next

            For i = 0 To 3
                If suits(i) > suits(masterSuit) Then masterSuit = i
            Next

            Select Case suits(masterSuit)
                Case 1
                    For i = 0 To 2
                        If .Hand(i).Value > .Hand(highestCard).Value Then highestCard = i
                    Next
                    masterSuit = .Hand(highestCard).Suit
                    .Hand(highestCard).Flagged = True
                Case 2
                    For i = 0 To 2
                        If .Hand(i).Suit = masterSuit Then
                            .Hand(i).Flagged = True
                            sumA += .Hand(i).Value
                        Else
                            sumB += .Hand(i).Value
                        End If
                    Next

                    For i = 0 To 2
                        If sumA > sumB Then
                            If .Hand(i).Flagged Then masterSuit = .Hand(i).Suit
                        Else
                            If Not .Hand(i).Flagged Then masterSuit = .Hand(i).Suit
                        End If
                    Next
                Case 3
                    For i = 0 To 2
                        .Hand(i).Flagged = True
                    Next
                    masterSuit = .Hand(0).Suit
            End Select

            Return masterSuit
        End With
    End Function

    Private Sub ComputerTurn()
        SyncLock computerObj
            takingTurn = True
        End SyncLock

        RefreshScreen()

        SyncLock computerObj
            takingTurn = False
        End SyncLock

        TurnOver()
    End Sub
#End Region

#Region "Delegates / Callbacks"
    Private Delegate Sub SimpleCallback()

    Private Sub DoRefreshScreen()
        Me.Refresh()
    End Sub

    Private Sub RefreshScreen()
        Dim cb As New SimpleCallback(AddressOf DoRefreshScreen)

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
