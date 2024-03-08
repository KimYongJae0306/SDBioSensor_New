using COG.Settings;
using JAS.Interface.localtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static JAS.Interface.localtime.SetlocalTime;

namespace COG.Device.PLC
{
    public class PlcScenarioManager
    {
        #region 필드
        private static PlcScenarioManager _instance = null;
        #endregion

        #region 속성
        private Task CommandTask { get; set; }

        private CancellationTokenSource CommandTaskCancellationTokenSource { get; set; }

        private Queue<int[]> PlcCommandQueue { get; set; } = new Queue<int[]>();
        #endregion

        #region 이벤트
        #endregion

        #region 델리게이트
        #endregion

        #region 생성자
        #endregion

        #region 메서드
        public static PlcScenarioManager Instance()
        {
            if (_instance == null)
                _instance = new PlcScenarioManager();

            return _instance;
        }

        public void Initialize()
        {
            StartScenarioTask();
        }

        private void StartScenarioTask()
        {
            if (CommandTask != null)
                return;

            CommandTaskCancellationTokenSource = new CancellationTokenSource();
            CommandTask = new Task(ScenarioTask, CommandTaskCancellationTokenSource.Token);
            CommandTask.Start();
        }

        private void ScenarioTask()
        {
            while (true)
            {
                if (CommandTaskCancellationTokenSource.IsCancellationRequested)
                    break;

                if (AppsStatus.Instance().MC_STATUS == MC_STATUS.RUN)
                {
                    var readData = PlcControlManager.Instance().GetReadData();

                    int cmdIndex = Convert.ToInt16(readData[PlcAddressMap.PLC_Command]);
                    int cmd = readData[cmdIndex];

                    int pcStatusIndex = Convert.ToInt16(readData[PlcAddressMap.PC_Status]);
                    int status = readData[pcStatusIndex];

                    if(cmd != 0 && status == 0 && cmd != 9000)
                        PlcCommandReceived((PlcCommand)cmd, readData);
                }

                Thread.Sleep(50);
            }
        }

        private void StopScenarioTask()
        {
            if (CommandTask == null)
                return;

            CommandTaskCancellationTokenSource.Cancel();
            CommandTask = null;
        }

        public void Release()
        {
            StopScenarioTask();
            PlcCommandQueue.Clear();
        }

        private void PlcCommandReceived(PlcCommand command, int[] readData)
        {
            switch (command)
            {
                case PlcCommand.Time_Change:
                    ChangeTime(readData);
                    break;

                case PlcCommand.Model_Change:
                    ChangeModel(readData);
                    break;
                case PlcCommand.Cmd_Clear:
                    ClearCmd(readData);
                    break;
                default:
                    break;
            }
        }

        private void ClearCmd(int[] readData)
        {
            throw new NotImplementedException();
        }

        private void ChangeTime(int[] readData)
        {
            SetlocalTime.SYSTEMTIME systemTime = new SetlocalTime.SYSTEMTIME();

            systemTime = SetlocalTime.GetTime();
            systemTime.wYear = (ushort)readData[PlcAddressMap.PLC_Time_Year];
            systemTime.wMonth = (ushort)readData[PlcAddressMap.PLC_Time_Year + 1];
            systemTime.wDay = (ushort)readData[PlcAddressMap.PLC_Time_Year + 2];
            systemTime.wHour = (ushort)readData[PlcAddressMap.PLC_Time_Year + 3];
            systemTime.wMinute = (ushort)readData[PlcAddressMap.PLC_Time_Year + 4];
            systemTime.wSecond = (ushort)readData[PlcAddressMap.PLC_Time_Year + 5];

            string logMessage = string.Empty;
            int errorCode = 0;
            if (SetlocalTime.SetLocalTime_(systemTime, ref errorCode))
            {
                logMessage = "LocalTiem Changed OK";
                PlcControlManager.Instance().WriteVisionStatus((int)PlcCommand.Time_Change);
            }
            else
            {
                logMessage = $"LocalTime Changed NG, Error Code : {errorCode}";
                PlcControlManager.Instance().WriteVisionStatus((int)PlcCommand.Time_Change * -1);
            }

            SystemManager.Instance().AddLogDisplay(0, logMessage, true);
            PlcControlManager.Instance().ClearPlcCommand();
            ClearCmdCheck();
        }

        private void ClearCmdCheck()
        {
            int seq = 0;
            bool LoopFlag = true;

            Stopwatch sw = new Stopwatch();
            while (LoopFlag)
            {
                var readData = PlcControlManager.Instance().GetReadData();
                switch (seq)
                {
                    case 0:
                        sw.Restart();
                        seq++;
                        break;

                    case 1:
                        if (sw.ElapsedMilliseconds > StaticConfig.CMD_CHECK_TIMEOUT)
                        {
                            seq++;
                            break;
                        }
                        if (readData[PlcAddressMap.PLC_Command] != 0)
                            break;
                        else
                            seq++;
                        break;

                    case 2:
                        LoopFlag = false;
                        break;
                }
                Thread.Sleep(50);
            }
        }

        private void ChangeModel(int[] readData)
        {
            int address = StaticConfig.BASE_ADDR + PlcAddressMap.PLC_ModelNo;
            string modelNo = readData[address].ToString("000");
            string logMessage = "";
            if(SystemManager.Instance().LoadModel(modelNo))
            {
                var inspModel = ModelManager.Instance().CurrentModel;
                PlcControlManager.Instance().ClearPlcCommand();
                PlcControlManager.Instance().WritePlcCommand((int)PlcCommand.Model_Change);
                logMessage = "MODEL: " + inspModel.ModelName + inspModel.ModelInfo + " LOAD OK";
            }
            else
            {
                PlcControlManager.Instance().ClearPlcCommand();
                PlcControlManager.Instance().WritePlcCommand((int)PlcCommand.Model_Change * -1);
                logMessage = "MODEL: " + modelNo + " LOAD NG";
            }
            logMessage = "<- " + logMessage;

            SystemManager.Instance().AddLogDisplay(0, logMessage, true);
            PlcControlManager.Instance().ClearPlcCommand();
            ClearCmdCheck();
        }

        private void StartInspection(int stageNo)
        {
            if(stageNo == 0)
            {
                string logMessage = "===== INSPECTION 1 =====";
                SystemManager.Instance().AddLogDisplay(stageNo, logMessage, true);

            }
        }

      
        #endregion
    }
}
