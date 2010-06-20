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
Imports Blitz.Objects
Imports Blitz.Objects.Player
Imports Blitz.Objects.Card
Imports System.Threading

Public Class GameTable

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.SetStyle(ControlStyles.DoubleBuffer Or ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint, True)
        Me.UpdateStyles()

        ' Clear status labels
        SetStatus("", 0, True)
        UpdateScores(False)
    End Sub

#Region "Variable Declarations"
    Private Deck(51) As Card
    Private _player(5) As Player
    Private Seed As Integer = 0
    Private CurrentPlayer As Byte = 0
    Private Knocker As Byte = 0
    Private Winner As Byte = 0
    Private Dealer As Byte = 0
    Private DiscardTop As Byte = NoCard
    Private DiscardBottom As Byte = NoCard
    Private PickupCard As Byte = NoCard
    Private DiscardCount As Byte = 0
    Private CardOffset_X As Byte = 16
    Private CardOffset_Y As Byte = 30
    Private GameActive As Boolean = False
    Private RoundActive As Boolean = False
    Private BlitzActive As Boolean = False
    Private KnockActive As Boolean = False
    Private DebugMode As Boolean = False
    Private Const NoCard As Byte = 52
    Private Const NoCard_X As Byte = 67
    Private Const NoCard_O As Byte = 68
    Private Const DeckPattern As Byte = 52
    Private Const CardFront As Byte = 0
    Private Const CardBack As Byte = 1
    Private Const CardInverted As Byte = 2
    Private ComputerThread As Thread
    Private SyncObj As New Object
    Private TakingTurn As Boolean
    Private Delegate Sub SimpleCallback()

    Private Enum CardOwners
        Deck = 0
        Player1 = 1
        Player2 = 2
        Player3 = 3
        Player4 = 4
        Discard = 5
        Used = 6
    End Enum
#End Region

