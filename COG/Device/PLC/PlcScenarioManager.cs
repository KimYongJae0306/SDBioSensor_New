using COG.Settings;
using JAS.Interface.localtime;
using System;
using System.Collections.Generic;
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

                if (GetCommand() is int[] command)
                {
                    if (command.Count() > 0)
                    {
                        int address = StaticConfig.PLC_BaseAddress + Convert.ToInt16(PlcCommonMap.PLC_Command);
                        PlcCommandReceived((PlcCommand)command[address]);
                    }
                }

                Thread.Sleep(100);
            }
        }

        private void StopScenarioTask()
        {
            if (CommandTask == null)
                return;

            CommandTaskCancellationTokenSource.Cancel();
            CommandTask = null;
        }

        public void AddCommand(int[] command)
        {
            if (command.Count() >= 0)
            {
                lock (PlcCommandQueue)
                    PlcCommandQueue.Enqueue(command);
            }
        }

        public void Release()
        {
            StopScenarioTask();
            PlcCommandQueue.Clear();
        }

        public int[] GetCommand()
        {
            lock (PlcCommandQueue)
            {
                if (PlcCommandQueue.Count() > 0)
                    return PlcCommandQueue.Dequeue();
                else
                    return null;
            }
        }

        private void PlcCommandReceived(PlcCommand command)
        {
            switch (command)
            {
                case PlcCommand.StartInspection:
                    StartInspection();
                    break;

                case PlcCommand.Time_Change:
                    ChangeTime();
                    break;

                case PlcCommand.Model_Change:
                    ChangeModel();
                    break;

                default:
                    break;
            }
        }

        private void ChangeTime()
        {
            int dateTime;

            if (GetCommand() is int[] command)
            {
                if (command.Count() > 0)
                {
                    int address = StaticConfig.PLC_BaseAddress + Convert.ToInt16(PlcCommonMap.PLC_Time);

                    SetlocalTime.SYSTEMTIME systemTime = new SetlocalTime.SYSTEMTIME();
                    systemTime.wYear = (ushort)command[address];
                    systemTime.wMonth = (ushort)command[address + 1];
                    systemTime.wDay = (ushort)command[address + 2];
                    systemTime.wHour = (ushort)command[address + 3];
                    systemTime.wMinute = (ushort)command[address + 4];
                    systemTime.wSecond = (ushort)command[address + 5];

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

                    PlcControlManager.Instance().ClearPlcCommand();
                }
                else
                    PlcControlManager.Instance().WriteVisionStatus((int)PlcCommand.Time_Change * -1);
            }
            else
                PlcControlManager.Instance().WriteVisionStatus((int)PlcCommand.Time_Change * -1);
        }

        private void ChangeModel()
        {
            int modelNo;

            if (GetCommand() is int[] command)
            {
                if (command.Count() > 0)
                {
                    int address = StaticConfig.PLC_BaseAddress + Convert.ToInt16(PlcCommonMap.PLC_Model_No);
                    modelNo = command[address];

                    // TODO : 용재형 ㄱㄱ
                    if (true) // model change
                    {
                        PlcControlManager.Instance().WriteVisionStatus((int)PlcCommand.Model_Change);
                        PlcControlManager.Instance().ClearPlcCommand();
                    }
                    else
                        PlcControlManager.Instance().WriteVisionStatus((int)PlcCommand.Model_Change * -1);

                }
                else
                    PlcControlManager.Instance().WriteVisionStatus((int)PlcCommand.Model_Change * -1);
            }
            else
                PlcControlManager.Instance().WriteVisionStatus((int)PlcCommand.Model_Change * -1);
        }

        private void StartInspection()
        {

        }
        #endregion
    }
}
