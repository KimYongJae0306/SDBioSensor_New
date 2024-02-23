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
        private PlcControl _plcControl = new PlcControl();

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

            if (_plcControl == null)
                return null;

            return _plcControl.ReadDeviceBlock(DataType.Word, _deviceName, BaseAddressMap.PLC_BaseAddress.ToString(), size);
        }

        private void WriteDevice(int device, int lplData)
        {
            try
            {
                int[] Data = new int[1];
                Data[0] = lplData;

                _plcControl.WriteDeviceBlock(DataType.Word, _deviceName, device.ToString(), Data);
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
                        var readData = ReadDevice(_plcControl.ReadSize);

                        if (readData.Length == _plcControl.ReadSize)
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

        public void Open(int _intReadLocalPort, int _intReadRemotePort, string _strRemoteIP, int _intReadRecTimeOut, int _intWriteLocalPort, int _intWriteRemotePort)
        {
            if (StaticConfig.VirtualMode == true /*false*/)
            {
                try
                {
                    _plcControl.SetPLCProperties(_strRemoteIP, _intReadLocalPort, _intReadRecTimeOut);

                    if (_plcControl.Open() == false)
                        MessageBox.Show("PORT OPEN ERROR:" + _intReadLocalPort.ToString());

                    _plcControl.SetPLCProperties(_strRemoteIP, _intWriteLocalPort, _intReadRecTimeOut);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("PLC OPEN ERROR " + ex.ToString());
                }
            }
        }

        public void Close()
        {
            _plcControl.Close();
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
