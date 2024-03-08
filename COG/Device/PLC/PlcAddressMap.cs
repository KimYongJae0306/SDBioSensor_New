using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Device.PLC
{
    public static class PlcAddressMap
    {
        public const int PC_Status = 6;

        public const int PLC_ModelNo = 100;

        public const int PLC_Time_Year = 100;
        public const int PLC_Time_Month = 101;
        public const int PLC_Time_Day = 102;
        public const int PLC_Time_Hour = 103;
        public const int PLC_Time_Minute = 104;
        public const int PLC_Time_Second = 105;

        public const int PLC_Command = 106;
    }

    public enum PlcCommonMap
    {
        PC_Model_No = 0,
        Vision_Ready = 4,
        Alive = 200,
    }

    public enum PlcCommand
    {
        StartInspection = 1100,
        Time_Change = 6000,
        Model_Change = 8000,
        Cmd_Clear = 9000,
    }
}
