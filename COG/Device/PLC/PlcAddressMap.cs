﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Device.PLC
{
    public class PlcAddressMap
    {
    }

    public enum BaseAddressMap
    {

    }

    public enum PlcCommonMap
    {
        PC_Model_No = 0,
        PLC_Model_No = 100,

        Vision_Ready = 4,

        PC_Status = 6,
        PLC_Command = 106,
        Alive = 200,

        PLC_Time = 300,
    }

    //public enum PlcCommonCommand
    //{
    //    Time_Change = 6000,
    //    Model_Change = 8000,
    //}

    public enum PlcCommand
    {
        StartInspection = 1100,
        Time_Change = 6000,
        Model_Change = 8000,
    }
}
