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

#Region "Fields"
        Private _name As String
        Public Property Name() As String
            Get
                Return _name
            End Get
            Set(ByVal value As String)
                _name = value
            End Set
        End Property

        Private _mode As Modes
        Public Property Mode() As Modes
            Get
                Return _mode
            End Get
            Set(ByVal value As Modes)
                _mode = value
            End Set
        End Property

        Private _hand(3) As Card
        Public Property Hand() As Card()
            Get
                Return _hand
            End Get
            Set(ByVal value As Card())
                _hand = value
            End Set
        End Property

        Private _handLocation As Point
        Public Property HandLocation() As Point
            Get
                Return _handLocation
            End Get
            Set(ByVal value As Point)
                _handLocation = value
            End Set
        End Property

        Private _handLocationMid As Point
        Public Property HandLocationMid() As Point
            Get
                Return _handLocationMid
            End Get
            Set(ByVal value As Point)
                _handLocationMid = value
            End Set
        End Property

        Private _totalCards As Byte
        Public Property TotalCards()
            Get
                Return _totalCards
            End Get
            Set(ByVal value)
                _totalCards = value
            End Set
        End Property

        Private _tokens As Byte
        Public Property Tokens() As Byte
            Get
                Return _tokens
            End Get
            Set(ByVal value As Byte)
                _tokens = value
            End Set
        End Property

        Private _inGame As Boolean
        Public Property InGame() As Boolean
            Get
                Return _inGame
            End Get
            Set(ByVal value As Boolean)
                _inGame = value
            End Set
        End Property

        Private _flagged As Boolean
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
        Public Sub New(ByVal mode As Modes)
            Me.Mode = mode

            CreateNewHand()
        End Sub

        Public Sub CreateNewHand()
            For i As Byte = 0 To 3
                Hand(i) = New Card(Nothing)
            Next

            TotalCards = 0
        End Sub

        Public Sub AddCard(ByVal card As Byte)
            If TotalCards = 4 Then Exit Sub

            Hand(FreeCard).Position = card
            Hand(FreeCard).InUse = True
            TotalCards += 1
        End Sub

        Private Function FreeCard() As Byte
            For FreeCard = 0 To UBound(Hand)
                If Not Hand(FreeCard).InUse Then Return FreeCard
            Next
        End Function
#End Region
    End Class
End Namespace