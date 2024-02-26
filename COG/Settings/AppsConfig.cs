

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Settings
{
    public partial class AppsConfig
    {
       
    }

    public partial class AppsConfig
    {
        #region 필드
        private static AppsConfig _instance = null;
        #endregion

        #region 속성
        public string ProjectName { get; set; } = "";

        public string ProjectInfo { get; set; } = ""; // 모델 이름

        public string m_EngineerPassword { get; set; } = "";

        public string m_MakerPassword { get; set; } = "";

        public int m_OldLogCheckPeriod { get; set; } = 0;

        public int m_OldLogCheckSpace { get; set; } = 0;

        public int m_RetryCount { get; set; } = 0;

        public bool m_RetryUse { get; set; } = false;

        public bool Overlay_Image_Onf { get; set; } = false;

        public bool BMP_ImageSave_Onf { get; set; } = false;

        public bool LogMsg_Onf { get; set; } = false;

        public bool LengthCheck_Onf { get; set; } = false;

        #endregion

        #region 이벤트
        #endregion

        #region 델리게이트
        #endregion

        #region 생성자
        #endregion

        #region 메서드
        public static AppsConfig Instance()
        {
            if (_instance == null)
            {
                _instance = new AppsConfig();
            }

            return _instance;
        }

        public void Initialize()
        {
            Load();
        }

        public void Load()
        {
            var systemFile = StaticConfig.SystemFile;

            ProjectName = systemFile.GetSData("SYSTEM", "LAST_PROJECT");

            m_EngineerPassword = systemFile.GetSData("PERMISSION_ENGINEER", "PASSWORD");
            m_MakerPassword = systemFile.GetSData("PERMISSION_MAKER", "PASSWORD");
            m_OldLogCheckPeriod = systemFile.GetIData("OPTION", "OLD_LOG_PERIOD");
            m_OldLogCheckSpace = systemFile.GetIData("OPTION", "OLD_LOG_SPACE");

            m_RetryCount = systemFile.GetIData("RETRY", "COUNT");
            m_RetryUse = systemFile.GetBData("RETRY", "USE");

            Overlay_Image_Onf = systemFile.GetBData("OPTION", "OVELAY_IMAGE_SAVE");
            BMP_ImageSave_Onf = systemFile.GetBData("OPTION", "BMP");
            LogMsg_Onf = systemFile.GetBData("OPTION", "GAP_LOG_MSG");
            LengthCheck_Onf = systemFile.GetBData("OPTION", "L_CHECK");

            var modelFile = StaticConfig.ModelFile;
            ProjectInfo = modelFile.GetSData("PROJECT", "NAME");
        }

        public void Save()
        {
            var systemFile = StaticConfig.SystemFile;

            systemFile.SetData("SYSTEM", "LAST_PROJECT", m_EngineerPassword);

            systemFile.SetData("PERMISSION_ENGINEER", "PASSWORD", m_EngineerPassword);
            systemFile.SetData("PERMISSION_MAKER", "PASSWORD", m_EngineerPassword);
            systemFile.SetData("OPTION", "OLD_LOG_PERIOD", m_OldLogCheckPeriod);
            systemFile.SetData("OPTION", "OLD_LOG_SPACE", m_OldLogCheckSpace);

            systemFile.SetData("RETRY", "COUNT", m_RetryCount);
            systemFile.SetData("RETRY", "USE", m_RetryUse);

            systemFile.SetData("OPTION", "OVELAY_IMAGE_SAVE", Overlay_Image_Onf);
            systemFile.SetData("OPTION", "BMP", BMP_ImageSave_Onf);
            systemFile.SetData("OPTION", "GAP_LOG_MSG", LogMsg_Onf);
            systemFile.SetData("OPTION", "L_CHECK", LengthCheck_Onf);

            var modelFile = StaticConfig.ModelFile;
            modelFile.GetSData("PROJECT", "NAME");
        }

        #endregion
    }
}
