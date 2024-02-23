

using System;
using System.Collections.Generic;
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

        private void Load()
        {
            var systemFile = StaticConfig.SystemFile;

            ProjectName = systemFile.GetSData("SYSTEM", "LAST_PROJECT");

            m_EngineerPassword = systemFile.GetSData("PERMISSION_ENGINEER", "PASSWORD");
            m_MakerPassword = systemFile.GetSData("PERMISSION_MAKER", "PASSWORD");

            var modelFile = StaticConfig.ModelFile;
            ProjectInfo = modelFile.GetSData("PROJECT", "NAME");
        }

        public void Save()
        {
            var systemFile = StaticConfig.SystemFile;

            systemFile.SetData("SYSTEM", "LAST_PROJECT", m_EngineerPassword);

            systemFile.SetData("PERMISSION_ENGINEER", "PASSWORD", m_EngineerPassword);
            systemFile.SetData("PERMISSION_MAKER", "PASSWORD", m_EngineerPassword);

            var modelFile = StaticConfig.ModelFile;
            modelFile.GetSData("PROJECT", "NAME");
        }

        #endregion
    }
}