#Region "Main Game Methods"
    Private Sub NewGame()
        ' Initialize card library
        Try
            If Not Card.Initialize() Then Exit Sub
        Catch ex As Exception
            MsgBox("Unable to load card library.", MsgBoxStyle.Critical + MsgBoxStyle.OkOnly, "Card Library Error")
            Exit Sub
        End Try

        ' Get a lock on SyncObj to check if a computer thread is running
        SyncLock SyncObj
            If TakingTurn Then
                Try
                    ComputerThread.Abort()
                Catch ex As Exception

                End Try
            End If
        End SyncLock

        ' Change cursor to wait while game is setup
        Cursor.Current = Cursors.WaitCursor

        ' Stop any running game/round if already playing
        GameActive = False
        RoundActive = False

        ' Create the players and set the dealer
        CreatePlayers()
        Dealer = 4
        CurrentPlayer = 1

        ' Game is ready
        GameActive = True

        ' Set cursor back to normal
        Cursor.Current = Cursors.Arrow

        ' Begin a new round
        NewRound()
    End Sub

    Public Sub NewRound()
        Dim Rnd As New Random

        ' Change cursor to wait while round is setup
        Cursor.Current = Cursors.WaitCursor

        ' Reset Players and Deck
        ResetPlayers()
        ResetDeck()

        ' Choose a seed for the game
        Seed = Rnd.Next(0, 65535)

        ' Reset form controls
        ResetUI()
        UpdateScores(True)

        ' Set the round to active
        RoundActive = True

        ' Deal cards to the players
        DealCards()

        ' Check if anyone has blitz
        For i As Byte = 1 To 4
            If HasBlitz(i) And RoundActive Then
                BlitzActive = True
                RoundActive = False
            End If
        Next i

        If RoundActive Then
            ' Remove wait cursor, round is ready to begin
            Cursor.Current = Cursors.Arrow

            ' Refresh screen to draw the cards
            Me.Refresh()

            ' Start the round
            TakeTurn()
        Else
            RoundOver()
        End If

    End Sub

    Private Sub TakeTurn()
        ' Present wait cursor while next turn is setup
        Cursor.Current = Cursors.WaitCursor

        ' Reset pickup card to nothing
        PickupCard = NoCard

        Me.Refresh()

        ' End game if no more cards
        If DeckEmpty() Then
            RoundOver()
        Else
            ' Continue if Player is still in the game otherwise skip their turn
            If _player(CurrentPlayer).InGame Then
                ' End the round if the current Player is the one who knocked
                If KnockActive And CurrentPlayer = Knocker Then
                    RoundOver()
                Else
                    ' Set status showing current Player's turn
                    Select Case CurrentPlayer
                        Case 1
                            SetStatus("Your turn!", 1, True)
                        Case 2, 3, 4
                            SetStatus(_player(CurrentPlayer).Name & "'s turn", CurrentPlayer, True)
                    End Select

                    ' Call ComputerTurn if Player is a computer
                    Select Case _player(CurrentPlayer).Mode
                        Case Modes.Computer
                            ' Disable user buttons
                            PlayerControls(False, True, False, True)

                            ComputerThread = New Thread(AddressOf ComputerTurn)
                            ComputerThread.Start()
                        Case Modes.Human
                            ' Setup UI for human Player
                            If KnockActive Then
                                PlayerControls(True, True, False, True, "Can't Knock")
                            Else
                                PlayerControls(True, True, True, True, "Knock")
                            End If
                        Case Modes.Online
                            ' Not implemented yet
                    End Select
                End If
            Else
                TurnOver()
            End If
        End If

        ' Return to normal cursor
        Cursor.Current = Cursors.Arrow
    End Sub

    Private Sub ComputerTurn()
        Dim masterSuit As Byte
        Dim suits(3) As Byte
        Dim highestCard As Byte
        Dim lowestCard As Byte
        Dim i As Byte
        Dim sumA As Byte
        Dim sumB As Byte
        Dim cardToTake As Byte = NoCard
        Dim cardToRemove As Byte = NoCard
        Dim oddCard As Byte = NoCard
        Dim cardA As Byte = NoCard
        Dim cardB As Byte = NoCard
        Dim hasAce As Boolean = False

        SyncLock SyncObj
            TakingTurn = True
        End SyncLock

        ' Determine if computer should knock
        If Not KnockActive Then
            Dim goal As Byte

            If CardsLeft() >= 20 Then
                goal = 26
            ElseIf CardsLeft() <= 19 Then
                goal = 28
            End If

            If GetScore(CurrentPlayer) > goal Then
                Knocker = CurrentPlayer
                KnockActive = True
            End If
        End If

        ' Take turn if no one has knocked, or someone has and it wasn't the current player
        If Not KnockActive Or (KnockActive And Knocker <> CurrentPlayer) Then
            With _player(CurrentPlayer)
                masterSuit = GetMasterSuit(CurrentPlayer)

                ' Get count of each suit in hand
                For i = 0 To 2
                    suits(.GetCardSuit(i)) += 1
                Next i

                sumA = 0 : sumB = 0
                For i = 0 To 2
                    If .GetCardSuit(i) = masterSuit Then
                        .SetFlag(i, True)
                    Else
                        .SetFlag(i, False)
                    End If
                Next i

                Thread.Sleep(1000)

                ' Look for a card to draw
                If Deck(DiscardTop).SuitVal = masterSuit Then
                    sumA += Deck(DiscardTop).Value

                    Select Case suits(masterSuit)
                        Case 1
                            For i = 0 To 2
                                If .GetFlag(i) = True Then
                                    sumA += .GetCardVal(i)
                                Else
                                    If cardA = NoCard Then
                                        cardA = i
                                    ElseIf cardB = NoCard Then
                                        cardB = i
                                    End If
                                End If
                            Next i

                            If cardA > cardB Then
                                highestCard = cardA
                            Else
                                highestCard = cardB
                            End If

                            sumB = .GetCardVal(highestCard)

                            If sumA > sumB Then
                                cardToTake = DiscardTop
                            End If
                        Case 2
                            For i = 0 To 2
                                If .GetFlag(i) = True Then
                                    sumA += .GetCardVal(i)
                                Else
                                    sumB += .GetCardVal(i)
                                    cardToTake = .GetCardPos(i)
                                End If
                            Next i

                            If sumA > sumB Then
                                cardToTake = DiscardTop
                            End If
                        Case 3
                            lowestCard = 0
                            For i = 0 To 2
                                If .GetCardVal(i) < .GetCardVal(lowestCard) Then lowestCard = i
                            Next i

                            If .GetCardVal(lowestCard) < Deck(DiscardTop).Value Then
                                cardToTake = DiscardTop
                            End If
                    End Select
                Else
                    Select Case suits(masterSuit)
                        Case 1
                            highestCard = 0
                            For i = 0 To 2
                                If .GetCardVal(i) > .GetCardVal(highestCard) Then highestCard = i
                            Next i

                            sumA = .GetCardVal(highestCard) : sumB = 0

                            For i = 0 To 2
                                If .GetCardSuit(i) = Deck(DiscardTop).SuitVal Then
                                    sumB = .GetCardVal(i) + Deck(DiscardTop).Value
                                Else
                                    If .GetCardVal(i) > sumB Then
                                        sumB = .GetCardVal(i)
                                    End If
                                End If
                            Next i

                            If sumA < sumB Then
                                cardToTake = DiscardTop
                            End If
                        Case 2
                            For i = 0 To 2
                                If .GetFlag(i) = True Then
                                    sumA += .GetCardVal(i)
                                    If .GetCardVal(i) = 11 Then hasAce = True
                                Else
                                    sumB = .GetCardVal(i)
                                    oddCard = i
                                End If
                            Next i

                            If .GetCardSuit(oddCard) = Deck(DiscardTop).SuitVal Then
                                sumB += Deck(DiscardTop).Value
                            End If

                            If sumA < sumB Then
                                cardToTake = DiscardTop
                            Else
                                If Deck(DiscardTop).Value = 11 Then
                                    If Not hasAce Then
                                        cardToTake = DiscardTop
                                    End If
                                End If
                            End If
                        Case 3
                            For i = 0 To 2
                                sumA += .GetCardVal(i)
                            Next i

                            If sumA < Deck(DiscardTop).Value Then
                                cardToTake = DiscardTop
                            End If
                    End Select
                End If

                If cardToTake = NoCard Then
                    MoveCard(CardOwners.Deck, CurrentPlayer)
                Else
                    MoveCard(CardOwners.Discard, CurrentPlayer, cardToTake)
                End If

                ' Refresh screen to show the new card in hand
                RefreshScreen()

                ' Slow down game while computer decides which card to discard
                Thread.Sleep(1000)

                ' Discard a card
                sumA = 0 : sumB = 0

                For i = 0 To 3
                    suits(i) = 0
                    .SetFlag(i, False)
                Next i

                For i = 0 To 3
                    suits(.GetCardSuit(i)) += 1
                Next i

                masterSuit = 0
                For i = 0 To 3
                    If suits(i) > suits(masterSuit) Then masterSuit = i
                Next i

                For i = 0 To 3
                    If .GetCardSuit(i) = masterSuit Then
                        sumA += .GetCardVal(i)
                        .SetFlag(i, True)
                    End If
                Next i

                Select Case suits(masterSuit)
                    Case 1
                        lowestCard = 0
                        For i = 0 To 3
                            If .GetCardVal(i) < .GetCardVal(lowestCard) Then lowestCard = i
                        Next i

                        cardToRemove = .GetCardPos(lowestCard)
                    Case 2
                        For i = 0 To 3
                            If .GetFlag(i) = False Then
                                If cardA = NoCard Then
                                    cardA = i
                                ElseIf cardB = NoCard Then
                                    cardB = i
                                End If
                            End If
                        Next i

                        If .GetCardSuit(cardA) = .GetCardSuit(cardB) Then
                            sumB = .GetCardVal(cardA) + .GetCardVal(cardB)

                            If sumA > sumB Then
                                If .GetCardVal(cardA) > .GetCardVal(cardB) Then
                                    cardToRemove = .GetCardPos(cardB)
                                Else
                                    cardToRemove = .GetCardPos(cardA)
                                End If
                            Else
                                lowestCard = 0

                                For i = 0 To 3
                                    If .GetFlag(i) = True Then
                                        If .GetFlag(lowestCard) = False Then
                                            lowestCard = i
                                        Else
                                            If .GetCardVal(i) < .GetCardVal(lowestCard) Then lowestCard = i
                                        End If
                                    End If
                                Next i

                                cardToRemove = .GetCardPos(lowestCard)
                            End If
                        Else
                            If .GetCardVal(cardA) > .GetCardVal(cardB) Then
                                cardToRemove = .GetCardPos(cardB)
                            Else
                                cardToRemove = .GetCardPos(cardA)
                            End If
                        End If
                    Case 3
                        For i = 0 To 3
                            If .GetFlag(i) = True Then
                                sumA += .GetCardVal(i)
                            Else
                                sumB = .GetCardVal(i)
                                oddCard = i
                            End If
                        Next i

                        If sumA > sumB Then
                            cardToRemove = .GetCardPos(oddCard)
                        Else
                            lowestCard = 0
                            For i = 0 To 3
                                If .GetFlag(i) = True Then
                                    If .GetFlag(lowestCard) = False Then
                                        lowestCard = i
                                    Else
                                        If .GetCardVal(i) < .GetCardVal(lowestCard) Then lowestCard = i
                                    End If
                                End If
                            Next i
                            cardToRemove = .GetCardPos(lowestCard)
                        End If
                    Case 4
                        lowestCard = 0
                        For i = 0 To 3
                            If .GetCardVal(i) < .GetCardVal(lowestCard) Then lowestCard = i
                        Next i
                        cardToRemove = .GetCardPos(lowestCard)
                End Select

                MoveCard(CurrentPlayer, CardOwners.Discard, cardToRemove)
            End With
        End If

        ' Refresh screen to show new hand
        RefreshScreen()

        ' End the computer's turn
        SyncLock SyncObj
            TakingTurn = False
        End SyncLock

        TurnOver()
    End Sub

    Private Sub TurnOver()
        Dim cb As New SimpleCallback(AddressOf DoTurnOver)

        Try
            Me.Invoke(cb)
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub DoTurnOver()
        If KnockActive And Knocker = CurrentPlayer Then
            SetStatus(_player(CurrentPlayer).Name & " has knocked!")
        End If

        ' Check if player has a blitz
        If HasBlitz(CurrentPlayer) And RoundActive Then
            BlitzActive = True
            RoundActive = False
        End If

        ' Check if deck is empty before continuing
        If DeckEmpty() Or Not RoundActive Then
            RoundOver()
        Else
            ' Move to next Player
            CurrentPlayer += 1
            If CurrentPlayer > 4 Then CurrentPlayer = 1
            TakeTurn()
        End If
    End Sub

    Private Sub RoundOver()
        ' Set round to inactive
        RoundActive = False

        ' Determine who won
        DetermineWinner()

        ' Hide player controls
        PlayerControls(False, False, False, False)

        ' Check to see if game is over
        Dim activePlayers As Byte = 0

        For i As Byte = 1 To 4
            If _player(i).InGame Then activePlayers += 1
        Next

        If activePlayers = 1 Or Not _player(1).InGame Then
            GameOver()
        Else
            btnNewRound.Visible = True
        End If

        UpdateScores(True)

        ' Set dealer for next round
        Do
            Dealer += 1
            If Dealer > 4 Then Dealer = 1
        Loop Until _player(Dealer).InGame

        ' Set current player to player clock-wise of dealer
        CurrentPlayer = Dealer
        Do
            CurrentPlayer += 1
            If CurrentPlayer > 4 Then CurrentPlayer = 1
        Loop Until _player(CurrentPlayer).InGame

        Me.Refresh()
    End Sub

    Private Sub DetermineWinner()
        Dim i As Byte
        Dim LowestScore As Byte
        Dim tie As Boolean
        Dim totalLosers As Byte

        ' If no one has knocked or no one has Blitz
        If KnockActive And Not BlitzActive Then
            LowestScore = 32

            ' Find the lowest score of the round
            For i = 1 To 4
                _player(i).Flag = False
                If _player(i).InGame And GetScore(i) < LowestScore Then LowestScore = GetScore(i)
            Next

            ' Find the players who score matched the lowest score
            For i = 1 To 4
                If _player(i).InGame And GetScore(i) = LowestScore Then
                    _player(i).Flag = True
                    totalLosers += 1
                End If
            Next

            If totalLosers > 1 Then tie = True
        Else
            ' Flag players who didn't have Blitz
            For i = 1 To 4
                If _player(i).InGame Then
                    If GetScore(i) = 31 Then
                        _player(i).Flag = False
                    Else
                        _player(i).Flag = True
                    End If
                End If
            Next
        End If

        If BlitzActive Then
            SetStatus("Blitz!", 0, True)
        Else
            SetStatus("Round Over", 0, True)
        End If

        For i = 1 To 4
            If _player(i).Flag And i = Knocker And tie Then _player(i).Flag = False

            ' Remove 1 token for any flagged player
            If _player(i).Flag And _player(i).InGame Then
                _player(i).RemoveToken(1)
                SetStatus(_player(i).Name & " lost!", i)
            End If

            ' Remove an additional token if the person who knocked had the lowest score
            If _player(i).Flag And Knocker = i Then _player(i).RemoveToken(1)
        Next
    End Sub

    Private Sub GameOver()
        ' Hide player controls
        SetStatus("Game Over", 0, True)
        PlayerControls(False, False, False, False)
    End Sub
