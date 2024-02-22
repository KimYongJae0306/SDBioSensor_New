using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG
{
    class SystemManager
    {
        #region 필드
        private static SystemManager _instance = null;

        private MainForm _mainForm = null;
        #endregion

        #region 속성

        #endregion

        #region 이벤트

        #endregion

        #region 델리게이트

        #endregion

        #region 생성자

        #endregion

        #region 메서드
        public static SystemManager Instance()
        {
            if (_instance == null)
                _instance = new SystemManager();

            return _instance;
        }

        public void SetMainForm(MainForm mainForm)
        {
            _mainForm = mainForm;
        }
        #endregion
    }
}
