using COG.Settings;
using COG.UI.Forms;
using System.IO;
using System.Windows.Forms;

namespace COG.Class
{
    public class InspModel
    {
        public bool Load(string newModelName)
        {
            string modelPath = StaticConfig.ModelPath;
            string buf;
            if (!Directory.Exists(modelPath))
            {
                MessageBox.Show(modelPath + "not Directory", "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!Directory.Exists(modelPath + newModelName))
            {
                return false;
            }
            if (AppsConfig.Instance().ProjectName == newModelName)
            {
                return true;
            }

            AppsConfig.Instance().ProjectName = newModelName;
            StaticConfig.SystemFile.SetData("SYSTEM", "LAST_PROJECT", AppsConfig.Instance().ProjectName);

            buf = modelPath + AppsConfig.Instance().ProjectName + "\\Model.ini";
            StaticConfig.ModelFile.SetFileName(buf);
            AppsConfig.Instance().ProjectInfo = StaticConfig.ModelFile.GetSData("PROJECT", "NAME");

            //ToDo: PLC
            //Main.WriteDevice(PLCDataTag.BASE_RW_ADDR + DEFINE.CURRENT_MODEL_CODE, Convert.ToInt16(Main.ProjectName));

            ProgressBarForm form = new ProgressBarForm();

            form.Message = "Unit";
            form.Maximum = 2;
            form.Show();
            form.ProgressMaxSet();

            //Main.ProgerssBar_Unit(Main.formProgressBar, DEFINE.AlignUnit_Max, true, 0);
            //for (int i = 0; i < Main.DEFINE.AlignUnit_Max; i++)
            //{
            //    //  for (int j = 0; j < Main.AlignUnit[i].m_AlignPatTagMax; j++)
            //    for (int j = AlignUnit[i].m_AlignPatTagMax - 1; j >= 0; j--)
            //    {
            //        AlignUnit[i].Load(j);
            //    }
            //    Main.ProgerssBar_Unit(Main.formProgressBar, DEFINE.AlignUnit_Max, true, i + 1);
            //}
            return true;
        }
    }
}
