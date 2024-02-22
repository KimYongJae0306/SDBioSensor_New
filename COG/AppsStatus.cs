using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG
{
    public class AppsStatus
    {
        #region 필드
        private static AppsStatus _instance = null;
        #endregion

        #region 속성
    
        #endregion

        #region 메서드
        public static AppsStatus Instance()
        {
            if (_instance == null)
            {
                _instance = new AppsStatus();
            }

            return _instance;
        }

   
        #endregion
    }
}
