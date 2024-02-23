using Cog.Framework.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cog.Framework.Core
{
    public static class LogType
    {
        public const string CMD = "CMD";
        public const string INSPECTION = "INSPECTION";
        public const string PIXEL = "PIXEL_ROBOT";
        public const string ALIGN = "ALIGN";
        public const string ERROR = "ERROR";
        public const string TABLENGTH = "TABLENGTH";
        public const string DATA = "DATA";
        public const string MARKSCORE = "MARK_SCORE";
        public const string LIGHTCTRL = "LIGHT_CTRL";
        public const string CHANGEPARA = "CHANGE_PARA";
    }

    public static class LogHelper
    {
        private static object _lock = new object();

        public static void Save_SystemLog(string nMessage, string nType)
        {
            string nFileName = "";
            string folderPath = StaticConfig.LogDataPath + DateTime.Now.ToString("yyyyMMdd") + "\\";

            if (!Directory.Exists(StaticConfig.LogDataPath))
                Directory.CreateDirectory(StaticConfig.LogDataPath);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string Date = DateTime.Now.ToString("[MM_dd HH:mm:ss:fff] ");

            lock (_lock)
            {
                try
                {
                    switch (nType)
                    {
                        case LogType.CMD:
                            nFileName = "SystemLog.txt";
                            nMessage = Date + nMessage;
                            break;
                        case LogType.LIGHTCTRL:
                            nFileName = "CommsLog.txt";
                            nMessage = Date + nMessage;
                            break;
                    }

                    StreamWriter SW = new StreamWriter(folderPath + nFileName, true, Encoding.Unicode);
                    SW.WriteLine(nMessage);
                    SW.Close();
                }
                catch
                {

                }
            }
        }
    }
}
