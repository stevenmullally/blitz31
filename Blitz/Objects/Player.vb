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

Namespace Objects
    Public Class Player

        ''' <summary>
        ''' Contructs a new player.
        ''' </summary>
        ''' <param name="Mode">Player mode.</param>
        ''' <remarks>A player is created using the given mode.
        ''' The new player is given default properties and a reset is called.</remarks>
        Public Sub New(ByVal Mode As Modes)
            _mode = Mode
            _tokens = 4

            ResetPlayer()
        End Sub

#Region "Properties"
        ''' <summary>
        ''' The player's card array.
        ''' </summary>
        ''' <remarks></remarks>
        Private _hand(3) As Card

        Private _name As String
        ''' <summary>
        ''' Gets or sets the name of the player.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Name() As String
            Get
                Return _name
            End Get
            Set(ByVal value As String)
                _name = value
            End Set
        End Property

        Private _x As Integer
        ''' <summary>
        ''' Gets or sets the X location of the player hand.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property X() As Integer
            Get
                Return _x
            End Get
            Set(ByVal value As Integer)
                _x = value
            End Set
        End Property

        Private _y As Integer
        ''' <summary>
        ''' Gets or sets the Y location of the player hand.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Y() As Integer
            Get
                Return _y
            End Get
            Set(ByVal value As Integer)
                _y = value
            End Set
        End Property

        Private _midX As Integer
        ''' <summary>
        ''' Gets or sets the midpoint of the player's X location.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property MidX() As Integer
            Get
                Return _midX
            End Get
            Set(ByVal value As Integer)
                _midX = value
            End Set
        End Property

        Private _midY As Integer
        ''' <summary>
        ''' Gets or sets the midpoint of the player's Y location.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property MidY() As Integer
            Get
                Return _midY
            End Get
            Set(ByVal value As Integer)
                _midY = value
            End Set
        End Property

        Private _mode As Byte
        ''' <summary>
        ''' Gets or sets the player's mode.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>The mode determines how the game perceives the player, ie computer or human.</remarks>
        Public Property Mode() As Byte
            Get
                Return _mode
            End Get
            Set(ByVal value As Byte)
                _mode = value
            End Set
        End Property

        Private _score As Byte
        ''' <summary>
        ''' Gets or sets the player's score.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>Marked for removal. Reevaluate the need for this property.</remarks>
        Public Property Score() As Byte
            Get
                Return _score
            End Get
            Set(ByVal value As Byte)
                _score = value
            End Set
        End Property

        Private _flag As Boolean
        ''' <summary>
        ''' Gets or sets the player's flag.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>Used in determing if a player should lose a token.</remarks>
        Public Property Flag() As Boolean
            Get
                Return _flag
            End Get
            Set(ByVal value As Boolean)
                _flag = value
            End Set
        End Property

        Private _totalCards As Byte
        ''' <summary>
        ''' Gets or sets the total card count for the player.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property TotalCards() As Byte
            Get
                Return _totalCards
            End Get
            Set(ByVal value As Byte)
                _totalCards = value
            End Set
        End Property

        Private _tokens As Integer
        ''' <summary>
        ''' Gets or sets the player's token count.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>Marked for adjustment. Might be able to use ReadOnly attribute.</remarks>
        Public Property Tokens() As Integer
            Get
                Return _tokens
            End Get
            Set(ByVal value As Integer)
                _tokens = value
            End Set
        End Property

        ''' <summary>
        ''' Contains the list of possible player modes.
        ''' </summary>
        ''' <remarks>Online will be used if/when multiplayer is released.</remarks>
        Public Enum Modes As Byte
            Computer = 0
            Human = 1
            Online = 2
        End Enum
#End Region

#Region "Public Methods"
        ''' <summary>
        ''' Resets the player's stats for a new game.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub ResetPlayer()
            For i As Byte = 0 To UBound(_hand)
                _hand(i) = New Card(Nothing)
            Next i

            _totalCards = 0
            _score = 0
            _flag = False
        End Sub

        ''' <summary>
        ''' Returns the flag status a card in the player's hand.
        ''' </summary>
        ''' <param name="Card"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetFlag(ByVal Card As Byte) As Boolean
            Return _hand(Card).Flag
        End Function

        ''' <summary>
        ''' Sets the status of a card in the player's hand.
        ''' </summary>
        ''' <param name="Card"></param>
        ''' <param name="Status"></param>
        ''' <remarks></remarks>
        Public Sub SetFlag(ByVal Card As Byte, ByVal Status As Boolean)
            _hand(Card).Flag = Status
        End Sub

        ''' <summary>
        ''' Get the value of a card in the player's hand.
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetCardVal(ByVal value As Byte) As Byte
            Return _hand(value).Value
        End Function

        ''' <summary>
        ''' Returns the position of a card in the player's hand relative to the deck.
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetCardPos(ByVal value As Byte) As Byte
            Return _hand(value).Position
        End Function

        ''' <summary>
        ''' Returns the suit value of a card in the player's hand.
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetCardSuit(ByVal value As Byte) As Byte
            Return _hand(value).SuitVal
        End Function

        ''' <summary>
        ''' Returns the face value of a card in the player's hand.
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetCardFace(ByVal value As Byte) As Byte
            Return _hand(value).FaceVal
        End Function

        ''' <summary>
        ''' Adds a card to the player's hand with the given position in the deck.
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddCard(ByVal value As Byte) As Boolean
            If _totalCards = 4 Then Return False

            With _hand(_freeCard)
                .Position = value
                .InUse = True
            End With

            _totalCards += 1
            Return True
        End Function

        ''' <summary>
        ''' Remove a card from the player's hand based off of its position relative to the deck.
        ''' </summary>
        ''' <param name="value"></param>
        ''' <remarks></remarks>
        Public Sub RemoveCard(ByVal value As Byte)
            Dim x As Byte

            For x = 0 To UBound(_hand)
                If _hand(x).Position = value Then
                    _hand(x) = New Card(Nothing)
                    _totalCards -= 1
                    Exit For
                End If
            Next x

            If _hand(3).InUse = True Then
                For x = 0 To 2
                    If _hand(x).InUse = False Then
                        _hand(x).Position = _hand(3).Position
                        _hand(x).InUse = True

                        _hand(3) = New Card(Nothing)
                    End If
                Next x
            End If
        End Sub

        ''' <summary>
        ''' Removes tokens from the player based on a given value.
        ''' </summary>
        ''' <param name="value"></param>
        ''' <remarks></remarks>
        Public Sub RemoveToken(ByVal value As Byte)
            _tokens -= value
        End Sub

        ''' <summary>
        ''' Returns the player's status in the game.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function InGame() As Boolean
            If _tokens >= 0 Then
                Return True
            Else
                Return False
            End If
        End Function
#End Region

#Region "Private Methods"
        ''' <summary>
        ''' Returns the next free card slot in the player's hand.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function _freeCard() As Byte
            For i As Byte = 0 To UBound(_hand)
                If Not _hand(i).InUse Then Return i
            Next
        End Function
#End Region
    End Class
End Namespace