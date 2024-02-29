using COG.Class.Units;
using COG.Settings;
using Cognex.VisionPro;
using Cognex.VisionPro.Caliper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Class.Data
{
    public class FilmAlignParam
    {
        public string ModelSection { get; set; } = "";

        public string VppTitleName { get; set; } = "";

        public double AlignSpec_T { get; set; } = 0;

        public double AmpModuleDistanceX { get; set; } = 0;

        public double FilmAlignSpecX { get; set; } = 0;

        public List<FilmAlignTool> ToolList { get; set; } = new List<FilmAlignTool>();

        public FilmAlignTool GetTool(FilmROIType type)
        {
            return ToolList.Where(x => x.FilmROIType == type).FirstOrDefault();
        }

        public void Dispose()
        {
            ToolList?.ForEach(x => x.Dispose());
            ToolList?.Clear();
        }

        public FilmAlignParam DeepCopy()
        {
            FilmAlignParam filmAlign = new FilmAlignParam();
            filmAlign.AlignSpec_T = AlignSpec_T;
            filmAlign.AmpModuleDistanceX = AmpModuleDistanceX;
            filmAlign.FilmAlignSpecX = FilmAlignSpecX;

            for (int i = 0; i < ToolList?.Count(); i++)
                filmAlign.ToolList.Add(ToolList[i].DeepCopy());

            return filmAlign;
        }

        public void Load(string modelDir)
        {
            AlignSpec_T = StaticConfig.ModelFile.GetFData(ModelSection, "ROIFinealign_T_Spec");
            AmpModuleDistanceX = StaticConfig.ModelFile.GetFData(ModelSection, "Object_Distance_X");
            FilmAlignSpecX = StaticConfig.ModelFile.GetFData(ModelSection, "Object_Distance_X_Spec");

            foreach (var filmTool in ToolList)
            {
                var vppFileName = Path.Combine(modelDir, $"{VppTitleName}_{filmTool.Index}.vpp");
                if (File.Exists(vppFileName))
                {
                    var tool = CogSerializer.LoadObjectFromFile(vppFileName) as CogFindLineTool;
                    filmTool.SetTool(tool);
                }
            }
        }
    }
}
