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

Namespace Objects
    Public Class Player

#Region "Fields"
        Private _name As String
        ''' <summary>
        ''' Gets or sets the Player's name.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The Player's name.</returns>
        ''' <remarks></remarks>
        Public Property Name() As String
            Get
                Return _name
            End Get
            Set(ByVal value As String)
                _name = value
            End Set
        End Property

        Private _mode As Modes
        ''' <summary>
        ''' Gets or sets the Player's mode.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The Player's mode.</returns>
        ''' <remarks></remarks>
        Public Property Mode() As Modes
            Get
                Return _mode
            End Get
            Set(ByVal value As Modes)
                _mode = value
            End Set
        End Property

        Private _hand(3) As Card
        ''' <summary>
        ''' The Player's array of cards.
        ''' </summary>
        ''' <value></value>
        ''' <returns>A Player's card.</returns>
        ''' <remarks></remarks>
        Public Property Hand() As Card()
            Get
                Return _hand
            End Get
            Set(ByVal value As Card())
                _hand = value
            End Set
        End Property

        Private _handLocation As Point
        ''' <summary>
        ''' Gets or sets the location of the Player's hand.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The location of the Player's hands.</returns>
        ''' <remarks></remarks>
        Public Property HandLocation() As Point
            Get
                Return _handLocation
            End Get
            Set(ByVal value As Point)
                _handLocation = value
            End Set
        End Property

        Private _handLocationMid As Point
        ''' <summary>
        ''' Gets or sets the middle of the Player's hand.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The middle of the Player's hand.</returns>
        ''' <remarks></remarks>
        Public Property HandLocationMid() As Point
            Get
                Return _handLocationMid
            End Get
            Set(ByVal value As Point)
                _handLocationMid = value
            End Set
        End Property

        Private _totalCards As Byte
        ''' <summary>
        ''' Gets or sets the Player's total cards.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The Player's total cards.</returns>
        ''' <remarks></remarks>
        Public Property TotalCards()
            Get
                Return _totalCards
            End Get
            Set(ByVal value)
                _totalCards = value
            End Set
        End Property

        Private _tokens As Byte
        ''' <summary>
        ''' Gets or sets the Player's token count.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The Player's token count.</returns>
        ''' <remarks></remarks>
        Public Property Tokens() As Byte
            Get
                Return _tokens
            End Get
            Set(ByVal value As Byte)
                _tokens = value
            End Set
        End Property

        Private _inGame As Boolean
        ''' <summary>
        ''' Gets or sets the Player's game status.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The Player's game status.</returns>
        ''' <remarks></remarks>
        Public Property InGame() As Boolean
            Get
                Return _inGame
            End Get
            Set(ByVal value As Boolean)
                _inGame = value
            End Set
        End Property

        Private _flagged As Boolean
        ''' <summary>
        ''' Gets or sets the Player's flag.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The Player's flag.</returns>
        ''' <remarks></remarks>
        Public Property Flagged() As Boolean
            Get
                Return _flagged
            End Get
            Set(ByVal value As Boolean)
                _flagged = value
            End Set
        End Property

        Public Enum Modes As Byte
            Computer = 0
            Human = 1
        End Enum
#End Region

#Region "Methods"
        ''' <summary>
        ''' Creates a new player.
        ''' </summary>
        ''' <param name="mode">The Player's mode.</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal mode As Modes)
            Me.Mode = mode

            CreateNewHand()
        End Sub

        ''' <summary>
        ''' Creates a new hand for the Player.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub CreateNewHand()
            For i As Byte = 0 To 3
                Hand(i) = New Card(Nothing)
            Next

            TotalCards = 0
        End Sub

        ''' <summary>
        ''' Adds a card to the Player's hand.
        ''' </summary>
        ''' <param name="card">Position of the card in the deck to add.</param>
        ''' <remarks></remarks>
        Public Sub AddCard(ByVal card As Byte)
            If TotalCards = 4 Then Exit Sub

            Hand(FreeCard).Position = card
            Hand(FreeCard).InUse = True
            TotalCards += 1
        End Sub

        ''' <summary>
        ''' Removes a card from the Player's hand.
        ''' </summary>
        ''' <param name="card">Position of the card in the deck to remove.</param>
        ''' <remarks></remarks>
        Public Sub RemoveCard(ByVal card As Byte)
            Dim i As Byte

            For i = 0 To UBound(Hand)
                If Hand(i).Position = card Then
                    Hand(i) = New Card(Nothing)
                    TotalCards -= 1
                    Exit For
                End If
            Next

            If Hand(3).InUse Then
                For i = 0 To 2
                    If Not Hand(i).InUse Then
                        Hand(i).Position = Hand(3).Position
                        Hand(i).InUse = True

                        Hand(3) = New Card(Nothing)
                    End If
                Next
            End If
        End Sub

        ''' <summary>
        ''' Finds the next free card in the Player's hand.
        ''' </summary>
        ''' <returns>The next free card in the Player's hand.</returns>
        ''' <remarks></remarks>
        Private Function FreeCard() As Byte
            For FreeCard = 0 To UBound(Hand)
                If Not Hand(FreeCard).InUse Then Return FreeCard
            Next
        End Function

        ''' <summary>
        ''' The suit in the player's hand with the highest value.
        ''' </summary>
        ''' <returns>The suit with the highest value.</returns>
        ''' <remarks></remarks>
        Public Function MasterSuit() As Byte
            Dim highestCard As Byte
            Dim suits(3) As Byte
            Dim sumA As Byte = 0
            Dim sumB As Byte = 0
            Dim i As Byte

            MasterSuit = 0

            With Me
                For i = 0 To 2
                    suits(.Hand(i).Suit) += 1
                    .Hand(i).Flagged = False
                Next

                For i = 0 To 3
                    If suits(i) > suits(MasterSuit) Then MasterSuit = i
                Next

                Select Case suits(MasterSuit)
                    Case 1
                        For i = 0 To 2
                            If .Hand(i).Value > .Hand(highestCard).Value Then highestCard = i
                        Next
                        MasterSuit = .Hand(highestCard).Suit
                        .Hand(highestCard).Flagged = True
                    Case 2
                        For i = 0 To 2
                            If .Hand(i).Suit = MasterSuit Then
                                .Hand(i).Flagged = True
                                sumA += .Hand(i).Value
                            Else
                                sumB += .Hand(i).Value
                            End If
                        Next

                        For i = 0 To 2
                            If sumA > sumB Then
                                If .Hand(i).Flagged Then MasterSuit = .Hand(i).Suit
                            Else
                                If Not .Hand(i).Flagged Then MasterSuit = .Hand(i).Suit
                            End If
                        Next
                    Case 3
                        For i = 0 To 2
                            .Hand(i).Flagged = True
                        Next
                        MasterSuit = .Hand(0).Suit
                End Select

                Return MasterSuit
            End With
        End Function

        ''' <summary>
        ''' The player's score.
        ''' </summary>
        ''' <returns>The player's score.</returns>
        ''' <remarks></remarks>
        Public Function Score() As Byte
            Dim masterSuit As Byte = Me.MasterSuit

            Score = 0

            For i As Byte = 0 To 2
                If Me.Hand(i).Suit = masterSuit Then Score += Me.Hand(i).Value
            Next

            Return Score
        End Function
#End Region
    End Class
End Namespace