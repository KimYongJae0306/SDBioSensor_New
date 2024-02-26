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
        #endregion

        #region 속성
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

        public int[] ReadDevice(int size/*, out int[] lplData*/)
        {
            //int[] returnValue = new int[lSize];
            //try
            //{
            //    returnValue = _plcControl.ReadDeviceBlock(SubCommand.Word, _deviceName, AddressMap.PLC_BaseAddress.ToString(), lSize);
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Source + ex.Message + ex.StackTrace);
            //    _plcControl.WriteLogFile("PLC READ DISCONNECT");
            //}
            //finally
            //{
            //    lplData = returnValue;
            //}

            if (MCClient_READ == null)
                return null;

            return MCClient_READ.ReadDeviceBlock(DataType.Word, _deviceName, BaseAddressMap.PLC_BaseAddress.ToString(), size);
        }

        private void WriteDevice(int device, int lplData)
        {
            try
            {
                int[] Data = new int[1];
                Data[0] = lplData;

                MCClient_READ.WriteDeviceBlock(DataType.Word, _deviceName, device.ToString(), Data);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + ex.Message + ex.StackTrace);
            }
            finally { }
        }

        private void StartReadTask()
        {
            if (PlcActionTask != null)
                return;

            CancelPlcActionTask = new CancellationTokenSource();
            PlcActionTask = new Task(ThreadPLC_Read, CancelPlcActionTask.Token);
            PlcActionTask.Start();
        }

        public void ThreadPLC_Read()
        {
            Stopwatch alive = new Stopwatch();

            while (true)
            {
                if (CancelPlcActionTask.IsCancellationRequested)
                    break;

                if (alive.ElapsedMilliseconds > 1000)
                {
                    _alive = !_alive;
                    int address = Convert.ToInt16(BaseAddressMap.PLC_BaseAddress) + Convert.ToInt16(PlcCommonMap.Alive);
                    WriteVisionAlive(_alive);
                    alive.Restart();
                }
                else
                {
                    try
                    {
                        var readData = ReadDevice(MCClient_READ.ReadSize);

                        if (readData.Length == MCClient_READ.ReadSize)
                        {
                            //_plcControl.ReadDatas = readData;
                            PlcScenarioManager.Instance().AddCommand(readData);
                        }
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
            if (StaticConfig.VirtualMode == true /*false*/)
            {
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
        }

        public void Close()
        {
            MCClient_READ.Close();
        }

        public void WriteCurrentModel(string modelName)
        {
            int address = Convert.ToInt16(BaseAddressMap.PC_BaseAddress) + Convert.ToInt16(PlcCommonMap.PC_Model_No);
            WriteDevice(address, Convert.ToInt16(modelName));
        }

        public void WriteVisionReady(bool isReady)
        {
            int value = -1;

            if (isReady)
                value = 9000;
            else
                value = 0;

            int address = Convert.ToInt16(BaseAddressMap.PC_BaseAddress) + Convert.ToInt16(PlcCommonMap.Vision_Ready);

            WriteDevice(address, value);
        }

        public void WriteVisionStatus(int command)
        {
            int address = Convert.ToInt16(BaseAddressMap.PC_BaseAddress) + Convert.ToInt16(PlcCommonMap.PC_Status);
            WriteDevice(address, command);
        }

        public void WriteVisionAlive(bool isAlive)
        {
            int address = Convert.ToInt16(BaseAddressMap.PC_BaseAddress) + Convert.ToInt16(PlcCommonMap.Alive);
            WriteDevice(address, isAlive == true ? 1 : 0);
        }

        public void ClearPlcCommand()
        {
            int address = Convert.ToInt16(BaseAddressMap.PC_BaseAddress) + Convert.ToInt16(PlcCommonMap.PLC_Command);
            WriteDevice(address, 0);
        }
        #endregion
    }
}
