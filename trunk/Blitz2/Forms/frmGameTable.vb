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
Option Explicit On
Imports System.Threading
Imports Blitz.Objects
Imports Blitz.Objects.Card
Imports Blitz.Objects.Player

Public Class frmGameTable
#Region "Game Fields"
    Public Const ConfigFile As String = "config.xml"
    Private deck(51) As Card
    Private player(5) As Player

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
    Private cardsDealt As DateTime = Nothing

    Private Enum CardOwners
        Deck = 0
        Discard = 5
        Used = 6
    End Enum
#End Region

#Region "Form Methods"
    Private Sub GameTable_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ' Initialize card library.
        Try
            If Not Card.Initialize Then Exit Sub
        Catch ex As Exception
            MsgBox("Unable to initialize cards library.", vbOKOnly + vbCritical, "Card Library Error")
            Me.Close()
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

        Dim cardFound As Boolean = False

        ' Check if cards are being dealt.
        SyncLock dealingObj
            If dealingCards Then Exit Sub
        End SyncLock

        ' Check if computer taking turn.
        SyncLock computerObj
            If takingTurn Then Exit Sub
        End SyncLock

        ' Find the inverted card and move it.
        For i As Byte = 0 To UBound(deck)
            If deck(i).Invert Then
                Select Case deck(i).Owner
                    Case 1
                        If pickupCard = i Then
                            MsgBox("You cannot discard the card you just picked up from the discard pile.", vbOKOnly + vbInformation, "Illegal Move")
                        Else
                            cardFound = True
                            MoveCard(1, CardOwners.Discard, i)
                            RefreshScreen()
                            TurnOver()
                        End If
                    Case CardOwners.Deck, CardOwners.Discard
                        cardFound = True
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

        SyncLock computerObj
            If takingTurn Then Exit Sub
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
            If deck(i).Owner = 1 And player(1).TotalCards = 4 Then
                With player(1).HandLocation
                    If (x >= .X) And (x <= (.X + Objects.Card.CardWidth)) And _
                       (y >= .Y) And (y <= (.Y + Objects.Card.CardHeight)) Then

                        card = i
                        cardOwner = 1
                        validSelection = True
                    End If
                End With

                With player(1)
                    .HandLocation = New Point(.HandLocation.X + cardOffsetX, .HandLocation.Y)
                End With
            ElseIf deck(i).Owner = CardOwners.Discard Then
                With player(CardOwners.Discard).HandLocation
                    If (x >= .X) And (x <= (.X + Objects.Card.CardWidth)) And _
                       (y >= .Y) And (y <= (.Y + Objects.Card.CardHeight)) And _
                       player(1).TotalCards < 4 Then

                        card = i
                        cardOwner = CardOwners.Discard
                        validSelection = True
                    End If
                End With
            Else
                If (x >= deckTopLocation.X) And (x <= (deckTopLocation.X + Objects.Card.CardWidth)) And _
                   (y >= deckTopLocation.Y) And (y <= (deckTopLocation.Y + Objects.Card.CardHeight)) And _
                    player(1).TotalCards < 4 And Not DeckEmpty() Then

                    card = FreeCard()
                    validSelection = True
                    Exit For
                End If
            End If
        Next

        ' Flag the card if it was a valid selection.
        If validSelection Then
            If deck(card).Owner = CardOwners.Deck Then
                player(CardOwners.Deck).Flagged = True
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

        ' Check to see if form is visible before trying to refresh the screen
        If Me.Visible Then RefreshScreen()
    End Sub

    Private Sub GameTable_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint
        If Not gameActive Then Exit Sub

        SetCardLocations()
        DrawCards(e)
        DrawDeck(e)
        DrawDiscard(e)
    End Sub
#End Region

