using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cog.Framework
{
    public class AppsStatus
    {
        #region 필드
        private static AppsStatus _instance = null;
        #endregion

        #region 속성
        public MC_STATUS MC_STATUS { get; set; } = MC_STATUS.STOP;

        public UI_STATUS UI_STATUS { get; set; } = UI_STATUS.MAINFORM;

        public User CurrentUser { get; set; } = User.OPERATOR;

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

    public enum MC_STATUS
    {
        STOP = 0,
        RUN = 1,
        ERROR = 2,
        WARNING = 3,
        RESET = 4,
        //TEACHFORM = 5,
        //MAINFORM = 6,
        //SETUPFORM = 7,
        //LIVEFORM = 8,
        //CAMERAFORM = 9,
        //RCSFORM = 10,
        //PERMISSIONFORM = 11,
    }

    public enum UI_STATUS
    {
        TEACHFORM = 5,
        MAINFORM = 6,
        SETUPFORM = 7,
        LIVEFORM = 8,
        CAMERAFORM = 9,
        RCSFORM = 10,
        PERMISSIONFORM = 11,
    }

    public enum User
    {
        OPERATOR = 1,
        ENGINEER = 2,
        MAKER = 3,
    }
}
