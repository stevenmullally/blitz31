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
        ''' <summary>
        ''' Initializes a client connection.
        ''' </summary>
        ''' <param name="sender">Sending form object.</param>
        ''' <param name="bufferSize">Buffersize for socket connection.</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal sender As ISynchronizeInvoke, ByVal bufferSize As Integer)
            ReDim _recvBuffer(bufferSize - 1)
            _syncObj = sender
        End Sub

#Region "Properties"
        ''' <summary>
        ''' Synchronizing objects used for invoking callbacks.
        ''' </summary>
        ''' <remarks></remarks>
        Private _syncObj As ISynchronizeInvoke

        ''' <summary>
        ''' A socket connection.
        ''' </summary>
        ''' <remarks></remarks>
        Private _clientSocket As Socket

        ''' <summary>
        ''' Receive buffer.
        ''' </summary>
        ''' <remarks></remarks>
        Private _recvBuffer() As Byte

        ''' <summary>
        ''' Gets the connection status of the socket.
        ''' </summary>
        ''' <value></value>
        ''' <returns>True if client is connected, otherwise false.</returns>
        ''' <remarks></remarks>
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

        ''' <summary>
        ''' Generic callback with no parameters.
        ''' </summary>
        ''' <remarks></remarks>
        Private Delegate Sub SimpleCallback()

        ''' <summary>
        ''' Generic callback with a single string parameter.
        ''' </summary>
        ''' <param name="data"></param>
        ''' <remarks></remarks>
        Private Delegate Sub ComplexCallback(ByVal data As String)
#End Region

#Region "Events"
        ''' <summary>
        ''' Raised when a socket is connected.
        ''' </summary>
        ''' <remarks></remarks>
        Public Event OnSocketConnected()

        ''' <summary>
        ''' Raised when a socket is disconnected.
        ''' </summary>
        ''' <remarks></remarks>
        Public Event OnSocketDisconnected()

        ''' <summary>
        ''' Raised when data is received.
        ''' </summary>
        ''' <param name="data"></param>
        ''' <remarks></remarks>
        Public Event OnDataReceived(ByVal data As String)
#End Region

#Region "Public Methods"
        ''' <summary>
        ''' Opens a socket connection to the given server address and port.
        ''' </summary>
        ''' <param name="serverAddress"></param>
        ''' <param name="serverPort"></param>
        ''' <returns>True if connection was successful, otherwise false.</returns>
        ''' <remarks></remarks>
        Public Function OpenConnection(ByVal serverAddress As String, ByVal serverPort As Integer)
            Try
                CloseConnection()

                _clientSocket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                _clientSocket.BeginConnect(serverAddress, serverPort, AddressOf _doConnection, Nothing)
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Closes the socket connection.
        ''' </summary>
        ''' <remarks></remarks>
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
        ''' <summary>
        ''' Connection asyncresult.
        ''' </summary>
        ''' <param name="ar"></param>
        ''' <remarks></remarks>
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

        ''' <summary>
        ''' Receive data asyncresult.
        ''' </summary>
        ''' <param name="ar"></param>
        ''' <remarks></remarks>
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

        ''' <summary>
        ''' SocketConnected delegate.
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub _callSocketConnected()
            Dim cb As New SimpleCallback(AddressOf _socketConnected)
            _syncObj.Invoke(cb, Nothing)
        End Sub

        ''' <summary>
        ''' Actual socketConnected event caller.
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub _socketConnected()
            RaiseEvent OnSocketConnected()
        End Sub

        ''' <summary>
        ''' SocketDisconnected delegate.
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub _callSocketDisconnected()
            Dim cb As New SimpleCallback(AddressOf _socketDisconnected)
            _syncObj.Invoke(cb, Nothing)
        End Sub

        ''' <summary>
        ''' Actual socketDisconnected event caller.
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub _socketDisconnected()
            RaiseEvent OnSocketDisconnected()
        End Sub

        ''' <summary>
        ''' DataReceived delegate.
        ''' </summary>
        ''' <param name="data"></param>
        ''' <remarks></remarks>
        Private Sub _callDataReceived(ByVal data As String)
            Dim cb As New ComplexCallback(AddressOf _dataReceived)
            Dim args() As Object = {data}

            _syncObj.Invoke(cb, args)
        End Sub

        ''' <summary>
        ''' Actual dataReceived event caller.
        ''' </summary>
        ''' <param name="data"></param>
        ''' <remarks></remarks>
        Private Sub _dataReceived(ByVal data As String)
            RaiseEvent OnDataReceived(data)
        End Sub
#End Region
    End Class
End Namespace