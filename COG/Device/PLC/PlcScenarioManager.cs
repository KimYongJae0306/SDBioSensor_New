using COG.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
                    dateTime = command[address];
                }
            }
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
                }
            }
        }

        private void StartInspection()
        {

        }
        #endregion
    }
}
