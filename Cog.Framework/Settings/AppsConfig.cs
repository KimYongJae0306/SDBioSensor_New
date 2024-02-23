

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cog.Framework.Settings
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
        #endregion
    }
}
