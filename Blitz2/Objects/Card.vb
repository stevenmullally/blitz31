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
    Public Class Card

#Region "Fields"
        Private Shared _libraryInitialized As Boolean
        Public Shared Property LibraryInitialized() As Boolean
            Get
                Return _libraryInitialized
            End Get
            Set(ByVal value As Boolean)
                _libraryInitialized = value
            End Set
        End Property

        Private Shared _cardWidth As Integer
        Public Shared Property CardWidth() As Integer
            Get
                Return _cardWidth
            End Get
            Set(ByVal value As Integer)
                _cardWidth = value
            End Set
        End Property

        Private Shared _cardHeight As Integer
        Public Shared Property CardHeight() As Integer
            Get
                Return _cardHeight
            End Get
            Set(ByVal value As Integer)
                _cardHeight = value
            End Set
        End Property

        Private _position As Byte
        Public Property Position() As Byte
            Get
                Return _position
            End Get
            Set(ByVal value As Byte)
                _position = value
            End Set
        End Property

        Private _owner As Byte
        Public Property Owner() As Byte
            Get
                Return _owner
            End Get
            Set(ByVal value As Byte)
                _owner = value
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

        Private _inUse As Boolean
        Public Property InUse() As Boolean
            Get
                Return _inUse
            End Get
            Set(ByVal value As Boolean)
                _inUse = value
            End Set
        End Property

        Private _invert As Boolean
        Public Property Invert() As Boolean
            Get
                Return _invert
            End Get
            Set(ByVal value As Boolean)
                _invert = value
            End Set
        End Property

        Public Enum Suits As Byte
            Clubs = 0
            Diamonds = 1
            Hearts = 2
            Spades = 3
        End Enum

        Public Enum Faces As Byte
            Ace = 0
            Two = 1
            Three = 2
            Four = 3
            Five = 4
            Six = 5
            Seven = 6
            Eight = 7
            Nine = 8
            Ten = 9
            Jack = 10
            Queen = 11
            King = 12
        End Enum
#End Region

#Region "Methods"
        Public Sub New(ByVal position As Byte)
            Me.Position = position
        End Sub

        Public Shared Function Initialize() As Boolean
            If LibraryInitialized Then Return True

            Try
                If BlitzCards.InitCards Then
                    LibraryInitialized = True
                    CardHeight = BlitzCards.CardHeight
                    CardWidth = BlitzCards.CardWidth
                    Return True
                End If
            Catch ex As Exception
                MsgBox(ex.Message, MsgBoxStyle.Critical + MsgBoxStyle.OkOnly, "Card Library Error")
                Return False
            End Try
        End Function

        Public Shared Sub PaintCard(ByVal drawingSurface As Graphics, ByVal location As Point, ByVal card As Byte, ByVal invert As Boolean)
            BlitzCards.DrawCard(drawingSurface, location.X, location.Y, card, invert)
        End Sub

        Public Function Value() As Byte
            If FaceValue() = Faces.Ace Then
                Return 11
            ElseIf FaceValue() = Faces.Jack Or FaceValue() = Faces.Queen Or FaceValue() = Faces.King Then
                Return 10
            Else
                Return FaceValue() + 1
            End If
        End Function

        Private Function FaceValue() As Byte
            Return (Position - SuitVal()) / 4
        End Function

        Public Function SuitVal() As Byte
            Select Case Position
                Case 0, 4, 8, 12, 16, 20, 24, 28, 32, 36, 40, 44, 48
                    SuitVal = Suits.Clubs
                Case 1, 5, 9, 13, 17, 21, 25, 29, 33, 37, 41, 45, 49
                    SuitVal = Suits.Diamonds
                Case 2, 6, 10, 14, 18, 22, 26, 30, 34, 38, 42, 46, 50
                    SuitVal = Suits.Hearts
                Case 3, 7, 11, 15, 19, 23, 27, 31, 35, 39, 43, 47, 51
                    SuitVal = Suits.Spades
            End Select
        End Function
#End Region
    End Class
End Namespace