#End Region

#Region "Intialization Methods"
    Private Sub CreatePlayers()
        ' Create new players
        For i As Byte = 0 To UBound(_player)
            _player(i) = New Player(Modes.Computer)
        Next i

        ' Set Player 1 to Human
        _player(1).Mode = Modes.Human

        ' Set Player names
        _player(1).Name = "Player 1"
        _player(2).Name = "Player 2"
        _player(3).Name = "Player 3"
        _player(4).Name = "Player 4"

        ' Set objDeck and discard locations
        _player(CardOwners.Deck).X = 290 - Card.CardWidth
        _player(CardOwners.Deck).Y = 300 - (Card.CardHeight / 2)
        _player(CardOwners.Discard).X = 310
        _player(CardOwners.Discard).Y = 300 - (Card.CardHeight / 2)

        ' Set objPlayer locations
        _player(1).MidX = 300
        _player(1).MidY = 500
        _player(2).MidX = 100
        _player(2).MidY = 300
        _player(3).MidX = 300
        _player(3).MidY = 100
        _player(4).MidX = 500
        _player(4).MidY = 300
    End Sub

    Private Sub DealCards()
        Dim myPlayer As Byte = Dealer + 1

        SetStatus(_player(Dealer).Name & " is dealing cards", 0, True)

        ' Deal cards to each player
        For i As Byte = 1 To 12
            If myPlayer > 4 Then myPlayer = 1

            If _player(myPlayer).InGame Then
                MoveCard(CardOwners.Deck, myPlayer)
                Me.Refresh()

                System.Threading.Thread.Sleep(100)
            End If

            myPlayer += 1
        Next

        ' Setup discard pile
        MoveCard(CardOwners.Deck, CardOwners.Discard)
        DiscardCount = 1
        DiscardBottom = NoCard

        SetStatus("", 0, True)
    End Sub

    Private Sub ResetDeck()
        For i As Byte = 0 To UBound(Deck)
            Deck(i) = New Card(i)
        Next i

        ResetInverts()

        DiscardTop = NoCard
        DiscardBottom = NoCard
        DiscardCount = 0
    End Sub

    Private Sub ResetInverts()
        For i As Byte = 0 To UBound(Deck)
            Deck(i).Status = CardFront
        Next
    End Sub

    Private Sub ResetPlayers()
        KnockActive = False
        Knocker = Nothing
        BlitzActive = False

        For i As Byte = 0 To UBound(_player)
            _player(i).ResetPlayer()
        Next
    End Sub

    Private Sub ResetUI()
        PlayerControls(False, True, False, True, "Knock")
        SetLabelLocations()
        SetStatus("", 0, True)
        btnNewRound.Visible = False
        Me.Text = "Blitz     Game #" & Seed
    End Sub

    Private Sub SetCardLocations()
        If Not GameActive Then Exit Sub

        With _player(1)
            .X = .MidX - (Card.CardWidth + (CardOffset_X * (.TotalCards - 1))) / 2
            .Y = .MidY - (Card.CardHeight / 2)
        End With
        With _player(2)
            .X = .MidX - (Card.CardWidth / 2)
            .Y = .MidY - (Card.CardHeight + CardOffset_Y * (.TotalCards - 1)) / 2
        End With
        With _player(3)
            .X = .MidX - ((Card.CardWidth + CardOffset_X * (.TotalCards - 1)) / 2)
            .Y = .MidY - (Card.CardHeight / 2)
        End With
        With _player(4)
            .X = .MidX - (Card.CardWidth / 2)
            .Y = .MidY - (Card.CardHeight + CardOffset_Y * (.TotalCards - 1)) / 2
        End With
    End Sub

    Private Sub SetLabelLocations()
        lblPlayer1.Location = New Point(_player(1).MidX - 50, _player(1).MidY + (CardHeight / 2) + 5)
        lblPlayer2.Location = New Point(_player(2).MidX - (CardWidth / 2) - 20, _player(2).MidY + CardHeight + 10)
        lblPlayer3.Location = New Point(_player(3).MidX - 50, _player(3).MidY + (CardHeight / 2) + 5)
        lblPlayer4.Location = New Point(_player(4).MidX - (CardWidth / 2) - 20, _player(4).MidY + CardHeight + 10)
    End Sub

    Private Sub PlayerControls(ByVal DrawCardEnabled As Boolean, _
                              ByVal DrawCardVisible As Boolean, _
                              ByVal DiscardCardEnabled As Boolean, _
                              ByVal DiscardCardVisible As Boolean, _
                              Optional ByVal DiscardText As String = Nothing)
        btnDrawCard.Enabled = DrawCardEnabled
        btnDrawCard.Visible = DrawCardVisible
        btnDiscard.Enabled = DiscardCardEnabled
        btnDiscard.Visible = DiscardCardVisible

        If DiscardText <> Nothing Then
            btnDiscard.Text = DiscardText
        End If
    End Sub

    Private Sub SetStatus(ByVal text As String, Optional ByVal id As Integer = 0, Optional ByVal reset As Boolean = False)
        If reset Then
            lblPlayer1.Text = ""
            lblPlayer2.Text = ""
            lblPlayer3.Text = ""
            lblPlayer4.Text = ""
        End If

        Select Case id
            Case 0
                lblStatus.Text = text
            Case 1
                lblPlayer1.Text = text
            Case 2
                lblPlayer2.Text = text
            Case 3
                lblPlayer3.Text = text
            Case 4
                lblPlayer4.Text = text
        End Select
    End Sub

    Private Sub UpdateScores(ByVal showScoreBox As Boolean)
        If showScoreBox Then
            ScoreBox.Visible = True
            lblScoreName1.Text = _player(1).Name
            lblScoreName2.Text = _player(2).Name
            lblScoreName3.Text = _player(3).Name
            lblScoreName4.Text = _player(4).Name

            Select Case _player(1).Tokens
                Case Is > 0
                    lblScore1.ForeColor = Color.White
                    lblScore1.Text = _player(1).Tokens & " tokens"
                Case 0
                    lblScore1.ForeColor = Color.Yellow
                    lblScore1.Text = "On their honor"
                Case Is < 0
                    lblScore1.ForeColor = Color.Red
                    lblScore1.Text = "Out"
            End Select
            Select Case _player(2).Tokens
                Case Is > 0
                    lblScore2.ForeColor = Color.White
                    lblScore2.Text = _player(2).Tokens & " tokens"
                Case 0
                    lblScore2.ForeColor = Color.Yellow
                    lblScore2.Text = "On their honor"
                Case Is < 0
                    lblScore2.ForeColor = Color.Red
                    lblScore2.Text = "Out"
            End Select
            Select Case _player(3).Tokens
                Case Is > 0
                    lblScore3.ForeColor = Color.White
                    lblScore3.Text = _player(3).Tokens & " tokens"
                Case 0
                    lblScore3.ForeColor = Color.Yellow
                    lblScore3.Text = "On their honor"
                Case Is < 0
                    lblScore3.ForeColor = Color.Red
                    lblScore3.Text = "Out"
            End Select
            Select Case _player(4).Tokens
                Case Is > 0
                    lblScore4.ForeColor = Color.White
                    lblScore4.Text = _player(4).Tokens & " tokens"
                Case 0
                    lblScore4.ForeColor = Color.Yellow
                    lblScore4.Text = "On their honor"
                Case Is < 0
                    lblScore4.ForeColor = Color.Red
                    lblScore4.Text = "Out"
            End Select
        Else
            ScoreBox.Visible = False
        End If
    End Sub
