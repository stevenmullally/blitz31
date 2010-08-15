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
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports Blitz.Objects
Imports Blitz.Objects.Player
Imports Blitz.Objects.Card

Public Class GameTable

    ''' <summary>
    ''' Initializes game table.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.SetStyle(ControlStyles.DoubleBuffer Or ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint, True)
        Me.UpdateStyles()

        ' Initialize card library
        Try
            If Not Card.Initialize() Then Exit Sub
        Catch ex As Exception
            MsgBox("Unable to load card library.", MsgBoxStyle.Critical + MsgBoxStyle.OkOnly, "Card Library Error")
            Exit Sub
        End Try

        SetStatus("", 0, True)
        UpdateScores(False)
        CreatePlayers()
        LoadSettings()
    End Sub

#Region "Variable Declarations"
    Private Seed As Integer = 0
    Private SettingsFile As String = Application.StartupPath & "\settings.ini"
    Private FirstRun As Boolean = True
    Private UpdateOnStart As Boolean = True

    Private Deck(51) As Card
    Public Players(5) As Player

    Private CurrentPlayer As Byte = 0
    Private Knocker As Byte = 0
    Private Winner As Byte = 0
    Private Dealer As Byte = 0

    ' Card pointers
    Private DiscardTop As Byte = NoCard
    Private DiscardBottom As Byte = NoCard
    Private PickupCard As Byte = NoCard
    Private DiscardCount As Byte = 0

    ' Game states
    Public GameActive As Boolean = False
    Private RoundActive As Boolean = False
    Private BlitzActive As Boolean = False
    Private KnockActive As Boolean = False
    Private DebugMode As Boolean = False

    ' Card properties
    Private Const NoCard As Byte = 52
    Private Const DeckPattern As Byte = 52
    Private Const CardFront As Byte = 0
    Private Const CardBack As Byte = 1
    Private Const CardInverted As Byte = 2
    Private CardOffset_X As Byte = 16
    Private CardOffset_Y As Byte = 30

    ' Threading objects
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
        Dim i As Byte

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

        ' Setup the players and dealer
        For i = 1 To 4
            Players(i).Tokens = 4
            Players(i).InGame = True
        Next

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
        Dim i As Byte

        ' Change cursor to wait while round is setup
        Cursor.Current = Cursors.WaitCursor

        KnockActive = False
        BlitzActive = False
        Knocker = Nothing

        For i = 1 To 4
            Players(i).ResetHand()
            Players(i).Flag = False
        Next

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
        For i = 1 To 4
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
            If Players(CurrentPlayer).InGame Then
                ' End the round if the current Player is the one who knocked
                If KnockActive And CurrentPlayer = Knocker Then
                    RoundOver()
                Else
                    ' Set status showing current Player's turn
                    Select Case CurrentPlayer
                        Case 1
                            SetStatus("Your turn!", 1, True)
                        Case 2, 3, 4
                            SetStatus(Players(CurrentPlayer).Name & "'s turn", CurrentPlayer, True)
                    End Select

                    ' Call ComputerTurn if Player is a computer
                    Select Case Players(CurrentPlayer).Mode
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
        Dim masterSuit As Byte = GetMasterSuit(CurrentPlayer)
        Dim suits(3) As Byte
        Dim highestCard As Byte
        Dim lowestCard As Byte
        Dim sumA As Byte
        Dim sumB As Byte
        Dim cardToTake As Byte = NoCard
        Dim cardToRemove As Byte = NoCard
        Dim oddCard As Byte = NoCard
        Dim cardA As Byte = NoCard
        Dim cardB As Byte = NoCard
        Dim hasAce As Boolean = False
        Dim i As Byte

        SyncLock SyncObj
            TakingTurn = True
        End SyncLock

        If Not KnockActive Then
            If Deck(DiscardTop).SuitVal <> masterSuit Then
                Dim goal As Byte

                If CardsLeft() >= 20 Then
                    goal = 25
                ElseIf CardsLeft() <= 19 Then
                    goal = 29
                End If

                If GetScore(CurrentPlayer) > goal Then
                    Knocker = CurrentPlayer
                    KnockActive = True
                End If
            End If
        End If

        If Not KnockActive Or (KnockActive And Knocker <> CurrentPlayer) Then
            With Players(CurrentPlayer)
                For i = 0 To 2
                    suits(.GetCardSuit(i)) += 1
                Next i

                sumA = 0
                sumB = 0

                For i = 0 To 2
                    If .GetCardSuit(i) = masterSuit Then
                        .CardFlag(i) = True
                    Else
                        .CardFlag(i) = False
                    End If
                Next i

                Thread.Sleep(1000)

                If Deck(DiscardTop).SuitVal = masterSuit Then
                    sumA += Deck(DiscardTop).Value

                    Select Case suits(masterSuit)
                        Case 1
                            For i = 0 To 2
                                If .CardFlag(i) = True Then
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
                                If .CardFlag(i) = True Then
                                    sumA += .GetCardVal(i)
                                Else
                                    sumB += .GetCardVal(i)
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
                                If .CardFlag(i) = True Then
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

                RefreshScreen()

                Thread.Sleep(1000)

                sumA = 0
                sumB = 0

                For i = 0 To 3
                    suits(i) = 0
                    .CardFlag(i) = False
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
                        .CardFlag(i) = True
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
                            If .CardFlag(i) = False Then
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
                                    If .CardFlag(i) = True Then
                                        If .CardFlag(lowestCard) = False Then
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
                            If .CardFlag(i) = True Then
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
                                If .CardFlag(i) = True Then
                                    If .CardFlag(lowestCard) = False Then
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

        RefreshScreen()

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

        End Try
    End Sub

    Private Sub DoTurnOver()
        If KnockActive And Knocker = CurrentPlayer Then
            SetStatus(Players(CurrentPlayer).Name & " has knocked!")
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
        Dim i As Byte

        ' Set round to inactive
        RoundActive = False

        ' Determine who won
        DetermineWinner()

        ' Hide player controls
        PlayerControls(False, False, False, False)

        UpdateScores(True)

        Me.Refresh()

        Dim activePlayers As Byte = 0

        For i = 1 To 4
            With Players(i)
                If .Tokens >= 0 Then
                    .InGame = True
                    activePlayers += 1
                Else
                    .InGame = False
                End If
            End With
        Next

        If activePlayers = 1 Or Players(1).InGame = False Then
            GameOver()
        Else
            btnNewRound.Visible = True
        End If

        ' Set dealer for next round
        Do
            Dealer += 1
            If Dealer > 4 Then Dealer = 1
        Loop Until Players(Dealer).InGame

        ' Set current player to player clock-wise of dealer
        CurrentPlayer = Dealer
        Do
            CurrentPlayer += 1
            If CurrentPlayer > 4 Then CurrentPlayer = 1
        Loop Until Players(CurrentPlayer).InGame
    End Sub

    Private Sub DetermineWinner()
        Dim LowestScore As Byte
        Dim tie As Boolean
        Dim totalLosers As Byte
        Dim i As Byte

        ' If no one has knocked or no one has Blitz
        If KnockActive And Not BlitzActive Then
            LowestScore = 32

            ' Find the lowest score of the round
            For i = 1 To 4
                Players(i).Flag = False
                If Players(i).InGame And GetScore(i) < LowestScore Then LowestScore = GetScore(i)
            Next

            ' Find the players who score matched the lowest score
            For i = 1 To 4
                If Players(i).InGame And GetScore(i) = LowestScore Then
                    Players(i).Flag = True
                    totalLosers += 1
                End If
            Next

            If totalLosers > 1 Then tie = True
        Else
            ' Flag players who didn't have Blitz
            For i = 1 To 4
                If Players(i).InGame Then
                    If GetScore(i) = 31 Then
                        Players(i).Flag = False
                    Else
                        Players(i).Flag = True
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
            If Players(i).Flag And i = Knocker And tie Then Players(i).Flag = False

            ' Remove 1 token for any flagged player
            If Players(i).Flag And Players(i).InGame Then
                Players(i).RemoveToken(1)
                SetStatus(Players(i).Name & " lost!", i)
            End If

            ' Remove an additional token if the person who knocked had the lowest score
            If Players(i).Flag And Knocker = i Then Players(i).RemoveToken(1)
        Next
    End Sub

    Private Sub GameOver()
        ' Hide player controls
        SetStatus("Game Over", 0, True)

        If Players(1).InGame = False Then
            MsgBox("You lose!", MsgBoxStyle.Information + MsgBoxStyle.OkOnly, "Game Over")
        Else
            MsgBox("You win!", MsgBoxStyle.Information + MsgBoxStyle.OkOnly, "Game Over")
        End If

        PlayerControls(False, False, False, False)
    End Sub
