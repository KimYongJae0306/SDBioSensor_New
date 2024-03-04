using COG.Class.Units;
using COG.Core;
using COG.Settings;
using COG.UI.Forms;
using Cognex.VisionPro;
using Cognex.VisionPro.SearchMax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace COG.Class
{
    public class InspModelService
    {
        public bool LoadModel(string newModelName, bool forceLoading)
        {
            string modelPath = StaticConfig.ModelPath;

            if (!Directory.Exists(modelPath))
            {
                MessageBox.Show(modelPath + "not Directory", "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!Directory.Exists(modelPath + newModelName))
            {
                return false;
            }
            //if (AppsConfig.Instance().ProjectName == newModelName && forceLoading == false)
            //{
            //    return true;
            //}

            InspModel inspModel = new InspModel();
            inspModel.Load(newModelName);

            ModelManager.Instance().CurrentModel = inspModel;

            return true;
        }

        public bool SaveModel(string newModelName, string newModelInfo)
        {
            InspModel inspModel = ModelManager.Instance().CurrentModel;
            if (inspModel == null)
                return false;

            string modelDir = Path.Combine(StaticConfig.ModelPath, newModelName);

            if (Directory.Exists(modelDir) == false)
                Directory.CreateDirectory(modelDir);

            inspModel.ModelName = newModelName;
            inspModel.ModelPath = Path.Combine(modelDir, "model.ini");
            inspModel.ModelInfo = newModelInfo;

            StaticConfig.ModelFile.SetFileName(inspModel.ModelPath);

            inspModel.Save(modelDir);

            return true;
        }

        public bool SaveModel()
        {

            InspModel inspModel = ModelManager.Instance().CurrentModel;
            if (inspModel == null)
                return false;

            inspModel.Save(StaticConfig.ModelPath);

            return true;
        }
    }
}