#End Region

#Region "Card Methods"
    Private Function CardsLeft() As Byte
        CardsLeft = 52

        For x As Byte = 0 To UBound(Deck)
            If Deck(x).Owner <> CardOwners.Deck Then CardsLeft -= 1
        Next x
    End Function

    Private Function DeckEmpty() As Boolean
        For i As Byte = 0 To UBound(Deck)
            If Deck(i).Owner = CardOwners.Deck Then Return False
        Next i

        Return True
    End Function

    Private Function InvertedCard() As Byte
        For i As Byte = 0 To UBound(Deck)
            If Deck(i).Status = CardInverted Then Return i
        Next

        Return NoCard
    End Function

    Private Function MoveCard(ByVal fromPlayer As Byte, ByVal toPlayer As Byte, Optional ByVal cardToMove As Byte = NoCard) As Boolean
        Select Case fromPlayer
            Case CardOwners.Deck
                Dim CardFound As Boolean = False
                Dim rnd As New Random(Seed)

                Do While Not CardFound
                    cardToMove = rnd.Next(0, 52)

                    If Deck(cardToMove).Owner = CardOwners.Deck Then
                        Deck(cardToMove).Owner = toPlayer

                        Select Case toPlayer
                            Case 1, 2, 3, 4
                                _player(toPlayer).AddCard(cardToMove)
                            Case CardOwners.Discard
                                DiscardTop = cardToMove
                        End Select
                        CardFound = True
                    End If
                Loop
            Case CardOwners.Discard
                PickupCard = cardToMove
                Deck(cardToMove).Owner = toPlayer
                _player(toPlayer).AddCard(cardToMove)

                DiscardTop = DiscardBottom

                If DiscardTop <> NoCard Then
                    Deck(DiscardTop).Owner = CardOwners.Discard
                End If

                DiscardCount -= 1
                DiscardBottom = NoCard
                ResetInverts()
            Case Else
                If DiscardTop <> NoCard Then
                    DiscardBottom = DiscardTop
                    Deck(DiscardBottom).Owner = CardOwners.Used
                End If

                DiscardTop = cardToMove
                Deck(cardToMove).Owner = CardOwners.Discard
                DiscardCount += 1

                _player(fromPlayer).RemoveCard(cardToMove)
                ResetInverts()
        End Select
    End Function