#End Region

#Region "Intialization Methods"
    Private Sub CreatePlayers()
        Dim i As Byte

        ' Create new players
        For i = 0 To UBound(Players)
            Players(i) = New Player(Modes.Computer)
        Next i

        ' Set Player 1 to Human
        Players(1).Mode = Modes.Human

        ' Set Player names
        Players(1).Name = "Player 1"
        Players(2).Name = "Player 2"
        Players(3).Name = "Player 3"
        Players(4).Name = "Player 4"

        ' Set Deck and discard locations
        Players(CardOwners.Deck).X = 290 - Card.CardWidth
        Players(CardOwners.Deck).Y = 300 - (Card.CardHeight / 2)
        Players(CardOwners.Discard).X = 310
        Players(CardOwners.Discard).Y = 300 - (Card.CardHeight / 2)

        ' Set objPlayer locations
        Players(1).MidX = 300
        Players(1).MidY = 500
        Players(2).MidX = 100
        Players(2).MidY = 300
        Players(3).MidX = 300
        Players(3).MidY = 100
        Players(4).MidX = 500
        Players(4).MidY = 300
    End Sub

    Private Sub DealCards()
        Dim i As Byte
        Dim myPlayer As Byte = Dealer + 1

        SetStatus(Players(Dealer).Name & " is dealing cards", 0, True)

        ' Deal cards to each player
        For i = 1 To 12
            If myPlayer > 4 Then myPlayer = 1

            If Players(myPlayer).InGame Then
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
        Dim i As Byte

        For i = 0 To UBound(Deck)
            Deck(i) = New Card(i)
        Next i

        ResetInverts()

        DiscardTop = NoCard
        DiscardBottom = NoCard
        DiscardCount = 0
    End Sub

    Private Sub ResetInverts()
        Dim i As Byte

        For i = 0 To UBound(Deck)
            Deck(i).Status = CardFront
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

        With Players(1)
            .X = .MidX - (Card.CardWidth + (CardOffset_X * (.TotalCards - 1))) / 2
            .Y = .MidY - (Card.CardHeight / 2)
        End With
        With Players(2)
            .X = .MidX - (Card.CardWidth / 2)
            .Y = .MidY - (Card.CardHeight + CardOffset_Y * (.TotalCards - 1)) / 2
        End With
        With Players(3)
            .X = .MidX - ((Card.CardWidth + CardOffset_X * (.TotalCards - 1)) / 2)
            .Y = .MidY - (Card.CardHeight / 2)
        End With
        With Players(4)
            .X = .MidX - (Card.CardWidth / 2)
            .Y = .MidY - (Card.CardHeight + CardOffset_Y * (.TotalCards - 1)) / 2
        End With
    End Sub

    Private Sub SetLabelLocations()
        lblPlayer1.Location = New Point(Players(1).MidX - 50, Players(1).MidY + (CardHeight / 2) + 5)
        lblPlayer2.Location = New Point(Players(2).MidX - (CardWidth / 2) - 20, Players(2).MidY + CardHeight + 10)
        lblPlayer3.Location = New Point(Players(3).MidX - 50, Players(3).MidY + (CardHeight / 2) + 5)
        lblPlayer4.Location = New Point(Players(4).MidX - (CardWidth / 2) - 20, Players(4).MidY + CardHeight + 10)
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

    Public Sub UpdateScores(ByVal showScoreBox As Boolean)
        If showScoreBox Then
            ScoreBox.Visible = True
            lblScoreName1.Text = Players(1).Name
            lblScoreName2.Text = Players(2).Name
            lblScoreName3.Text = Players(3).Name
            lblScoreName4.Text = Players(4).Name

            Select Case Players(1).Tokens
                Case Is > 0
                    lblScore1.ForeColor = Color.White
                    lblScore1.Text = Players(1).Tokens & " tokens"
                Case 0
                    lblScore1.ForeColor = Color.Yellow
                    lblScore1.Text = "On their honor"
                Case Is < 0
                    lblScore1.ForeColor = Color.Red
                    lblScore1.Text = "Out"
            End Select
            Select Case Players(2).Tokens
                Case Is > 0
                    lblScore2.ForeColor = Color.White
                    lblScore2.Text = Players(2).Tokens & " tokens"
                Case 0
                    lblScore2.ForeColor = Color.Yellow
                    lblScore2.Text = "On their honor"
                Case Is < 0
                    lblScore2.ForeColor = Color.Red
                    lblScore2.Text = "Out"
            End Select
            Select Case Players(3).Tokens
                Case Is > 0
                    lblScore3.ForeColor = Color.White
                    lblScore3.Text = Players(3).Tokens & " tokens"
                Case 0
                    lblScore3.ForeColor = Color.Yellow
                    lblScore3.Text = "On their honor"
                Case Is < 0
                    lblScore3.ForeColor = Color.Red
                    lblScore3.Text = "Out"
            End Select
            Select Case Players(4).Tokens
                Case Is > 0
                    lblScore4.ForeColor = Color.White
                    lblScore4.Text = Players(4).Tokens & " tokens"
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
        Dim i As Byte

        CardsLeft = 52

        For i = 0 To UBound(Deck)
            If Deck(i).Owner <> CardOwners.Deck Then CardsLeft -= 1
        Next i
    End Function

    Private Function DeckEmpty() As Boolean
        Dim i As Byte

        For i = 0 To UBound(Deck)
            If Deck(i).Owner = CardOwners.Deck Then Return False
        Next i

        Return True
    End Function

    Private Function InvertedCard() As Byte
        Dim i As Byte

        For i = 0 To UBound(Deck)
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
                                Players(toPlayer).AddCard(cardToMove)
                            Case CardOwners.Discard
                                DiscardTop = cardToMove
                        End Select
                        CardFound = True
                    End If
                Loop
            Case CardOwners.Discard
                PickupCard = cardToMove
                Deck(cardToMove).Owner = toPlayer
                Players(toPlayer).AddCard(cardToMove)

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

                Players(fromPlayer).RemoveCard(cardToMove)
                ResetInverts()
        End Select
    End Function