#Region "Graphic Methods"
    Private Sub DrawCards(ByVal e As PaintEventArgs)
        Dim i As Byte

        For i = 0 To UBound(deck)
            Select Case deck(i).Owner
                Case 1
                    With player(1)
                        If .InGame Then
                            PaintCard(e.Graphics, New Point(.HandLocation.X, .HandLocation.Y), i, deck(i).Invert)
                            .HandLocation = New Point(.HandLocation.X + cardOffsetX, .HandLocation.Y)
                        End If
                    End With
                Case 2, 3, 4
                    With player(deck(i).Owner)
                        If .InGame Then
                            If Not roundActive Then
                                PaintCard(e.Graphics, New Point(.HandLocation.X, .HandLocation.Y), i, deck(i).Invert)
                            Else
#If DEBUG Then
                                PaintCard(e.Graphics, New Point(.HandLocation.X, .HandLocation.Y), i, deck(i).Invert)
#Else
                                PaintCard(e.Graphics, New Point(.HandLocation.X, .HandLocation.Y), noCard, deck(i).Invert)
#End If
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
                    PaintCard(e.Graphics, New Point(player(CardOwners.Deck).HandLocation.X - x, player(CardOwners.Deck).HandLocation.Y - y), noCard, player(CardOwners.Deck).Flagged)
                Else
                    PaintCard(e.Graphics, New Point(player(CardOwners.Deck).HandLocation.X - x, player(CardOwners.Deck).HandLocation.Y - y), noCard, False)
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
                    PaintCard(e.Graphics, New Point(player(CardOwners.Discard).HandLocation.X - x, player(CardOwners.Discard).HandLocation.Y - y), discardTop, False)
                    x += 1 : y += 1
                Next
            End If

            PaintCard(e.Graphics, New Point(player(CardOwners.Discard).HandLocation.X - x, player(CardOwners.Discard).HandLocation.Y - y), discardTop, deck(discardTop).Invert)
        End If
    End Sub

#End Region

