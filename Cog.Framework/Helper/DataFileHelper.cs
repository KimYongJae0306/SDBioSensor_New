using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Cog.Framework.Helper
{
    public class DataFileHelper
    {
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(String section, String key, String def, StringBuilder retVal, int size, String filePath);

        [DllImport("kernel32.dll")]
        private static extern long WritePrivateProfileString(String section, String key, String val, String filePath);

        #region 속성
        private string FileName { get; set; } = "";
        #endregion

        #region 생성자
        public void SetFileName(String fileName)
        {
            FileName = fileName;
        }
        #endregion

        #region 메서드
        public void SetData(String section, String key, int dataValue)
        {
            WritePrivateProfileString(section, key, dataValue.ToString(), FileName);
        }

        public void SetData(String section, String key, double dataValue)
        {
            WritePrivateProfileString(section, key, dataValue.ToString(), FileName);
        }

        public void SetData(String section, String key, string dataValue)
        {
            WritePrivateProfileString(section, key, dataValue, FileName);
        }

        public void SetData(String section, String key, string dataValue, string fileName)
        {
            long nRet = WritePrivateProfileString(section, key, dataValue, fileName);
            if (nRet == 0)
                nRet = Marshal.GetLastWin32Error();
        }

        public void SetData(String section, String key, bool dataValue)
        {
            WritePrivateProfileString(section, key, dataValue.ToString(), FileName);
        }

        public int GetIData(String section, String Key)
        {
            StringBuilder temp = new StringBuilder(80);
            GetPrivateProfileString(section, Key, "0", temp, 80, FileName);

            return Convert.ToInt32(temp.ToString());
        }

        public double GetFData(String Section, String Key)
        {
            StringBuilder temp = new StringBuilder(80);

            GetPrivateProfileString(Section, Key, "0", temp, 80, FileName);

            return Convert.ToDouble(temp.ToString());
        }

        public String GetSData(String Section, String Key)
        {
            StringBuilder temp = new StringBuilder(80);

            GetPrivateProfileString(Section, Key, " ", temp, 80, FileName);

            return temp.ToString();
        }

        public bool GetBData(String Section, String Key)
        {
            bool Ret;
            StringBuilder temp = new StringBuilder(80);

            GetPrivateProfileString(Section, Key, "false", temp, 80, FileName);

            return bool.TryParse(temp.ToString(), out Ret);
        }
        #endregion
    }
}
