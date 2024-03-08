using COG.Settings;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace COG.Device.PLC
{
    public class PlcControlManager
    {
        #region 필드
        private static PlcControlManager _instance = null;

        private DeviceName _deviceName = DeviceName.R;

        private bool _alive = false;

        private object _lock = new object();
        #endregion

        #region 속성
        private int[] ReadData { get; set; } = new int[StaticConfig.PLC_READ_SIZE];

        private PlcControl MCClient_READ = new PlcControl();

        private PlcControl MCClient_WRITE = new PlcControl();

        public Task PlcActionTask { get; set; }

        public CancellationTokenSource CancelPlcActionTask { get; set; }
        #endregion

        #region 이벤트
        #endregion

        #region 델리게이트
        #endregion

        #region 생성자
        #endregion

        #region 메서드
        public static PlcControlManager Instance()
        {
            if (_instance == null)
                _instance = new PlcControlManager();

            return _instance;
        }

        public void Initialize()
        {
            _deviceName = DeviceName.R;
            StartReadTask();
        }

        public void ReadDevice()
        {
            if (StaticConfig.VirtualMode)
                return;
            int size = StaticConfig.PLC_READ_SIZE;
            int[] returnValue = new int[size];
            try
            {
                returnValue = MCClient_READ.ReadDeviceBlock(DataType.Word, _deviceName, StaticConfig.BASE_ADDR.ToString(), size);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + ex.Message + ex.StackTrace);
                MCClient_READ.WriteLogFile("PLC READ DISCONNECT");
            }
            finally
            {
                lock(_lock)
                    ReadData = returnValue;
            }
        }

        public int[] GetReadData()
        {
            int[] value = null;
            lock (_lock)
                value = ReadData;

            return value;
        }

        public void WriteDevice(int address, int lplData)
        {
            if (StaticConfig.VirtualMode)
                return;
            try
            {
                int[] Data = new int[1];
                Data[0] = lplData;

                MCClient_WRITE.WriteDeviceBlock(DataType.Word, _deviceName, address.ToString(), Data);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + ex.Message + ex.StackTrace);
            }
            finally { }
        }

        private void StartReadTask()
        {
            if (StaticConfig.VirtualMode)
                return;
            if (PlcActionTask != null)
                return;

            CancelPlcActionTask = new CancellationTokenSource();
            PlcActionTask = new Task(ThreadPLC_Read, CancelPlcActionTask.Token);
            PlcActionTask.Start();
        }

        public void ThreadPLC_Read()
        {
            if (StaticConfig.VirtualMode)
                return;

            Stopwatch alive = new Stopwatch();

            while (true)
            {
                if (CancelPlcActionTask.IsCancellationRequested)
                    break;

                if (alive.ElapsedMilliseconds > 1000)
                {
                    _alive = !_alive;
                    int address = StaticConfig.BASE_ADDR + Convert.ToInt16(PlcCommonMap.Alive);
                    WriteVisionAlive(_alive);
                    alive.Restart();
                }
                else
                {
                    try
                    {
                        ReadDevice();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("PLC READ DISCONNECT" + ex.Source + ex.Message + ex.StackTrace);
                    }
                }

                Thread.Sleep(50);
            }
        }

        public void Open(int readLocalPort, int writeLocalPort, string remoteIp, int timeOut)
        {
            if (StaticConfig.VirtualMode)
                return;
            try
            {
                MCClient_READ.SetPLCProperties(remoteIp, readLocalPort, timeOut);

                if (MCClient_READ.Open() == false)
                    MessageBox.Show("READ PORT OPEN ERROR:" + readLocalPort.ToString());


                MCClient_WRITE.SetPLCProperties(remoteIp, writeLocalPort, timeOut);

                if (MCClient_WRITE.Open() == false)
                    MessageBox.Show("WRITE PORT OPEN ERROR:" + writeLocalPort.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show("PLC OPEN ERROR " + ex.ToString());
            }
        }

        public void Close()
        {
            if (StaticConfig.VirtualMode)
                return;

            MCClient_READ.Close();
        }

        public void WriteCurrentModel(string modelName)
        {
            if (StaticConfig.VirtualMode)
                return;

            var currentModel = AppsConfig.Instance().ProjectInfo;

            try
            {
                int address = StaticConfig.BASE_ADDR + Convert.ToInt16(currentModel);

                WriteDevice(address, Convert.ToInt16(modelName));
            }
            catch (Exception err)
            {
                MessageBox.Show("Model Name is Incorrect");
            }
        }

        public void WritePlcCommand(int value)
        {
            if (StaticConfig.VirtualMode)
                return;

            int address = StaticConfig.BASE_ADDR + PlcAddressMap.PLC_Command;

            WriteDevice(address, value);
        }


        public void WriteVisionReady(bool isReady)
        {
            if (StaticConfig.VirtualMode)
                return;
            int value = -1;

            if (isReady)
                value = 9000;
            else
                value = 0;

            int address = StaticConfig.BASE_ADDR + Convert.ToInt16(PlcCommonMap.Vision_Ready);

            WriteDevice(address, value);
        }

        public void WriteVisionStatus(int command)
        {
            if (StaticConfig.VirtualMode)
                return;

            int address = StaticConfig.BASE_ADDR + PlcAddressMap.PC_Status;
            WriteDevice(address, command);
        }

        public void WriteVisionAlive(bool isAlive)
        {
            if (StaticConfig.VirtualMode)
                return;

            int address = StaticConfig.BASE_ADDR + Convert.ToInt16(PlcCommonMap.Alive);
            WriteDevice(address, isAlive == true ? 1 : 0);
        }

        public void ClearPlcCommand()
        {
            if (StaticConfig.VirtualMode)
                return;

            int address = StaticConfig.BASE_ADDR + PlcAddressMap.PLC_Command;
            WriteDevice(address, 0);
        }
        #endregion
    }
}
