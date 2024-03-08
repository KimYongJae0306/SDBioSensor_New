using COG.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace COG.Device.PLC
{
    public class PlcInspManager
    {
        private static PlcInspManager _instance = null;
        private Task CommandTask { get; set; }

        private CancellationTokenSource CommandTaskCancellationTokenSource { get; set; }

        public static PlcInspManager Instance()
        {
            if (_instance == null)
                _instance = new PlcInspManager();

            return _instance;
        }

        public void Initialize()
        {
            StartInspScenarioTask();
        }

        private void StartInspScenarioTask()
        {
            if (CommandTask != null)
                return;

            CommandTaskCancellationTokenSource = new CancellationTokenSource();
            CommandTask = new Task(InspScenarioTask, CommandTaskCancellationTokenSource.Token);
            CommandTask.Start();
        }

        private void InspScenarioTask()
        {
            while (true)
            {
                if (CommandTaskCancellationTokenSource.IsCancellationRequested)
                    break;

                InspModel inspModel = ModelManager.Instance().CurrentModel;
                if (inspModel != null)
                {
                    if (AppsStatus.Instance().MC_STATUS == MC_STATUS.RUN)
                    {
                        var readData = PlcControlManager.Instance().GetReadData();

                        int cmdIndex = Convert.ToInt16(readData[PlcAddressMap.PLC_Command]);
                        int cmd = readData[cmdIndex];

                        int pcStatusIndex = Convert.ToInt16(readData[PlcAddressMap.PC_Status]);
                        int status = readData[pcStatusIndex];

                        if (cmd != 0 && status == 0 && cmd != 9000)
                            PlcCommandReceived((PlcCommand)cmd, readData);
                    }

                }

                Thread.Sleep(50);
            }
        }

        private void PlcCommandReceived(PlcCommand cmd, int[] readData)
        {
            throw new NotImplementedException();
        }

        private void StopScenarioTask()
        {
            if (CommandTask == null)
                return;

            CommandTaskCancellationTokenSource.Cancel();
            CommandTask = null;
        }

    }
}