#End Region

#Region "Player Methods"
    Private Function GetMasterSuit(ByVal bytPlayer As Byte) As Byte
        Dim bytMasterSuit As Byte
        Dim bytSuits(3) As Byte
        Dim x As Byte
        Dim bytHighestCard As Byte
        Dim bytSumA As Byte
        Dim bytSumB As Byte

        With _player(bytPlayer)
            ' Find out the count of each suit in the hand, reset the flag for each card
            For x = 0 To 2
                bytSuits(.GetCardSuit(x)) += 1
                .SetFlag(x, False)
            Next

            ' Find out which suit has the most cards
            For x = 0 To 3
                If bytSuits(x) > bytSuits(bytMasterSuit) Then bytMasterSuit = x
            Next

            Select Case bytSuits(bytMasterSuit)
                Case 1 ' There were only 1 of each suit, no cards were the same suit
                    ' Determine which card has the highest value and set it as 
                    ' the MasterSuit
                    For x = 0 To 2
                        If .GetCardVal(x) > .GetCardVal(bytHighestCard) Then bytHighestCard = x
                    Next
                    bytMasterSuit = .GetCardSuit(bytHighestCard)
                    .SetFlag(bytHighestCard, True)

                Case 2 ' There were 2 cards with the same suit, leaving 1 remainder
                    ' Determine if the sum of the 2 matching cards is greater than
                    ' or less than the remaining card.
                    bytSumA = 0 : bytSumB = 0

                    For x = 0 To 2
                        If .GetCardSuit(x) = bytMasterSuit Then
                            .SetFlag(x, True)
                            bytSumA += .GetCardVal(x)
                        Else
                            bytSumB += .GetCardVal(x)
                        End If
                    Next

                    If bytSumA < bytSumB Then
                        For x = 0 To 2
                            If .GetFlag(x) = False Then bytMasterSuit = .GetCardSuit(x)
                        Next
                    End If

                Case 3 ' All cards had the same suit
                    ' Nothing to do. Set MasterSuit to any card in hand
                    For x = 0 To 2
                        .SetFlag(x, True)
                    Next
                    bytMasterSuit = .GetCardSuit(0)

                Case Else ' This should never occur
                    MsgBox("Please send an email to psykad@gmail.com with the subject:" & vbCrLf & _
                           "Blitz Error: " & Seed & vbCrLf & vbCrLf & _
                           "Please include a description of what you were doing before you received this error", MsgBoxStyle.Critical + MsgBoxStyle.OkOnly, "Master suit error")
            End Select
        End With

        Return bytMasterSuit
    End Function

    Private Function GetScore(ByVal bytPlayer As Byte) As Byte
        Dim bytMasterSuit As Byte = GetMasterSuit(bytPlayer)

        With _player(bytPlayer)
            .Score = Nothing

            For x As Byte = 0 To 2
                If .GetCardSuit(x) = bytMasterSuit Then .Score += .GetCardVal(x)
            Next

            Return .Score
        End With
    End Function

    Private Function HasBlitz(ByVal bytPlayer As Byte) As Boolean
        If GetScore(bytPlayer) = 31 Then
            Return True
        Else
            Return False
        End If
    End Function
