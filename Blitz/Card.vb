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
Imports System.Drawing

Namespace Objects
    Public Class Card

        ''' <summary>
        ''' Creates a new card at the given position.
        ''' </summary>
        ''' <param name="value">Location of the card in a zero-based deck. (0 - 51)</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal value As Byte)
            _position = value
        End Sub

#Region "Properties"
        Private Declare Function cdtInit Lib "cards.dll" (ByRef width As Integer, ByRef height As Integer) As Boolean
        Private Declare Function cdtDraw Lib "cards.dll" (ByVal hDC As IntPtr, ByVal x As Integer, ByVal y As Integer, ByVal Card As Integer, ByVal Type As Integer, ByVal clr As Integer) As Integer
        Private Declare Sub cdtTerm Lib "cards.dll" ()

        Private Shared LibraryInitialized As Boolean = False
        Public Shared CardWidth As Integer = 0
        Public Shared CardHeight As Integer = 0

        Private _flag As Boolean = False
        ''' <summary>
        ''' Gets or sets the card's flag.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Flag() As Boolean
            Get
                Return _flag
            End Get
            Set(ByVal value As Boolean)
                _flag = value
            End Set
        End Property

        Private _owner As Byte = 0
        ''' <summary>
        ''' Gets or sets the card's owner.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Owner() As Byte
            Get
                Return _owner
            End Get
            Set(ByVal value As Byte)
                _owner = value
            End Set
        End Property

        Private _position As Byte = Nothing
        ''' <summary>
        ''' Gets or sets the position of the card in a zero-based deck. (0 - 51)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Position() As Byte
            Get
                Return _position
            End Get
            Set(ByVal value As Byte)
                _position = value
            End Set
        End Property

        Private _status As Byte = 0
        ''' <summary>
        ''' Gets or sets the invert status of the card.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Status() As Byte
            Get
                Return _status
            End Get
            Set(ByVal value As Byte)
                _status = value
            End Set
        End Property

        Private _inUse As Boolean = False
        ''' <summary>
        ''' Gets or sets the status of the card in use.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property InUse() As Boolean
            Get
                Return _inUse
            End Get
            Set(ByVal value As Boolean)
                _inUse = value
            End Set
        End Property

        ''' <summary>
        ''' Contains the suit names.
        ''' </summary>
        ''' <remarks></remarks>
        Public Enum Suits As Byte
            Clubs = 0
            Diamonds = 1
            Hearts = 2
            Spades = 3
        End Enum

        ''' <summary>
        ''' Contains the face names.
        ''' </summary>
        ''' <remarks></remarks>
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
            king = 12
        End Enum
#End Region

#Region "Public Methods"
        ''' <summary>
        ''' Initializes the cards.dll library.
        ''' </summary>
        ''' <returns>True if the library was successfully initialized.</returns>
        ''' <remarks></remarks>
        Public Shared Function Initialize() As Boolean
            If LibraryInitialized Then Return True
            Try
                If cdtInit(CardWidth, CardHeight) Then
                    LibraryInitialized = True
                    Return True
                End If
            Catch ex As Exception
                MsgBox(ex.Message, MsgBoxStyle.Critical + MsgBoxStyle.OkOnly, "Card Library Error")
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Deinitializes the cards.dll library.
        ''' </summary>
        ''' <remarks></remarks>
        Public Shared Sub Deinitialize()
            If LibraryInitialized Then
                cdtTerm()
                LibraryInitialized = False
            End If
        End Sub

        ''' <summary>
        ''' Paints a card at the given X Y location.
        ''' </summary>
        ''' <param name="DrawingSurface"></param>
        ''' <param name="XLoc">X location of the card.</param>
        ''' <param name="YLoc">Y location of the card.</param>
        ''' <param name="Card">Card position in the deck.</param>
        ''' <param name="Type">Card face type.</param>
        ''' <remarks></remarks>
        Public Shared Sub PaintCard(ByVal DrawingSurface As Graphics, ByVal XLoc As Integer, ByVal YLoc As Integer, _
                                    ByVal Card As Byte, ByVal Type As Byte)
            Dim hDC As IntPtr = DrawingSurface.GetHdc

            cdtDraw(hDC, XLoc, YLoc, Card, Type, Color.White.ToArgb And 16777215)

            DrawingSurface.ReleaseHdc()
        End Sub

        ''' <summary>
        ''' Determines the suit value.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SuitVal() As Byte
            Select Case _position
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

        ''' <summary>
        ''' Determines the suit reference.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SuitRef() As String
            Select Case SuitVal()
                Case Suits.Clubs
                    Return "Clubs"
                Case Suits.Diamonds
                    Return "Diamonds"
                Case Suits.Hearts
                    Return "Hearts"
                Case Suits.Spades
                    Return "Spades"
                Case Else
                    Return Nothing
            End Select
        End Function

        ''' <summary>
        ''' Determines the face value.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function FaceVal() As Byte
            Return (_position - SuitVal()) / 4
        End Function

        ''' <summary>
        ''' Determines the face reference.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function FaceRef() As String
            Select Case FaceVal()
                Case 0 : Return "Ace"
                Case 1 : Return "Two"
                Case 2 : Return "Three"
                Case 3 : Return "Four"
                Case 4 : Return "Five"
                Case 5 : Return "Six"
                Case 6 : Return "Seven"
                Case 7 : Return "Eight"
                Case 8 : Return "Nine"
                Case 9 : Return "Ten"
                Case 10 : Return "Jack"
                Case 11 : Return "Queen"
                Case 12 : Return "King"
                Case Else : Return Nothing
            End Select
        End Function

        ''' <summary>
        ''' Determines the card value.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Value() As Byte
            If FaceVal() = Faces.Ace Then
                Return 11
            ElseIf FaceVal() = Faces.Jack Or FaceVal() = Faces.Queen Or FaceVal() = Faces.king Then
                Return 10
            Else
                Return FaceVal() + 1
            End If
        End Function
#End Region
    End Class
End Namespace