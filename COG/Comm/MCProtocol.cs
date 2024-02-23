using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Globalization;

namespace COG.Comm
{
    public class MCProtocol
    {
        #region 필드
        private TcpClient[] _tcpClient;

        private IPAddress _ipAddress;

        private IPEndPoint[] _ipEndPoint = new IPEndPoint[2];

        private bool _portOpenConfirm = false;

        private int _returnLength = 0;

        private int[] _receiveResult = new int[0x1000];

        private bool _readPLCFlag = false;

        private Semaphore _semaphore = new Semaphore(0, 0x2710);

        private const int M_READ = 0;

        private const int M_WRITE = 1;
        #endregion

        #region 메서드
        #endregion

        #region 생성자
        #endregion

        public bool CheckConnected(Socket socket)
        {
            bool isConnect = true;

            if (socket.Connected)
            {
                try
                {
                    if ((uint)new Ping().Send(((IPEndPoint)socket.RemoteEndPoint).Address).Status > 0U)
                        isConnect = false;
                }
                catch (PingException ex)
                {
                    isConnect = false;
                }

                if (socket.Poll(5000, SelectMode.SelectRead) && socket.Available == 0)
                    isConnect = false;
            }
            else
                isConnect = socket.IsBound;

            return isConnect;
        }

        public int ReadDevice_W(string address, int length, ref int[] value)
        {
            if (!CheckConnected(_tcpClient[M_READ].Client))
            {
                _tcpClient[M_READ].Close();
                _tcpClient[M_READ] = new TcpClient();
                //m_TcpClient[M_READ_].Connect(m_IPEndPoint[M_READ_]);      //shkang
            }

            int statementSize = 21; //nStatementSize
            int sendCount = length;

            int sendTotalLength = statementSize;
            int requestLength = 12;

            byte[] datagram = new byte[sendTotalLength];
            //-------------------------nSendTotalLength는 이거 밑으로 갯수-------------------------------------
            datagram[0] = 0x50; //서브헤더
            datagram[1] = 0x00; //서브헤더
            datagram[2] = 0x00; //네트워크번호
            datagram[3] = 0xFF; //PLC 번호
            datagram[4] = 0xFF; //I/O 번호
            datagram[5] = 0x03; //요구 상대 모듈
            datagram[6] = 0x00; //요구 상대 모듈 국번호
            //---------------------------------------------------------------------------
            datagram[7] = (byte)((requestLength) & 0xff); //요구데이터 개수 L 
            datagram[8] = (byte)((requestLength >> 8) & 0xFF); //요구데이터 개수 H 
            //-------------------------nReqLength는 이거 밑으로 갯수-------------------------------------------
            datagram[9] = 0x0A; //(CPU감시 타임 L) //10
            datagram[10] = 0x00; //(CPU감시 타임 H)
            datagram[11] = 0x01; //(커맨드 L)                //L
            datagram[12] = 0x04; //(커맨드 H) 0x14           //H
            datagram[13] = 0x00; //(서브커맨드 L)            //L
            datagram[14] = 0x00; //(서브커맨드 H)            //H

            datagram[15] = (byte)((Int32.Parse(address.Remove(0, 1), GetNumberStyles(address.Substring(0, 1)))) & 0xFF);   //디바이스 번지 L
            datagram[16] = (byte)((Int32.Parse(address.Remove(0, 1), GetNumberStyles(address.Substring(0, 1))) >> 8) & 0xFF);   //디바이스 번지 -
            datagram[17] = (byte)((Int32.Parse(address.Remove(0, 1), GetNumberStyles(address.Substring(0, 1))) >> 16) & 0xFF);   //디바이스 번지 H
            datagram[18] = (byte)(GetDeviceCode(address.Substring(0, 1)));                                      //디바이스 코드
            datagram[19] = (byte)(length & 0xFF);                            //쓰는갯수 L
            datagram[20] = (byte)((length >> 8) & 0xFF);                     //쓰는갯수 H

            int num9 = 0;
            try
            {
                _readPLCFlag = true;
                num9 = _tcpClient[M_READ].Client.Send(datagram, sendTotalLength, SocketFlags.None);


                if (_readPLCFlag)
                {
                    int nReceiveStatement = 11;

                    byte[] buffer = new byte[nReceiveStatement + length * 2];

                    int receiveCount = _tcpClient[M_READ].Client.Receive(buffer);
                    int index = 0;

                    int[] numArray = new int[length];
                    _returnLength = 0;

                    for (int i = 0; i < length; i++)
                    {
                        numArray[i] = buffer[nReceiveStatement + index + 1] << 8;
                        numArray[i] += (Int16)buffer[nReceiveStatement + index];
                        index += 2;
                        _returnLength = i + 1;
                    }
                    _receiveResult = numArray;
                    value = numArray;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + ex.Message + ex.StackTrace);
                if (num9 <= 0)
                {
                    MessageBox.Show("전송오류");
                }
            }
            return _returnLength;
        }

