﻿using System;
using System.Net;
using System.Net.Sockets;
using Dragon.Interfaces;

namespace Dragon.Client
{
    public delegate void SocketAsyncEventHandler(object sender, SocketAsyncEventArgs e);
    public class Unity3DNetworkManager
    {
        private readonly Socket _socket;
        
        private bool _started;
        private SocketAsyncEventArgs _connectEventArgs;
        private SocketAsyncEventArgs _readEventArgs; 
        private SocketAsyncEventArgs _writeEventArgs;
        private readonly EndPoint _ipEndpoint;

        public bool OnLine { get; set; }
        
        public event EventHandler<SocketAsyncEventArgs> OnAfterMessageSend;
        public event EventHandler<SocketAsyncEventArgs> OnAfterMessageReceive;

        public Unity3DNetworkManager(string ipAddress, int port)
        {
            _socket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            _ipEndpoint = new IPEndPoint( IPAddress.Parse(ipAddress), port);
        }
        
        public void SendMessage(IGameMessage gameMessage)
        {
            byte[] byteArray = gameMessage.ToByteArray();

            Console.WriteLine("send {0} bytes.", byteArray.Length);

            byteArray.CopyTo(_writeEventArgs.Buffer, _writeEventArgs.Offset);
            
            if (!_socket.SendAsync(_writeEventArgs))
            {
                Send_Completed(_writeEventArgs);
            }
        }

        private void Send_Completed(SocketAsyncEventArgs writeEventArgs)
        {
            OnAfterMessageSend(this, writeEventArgs);
        }

        public void Start()
        {
            if (_started) return;

            Init();

            Connect();
        }

        private void Connect()
        {
            if (!_socket.ConnectAsync(_connectEventArgs))
            {
                Connect_Completed(this, _connectEventArgs);
            }
        }

        private void Init()
        {
            if (null == OnAfterMessageReceive)
            {
                throw new InvalidOperationException("OnAfterMessageReceive is not setted.");
            }
            _readEventArgs = new SocketAsyncEventArgs();
            _readEventArgs.Completed += OnAfterMessageReceive;
            _readEventArgs.Completed += Read_Completed;
            _readEventArgs.SetBuffer(new byte[1024], 0, 1024);
            

            if (null == OnAfterMessageSend)
            {
                throw new InvalidOperationException("OnAfterMessageSend is not setted.");
            }
            _writeEventArgs = new SocketAsyncEventArgs();
            _writeEventArgs.Completed += OnAfterMessageSend;
            _writeEventArgs.SetBuffer(new byte[1024], 0, 1024);

            //started
            _started = true;

            _connectEventArgs = new SocketAsyncEventArgs
            {
                RemoteEndPoint = _ipEndpoint
            };
            _connectEventArgs.Completed += Connect_Completed;
        }

        private void Read_Completed(object sender, SocketAsyncEventArgs e)
        {
            while (true)
            {
                if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
                {
                    if (!_socket.ReceiveAsync(e))
                    {
                        OnAfterMessageReceive(this, e);
                        continue;
                    }
                }
                else
                {
                    Console.WriteLine("read. socket error : {0}", e.SocketError);
                }
                break;
            }
        }

        private void Connect_Completed(object sender, SocketAsyncEventArgs e)
        {
            OnLine = true;

            if (e.SocketError == SocketError.Success)
            {
                _readEventArgs.UserToken = new QueueAsyncClientUserToken() ;
                _writeEventArgs.UserToken = new SimpleAsyncClientUserToken() ;

                Read_Completed(this, _readEventArgs);
            }
        }

        public void Reconnect()
        {
            _socket.Disconnect(true);
            Connect();
        }
    }
}