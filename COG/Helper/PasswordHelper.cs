using Microsoft.Win32;
using System;

namespace COG.Helper
{
    public static class PasswordHelper
    {
        public const string PASSWORD_DIR = "Software\\JAStech"; //Regedit : HKEY_CURRENT_USER-> SOFTWARE-> JASTECHJAS_DAWIN

        public static void WriteRegistry(string _Mode, string _Password)
        {
            RegistryKey regKey = Registry.CurrentUser.CreateSubKey(PASSWORD_DIR, RegistryKeyPermissionCheck.ReadWriteSubTree);
            regKey.SetValue(_Mode, _Password, RegistryValueKind.String);
        }
        public static string ReadRegistry(string _Mode)
        {
            RegistryKey reg = Registry.CurrentUser;
            reg = reg.OpenSubKey(PASSWORD_DIR, true);

            if (reg == null) return "";

            if (null != reg.GetValue(_Mode))
            {
                return Convert.ToString(reg.GetValue(_Mode));
            }
            else
            {
                return "";
            }
        }
        public static void DeleteRegistry()
        {
            Registry.CurrentUser.DeleteSubKey(PASSWORD_DIR);
        }
    }
}