        public int WriteDevice_W(string address, int length, int[] value)
        {
            int returnValue = 0;

            if (_tcpClient == null)
                return returnValue = -1;

            if (CheckConnected(_tcpClient[M_WRITE].Client) == false)
            {
                _tcpClient[M_WRITE].Close();
                _tcpClient[M_WRITE] = new TcpClient();
                _tcpClient[M_WRITE].Connect(_ipEndPoint[M_WRITE]);
            }

            int dataArraySize = 2;
            int statementSize = 21; //nStatementSize

            int sendCount = length;
            int sendTotalLength = statementSize + (sendCount * dataArraySize);
            int requestLength = 12 + (sendCount * dataArraySize);

            byte[] datagram = new byte[sendTotalLength];
            //-------------------------nSendTotalLength는 이거 밑으로 갯수-------------------------------------
            datagram[0] = 0x50; //서브헤더
            datagram[1] = 0x00; //서브헤더
            datagram[2] = 0x00; //네트워크번호
            datagram[3] = 0xFF; //PLC 번호
            datagram[4] = 0xFF; //I/O 번호
            datagram[5] = 0x03; //요구 상대 모듈
            datagram[6] = 0x00; //요구 상대 모듈 국번호
            //---------------------------------------------------------------------------
            datagram[7] = (byte)((requestLength) & 0xff); //요구데이터 개수 L 
            datagram[8] = (byte)((requestLength >> 8) & 0xFF); //요구데이터 개수 H 
            //-------------------------nReqLength는 이거 밑으로 갯수-------------------------------------------
            datagram[9] = 0x0A; //(CPU감시 타임 L) //10
            datagram[10] = 0x00; //(CPU감시 타임 H)

            datagram[11] = 0x01; //(커맨드 L)                //L
            datagram[12] = 0x14; //(커맨드 H) 0x14           //H
            datagram[13] = 0x00; //(서브커맨드 L)            //L
            datagram[14] = 0x00; //(서브커맨드 H)            //H

            datagram[15] = (byte)((Int32.Parse(address.Remove(0, 1), GetNumberStyles(address.Substring(0, 1)))) & 0xFF);   //디바이스 번지 L
            datagram[16] = (byte)((Int32.Parse(address.Remove(0, 1), GetNumberStyles(address.Substring(0, 1))) >> 8) & 0xFF);   //디바이스 번지 -
            datagram[17] = (byte)((Int32.Parse(address.Remove(0, 1), GetNumberStyles(address.Substring(0, 1))) >> 16) & 0xFF);   //디바이스 번지 H
            datagram[18] = (byte)(GetDeviceCode(address.Substring(0, 1)));                                      //디바이스 코드
            datagram[19] = (byte)(length & 0xFF);                            //쓰는갯수 L
            datagram[20] = (byte)((length >> 8) & 0xFF);                     //쓰는갯수 H

            //----------------------------------------------------------------------------
            for (int sendIndex = 0; sendIndex < sendCount; sendIndex++)
            {
                datagram[(statementSize + (sendIndex * dataArraySize)) + 00] = (byte)(value[sendIndex] & 0xFF);                            //데이터 L
                datagram[(statementSize + (sendIndex * dataArraySize)) + 01] = (byte)((value[sendIndex] >> 8) & 0xFF);                     //데이터 H
            }

            _tcpClient[M_WRITE].Client.Send(datagram, sendTotalLength, SocketFlags.None);

            try
            {
                int receiveStatement = 11;
                byte[] buffer = new byte[receiveStatement/* + Length  * sizeof(short)*/];
                int receiveCount = this._tcpClient[M_WRITE].Client.Receive(buffer);

                returnValue = buffer[10] << 8;
                returnValue += (Int16)buffer[9];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + ex.Message + ex.StackTrace);
                returnValue = -1;
            }

            return returnValue;
        }

        private byte GetDeviceCode(string DeviceCode)
        {
            byte returnValue = 0;

            switch (DeviceCode)
            {
                case "L": returnValue = 0x92; break;
                case "F": returnValue = 0x93; break;
                case "V": returnValue = 0x94; break;
                case "TS": returnValue = 0xC1; break;
                case "TC": returnValue = 0xC0; break;
                case "TN": returnValue = 0xC2; break;
                case "SS": returnValue = 0xC7; break;
                case "SC": returnValue = 0xC6; break;
                case "SN": returnValue = 0xC8; break;
                case "CS": returnValue = 0xC4; break;
                case "CC": returnValue = 0xC4; break;
                case "CN": returnValue = 0xC5; break;
                case "S": returnValue = 0x98; break;
                case "DX": returnValue = 0xA2; break;
                case "DY": returnValue = 0xA3; break;
                case "Z": returnValue = 0xCC; break;
                case "R": returnValue = 0xAF; break;
                case "ZR": returnValue = 0xB0; break;
                //-----------------------------------------------MELSECNET
                case "SM": returnValue = 0x91; break;
                case "SD": returnValue = 0xA9; break;
                case "X": returnValue = 0x9C; break;
                case "Y": returnValue = 0x9D; break;
                case "M": returnValue = 0x90; break;
                case "B": returnValue = 0xA0; break;
                case "D": returnValue = 0xA8; break;
                case "W": returnValue = 0xB4; break;
                case "SB": returnValue = 0xA1; break;
                case "SW": returnValue = 0xB5; break;
                //------------------------------------------------------------
                case "null":
                    returnValue = 0xA8; //"D"
                    break;
            }

            return returnValue;
        }

        private NumberStyles GetNumberStyles(string DeviceCode)
        {
            NumberStyles returnValue = NumberStyles.AllowDecimalPoint;

            switch (DeviceCode)
            {
                case "L":
                case "F":
                case "V":
                case "TS":
                case "TC":
                case "TN":
                case "SS":
                case "SC":
                case "SN":
                case "CS":
                case "CC":
                case "CN":
                case "S":
                case "Z":
                case "R":
                case "M":
                case "D":
                case "SM":
                case "SD":
                    returnValue = NumberStyles.AllowDecimalPoint;
                    break;
                //-----------------------------------------------MELSECNET
                case "DX":
                case "DY":
                case "SB":
                case "SW":
                case "B":
                case "W":
                case "X":
                case "Y":
                case "ZR":
                    returnValue = NumberStyles.HexNumber;
                    break;

                //------------------------------------------------------------
                case "null":
                    returnValue = NumberStyles.AllowDecimalPoint;
                    break;
            }
            return returnValue;
        }
    }
}