#End Region

#Region "GameTable Handlers"
    Private Sub GameTable_Closing() Handles Me.Closing
        SyncLock SyncObj
            If TakingTurn Then
                Try
                    ComputerThread.Abort()
                Catch ex As Exception

                End Try
            End If
        End SyncLock
    End Sub

    Private Sub GameTable_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseDown
        ' Exit method if current objPlayer isn't a human
        If Not GameActive Then Exit Sub
        If _player(CurrentPlayer).Mode <> Modes.Human Then Exit Sub

        ' If a game or round is not in play then ignore all clicks
        If Not GameActive Or Not RoundActive Then Return

        Dim _x As Integer = e.X
        Dim _y As Integer = e.Y
        Dim iCard As Integer = -1
        Dim iSelectedCard As Byte = CardOwners.Deck

        ' Reset card locations based on each objPlayer's TotalCards
        SetCardLocations()

        ' Sort through objDeck looking for a card matching the X/Y location of the mouse click
        For i As Byte = 0 To UBound(Deck)
            Deck(i).Status = CardFront

            If Deck(i).Owner = 1 And _player(1).TotalCards = 4 Then
                If (_x >= _player(1).X) And (_x <= _player(1).X + Card.CardWidth) And _
                   (_y >= _player(1).Y) And (_y <= _player(1).Y + Card.CardHeight) Then

                    iCard = i
                    iSelectedCard = 1
                End If

                _player(1).X += CardOffset_X
            ElseIf Deck(i).Owner = CardOwners.Discard Then
                If (_x >= _player(CardOwners.Discard).X) And (_x <= _player(CardOwners.Discard).X + Card.CardWidth) And _
                   (_y >= _player(CardOwners.Discard).Y) And (_y <= _player(CardOwners.Discard).Y + Card.CardHeight) And _
                   _player(1).TotalCards < 4 Then

                    iCard = i
                    iSelectedCard = CardOwners.Discard
                End If
            End If
        Next i

        ' Decide what to do if a valid card was selected
        If iCard <> -1 Then
            Deck(iCard).Status = CardInverted

            If iSelectedCard = 1 Then
                PlayerControls(False, True, True, True, "Discard")
            ElseIf iSelectedCard = CardOwners.Discard Then
                PlayerControls(True, True, True, True, "Take Card")
            End If
        Else
            If _player(1).TotalCards < 4 Then
                If KnockActive Then
                    PlayerControls(True, True, False, True, "Can't Knock")
                Else
                    PlayerControls(True, True, True, True, "Knock")
                End If
            Else
                btnDiscard.Text = "Select Card"
                PlayerControls(False, True, False, True, "Select Card")
            End If

            ResetInverts()
        End If

        Me.Refresh()
    End Sub

    Private Sub GameTable_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint
        If Not GameActive Then Exit Sub

        '// DEBUG SECTION //
        If DebugMode Then
            CardOffset_X = Card.CardWidth + 10
            CardOffset_Y = Card.CardHeight + 10
            ScoreBox.Visible = False
        Else
            CardOffset_X = 16
            CardOffset_Y = 30
            ScoreBox.Visible = True
        End If

        ' Reset Player card locations based on TotalCards
        SetCardLocations()

        ' Run through Deck and draw cards per their owner
        For i As Byte = 0 To UBound(Deck)
            Select Case Deck(i).Owner
                Case 1
                    With _player(1)
                        If .InGame Then
                            PaintCard(e.Graphics, .X, .Y, i, Deck(i).Status)
                            .X += CardOffset_X
                        End If
                    End With
                Case 2
                    With _player(2)
                        If .InGame Then
                            If DebugMode Or RoundActive = False Then
                                PaintCard(e.Graphics, .X, .Y, i, CardFront)
                            Else
                                PaintCard(e.Graphics, .X, .Y, DeckPattern, CardBack)
                            End If
                            .Y += CardOffset_Y
                        End If
                    End With
                Case 3
                    With _player(3)
                        If .InGame Then
                            If DebugMode Or RoundActive = False Then
                                PaintCard(e.Graphics, .X, .Y, i, CardFront)
                            Else
                                PaintCard(e.Graphics, .X, .Y, DeckPattern, CardBack)
                            End If
                            .X += CardOffset_X
                        End If
                    End With
                Case 4
                    With _player(4)
                        If .InGame Then
                            If DebugMode Or RoundActive = False Then
                                PaintCard(e.Graphics, .X, .Y, i, CardFront)
                            Else
                                PaintCard(e.Graphics, .X, .Y, DeckPattern, CardBack)
                            End If
                            .Y += CardOffset_Y
                        End If
                    End With
            End Select
        Next i

        ' Continue to draw Deck pile if it is not empty
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
                PaintCard(e.Graphics, _player(CardOwners.Deck).X - x, _player(CardOwners.Deck).Y - y, DeckPattern, CardBack)
                x += 2
                y += 2
            Next
        End If

        Select Case DiscardCount
            Case Is > 35 : n = 7
            Case Is > 30 : n = 6
            Case Is > 25 : n = 5
            Case Is > 20 : n = 4
            Case Is > 15 : n = 3
            Case Is > 10 : n = 2
            Case Is > 5 : n = 1
            Case Else : n = 0
        End Select

        If DiscardTop <> NoCard Then
            x = 0 : y = 0
            If DiscardCount > 1 Then
                For i = 0 To n
                    PaintCard(e.Graphics, _player(CardOwners.Discard).X - x, _player(CardOwners.Discard).Y - y, DiscardTop, CardFront)
                    x += 2 : y += 2
                Next
            End If
            PaintCard(e.Graphics, _player(CardOwners.Discard).X - x, _player(CardOwners.Discard).Y - y, DiscardTop, Deck(DiscardTop).Status)
        End If
    End Sub

    Private Sub DoRefreshScreen()
        Me.Refresh()
    End Sub

    Private Sub RefreshScreen()
        Dim cb As New SimpleCallback(AddressOf DoRefreshScreen)

        Try
            Me.Invoke(cb)
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub
#End Region

