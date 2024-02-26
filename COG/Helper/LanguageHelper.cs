using COG.Settings;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Helper
{
    public static class LanguageHelper
    {
        public const string LANGUAGE_DIR = "Software\\JAStechLan"; //Regedit : HKEY_CURRENT_USER-> SOFTWARE-> JASTECHJAS_DAWIN

        public const int ENGLISH = 1033;//"en-us"
        public const int KOREA = 1042;//"ko-kr"
        public const int VIETNAM = 1066;//"vi-VN"
        public const int CHINA = 2052;//"zh-cn"

        public static void WriteRegistryLan(string _Mode)
        {
            RegistryKey regKey = Registry.CurrentUser.CreateSubKey(LanguageHelper.LANGUAGE_DIR, RegistryKeyPermissionCheck.ReadWriteSubTree);

            regKey.SetValue("language", _Mode, RegistryValueKind.String);
        }
        public static int ReadRegistryLan()
        {
            string _Mode = "language";
            RegistryKey reg = Registry.CurrentUser;
            reg = reg.OpenSubKey(LanguageHelper.LANGUAGE_DIR, true);

            if (reg == null)
                return LanguageHelper.KOREA;

            if (null != reg.GetValue(_Mode))
            {
                return Convert.ToInt32(reg.GetValue(_Mode));
            }
            else
            {
                return Convert.ToInt32(LanguageHelper.KOREA);
            }
        }
    }
}
