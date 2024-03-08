using COG.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Helper
{
    public static class LoggerHelper
    {
        private static object _syncLock_Log = new object();

        public static void Save_SystemLog(string message, LogType loggerType)
        {
            string fileName = "";
            string folder = StaticConfig.LogDataPath + DateTime.Now.ToString("yyyyMMdd") + "\\";
            if (!Directory.Exists(StaticConfig.LogDataPath))
                Directory.CreateDirectory(StaticConfig.LogDataPath);

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string Date = DateTime.Now.ToString("[MM_dd HH:mm:ss:fff] ");
            lock (_syncLock_Log)
            {
                try
                {
                    switch (loggerType)
                    {
                        case LogType.Cmd:
                            fileName = "CMD.txt";
                            message = Date + message;
                            break;
                        case LogType.Inspection:
                            fileName = "INSPECTION.txt";
                            message = Date + message;
                            break;
                        case LogType.Pixel:
                            fileName = "PIXEL_ROBOT.txt";
                            message = Date + message;
                            break;

                        case LogType.Align:
                            fileName = "ALIGN.txt";
                            message = Date + message;
                            break;
                        case LogType.Error:
                            fileName = "ERROR.txt";
                            message = Date + message;
                            break;
                        case LogType.TabLength:
                            fileName = "TABLENGTH.txt";
                            message = Date + message;
                            break;
                        case LogType.MarkSocre:
                            fileName = "MARK_SCORE.txt";
                            message = Date + message;
                            break;
                        case LogType.LightCtrl:
                            fileName = "LIGHT_CTRL.txt";
                            message = Date + message;
                            break;
                        case LogType.ChangeParam:
                            fileName = "ChangePara.txt";
                            message = Date + message;
                            break;
                        case LogType.Data:
                            fileName = "Data.csv";
                            message = Date + message;
                            break;
                   
                    }

                    StreamWriter SW = new StreamWriter(folder + fileName, true, Encoding.Unicode);
                    SW.WriteLine(message);
                    SW.Close();
                }
                catch
                {

                }
            }
        }

        public static void Save_Command(string message, LogType loggerType, int stageNo)
        {
            string nFileName = "";
            string nFolder = Path.Combine(StaticConfig.LogDataPath, DateTime.Now.ToString("yyyyMMdd"));

            if (!Directory.Exists(StaticConfig.LogDataPath))
                Directory.CreateDirectory(StaticConfig.LogDataPath);
            if (!Directory.Exists(nFolder))
                Directory.CreateDirectory(nFolder);

            lock (_syncLock_Log)
            {
                try
                {
                    switch (loggerType)
                    {
                        case LogType.Cmd:
                            nFileName = "_Command.txt";
                            break;
                        case LogType.Inspection:
                            nFileName = "_GapResult.txt";
                            break;
                        case LogType.Pixel:
                            nFileName = "_PIXEL_ROBOT.txt";
                            break;
                        case LogType.Align:
                            nFileName = "_AlignResult.txt";
                            break;
                        case LogType.Error:
                            nFileName = "_ErrorList.txt";
                            break;
                        case LogType.TabLength:
                            nFileName = "_Length.txt";
                            break;
                        case LogType.Data:
                            nFileName = "_Data.csv";
                            break;
                        //case LogType.MARKSCORE:
                        //    nFileName = "_MarkScore.txt";
                            break;
                    }
                    string path = nFolder + $"\\INSPECTION_{stageNo + 1}" + nFileName;
                    StreamWriter SW = new StreamWriter(path, true, Encoding.UTF8);
                    SW.WriteLine(message);
                    SW.Close();
                }
                catch
                {

                }
            }
        }
    }

    public enum LogType
    {
        Cmd,
        Inspection,
        Pixel,
        Align,
        Error,
        TabLength,
        MarkSocre,
        LightCtrl,
        ChangeParam,
        Data,
    }
}