#Region "GameTable Component Handlers"
    Private Sub btnDrawCard_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDrawCard.Click
        ' Check if objDeck is empty
        If DeckEmpty() Then Exit Sub

        ' Reset any inverted cards
        ResetInverts()

        ' Check if objPlayer didn't already draw a card
        If _player(CurrentPlayer).TotalCards < 4 Then
            MoveCard(CardOwners.Deck, CurrentPlayer)
            PlayerControls(False, True, False, True, "Select Card")
        End If

        ' Redraw cards
        Me.Refresh()
    End Sub

    Private Sub btnDiscard_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDiscard.Click
        ' Check the status of the discard button and continue based on the button's text
        Select Case btnDiscard.Text
            Case "Knock"
                Knocker = CurrentPlayer
                KnockActive = True
                SetStatus(_player(CurrentPlayer).Name & " has knocked!", 0, True)
                TurnOver()
            Case "Take Card"
                If _player(CurrentPlayer).TotalCards < 4 Then
                    MoveCard(CardOwners.Discard, CurrentPlayer, DiscardTop)
                    PlayerControls(False, True, False, True, "Select Card")
                    Me.Refresh()
                End If
            Case "Discard"
                If InvertedCard() = PickupCard Then
                    MsgBox("You cannot discard the card you picked up", MsgBoxStyle.Exclamation + MsgBoxStyle.OkOnly, "Discard Error")
                Else
                    MoveCard(CurrentPlayer, CardOwners.Discard, InvertedCard)
                    PlayerControls(True, True, False, True, "Knock")
                    Me.Refresh()
                    TurnOver()
                End If
        End Select
    End Sub

    Private Sub btnNewRound_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnNewRound.Click
        btnNewRound.Visible = False
        NewRound()
    End Sub
