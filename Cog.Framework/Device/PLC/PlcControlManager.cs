using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cog.Framework.Device.PLC
{
    public class PlcControlManager
    {
        #region 필드
        private static PlcControlManager _instance = null;

        private DeviceName _deviceName = DeviceName.R;
        #endregion

        #region 속성
        private PlcControl MCClient_READ = new PlcControl();

        private PlcControl MCClient_WRITE = new PlcControl();
        #endregion

        #region 이벤트
        #endregion

        #region 델리게이트
        #endregion

        #region 생성자
        #endregion

        #region 메서드
        public static PlcControlManager Instance()
        {
            if (_instance == null)
                _instance = new PlcControlManager();

            return _instance;
        }

        public void Initialize()
        {
            _deviceName = DeviceName.R;
        }

        public void WriteDevice(int device, int lplData)
        {
            try
            {
                int[] Data = new int[1];
                Data[0] = lplData;

                MCClient_WRITE.WriteDeviceBlock(SubCommand.Word, _deviceName, device.ToString(), Data);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + ex.Message + ex.StackTrace);
            }
            finally { }
        }

        public static void PlcReadThread()
        {
        }
        #endregion
    }
}
