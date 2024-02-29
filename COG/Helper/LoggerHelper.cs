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

        public static void Save_SystemLog(string nMessage, LogType loggerType)
        {
            string nFolder;
            string nFileName = "";
            nFolder = StaticConfig.LogDataPath + DateTime.Now.ToString("yyyyMMdd") + "\\";
            if (!Directory.Exists(StaticConfig.LogDataPath))
                Directory.CreateDirectory(StaticConfig.LogDataPath);

            if (!Directory.Exists(nFolder))
                Directory.CreateDirectory(nFolder);

            string Date;
            Date = DateTime.Now.ToString("[MM_dd HH:mm:ss:fff] ");

            lock (_syncLock_Log)
            {
                try
                {
                    switch (loggerType)
                    {
                        case LogType.Cmd:
                            nFileName = "CMD.txt";
                            nMessage = Date + nMessage;
                            break;
                        case LogType.Inspection:
                            nFileName = "INSPECTION.txt";
                            nMessage = Date + nMessage;
                            break;
                        case LogType.Pixel:
                            nFileName = "PIXEL_ROBOT.txt";
                            nMessage = Date + nMessage;
                            break;

                        case LogType.Align:
                            nFileName = "ALIGN.txt";
                            nMessage = Date + nMessage;
                            break;
                        case LogType.Error:
                            nFileName = "ERROR.txt";
                            nMessage = Date + nMessage;
                            break;
                        case LogType.TabLength:
                            nFileName = "TABLENGTH.txt";
                            nMessage = Date + nMessage;
                            break;
                        case LogType.MarkSocre:
                            nFileName = "MARK_SCORE.txt";
                            nMessage = Date + nMessage;
                            break;
                        case LogType.LightCtrl:
                            nFileName = "LIGHT_CTRL.txt";
                            nMessage = Date + nMessage;
                            break;
                        case LogType.ChangeParam:
                            nFileName = "ChangePara.txt";
                            nMessage = Date + nMessage;
                            break;
                        case LogType.Data:
                            nFileName = "Data.csv";
                            nMessage = Date + nMessage;
                            break;
                   
                    }

                    StreamWriter SW = new StreamWriter(nFolder + nFileName, true, Encoding.Unicode);
                    SW.WriteLine(nMessage);
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
