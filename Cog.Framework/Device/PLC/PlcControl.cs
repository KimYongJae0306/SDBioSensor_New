using Cog.Framework.Comm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cog.Framework.Device.PLC
{
    public class PlcControl
    {
        #region 필드
        private string _logPath = "D:\\ePLCLog\\";
        #endregion

        #region 속성
        private ManualResetEvent _plcSendResetEvent = new ManualResetEvent(true);
        #endregion

        #region 이벤트
        #endregion

        #region 델리게이트
        #endregion

        #region 생성자
        #endregion

        #region 메서드
        #endregion

        private string IP = "";

        private int Port = 5002;

        private int NetworkNO = 0;

        private int PLCStationNO = 255;

        private int PCStationNO = 00;

        public int TimeOut = 3000;

        private byte[] ReceivedData = new byte[0];

        private byte[] SendBytes_BlockRead = new byte[21]   {
                                                            (byte) 80,
                                                            (byte) 0,
                                                            (byte) 1,
                                                            (byte) 1,
                                                            byte.MaxValue,
                                                            (byte) 3,
                                                            (byte) 2,
                                                            (byte) 12,
                                                            (byte) 0,
                                                            (byte) 16,
                                                            (byte) 0,
                                                            (byte) 1,
                                                            (byte) 20,
                                                            (byte) 0,
                                                            (byte) 0,
                                                            (byte) 0,
                                                            (byte) 0,
                                                            (byte) 0,
                                                            (byte) 168,
                                                            (byte) 192,
                                                            (byte) 3
                                                            };

        private byte[] SendBytes_BlockWrite = new byte[21] {
                                                            (byte) 80,
                                                            (byte) 0,
                                                            (byte) 1,
                                                            (byte) 1,
                                                            byte.MaxValue,
                                                            (byte) 3,
                                                            (byte) 2,
                                                            (byte) 12,
                                                            (byte) 0,
                                                            (byte) 16,
                                                            (byte) 0,
                                                            (byte) 1,
                                                            (byte) 20,
                                                            (byte) 0,
                                                            (byte) 0,
                                                            (byte) 0,
                                                            (byte) 0,
                                                            (byte) 0,
                                                            (byte) 168,
                                                            (byte) 192,
                                                            (byte) 3
                                                            };
        private byte[] SendBytes_RandomRead = new byte[21]  {
                                                            (byte) 80,
                                                            (byte) 0,
                                                            (byte) 1,
                                                            (byte) 1,
                                                            byte.MaxValue,
                                                            (byte) 3,
                                                            (byte) 2,
                                                            (byte) 12,
                                                            (byte) 0,
                                                            (byte) 16,
                                                            (byte) 0,
                                                            (byte) 3,
                                                            (byte) 4,
                                                            (byte) 0,
                                                            (byte) 0,
                                                            (byte) 0,
                                                            (byte) 0,
                                                            (byte) 0,
                                                            (byte) 0,
                                                            (byte) 0,
                                                            (byte) 168
                                                            };
        private byte[] SendBytes_RandomWrite = new byte[23] {
                                                            (byte) 80,
                                                            (byte) 0,
                                                            (byte) 1,
                                                            (byte) 1,
                                                            byte.MaxValue,
                                                            (byte) 3,
                                                            (byte) 2,
                                                            (byte) 16,
                                                            (byte) 0,
                                                            (byte) 16,
                                                            (byte) 0,
                                                            (byte) 2,
                                                            (byte) 20,
                                                            (byte) 0,
                                                            (byte) 0,
                                                            (byte) 0,
                                                            (byte) 0,
                                                            (byte) 0,
                                                            (byte) 0,
                                                            (byte) 0,
                                                            (byte) 168,
                                                            (byte) 0,
                                                            (byte) 0
                                                            };
        private byte[] SendBytes_MultiBlockRead = new byte[23]  {
                                                                (byte) 80,
                                                                (byte) 0,
                                                                (byte) 1,
                                                                (byte) 1,
                                                                byte.MaxValue,
                                                                (byte) 3,
                                                                (byte) 2,
                                                                (byte) 14,
                                                                (byte) 0,
                                                                (byte) 16,
                                                                (byte) 0,
                                                                (byte) 6,
                                                                (byte) 4,
                                                                (byte) 0,
                                                                (byte) 0,
                                                                (byte) 0,
                                                                (byte) 0,
                                                                (byte) 0,
                                                                (byte) 0,
                                                                (byte) 0,
                                                                (byte) 168,
                                                                (byte) 1,
                                                                (byte) 0
                                                                };

        private byte[] SendBytes_MultiBlockWrite = new byte[25] {
                                                                (byte) 80,
                                                                (byte) 0,
                                                                (byte) 1,
                                                                (byte) 1,
                                                                byte.MaxValue,
                                                                (byte) 3,
                                                                (byte) 2,
                                                                (byte) 16,
                                                                (byte) 0,
                                                                (byte) 16,
                                                                (byte) 0,
                                                                (byte) 6,
                                                                (byte) 20,
                                                                (byte) 0,
                                                                (byte) 0,
                                                                (byte) 1,
                                                                (byte) 0,
                                                                (byte) 0,
                                                                (byte) 0,
                                                                (byte) 0,
                                                                (byte) 168,
                                                                (byte) 1,
                                                                (byte) 0,
                                                                (byte) 0,
                                                                (byte) 0
                                                                };


        private static int _id;

        private int _socketId;

        private AsyncSocketClient _client;

        private bool _isConnected;

        private bool _isReceived;

        #region 생성자
        public PlcControl()
        {
            this._socketId = _id;
            ++_id;
            Directory.CreateDirectory(this._logPath);
        }
        #endregion

        #region 메서드
        public void WriteLogFile(string sData)
        {
            StreamWriter streamWriter = new StreamWriter(this._logPath + DateTime.Now.ToString("yyyyMMddHH") + ".txt", true);
            streamWriter.WriteLine(DateTime.Now.ToString() + "    " + sData);
            streamWriter.Close();
        }

        public void SetPLCProperties(string ip, int port, int timeOut)
        {
            this.IP = ip;
            this.Port = port;
            this.TimeOut = timeOut;
            this.SendBytes_BlockRead[2] = (byte)this.NetworkNO;
            this.SendBytes_BlockRead[3] = (byte)this.PLCStationNO;
            this.SendBytes_BlockRead[6] = (byte)this.PCStationNO;
            this.SendBytes_BlockWrite[2] = (byte)this.NetworkNO;
            this.SendBytes_BlockWrite[3] = (byte)this.PLCStationNO;
            this.SendBytes_BlockWrite[6] = (byte)this.PCStationNO;
            this.SendBytes_RandomRead[2] = (byte)this.NetworkNO;
            this.SendBytes_RandomRead[3] = (byte)this.PLCStationNO;
            this.SendBytes_RandomRead[6] = (byte)this.PCStationNO;
            this.SendBytes_RandomWrite[2] = (byte)this.NetworkNO;
            this.SendBytes_RandomWrite[3] = (byte)this.PLCStationNO;
            this.SendBytes_RandomWrite[6] = (byte)this.PCStationNO;
            this.SendBytes_MultiBlockRead[2] = (byte)this.NetworkNO;
            this.SendBytes_MultiBlockRead[3] = (byte)this.PLCStationNO;
            this.SendBytes_MultiBlockRead[6] = (byte)this.PCStationNO;
            this.SendBytes_MultiBlockWrite[2] = (byte)this.NetworkNO;
            this.SendBytes_MultiBlockWrite[3] = (byte)this.PLCStationNO;
            this.SendBytes_MultiBlockWrite[6] = (byte)this.PCStationNO;
        }

        public int[] ReadDeviceBlock(SubCommand subCommand, DeviceName deviceName, string _StartAddress, int _Length)
        {
            if (IsConnected() == false)
            {
                this.WriteLogFile("Read DisConnect Port:" + Port.ToString());
                this._client.Close();

                if (Open() == 0)
                    this.WriteLogFile("ReConnect OK");
                else
                    this.WriteLogFile("ReConnect NG");
            }

            int[] array = new int[0];

            if (_isConnected == false)
                return array;

            if (subCommand == SubCommand.Bit)
            {
                if (deviceName == DeviceName.D || deviceName == DeviceName.R || (deviceName == DeviceName.W || deviceName == DeviceName.ZR))
                    throw new Exception("Word Device는 Bit단위 블락으로 읽을 수 없습니다.\r\n _Unit 또는 _DeviceName을 바꿔주시기 바랍니다.");
                if (_Length % 2 == 1)
                    throw new Exception("Bit Device의_Length는 항상짝수여야 합니다");
            }
            if (subCommand == SubCommand.Bit)
            {
                this.SendBytes_BlockRead[13] = (byte)1;
                this.SendBytes_BlockRead[14] = (byte)0;
            }
            else if (subCommand == SubCommand.Word)
            {
                this.SendBytes_BlockRead[13] = (byte)0;
                this.SendBytes_BlockRead[14] = (byte)0;
            }
            this.SendBytes_BlockRead[7] = (byte)12;
            this.SendBytes_BlockRead[8] = (byte)0;
            this.SendBytes_BlockRead[11] = (byte)1;
            this.SendBytes_BlockRead[12] = (byte)4;
            this.SendBytes_BlockRead[18] = (byte)deviceName;
            int num1 = 0;
            if (subCommand == SubCommand.Word)
            {
                num1 = _Length / 960 + 1;
                if (_Length % 960 == 0)
                    --num1;
            }
            else if (subCommand == SubCommand.Bit)
            {
                num1 = _Length / 7168 + 1;
                if (_Length % 7168 == 0)
                    --num1;
            }
            int num2 = num1;
            int index1 = 0;
            for (int index2 = 0; index2 < num2; ++index2)
            {
                switch (deviceName)
                {
                    case DeviceName.M:
                    case DeviceName.L:
                    case DeviceName.D:
                    case DeviceName.R:
                    case DeviceName.ZR:
                        int num3 = 0;
                        switch (subCommand)
                        {
                            case SubCommand.Bit:
                                num3 = Convert.ToInt32(_StartAddress) + index2 * 7168;
                                break;

                            case SubCommand.Word:
                                num3 = deviceName == DeviceName.L || deviceName == DeviceName.M ? Convert.ToInt32(_StartAddress) + index2 * 15360 : Convert.ToInt32(_StartAddress) + index2 * 960;
                                break;
                        }
                        this.SendBytes_BlockRead[15] = (byte)(num3 % 256);
                        this.SendBytes_BlockRead[16] = (byte)(num3 / 256);
                        this.SendBytes_BlockRead[17] = (byte)(num3 / 65536);
                        break;
                    case DeviceName.X:
                    case DeviceName.Y:
                    case DeviceName.B:
                    case DeviceName.W:
                        int num4 = 0;
                        switch (subCommand)
                        {
                            case SubCommand.Bit:
                                num4 = int.Parse(_StartAddress, NumberStyles.HexNumber) + index2 * 7168;
                                break;

                            case SubCommand.Word:
                                num4 = deviceName != DeviceName.W ? int.Parse(_StartAddress, NumberStyles.HexNumber) + index2 * 15360 : int.Parse(_StartAddress, NumberStyles.HexNumber) + index2 * 960;
                                break;
                        }
                        this.SendBytes_BlockRead[15] = (byte)(num4 % 256);
                        this.SendBytes_BlockRead[16] = (byte)(num4 / 256);
                        this.SendBytes_BlockRead[17] = (byte)(num4 / 65536);
                        break;
                }
                if (num1 == 1)
                {
                    switch (subCommand)
                    {
                        case SubCommand.Bit:
                            this.SendBytes_BlockRead[19] = (byte)((_Length - 7168 * index2) % 256);
                            this.SendBytes_BlockRead[20] = (byte)((_Length - 7168 * index2) / 256);
                            break;

                        case SubCommand.Word:
                            this.SendBytes_BlockRead[19] = (byte)((_Length - 960 * index2) % 256);
                            this.SendBytes_BlockRead[20] = (byte)((_Length - 960 * index2) / 256);
                            break;
                    }
                }
                else
                {
                    switch (subCommand)
                    {
                        case SubCommand.Bit:
                            this.SendBytes_BlockRead[19] = (byte)0;
                            this.SendBytes_BlockRead[20] = (byte)28;
                            break;

                        case SubCommand.Word:
                            this.SendBytes_BlockRead[19] = (byte)192;
                            this.SendBytes_BlockRead[20] = (byte)3;
                            break;
                    }
                }
                if (this._client == null || this._client.Connection == null || !this._client.Connection.Connected)
                    return array;

                _plcSendResetEvent.Reset();
                this._isReceived = false;

                Stopwatch receivedSW = new Stopwatch();
                receivedSW.Restart();

                this._client.Send(this.SendBytes_BlockRead);

                int retryCount = 3;
                int timeOut = 100;

                while (_isReceived == false)
                {
                    if (receivedSW.ElapsedMilliseconds > timeOut)
                    {
                        Console.WriteLine("여기??");

                        if (retryCount >= 0)
                        {
                            WriteLogFile("PLC Read TimeOut Retry Send:" + timeOut.ToString());
                            _client.Send(SendBytes_BlockRead);
                            retryCount--;
                        }
                        else
                            this.WriteLogFile("PLC Read TimeOut LimitTime:" + timeOut.ToString());
                    }
                    Thread.Sleep(50);
                }

                switch (subCommand)
                {
                    case SubCommand.Bit:
                        int newSize1 = array.Length + this.ReceivedData.Length * 2;
                        if (num1 == 1 && _Length % 2 == 1)
                            --newSize1;
                        int length = array.Length;
                        Array.Resize<int>(ref array, newSize1);
                        for (int index3 = 0; index3 < this.ReceivedData.Length; ++index3)
                        {
                            array[length] = (int)this.ReceivedData[index3] / 16;
                            ++length;
                            if (array.Length != length)
                            {
                                array[length] = (int)this.ReceivedData[index3] % 16;
                                ++length;
                            }
                        }
                        break;
                    case SubCommand.Word:
                        int newSize2 = array.Length + this.ReceivedData.Length / 2;
                        Array.Resize<int>(ref array, newSize2);
                        int num5 = this.ReceivedData.Length / 2;
                        for (int index3 = 0; index3 < num5; ++index3)
                        {
                            array[index1] = (int)this.ReceivedData[index3 * 2 + 1] * 256 + (int)this.ReceivedData[index3 * 2];
                            ++index1;
                        }
                        break;
                }

                --num1;
            }

            return array;
        }

        public int WriteDeviceBlock(SubCommand _Unit, DeviceName _DeviceName, string _StartAddress, int[] _Data)
        {
            if (IsConnected() == false)
            {
                this.WriteLogFile("Write DisConnect Port:" + Port.ToString());
                this._client.Close();

                if (Open() == 0)
                    this.WriteLogFile("ReConnect OK");
                else
                    this.WriteLogFile("ReConnect NG");
            }

            if (_isConnected == false)
                return 1;

            if (_Unit == SubCommand.Bit)
            {
                if (_DeviceName == DeviceName.D || _DeviceName == DeviceName.R || (_DeviceName == DeviceName.W || _DeviceName == DeviceName.ZR))
                    throw new Exception("Word Device는 Bit단위 블락으로 쓸 수 없습니다.\r\n _Unit 또는 _DeviceName을 바꿔주시기 바랍니다.");
                if (_Data.Length % 2 == 1)
                    throw new Exception("Bit Device의 _Data길이 는 항상짝수여야 합니다");
            }

            Array.Resize<byte>(ref this.SendBytes_BlockWrite, 21);

            if (_Unit == SubCommand.Bit)
            {
                this.SendBytes_BlockWrite[13] = (byte)1;
                this.SendBytes_BlockWrite[14] = (byte)0;
            }
            else if (_Unit == SubCommand.Word)
            {
                this.SendBytes_BlockWrite[13] = (byte)0;
                this.SendBytes_BlockWrite[14] = (byte)0;
            }

            this.SendBytes_BlockWrite[11] = (byte)1;
            this.SendBytes_BlockWrite[12] = (byte)20;
            this.SendBytes_BlockWrite[18] = (byte)_DeviceName;
            int num1 = 0;

            if (_Unit == SubCommand.Word)
            {
                num1 = _Data.Length / 960 + 1;
                if (_Data.Length % 960 == 0)
                    --num1;
            }
            else if (_Unit == SubCommand.Bit)
            {
                num1 = _Data.Length / 7168 + 1;
                if (_Data.Length % 7168 == 0)
                    --num1;
            }

            int num2 = num1;
            int index1 = 0;

            for (int index2 = 0; index2 < num2; ++index2)
            {
                switch (_DeviceName)
                {
                    case DeviceName.M:
                    case DeviceName.L:
                    case DeviceName.D:
                    case DeviceName.R:
                    case DeviceName.ZR:
                        int num3 = 0;
                        switch (_Unit)
                        {
                            case SubCommand.Bit:
                                num3 = Convert.ToInt32(_StartAddress) + index2 * 7168;
                                break;
                            case SubCommand.Word:
                                num3 = _DeviceName == DeviceName.M || _DeviceName == DeviceName.L ? Convert.ToInt32(_StartAddress) + index2 * 15360 : Convert.ToInt32(_StartAddress) + index2 * 960;
                                break;
                        }
                        this.SendBytes_BlockWrite[15] = (byte)(num3 % 256);
                        this.SendBytes_BlockWrite[16] = (byte)(num3 / 256);
                        this.SendBytes_BlockWrite[17] = (byte)(num3 / 65536);

                        this.SendBytes_BlockWrite[15] = (byte)(num3 & 0xFF);
                        this.SendBytes_BlockWrite[16] = (byte)((num3 >> 8) & 0xFF);
                        this.SendBytes_BlockWrite[17] = (byte)((num3 >> 16) & 0xFF);
                        break;

                    case DeviceName.X:
                    case DeviceName.Y:
                    case DeviceName.B:
                    case DeviceName.W:
                        int num4 = 0;

                        switch (_Unit)
                        {
                            case SubCommand.Bit:
                                num4 = int.Parse(_StartAddress, NumberStyles.HexNumber) + index2 * 7168;
                                break;
                            case SubCommand.Word:
                                num4 = _DeviceName != DeviceName.W ? int.Parse(_StartAddress, NumberStyles.HexNumber) + index2 * 15360 : int.Parse(_StartAddress, NumberStyles.HexNumber) + index2 * 960;
                                break;
                        }

                        this.SendBytes_BlockWrite[15] = (byte)(num4 % 256);
                        this.SendBytes_BlockWrite[16] = (byte)(num4 / 256);
                        this.SendBytes_BlockWrite[17] = (byte)(num4 / 65536);

                        this.SendBytes_BlockWrite[15] = (byte)(num4 & 0xFF);
                        this.SendBytes_BlockWrite[16] = (byte)((num4 >> 8) & 0xFF);
                        this.SendBytes_BlockWrite[17] = (byte)((num4 >> 16) & 0xFF);
                        break;
                }

                if (num1 == 1)
                {
                    switch (_Unit)
                    {
                        case SubCommand.Bit:
                            this.SendBytes_BlockWrite[19] = (byte)((_Data.Length - 7168 * index2) % 256);
                            this.SendBytes_BlockWrite[20] = (byte)((_Data.Length - 7168 * index2) / 256);
                            break;

                        case SubCommand.Word:
                            this.SendBytes_BlockWrite[19] = (byte)((_Data.Length - 960 * index2) % 256);
                            this.SendBytes_BlockWrite[20] = (byte)((_Data.Length - 960 * index2) / 256);
                            this.SendBytes_BlockWrite[19] = (byte)(_Data.Length & 0xFF);
                            this.SendBytes_BlockWrite[20] = (byte)((_Data.Length >> 8) & 0xFF);
                            break;
                    }
                }
                else
                {
                    switch (_Unit)
                    {
                        case SubCommand.Bit:
                            this.SendBytes_BlockWrite[19] = (byte)0;
                            this.SendBytes_BlockWrite[20] = (byte)28;
                            break;

                        case SubCommand.Word:
                            this.SendBytes_BlockWrite[19] = (byte)192;
                            this.SendBytes_BlockWrite[20] = (byte)3;
                            break;
                    }
                }
                int num5 = 0;
                if (_Unit == SubCommand.Word)
                {
                    if (num1 == 1)
                    {
                        num5 = 12 + (_Data.Length - index2 * 960) * 2;
                        //this.SendBytes_BlockWrite[7] = (byte)(num5 % 256);
                        //this.SendBytes_BlockWrite[8] = (byte)(num5 / 256);

                        this.SendBytes_BlockWrite[7] = (byte)(num5 & 0xFF);
                        this.SendBytes_BlockWrite[8] = (byte)((num5 >> 8) & 0xFF);
                    }
                    else
                    {
                        num5 = 1932;
                        this.SendBytes_BlockWrite[7] = (byte)(num5 % 256);
                        this.SendBytes_BlockWrite[8] = (byte)(num5 / 256);
                    }
                }
                else if (_Unit == SubCommand.Bit)
                {
                    if (num1 == 1)
                    {
                        num5 = 12 + (_Data.Length - index2 * 7168) / 2;
                        this.SendBytes_BlockWrite[7] = (byte)(num5 % 256);
                        this.SendBytes_BlockWrite[8] = (byte)(num5 / 256);
                    }
                    else
                    {
                        num5 = 3596;
                        this.SendBytes_BlockWrite[7] = (byte)(num5 % 256);
                        this.SendBytes_BlockWrite[8] = (byte)(num5 / 256);
                    }
                }

                Array.Resize<byte>(ref this.SendBytes_BlockWrite, 9 + num5);

                if (_Unit == SubCommand.Word)
                {
                    int num6 = num1 != 1 ? 960 : (_Data.Length % 960 != 0 ? _Data.Length % 960 : 960);
                    for (int index3 = 0; index3 < num6; ++index3)
                    {
                        // this.SendBytes_BlockWrite[21 + index3 * 2] = (byte)(_Data[index1] % 256);
                        // this.SendBytes_BlockWrite[22 + index3 * 2] = (byte)(_Data[index1] / 256);

                        this.SendBytes_BlockWrite[21 + index3 * 2] = (byte)(_Data[index1] & 0xFF);
                        this.SendBytes_BlockWrite[22 + index3 * 2] = (byte)((_Data[index1] >> 8) & 0xFF);
                        ++index1;
                    }
                }
                else if (_Unit == SubCommand.Bit)
                {
                    int num6 = num1 != 1 ? 7168 : (index2 != 0 ? _Data.Length % 7168 : _Data.Length);
                    for (int index3 = 0; index3 < num6 / 2; ++index3)
                    {
                        this.SendBytes_BlockWrite[21 + index3] = (byte)(_Data[index1 * 2] * 16 + _Data[index1 * 2 + 1]);
                        ++index1;
                    }
                }
                if (this._client == null || this._client.Connection == null || !this._client.Connection.Connected)
                    return 1;

                //PlcSendResetEvent.WaitOne(100);
                _plcSendResetEvent.Reset();
                this._isReceived = false;

                this._client.Send(this.SendBytes_BlockWrite);
                int tickCount = Environment.TickCount;
                while (!this._isReceived)
                {
                    if (Environment.TickCount - tickCount > this.TimeOut)
                    {
                        Console.WriteLine("Write Data TimeOut");
                        return 1;
                    }

                    Thread.Sleep(50);
                }
                --num1;
                //this.bReceived = false;
            }
            return 0;
        }

        private bool IsConnected()
        {
            return _client.Connection.Connected;
        }

        public int Open()
        {
            int num1 = 0;

            if (_isConnected == true)
                return num1;

            this._client = new AsyncSocketClient(this._socketId++);
            this._client.OnConnet -= new AsyncSocketConnectEventHandler(this.client_OnConnet);
            this._client.OnConnet += new AsyncSocketConnectEventHandler(this.client_OnConnet);

            this._client.Connect(this.IP, this.Port);
            int tickCount = Environment.TickCount;
            int num2;

            while (_isConnected == false)
            {
                if (Environment.TickCount - tickCount > this.TimeOut)
                {
                    num2 = 3;
                    break;
                }
            }

            num2 = 0;
            return num2;
        }

        private void client_OnConnet(object sender, AsyncSocketConnectionEventArgs e)
        {
            this._client.OnReceive += new AsyncSocketReceiveEventHandler(this.client_OnReceive);
            this._client.OnError += new AsyncSocketErrorEventHandler(this.client_OnError);
            this._client.OnClose += new AsyncSocketCloseEventHandler(this.client_OnClose);
            this._client.Receive();
            _isConnected = true;
        }

        private void client_OnReceive(object sender, AsyncSocketReceiveEventArgs e)
        {
            try
            {
                ReceivedData = new byte[e.ReceiveBytes - 11];
                Array.Copy((Array)e.ReceiveData, 11, (Array)this.ReceivedData, 0, e.ReceiveBytes - 11);
            }
            catch
            {
            }

            _isReceived = true;
            _plcSendResetEvent.Set();
        }

        private void client_OnClose(object sender, AsyncSocketConnectionEventArgs e)
        {
        }

        private void client_OnError(object sender, AsyncSocketErrorEventArgs e)
        {
        }

        public void Close()
        {
            if (this._client != null)
                this._client.Close();

            _isConnected = false;
        }
        #endregion
    }

    public enum DeviceName : byte
    {
        M = 144, // 0x90
        L = 146, // 0x92
        X = 156, // 0x9C
        Y = 157, // 0x9D
        B = 160, // 0xA0
        D = 168, // 0xA8
        R = 175, // 0xAF
        ZR = 176, // 0xB0
        W = 180, // 0xB4
    }

    public enum DeviceName_WORD : byte
    {
        D = 168, // 0xA8
        R = 175, // 0xAF
        ZR = 176, // 0xB0
        W = 180, // 0xB4
    }

    public enum DeviceName_BIT : byte
    {
        M = 144, // 0x90
        L = 146, // 0x92
        X = 156, // 0x9C
        Y = 157, // 0x9D
        B = 160, // 0xA0
    }

    public enum SubCommand
    {
        Bit,
        Word,
    }
}