#End Region

#Region "Player Methods"
    Private Function GetMasterSuit(ByVal player As Byte) As Byte
        Dim masterSuit As Byte
        Dim suits(3) As Byte
        Dim highestCard As Byte
        Dim sumA As Byte
        Dim sumB As Byte
        Dim i As Byte

        With Players(player)
            ' Find out the count of each suit in the hand, reset the flag for each card
            For i = 0 To 2
                suits(.GetCardSuit(i)) += 1
                .CardFlag(i) = False
            Next

            ' Find out which suit has the most cards
            For i = 0 To 3
                If suits(i) > suits(masterSuit) Then masterSuit = i
            Next

            Select Case suits(masterSuit)
                Case 1 ' There were only 1 of each suit, no cards were the same suit
                    ' Determine which card has the highest value and set it as 
                    ' the MasterSuit
                    For i = 0 To 2
                        If .GetCardVal(i) > .GetCardVal(highestCard) Then highestCard = i
                    Next
                    masterSuit = .GetCardSuit(highestCard)
                    .CardFlag(highestCard) = True
                Case 2 ' There were 2 cards with the same suit, leaving 1 remainder
                    ' Determine if the sum of the 2 matching cards is greater than
                    ' or less than the remaining card.
                    sumA = 0
                    sumB = 0

                    For i = 0 To 2
                        If .GetCardSuit(i) = masterSuit Then
                            .CardFlag(i) = True
                            sumA += .GetCardVal(i)
                        Else
                            sumB += .GetCardVal(i)
                        End If
                    Next

                    For i = 0 To 2
                        If sumA > sumB Then
                            If .CardFlag(i) = True Then masterSuit = .GetCardSuit(i)
                        Else
                            If .CardFlag(i) = False Then masterSuit = .GetCardSuit(i)
                        End If
                    Next
                Case 3 ' All cards had the same suit
                    ' Nothing to do. Set MasterSuit to any card in hand
                    For i = 0 To 2
                        .CardFlag(i) = True
                    Next
                    masterSuit = .GetCardSuit(0)
            End Select
        End With

        Return masterSuit
    End Function

    Private Function GetScore(ByVal bytPlayer As Byte) As Byte
        Dim bytMasterSuit As Byte = GetMasterSuit(bytPlayer)
        Dim i As Byte

        With Players(bytPlayer)
            .Score = Nothing

            For i = 0 To 2
                If .GetCardSuit(i) = bytMasterSuit Then .Score += .GetCardVal(i)
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

