using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;

namespace Cog.Framework.Comm
{
    public class AsyncSocketAcceptEventArgs : EventArgs
    {
        private readonly Socket _socket;

        public AsyncSocketAcceptEventArgs(Socket conn)
        {
            _socket = conn;
        }

        public Socket Worker
        {
            get
            {
                return _socket;
            }
        }
    }
    public delegate void AsyncSocketAcceptEventHandler(object sender, AsyncSocketAcceptEventArgs e);
    public class AsyncSocketClass
    {
        protected int id;

        public event AsyncSocketErrorEventHandler OnError;

        public event AsyncSocketConnectEventHandler OnConnet;

        public event AsyncSocketCloseEventHandler OnClose;

        public event AsyncSocketSendEventHandler OnSend;

        public event AsyncSocketReceiveEventHandler OnReceive;

        public event AsyncSocketAcceptEventHandler OnAccept;

        public AsyncSocketClass()
        {
            this.id = -1;
        }

        public AsyncSocketClass(int id)
        {
            this.id = id;
        }

        public int ID
        {
            get
            {
                return this.id;
            }
        }

        protected virtual void ErrorOccured(AsyncSocketErrorEventArgs e)
        {
            AsyncSocketErrorEventHandler onError = this.OnError;
            if (onError == null)
                return;
            onError((object)this, e);
        }

        protected virtual void Connected(AsyncSocketConnectionEventArgs e)
        {
            AsyncSocketConnectEventHandler onConnet = this.OnConnet;
            if (onConnet == null)
                return;
            onConnet((object)this, e);
        }

        protected virtual void Closed(AsyncSocketConnectionEventArgs e)
        {
            AsyncSocketCloseEventHandler onClose = this.OnClose;
            if (onClose == null)
                return;
            onClose((object)this, e);
        }

        protected virtual void Sent(AsyncSocketSendEventArgs e)
        {
            AsyncSocketSendEventHandler onSend = this.OnSend;
            if (onSend == null)
                return;
            onSend((object)this, e);
        }

        protected virtual void Received(AsyncSocketReceiveEventArgs e)
        {
            AsyncSocketReceiveEventHandler onReceive = this.OnReceive;
            if (onReceive == null)
                return;
            onReceive((object)this, e);
        }

        protected virtual void Accepted(AsyncSocketAcceptEventArgs e)
        {
            AsyncSocketAcceptEventHandler onAccept = this.OnAccept;
            if (onAccept == null)
                return;
            onAccept((object)this, e);
        }
    }

    public class AsyncSocketClient : AsyncSocketClass
    {

        private Socket conn = (Socket)null;

        public AsyncSocketClient(int id)
        {
            this.id = id;
        }

        public AsyncSocketClient(int id, Socket conn)
        {
            this.id = id;
            this.conn = conn;
        }

        public Socket Connection
        {
            get
            {
                return this.conn;
            }
            set
            {
                this.conn = value;
            }
        }

        public bool Connect(string hostAddress, int port)
        {
            try
            {
                IPEndPoint ipEndPoint = new IPEndPoint(Dns.GetHostAddresses(hostAddress)[0], port);
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.BeginConnect((EndPoint)ipEndPoint, new AsyncCallback(this.OnConnectCallback), (object)socket);
            }
            catch (Exception ex)
            {
                this.ErrorOccured(new AsyncSocketErrorEventArgs(this.id, ex));
                return false;
            }
            return true;
        }

