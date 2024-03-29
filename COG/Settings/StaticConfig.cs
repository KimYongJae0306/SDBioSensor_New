﻿#define GALO_PC1_MODE // 5공정
//#define GALO_PC2_MODE // 구미 장비

#define SDBIO_VENT  //전면
//#define SDBIO_PATH  //후면

using COG.Core;
using COG.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Settings
{
    public static partial class StaticConfig
    {
        public const bool VirtualMode = true;

        public const double PixelResolution = 13.36;

        public const int STAGE_MAX_COUNT = 2;
        public const int PATTERN_MAX_COUNT = 6; // 0번은 MainMark, 나머지는 SubMark

        public const int FILM_ALIGN_MAX_COUNT = 4;

        public const int PLC_READ_SIZE = 310;
        public const int CMD_CHECK_TIMEOUT = 1000;
#if GALO_PC1_MODE
        public const string PROGRAM_TYPE = "ATT_AREA_PC1";
        public const int CAM_COUNT  = 2;
        public const int STAGE_COUNT = 2;
        public const int LIGHT_COUNT = 1;
#endif
#if GALO_PC2_MODE
        public const string PROGRAM_TYPE = "ATT_AREA_PC2";
        public const int CAM_COUNT  = 2;
        public const int STAGE_COUNT = 1;
        public const int LIGHT_COUNT = 1;
#endif
#if SDBIO_VENT
        public const int BASE_ADDR = 27000;
#endif
#if SDBIO_PATH
        public const int PLC_RW_ADDR = 28000;
        public const int PC_BaseAddress = 28000;
#endif

        public const string SYS_DATADIR = "D:\\Systemdata_" + PROGRAM_TYPE + "\\";
        public const string MODEL_DATADIR = "VISION";
        public const string LOG_DATADIR = "logdata\\";
        public const string ERROR_DATADIR = "Error_Data\\";
        public const string CAM_SETDIR = "VPP_CAM\\";

        public const string IMAGE_FILE = SYS_DATADIR + "1-1.bmp";//"QDIDB.idb";//"D:\\SystemData\\20.idb";

        public const int M_1CAM2SHOT = 200;   // 1 camera 2 shot , Center Target      

        public const string FontStyle = "test";
        public const float FontSize = 35.0f;

        public const int MX_ARRAY_RSTAT_OFFSET = 200;
        public const int MODULED_NUM = 80;
    }


    public static partial class StaticConfig
    {
        public static PLCTag PLCTag { get; set; } = new PLCTag();

        public static string SysDataPath { get; set; }

        public static string ModelPath { get; set; }

        public static string LogDataPath { get; set; }

        public static string ErrDataPath { get; set; }

        public static string CamDataPath { get; set; }

        public static DataFileHelper SystemFile = new DataFileHelper();

        public static DataFileHelper OldLogCheckFile = new DataFileHelper();

        public static DataFileHelper ModelFile = new DataFileHelper();

        public static void Initialize()
        {
            SetUIDesign();

            SysDataPath = SYS_DATADIR;
            ModelPath = SYS_DATADIR + "MODEL_" + MODEL_DATADIR + "\\";
            LogDataPath = SYS_DATADIR + LOG_DATADIR;
            ErrDataPath = SYS_DATADIR + ERROR_DATADIR;
            CamDataPath = SYS_DATADIR + CAM_SETDIR;

            string buf = SysDataPath + "SYSTEM_" + MODEL_DATADIR + ".ini";
            SystemFile.SetFileName(buf);

            AppsConfig.Instance().ProjectName = SystemFile.GetSData("SYSTEM", "LAST_PROJECT");

            buf = SysDataPath + "OLD_LOG_CHECK_FILE.dat";
            OldLogCheckFile.SetFileName(buf);

            buf = ModelPath + AppsConfig.Instance().ProjectName + "\\Model.ini";
            ModelFile.SetFileName(buf);

            if (!Directory.Exists(SysDataPath))
                Directory.CreateDirectory(SysDataPath);
            if (!Directory.Exists(CamDataPath))
                Directory.CreateDirectory(CamDataPath);
            if (!Directory.Exists(ModelPath))
                Directory.CreateDirectory(ModelPath);
            if (!Directory.Exists(LogDataPath))
                Directory.CreateDirectory(LogDataPath);
            if (!Directory.Exists(ErrDataPath))
                Directory.CreateDirectory(ErrDataPath);
            if (!Directory.Exists(ModelPath))
                Directory.CreateDirectory(ModelPath);

            AppsStatus.Instance().MC_STATUS = MC_STATUS.STOP;
        }

        private static void SetUIDesign()
        {
            if(StaticConfig.PROGRAM_TYPE == "ATT_AREA_PC1")
            {
                UIDesign.VIEW_NAME = new string[] { "INSPECTION 1", "INSPECTION 3", "INSPECTION 2", "INSPECTION 4" };
                UIDesign.VIEW_Pos = new string[] { "1-1", "1-3", "1-2", "1-4" };
                UIDesign.VIEW_Size = new string[] { "1*2", "1*2", "1*2", "1*2" };
                UIDesign.VIEW_WIDTH_CNT = new int[] { 4, 2 };
                UIDesign.VIEW_TAB_NAME = new string[] { "CAM 1 (INSPECTION 1,2)", "CAM 2 (INSPECTION 3,4)" };
            }
            else
            {

            }
        }
    }

    public class PLCTag
    {
        public int[] BData = new int[StaticConfig.PLC_READ_SIZE];

        public Int16[] RData = new Int16[StaticConfig.PLC_READ_SIZE];

        public int[] SData = new int[StaticConfig.PLC_READ_SIZE];
    }

}