#Region "Load / Save Methods"
    Public Sub SaveSettings()
        Dim fileStream As New StreamWriter(SettingsFile)

        With fileStream
            .WriteLine("player1name=" & Players(1).Name)
            .WriteLine("player2name=" & Players(2).Name)
            .WriteLine("player3name=" & Players(3).Name)
            .WriteLine("player4name=" & Players(4).Name)
            .WriteLine("firstrun=" & FirstRun)
            .WriteLine("updateonstart=" & UpdateOnStart)

            .Flush()
            .Close()
        End With
    End Sub

    Private Sub LoadSettings()
        If File.Exists(SettingsFile) Then
            Dim fileStream As New StreamReader(SettingsFile)
            Dim buffer1 As String
            Dim buffer2() As String

            With fileStream
                Do Until .EndOfStream
                    buffer1 = .ReadLine

                    buffer2 = buffer1.Split("=")

                    Select Case buffer2(0)
                        Case "player1name"
                            Players(1).Name = buffer2(1)
                        Case "player2name"
                            Players(2).Name = buffer2(1)
                        Case "player3name"
                            Players(3).Name = buffer2(1)
                        Case "player4name"
                            Players(4).Name = buffer2(1)
                        Case "firstrun"
                            FirstRun = buffer2(1)
                        Case "updateonstart"
                            UpdateOnStart = buffer2(1)
                    End Select
                Loop

                .Close()
            End With
        Else
            Dim fileStream As New StreamWriter(SettingsFile)

            With fileStream
                .WriteLine("player1name=" & Players(1).Name)
                .WriteLine("player2name=" & Players(2).Name)
                .WriteLine("player3name=" & Players(3).Name)
                .WriteLine("player4name=" & Players(4).Name)
                .WriteLine("firstrun=true")
                .WriteLine("updateonstart=" & UpdateOnStart)

                .Flush()
                .Close()
            End With
        End If
    End Sub
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
        If Players(CurrentPlayer).Mode <> Modes.Human Then Exit Sub

        ' If a game or round is not in play then ignore all clicks
        If Not GameActive Or Not RoundActive Then Return

        Dim _x As Integer = e.X
        Dim _y As Integer = e.Y
        Dim iCard As Integer = NoCard
        Dim iSelectedCard As Byte = CardOwners.Deck
        Dim i As Byte

        ' Reset card locations based on each objPlayer's TotalCards
        SetCardLocations()

        ' Sort through objDeck looking for a card matching the X/Y location of the mouse click
        For i = 0 To UBound(Deck)
            Deck(i).Status = CardFront

            If Deck(i).Owner = 1 And Players(1).TotalCards = 4 Then
                If (_x >= Players(1).X) And (_x <= Players(1).X + Card.CardWidth) And _
                   (_y >= Players(1).Y) And (_y <= Players(1).Y + Card.CardHeight) Then

                    iCard = i
                    iSelectedCard = 1
                End If

                Players(1).X += CardOffset_X
            ElseIf Deck(i).Owner = CardOwners.Discard Then
                If (_x >= Players(CardOwners.Discard).X) And (_x <= Players(CardOwners.Discard).X + Card.CardWidth) And _
                   (_y >= Players(CardOwners.Discard).Y) And (_y <= Players(CardOwners.Discard).Y + Card.CardHeight) And _
                   Players(1).TotalCards < 4 Then

                    iCard = i
                    iSelectedCard = CardOwners.Discard
                End If
            End If
        Next i

        ' Decide what to do if a valid card was selected
        If iCard <> NoCard Then
            Deck(iCard).Status = CardInverted

            If iSelectedCard = 1 Then
                PlayerControls(False, True, True, True, "Discard")
            ElseIf iSelectedCard = CardOwners.Discard Then
                PlayerControls(True, True, True, True, "Take Card")
            End If
        Else
            If Players(1).TotalCards < 4 Then
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
        Dim i As Byte

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
        For i = 0 To UBound(Deck)
            Select Case Deck(i).Owner
                Case 1
                    With Players(1)
                        If .InGame Then
                            PaintCard(e.Graphics, .X, .Y, i, Deck(i).Status)
                            .X += CardOffset_X
                        End If
                    End With
                Case 2
                    With Players(2)
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
                    With Players(3)
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
                    With Players(4)
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
                PaintCard(e.Graphics, Players(CardOwners.Deck).X - x, Players(CardOwners.Deck).Y - y, DeckPattern, CardBack)
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
                    PaintCard(e.Graphics, Players(CardOwners.Discard).X - x, Players(CardOwners.Discard).Y - y, DiscardTop, CardFront)
                    x += 2 : y += 2
                Next
            End If
            PaintCard(e.Graphics, Players(CardOwners.Discard).X - x, Players(CardOwners.Discard).Y - y, DiscardTop, Deck(DiscardTop).Status)
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

        End Try
    End Sub

    Private Sub GameTable_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        Dim updateList As String = Application.StartupPath & "\updatelist.txt"

        If FirstRun Or File.Exists(updateList) Then
            If File.Exists(updateList) Then Kill(updateList)
            FirstRun = False
            SaveSettings()
        End If

        If UpdateOnStart Then
            RunAutoUpdate(False)
        End If
    End Sub