#Region "Game Methods"
    Private Sub NewGame()
        Dim i As Byte

        ' Check if cards were recently dealt to stop NewGame "spam"
        If cardsDealt <> Nothing Then
            If DateTime.Now.Subtract(cardsDealt).Seconds >= 1 Then
                cardsDealt = DateTime.Now
            Else
                Exit Sub
            End If
        Else
            cardsDealt = DateTime.Now
        End If

        ' Abort any running threads.
        SyncLock computerObj
            If takingTurn Then
                Try
                    computerThread.Abort()
                    takingTurn = False
                Catch ex As Exception

                End Try
            End If
        End SyncLock

        SyncLock dealingObj
            If dealingCards Then
                Try
                    dealCardsThread.Abort()
                    dealingCards = False
                Catch ex As Exception

                End Try
            End If
        End SyncLock

        ' Reset game/round status.
        gameActive = False
        roundActive = False

        ' Reset player scores.
        For i = 1 To 4
            player(i).Tokens = 4
            player(i).InGame = True
        Next

        dealer = 4
        currentPlayer = 1
        UpdateHandLocations()
        gameActive = True
        SetupNewRound()
    End Sub

    Private Sub SetupNewRound()
        Dim rnd As New Random
        Dim i As Byte

        knockActive = False
        blitzActive = False
        knocker = Nothing

        ' Reset player hands.
        For i = 0 To 4
            player(i).CreateNewHand()
            player(i).Flagged = False
        Next

        roundActive = True
        
        ' Get the seed for the current round.
        seed = rnd.Next(0, 65535)
        lblGameNumber.Text = "Game #" & seed.ToString

        ResetDeck()

        ' Deal the cards.
        dealCardsThread = New Thread(AddressOf DealCards)
        dealCardsThread.Start()
    End Sub

    Private Sub DoStartNewRound()
        ' Check if any player has Blitz
        For i As Byte = 1 To 4
            If player(i).Score = 31 Then
                blitzActive = True
                roundActive = False
            End If
        Next

        If roundActive Then
            TakeTurn()
        Else
            RoundOver()
        End If
    End Sub

    Private Sub StartNewRound()
        Dim cb As New SimpleCallback(AddressOf DoStartNewRound)

        Try
            Me.Invoke(cb)
        Catch ex As Exception

        End Try
    End Sub

    Private Sub RoundOver()
        roundActive = False
        RefreshScreen()
        DetermineWinner()

        Dim activePlayers As Byte = 0
        For i As Byte = 1 To 4
            With player(i)
                If .Tokens >= 0 Then
                    .InGame = True
                    activePlayers += 1
                Else
                    .InGame = False
                End If
            End With
        Next

        If activePlayers = 1 Or Not player(1).InGame Then
            GameOver()
        End If

        ' Show new round button

        ' Set dealer for next round
        Do
            dealer += 1
            If dealer > 4 Then dealer = 1
        Loop Until player(dealer).InGame

        ' Set currentPlayer to player clock-wise of dealer
        currentPlayer = dealer
        Do
            currentPlayer += 1
            If currentPlayer > 4 Then currentPlayer = 1
        Loop Until player(currentPlayer).InGame
    End Sub

    Private Sub TakeTurn()
        pickupCard = noCard

        If DeckEmpty() Then
            RoundOver()
        Else
            If player(currentPlayer).InGame Then
                If knockActive And currentPlayer = knocker Then
                    RoundOver()
                Else
                    Debug.WriteLine(player(currentPlayer).Name & "'s turn")

                    ' Show text under current player showing their turn.
                    Select Case currentPlayer
                        Case 1
                            ' You're turn!
                        Case 2, 3, 4
                            ' Player X's turn.
                    End Select

                    Select Case player(currentPlayer).Mode
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
    End Sub

    Private Sub DoTurnOver()
        If knockActive And knocker = currentPlayer Then
            ' Show that the player knocked
        End If

        If player(currentPlayer).Score = 31 And roundActive Then
            blitzActive = True
            roundActive = False
        End If

        If DeckEmpty() Or Not roundActive Then
            RoundOver()
        Else
            currentPlayer = NextPlayer()
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

    Private Sub DetermineWinner()
        Dim lowestScore As Byte = 32
        Dim totalLosers As Byte = 0
        Dim tie As Boolean = False

        If knockActive And Not blitzActive Then
            ' Find who had the lowest score
            For i As Byte = 1 To 4
                player(i).Flagged = False
                If player(i).InGame And player(i).Score < lowestScore Then lowestScore = player(i).Score
            Next

            For i As Byte = 1 To 4
                If Not player(i).InGame Then Continue For

                If player(i).Score = lowestScore Then
                    player(i).Flagged = True
                    totalLosers += 1
                End If
            Next

            If totalLosers > 1 Then tie = True
        Else
            ' Find who had Blitz and set everyone to lose a point
            For i As Byte = 1 To 4
                If Not player(i).InGame Then Continue For

                If player(i).Score = 31 Then
                    player(i).Flagged = False
                Else
                    player(i).Flagged = True
                End If
            Next
        End If

        If blitzActive Then
            Debug.WriteLine("Blitz!")
        Else
            Debug.WriteLine("Round over")
        End If

        ' Remove a token for anyone who lost
        For i As Byte = 1 To 4
            If Not player(i).InGame Then Continue For

            If player(i).Flagged And knocker = i And tie Then player(i).Flagged = False

            If player(i).Flagged Then
                player(i).Tokens -= 1
                Debug.WriteLine(player(i).Name & " lost")
            End If

            If player(i).Flagged And i = knocker And Not blitzActive Then player(i).Tokens -= 1
            Debug.WriteLine(player(i).Name & " score: " & player(i).Score.ToString & "  Tokens: " & player(i).Tokens.ToString)
        Next
    End Sub

    Private Sub GameOver()
        Debug.WriteLine("Game over")
    End Sub
#End Region

#Region "Card Methods"
    Private Sub DealCards()
        Dim p As Byte = dealer + 1

        SyncLock dealingObj
            dealingCards = True
        End SyncLock

        For i As Byte = 1 To 12
            If p > 4 Then p = 1

            If player(p).InGame Then
                MoveCard(CardOwners.Deck, p)
                RefreshScreen()
#If Not Debug Then
                Thread.Sleep(100)
