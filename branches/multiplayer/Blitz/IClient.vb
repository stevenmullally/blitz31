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
Imports System.Net
Imports System.Net.Sockets
Imports System.ComponentModel

Namespace Networking
    Public Class IClient

        Public Sub New(ByVal Sender As ISynchronizeInvoke, ByVal BufferSize As Integer)
            ReDim _recvBuffer(BufferSize - 1)
            _syncObj = Sender
        End Sub

#Region "Properties"
        Private _syncObj As ISynchronizeInvoke
        Private _clientSocket As Socket
        Private _recvBuffer() As Byte

        Public ReadOnly Property IsConnected() As Boolean
            Get
                Dim SocketConnected As Boolean

                Try
                    SocketConnected = Not (_clientSocket.Poll(1, SelectMode.SelectRead) And (_clientSocket.Available = 0))
                Catch
                End Try
                Return SocketConnected
            End Get
        End Property

        Private Delegate Sub SimpleCallback()
        Private Delegate Sub ComplexCallback(ByVal data As String)
#End Region

#Region "Events"
        Public Event OnSocketConnected()
        Public Event OnSocketDisconnected()
        Public Event OnDataReceived(ByVal data As String)
#End Region

#Region "Public Methods"
        Public Function OpenConnection(ByVal ServerAddress As String, ByVal ServerPort As Integer)
            Try
                CloseConnection()

                _clientSocket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                _clientSocket.BeginConnect(ServerAddress, ServerPort, AddressOf _doConnection, Nothing)
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function

        Public Sub CloseConnection()
            Try
                If IsConnected Then
                    _clientSocket.Shutdown(SocketShutdown.Both)
                    _clientSocket.Close()
                End If
                _callSocketDisconnected()
            Catch ex As Exception
            End Try
        End Sub
#End Region

#Region "Private Methods"
        Private Sub _doConnection(ByVal ar As IAsyncResult)
            Try
                _clientSocket.EndConnect(ar)
                _callSocketConnected()
                _clientSocket.BeginReceive(_recvBuffer, 0, _recvBuffer.Length, SocketFlags.None, AddressOf _doReceiveData, Nothing)
            Catch ex As Exception
                _callSocketDisconnected()
                MsgBox(ex.Message, MsgBoxStyle.Critical + MsgBoxStyle.OkOnly, "IClient Object Error")
            End Try
        End Sub

        Private Sub _doReceiveData(ByVal ar As IAsyncResult)
            Dim numBytes As Integer

            Try
                SyncLock _clientSocket
                    numBytes = _clientSocket.EndReceive(ar)
                End SyncLock
            Catch ex As Exception
                _callSocketDisconnected()
                MsgBox(ex.Message, MsgBoxStyle.Critical + MsgBoxStyle.OkOnly, "IClient Object Error")
                Exit Sub
            End Try

            If numBytes = 0 Then
                _callSocketDisconnected()
                Exit Sub
            End If

            Dim data As String = System.Text.ASCIIEncoding.ASCII.GetString(_recvBuffer, 0, numBytes)
            _callDataReceived(data)

            _clientSocket.BeginReceive(_recvBuffer, 0, _recvBuffer.Length, SocketFlags.None, AddressOf _doReceiveData, Nothing)
        End Sub

        Private Sub _callSocketConnected()
            Dim cb As New SimpleCallback(AddressOf _socketConnected)
            _syncObj.Invoke(cb, Nothing)
        End Sub

        Private Sub _socketConnected()
            RaiseEvent OnSocketConnected()
        End Sub

        Private Sub _callSocketDisconnected()
            Dim cb As New SimpleCallback(AddressOf _socketDisconnected)
            _syncObj.Invoke(cb, Nothing)
        End Sub

        Private Sub _socketDisconnected()
            RaiseEvent OnSocketDisconnected()
        End Sub

        Private Sub _callDataReceived(ByVal data As String)
            Dim cb As New ComplexCallback(AddressOf _dataReceived)
            Dim args() As Object = {data}

            _syncObj.Invoke(cb, args)
        End Sub

        Private Sub _dataReceived(ByVal data As String)
            RaiseEvent OnDataReceived(data)
        End Sub
#End Region
    End Class
End Namespace