#End Region

#Region "MainMenu Handlers"
    Private Sub NewGameToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NewGameToolStripMenuItem.Click
        NewGame()
    End Sub

    Private Sub ExitToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExitToolStripMenuItem.Click
        Application.Exit()
    End Sub

    Private Sub CheckForUpdatesToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckForUpdatesToolStripMenuItem.Click
        Try
            Dim PingOut As New Net.NetworkInformation.Ping
            Dim PingIn As Net.NetworkInformation.PingReply

            ' Ping Google
            PingIn = PingOut.Send("www.google.com", 3000)

            If PingIn.Status = Net.NetworkInformation.IPStatus.Success Then
                Dim UpdateInfoUrl As String = "http://www.psykad.com/software/blitz/versioninfo.txt"
                Dim wc As New Net.WebClient

                ' Download info on newest build
                Dim wcbuffer As Byte() = wc.DownloadData(UpdateInfoUrl)
                Dim VersionInfo As FileVersionInfo = FileVersionInfo.GetVersionInfo(Application.ExecutablePath)
                Dim CurrentBuild As Integer = Convert.ToInt32(VersionInfo.ProductPrivatePart)
                Dim NewestBuild As Integer = Convert.ToInt32(System.Text.ASCIIEncoding.ASCII.GetString(wcbuffer))

                ' Check to see if the current build is up to date
                If CurrentBuild < NewestBuild Or System.IO.File.Exists(Application.StartupPath & "\debug") Then
                    Dim MsgBoxAnswer As MsgBoxResult
                    Dim SourceFileURL As String = "http://www.psykad.com/software/blitz/bin/AutoUpdater.exe"
                    Dim DestinationFileURL As String = Application.StartupPath & "\AutoUpdater.exe"

                    MsgBoxAnswer = MsgBox("An update is available! Download update now?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Blitz Update")

                    If MsgBoxAnswer = MsgBoxResult.Yes Then
                        wc.DownloadFile(SourceFileURL, DestinationFileURL)
                        System.Diagnostics.Process.Start(DestinationFileURL)
                        Application.Exit()
                    End If
                Else
                    MsgBox("There are no updates available.", MsgBoxStyle.Information + MsgBoxStyle.OkOnly, "Blitz Update")
                End If
            Else
                MsgBox("Unable to perform update", MsgBoxStyle.Information + MsgBoxStyle.OkOnly, "Update Failed")
            End If
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical + MsgBoxStyle.OkOnly)
        End Try
    End Sub

    Private Sub AboutToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AboutToolStripMenuItem.Click
        About.Show()
    End Sub
#End Region
End Class