#End If
            End If

            p += 1
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
                                player(toPlayer).AddCard(card)
                            Case CardOwners.Discard
                                discardTop = card
                        End Select
                        cardFound = True
                    End If
                Loop
            Case CardOwners.Discard
                pickupCard = card
                deck(card).Owner = toPlayer
                player(toPlayer).AddCard(card)

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

                player(fromPlayer).RemoveCard(card)
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

        player(CardOwners.Deck).Flagged = False
    End Sub

    Private Function CardsLeft() As Byte
        Dim i As Byte

        CardsLeft = 52

        For i = 0 To UBound(deck)
            If deck(i).Owner <> CardOwners.Deck Then CardsLeft -= 1
        Next
    End Function

    Private Function DeckEmpty() As Boolean
        For i As Byte = 0 To UBound(deck)
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

        For i = 0 To UBound(player)
            player(i) = New Player(Modes.Computer)
        Next

        player(1).Mode = Modes.Human

        With My.Settings
            player(1).Name = .Player1Name
            player(2).Name = .Player2Name
            player(3).Name = .Player3Name
            player(4).Name = .Player4Name
        End With

        UpdateHandLocations()
    End Sub

    Private Function NextPlayer() As Byte
        NextPlayer = currentPlayer
        Do
            NextPlayer += 1
            If NextPlayer > 4 Then NextPlayer = 1
        Loop Until player(NextPlayer).InGame
    End Function

    Private Sub SetCardLocations()
        With player(CardOwners.Deck)
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
        With player(1)
            .HandLocation = New Point(.HandLocationMid.X - (Card.CardWidth + (cardOffsetX * (.TotalCards - 1))) / 2, _
                                      .HandLocationMid.Y - (Card.CardHeight / 2))
        End With
        With player(2)
            .HandLocation = New Point(.HandLocationMid.X - (Card.CardWidth + (cardOffsetX * (.TotalCards - 1))) / 2, _
                                      .HandLocationMid.Y - (Card.CardHeight / 2))
        End With
        With player(3)
            .HandLocation = New Point(.HandLocationMid.X - ((Card.CardWidth + cardOffsetX * (.TotalCards - 1)) / 2), _
                                      .HandLocationMid.Y - (Card.CardHeight / 2))
        End With
        With player(4)
            .HandLocation = New Point(.HandLocationMid.X - ((Card.CardWidth + cardOffsetX * (.TotalCards - 1)) / 2), _
                                      .HandLocationMid.Y - (Card.CardHeight / 2))
        End With
    End Sub

    Private Sub UpdateHandLocations()
        player(CardOwners.Deck).HandLocation = New Point(((screenWidth / 2) - 10 - Card.CardWidth), ((screenHeight / 2) - (Card.CardHeight / 2)))
        player(CardOwners.Discard).HandLocation = New Point((screenWidth / 2) + 10, ((screenHeight / 2) - (Card.CardHeight / 2)))
        player(1).HandLocationMid = New Point((screenWidth / 2), (screenHeight / 2) + 200)
        player(2).HandLocationMid = New Point((screenWidth / 2) - 200, (screenHeight / 2))
        player(3).HandLocationMid = New Point((screenWidth / 2), (screenHeight / 2) - 200)
        player(4).HandLocationMid = New Point((screenWidth / 2) + 200, (screenHeight / 2))
    End Sub

    Private Sub ComputerTurn()
        Dim masterSuit As Byte = player(currentPlayer).MasterSuit
        Dim suits(3) As Byte
        Dim highestCard As Byte
        Dim lowestCard As Byte
        Dim cardToTake As Byte = noCard
        Dim cardToRemove As Byte = noCard
        Dim sumA As Byte
        Dim sumB As Byte
        Dim cardA As Byte = noCard
        Dim cardB As Byte = noCard
        Dim oddCard As Byte = noCard
        Dim hasAce As Boolean
        Dim i As Byte
        Dim sleepTime As Byte = 1

        SyncLock computerObj
            takingTurn = True
        End SyncLock

        If Not knockActive Then
            If deck(discardTop).Suit <> masterSuit Then
                Dim goal As Byte

                If CardsLeft() >= 20 Then
                    goal = 25
                Else
                    goal = 29
                End If

                If player(currentPlayer).Score > goal Then
                    Debug.WriteLine(player(currentPlayer).Name & " knocked")
                    knocker = currentPlayer
                    knockActive = True
                End If
            End If
        End If

        If Not knockActive Or (knockActive And knocker <> currentPlayer) Then
            With player(currentPlayer)
                For i = 0 To 2
                    suits(.Hand(i).Suit) += 1
                Next

                sumA = 0 : sumB = 0

                For i = 0 To 2
                    If .Hand(i).Suit = masterSuit Then
                        .Hand(i).Flagged = True
                    Else
                        .Hand(i).Flagged = False
                    End If
                Next

                Thread.Sleep(sleepTime * 1000)

                If deck(discardTop).Suit = masterSuit Then
                    sumA += deck(discardTop).Value

                    Select Case suits(masterSuit)
                        Case 1
                            For i = 0 To 2
                                If .Hand(i).Flagged Then
                                    sumA += .Hand(i).Value
                                Else
                                    If cardA = noCard Then
                                        cardA = i
                                    Else
                                        cardB = i
                                    End If
                                End If
                            Next

                            If cardA > cardB Then
                                highestCard = cardA
                            Else
                                highestCard = cardB
                            End If

                            sumB = .Hand(highestCard).Value

                            If sumA > sumB Then
                                cardToTake = discardTop
                            End If
                        Case 2
                            For i = 0 To 2
                                If .Hand(i).Flagged Then
                                    sumA += .Hand(i).Value
                                Else
                                    sumB += .Hand(i).Value
                                End If
                            Next

                            If sumA > sumB Then
                                cardToTake = discardTop
                            End If
                        Case 3
                            lowestCard = 0

                            For i = 0 To 2
                                If .Hand(i).Value < .Hand(lowestCard).Value Then lowestCard = i
                            Next

                            If .Hand(lowestCard).Value < deck(discardTop).Value Then cardToTake = discardTop
                    End Select
                Else
                    Select Case suits(masterSuit)
                        Case 1
                            highestCard = 0
                            For i = 0 To 2
                                If .Hand(i).Value > .Hand(highestCard).Value Then highestCard = i
                            Next i

                            sumA = .Hand(highestCard).Value : sumB = 0

                            For i = 0 To 2
                                If .Hand(i).Suit = deck(discardTop).Suit Then
                                    sumB = .Hand(i).Value + deck(discardTop).Value
                                Else
                                    If .Hand(i).Value > sumB Then
                                        sumB = .Hand(i).Value
                                    End If
                                End If
                            Next i

                            If sumA < sumB Then
                                cardToTake = discardTop
                            End If
                        Case 2
                            For i = 0 To 2
                                If .Hand(i).Flagged Then
                                    sumA += .Hand(i).Value
                                    If .Hand(i).Value = 11 Then hasAce = True
                                Else
                                    sumB = .Hand(i).Value
                                    oddCard = i
                                End If
                            Next i

                            If .Hand(oddCard).Suit = deck(discardTop).Suit Then
                                sumB += deck(discardTop).Value
                            End If

                            If sumA < sumB Then
                                cardToTake = discardTop
                            Else
                                If deck(discardTop).Value = 11 Then
                                    If Not hasAce Then
                                        cardToTake = discardTop
                                    End If
                                End If
                            End If
                        Case 3
                            For i = 0 To 2
                                sumA += .Hand(i).Value
                            Next i

                            If sumA < deck(discardTop).Value Then
                                cardToTake = discardTop
                            End If
                    End Select
                End If

                If cardToTake = noCard Then
                    player(CardOwners.Deck).Flagged = True
                    RefreshScreen()
                    Thread.Sleep(sleepTime * 500)
                    MoveCard(CardOwners.Deck, currentPlayer)
                    player(CardOwners.Deck).Flagged = False
                Else
                    deck(cardToTake).Invert = True
                    RefreshScreen()
                    Thread.Sleep(sleepTime * 500)
                    MoveCard(CardOwners.Discard, currentPlayer, cardToTake)
                    deck(cardToTake).Invert = False
                End If

                RefreshScreen()
                Thread.Sleep(sleepTime * 500)
                sumA = 0 : sumB = 0

                For i = 0 To 3
                    suits(i) = 0
                    .Hand(i).Flagged = False
                Next

                For i = 0 To 3
                    suits(.Hand(i).Suit) += 1
                Next

                masterSuit = 0
                For i = 0 To 3
                    If suits(i) > suits(masterSuit) Then masterSuit = i
                Next

                For i = 0 To 3
                    If .Hand(i).Suit = masterSuit Then
                        sumA += .Hand(i).Value
                        .Hand(i).Flagged = True
                    End If
                Next

                Select Case suits(masterSuit)
                    Case 1
                        lowestCard = 0
                        For i = 0 To 3
                            If .Hand(i).Value < .Hand(lowestCard).Value Then lowestCard = i
                        Next i

                        cardToRemove = .Hand(lowestCard).Position
                    Case 2
                        For i = 0 To 3
                            If Not .Hand(i).Flagged Then
                                If cardA = noCard Then
                                    cardA = i
                                ElseIf cardB = noCard Then
                                    cardB = i
                                End If
                            End If
                        Next i

                        If .Hand(cardA).Suit = .Hand(cardB).Suit Then
                            sumB = .Hand(cardA).Value + .Hand(cardB).Value

                            If sumA > sumB Then
                                If .Hand(cardA).Value > .Hand(cardB).Value Then
                                    cardToRemove = .Hand(cardB).Position
                                Else
                                    cardToRemove = .Hand(cardA).Position
                                End If
                            Else
                                lowestCard = 0
                                For i = 0 To 3
                                    If .Hand(i).Flagged Then
                                        If Not .Hand(lowestCard).Flagged Then
                                            lowestCard = i
                                        Else
                                            If .Hand(i).Value < .Hand(lowestCard).Value Then lowestCard = i
                                        End If
                                    End If
                                Next i

                                cardToRemove = .Hand(lowestCard).Position
                            End If
                        Else
                            If .Hand(cardA).Value > .Hand(cardB).Value Then
                                cardToRemove = .Hand(cardB).Position
                            Else
                                cardToRemove = .Hand(cardA).Position
                            End If
                        End If
                    Case 3
                        For i = 0 To 3
                            If .Hand(i).Flagged Then
                                sumA += .Hand(i).Value
                            Else
                                sumB = .Hand(i).Value
                                oddCard = i
                            End If
                        Next i

                        If sumA > sumB Then
                            cardToRemove = .Hand(oddCard).Position
                        Else
                            lowestCard = 0
                            For i = 0 To 3
                                If .Hand(i).Flagged Then
                                    If Not .Hand(lowestCard).Flagged Then
                                        lowestCard = i
                                    Else
                                        If .Hand(i).Value < .Hand(lowestCard).Value Then lowestCard = i
                                    End If
                                End If
                            Next i
                            cardToRemove = .Hand(lowestCard).Position
                        End If
                    Case 4
                        lowestCard = 0
                        For i = 0 To 3
                            If .Hand(i).Value < .Hand(lowestCard).Value Then lowestCard = i
                        Next i
                        cardToRemove = .Hand(lowestCard).Position
                End Select

                deck(cardToRemove).Invert = True
                RefreshScreen()
                Thread.Sleep(sleepTime * 1000)
                deck(cardToRemove).Invert = False
                MoveCard(currentPlayer, CardOwners.Discard, cardToRemove)
            End With
        End If

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

    Private Sub AboutBlitzToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AboutBlitzToolStripMenuItem.Click
        frmAbout.Show()
    End Sub
#End Region

    Private Sub btnAction_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAction.Click
        If roundActive Then
            Debug.WriteLine("Player 1 knocked")
            knockActive = True
            knocker = 1
            TurnOver()
        Else
            SetupNewRound()
        End If
    End Sub
End Class