#End Region

#Region "GameTable Component Handlers"
    Private Sub btnDrawCard_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDrawCard.Click
        ' Check if objDeck is empty
        If DeckEmpty() Then Exit Sub

        ' Reset any inverted cards
        ResetInverts()

        ' Check if objPlayer didn't already draw a card
        If Players(CurrentPlayer).TotalCards < 4 Then
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
                SetStatus(Players(CurrentPlayer).Name & " has knocked!", 0, True)
                TurnOver()
            Case "Take Card"
                If Players(CurrentPlayer).TotalCards < 4 Then
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
        RunAutoUpdate(True)
    End Sub

    Private Sub AboutToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AboutToolStripMenuItem.Click
        About.Show()
    End Sub

    Private Sub OptionsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OptionsToolStripMenuItem.Click
        Options.Show()
    End Sub

    Private Sub RunAutoUpdate(ByVal showPopups As Boolean)
        Try
            Dim PingOut As New Net.NetworkInformation.Ping
            Dim PingIn As Net.NetworkInformation.PingReply
            Dim CurrentBuild As String = getFileSHA(Application.ExecutablePath)

            ' Ping Google
            PingIn = PingOut.Send("www.google.com", 3000)

            If PingIn.Status = Net.NetworkInformation.IPStatus.Success Then
                Dim UpdateInfoUrl As String = "http://www.psykad.com/software/blitz/updateinfo.txt"
                Dim wc As New Net.WebClient

                ' Download info on newest build
                Dim wcbuffer As Byte() = wc.DownloadData(UpdateInfoUrl)
                Dim NewestBuild As String = System.Text.ASCIIEncoding.ASCII.GetString(wcbuffer)

                ' Check to see if the current build is up to date
                If CurrentBuild <> NewestBuild Or System.IO.File.Exists(Application.StartupPath & "\debug") Then
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
                    If showPopups Then MsgBox("There are no updates available.", MsgBoxStyle.Information + MsgBoxStyle.OkOnly, "Blitz Update")
                End If
            Else
                If showPopups Then MsgBox("Unable to perform update", MsgBoxStyle.Information + MsgBoxStyle.OkOnly, "Update Failed")
            End If
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical + MsgBoxStyle.OkOnly)
        End Try
    End Sub

    Private Function getFileSHA(ByVal filePath As String) As String
        Dim shaProvider As SHA1CryptoServiceProvider = New SHA1CryptoServiceProvider
        Dim fStream As FileStream = New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192)

        shaProvider.ComputeHash(fStream)
        fStream.Close()

        Dim hash As Byte() = shaProvider.Hash
        Dim buff As StringBuilder = New StringBuilder
        Dim hashByte As Byte

        For Each hashByte In hash
            buff.Append(String.Format("{0:X2}", hashByte))
        Next

        Return buff.ToString
    End Function
#End Region
End Class