        private void OnConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket asyncState = (Socket)ar.AsyncState;
                asyncState.EndConnect(ar);
                this.conn = asyncState;
                this.Receive();
                this.Connected(new AsyncSocketConnectionEventArgs(this.id));
            }
            catch (Exception ex)
            {
                this.ErrorOccured(new AsyncSocketErrorEventArgs(this.id, ex));
            }
        }

        public void Receive()
        {
            try
            {
                StateObject stateObject = new StateObject(this.conn);
                stateObject.Worker.BeginReceive(stateObject.Buffer, 0, stateObject.BufferSize, SocketFlags.None, new AsyncCallback(this.OnReceiveCallBack), (object)stateObject);
            }
            catch (Exception ex)
            {
                this.ErrorOccured(new AsyncSocketErrorEventArgs(this.id, ex));
            }
        }

        private void OnReceiveCallBack(IAsyncResult ar)
        {
            try
            {
                sw.Stop();
                if (sw.ElapsedMilliseconds > 70)
                    Console.WriteLine("PLC Received : " + sw.ElapsedMilliseconds.ToString());
                StateObject asyncState = (StateObject)ar.AsyncState;
                int receiveBytes = asyncState.Worker.EndReceive(ar);
                AsyncSocketReceiveEventArgs e = new AsyncSocketReceiveEventArgs(this.id, receiveBytes, asyncState.Buffer);

                if (receiveBytes > 0)
                    this.Received(e);
                //else
                this.Receive();
            }
            catch (Exception ex)
            {
                this.ErrorOccured(new AsyncSocketErrorEventArgs(this.id, ex));
            }
        }

        Stopwatch sw = new Stopwatch();
        public bool Send(byte[] buffer)
        {
            try
            {
                sw.Restart();
                Socket conn = this.conn;
                conn.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(this.OnSendCallBack), (object)conn);
            }
            catch (Exception ex)
            {
                this.ErrorOccured(new AsyncSocketErrorEventArgs(this.id, ex));
                return false;
            }
            return true;
        }

        private void OnSendCallBack(IAsyncResult ar)
        {
            try
            {
                this.Sent(new AsyncSocketSendEventArgs(this.id, ((Socket)ar.AsyncState).EndSend(ar)));
            }
            catch (Exception ex)
            {
                this.ErrorOccured(new AsyncSocketErrorEventArgs(this.id, ex));
            }
        }

        public void Close()
        {
            try
            {
                Socket conn = this.conn;
                conn.Shutdown(SocketShutdown.Both);
                conn.BeginDisconnect(false, new AsyncCallback(this.OnCloseCallBack), (object)conn);
            }
            catch (Exception ex)
            {
                this.ErrorOccured(new AsyncSocketErrorEventArgs(this.id, ex));
            }
        }

        private void OnCloseCallBack(IAsyncResult ar)
        {
            try
            {
                Socket asyncState = (Socket)ar.AsyncState;
                asyncState.EndDisconnect(ar);
                asyncState.Close();
                Closed(new AsyncSocketConnectionEventArgs(this.id));
            }
            catch (Exception ex)
            {
                ErrorOccured(new AsyncSocketErrorEventArgs(this.id, ex));
            }
        }
    }

    public delegate void AsyncSocketCloseEventHandler(object sender, AsyncSocketConnectionEventArgs e);
    public delegate void AsyncSocketConnectEventHandler(object sender, AsyncSocketConnectionEventArgs e);
    public class AsyncSocketConnectionEventArgs : EventArgs
    {
        private readonly int id = 0;
        public AsyncSocketConnectionEventArgs(int id)
        {
            this.id = id;
        }
        public int ID
        {
            get
            {
                return this.id;
            }
        }
    }
    public class AsyncSocketErrorEventArgs : EventArgs
    {
        private readonly int id = 0;
        private readonly Exception exception;

        public AsyncSocketErrorEventArgs(int id, Exception exception)
        {
            this.id = id;
            this.exception = exception;
        }

        public Exception AsyncSocketException
        {
            get
            {
                return this.exception;
            }
        }

        public int ID
        {
            get
            {
                return this.id;
            }
        }
    }
    public delegate void AsyncSocketErrorEventHandler(object sender, AsyncSocketErrorEventArgs e);
    public class AsyncSocketReceiveEventArgs : EventArgs
    {
        private readonly int id = 0;
        private readonly int receiveBytes;
        private readonly byte[] receiveData;

        public AsyncSocketReceiveEventArgs(int id, int receiveBytes, byte[] receiveData)
        {
            this.id = id;
            this.receiveBytes = receiveBytes;
            this.receiveData = receiveData;
        }

        public int ReceiveBytes
        {
            get
            {
                return this.receiveBytes;
            }
        }

        public byte[] ReceiveData
        {
            get
            {
                return this.receiveData;
            }
        }

        public int ID
        {
            get
            {
                return this.id;
            }
        }
    }
    public delegate void AsyncSocketReceiveEventHandler(object sender, AsyncSocketReceiveEventArgs e);

    public class AsyncSocketSendEventArgs : EventArgs
    {
        private readonly int id = 0;
        private readonly int sendBytes;

        public AsyncSocketSendEventArgs(int id, int sendBytes)
        {
            this.id = id;
            this.sendBytes = sendBytes;
        }

        public int SendBytes
        {
            get
            {
                return this.sendBytes;
            }
        }

        public int ID
        {
            get
            {
                return this.id;
            }
        }
    }
    public delegate void AsyncSocketSendEventHandler(object sender, AsyncSocketSendEventArgs e);

    public class AsyncSocketServer : AsyncSocketClass
    {
        private const int backLog = 100;
        private int _port;
        private Socket _listener;

        public AsyncSocketServer(int port)
        {
            this._port = port;
        }

        public int Port
        {
            get
            {
                return this._port;
            }
        }

        public void Listen()
        {
            try
            {
                this._listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this._listener.Bind((EndPoint)new IPEndPoint(IPAddress.Any, this._port));
                this._listener.Listen(100);
                this.StartAccept();
            }
            catch (Exception ex)
            {
                this.ErrorOccured(new AsyncSocketErrorEventArgs(this.id, ex));
            }
        }

        private void StartAccept()
        {
            try
            {
                this._listener.BeginAccept(new AsyncCallback(this.OnListenCallBack), (object)this._listener);
            }
            catch (Exception ex)
            {
                this.ErrorOccured(new AsyncSocketErrorEventArgs(this.id, ex));
            }
        }

        private void OnListenCallBack(IAsyncResult ar)
        {
            try
            {
                this.Accepted(new AsyncSocketAcceptEventArgs(((Socket)ar.AsyncState).EndAccept(ar)));
                this.StartAccept();
            }
            catch (Exception ex)
            {
                this.ErrorOccured(new AsyncSocketErrorEventArgs(this.id, ex));
            }
        }

        public void Stop()
        {
            try
            {
                if (this._listener == null || !this._listener.IsBound)
                    return;
                this._listener.Close(100);
            }
            catch (Exception ex)
            {
                this.ErrorOccured(new AsyncSocketErrorEventArgs(this.id, ex));
            }
        }
    }

    public class StateObject
    {
        private const int BUFFER_SIZE = 327680;
        private Socket _worker;
        private byte[] _buffer;

        public StateObject(Socket worker)
        {
            this._worker = worker;
            this._buffer = new byte[327680];
        }

        public Socket Worker
        {
            get
            {
                return this._worker;
            }
            set
            {
                this._worker = value;
            }
        }

        public byte[] Buffer
        {
            get
            {
                return this._buffer;
            }
            set
            {
                this._buffer = value;
            }
        }

        public int BufferSize
        {
            get
            {
                return 327680;
            }
        }
    }
}
