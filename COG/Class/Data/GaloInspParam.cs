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
    public class GaloInspParam
    {
        public string ModelSection { get; set; } = "";

        public string LineVppTitleName { get; set; } = "";

        public string CircleVppTitleName { get; set; } = "";

        public int Count { get; set; }

        public List<GaloInspTool> GaloInspToolList { get; set; } = new List<GaloInspTool>();

        public void Dispose()
        {
            GaloInspToolList?.ForEach(x => x.Dispose());
            GaloInspToolList.Clear();
        }

        public GaloInspParam DeepCopy()
        {
            GaloInspParam param = new GaloInspParam();
            param.ModelSection = ModelSection;
            param.LineVppTitleName = LineVppTitleName;
            param.CircleVppTitleName = CircleVppTitleName;
            param.Count = Count;

            for (int i = 0; i < GaloInspToolList?.Count(); i++)
                GaloInspToolList.Add(GaloInspToolList[i].DeepCopy());

            return param;
        }

        public void Load(string modelDir)
        {
            string newModelSection = ModelSection + "_0";
            Count = StaticConfig.ModelFile.GetIData(newModelSection, "COUNT");
            for (int i = 0; i < Count; i++)
            {
                newModelSection = $"{ModelSection}_{i}";
                GaloInspTool galo = new GaloInspTool();
                galo.Type = (GaloInspType)StaticConfig.ModelFile.GetIData(newModelSection, $"INSPECTION TYPE" + i.ToString());
                galo.Distgnore = StaticConfig.ModelFile.GetIData(newModelSection, "Insp_Dist_Ingnore" + i.ToString());
                galo.SpecDistance = StaticConfig.ModelFile.GetFData(newModelSection, "Insp_Spec_Dist" + i.ToString());
                galo.SpecDistanceMax = StaticConfig.ModelFile.GetFData(newModelSection, "Insp_Spec_Dist_Max" + i.ToString());

                galo.DarkArea.ThresholdUse = StaticConfig.ModelFile.GetBData(newModelSection, "Insp_Edge_Threshold_Use" + i.ToString());
                galo.DarkArea.Threshold = StaticConfig.ModelFile.GetIData(newModelSection, "Insp_Edge_Threshold" + i.ToString());
                galo.DarkArea.TopCutPixel = StaticConfig.ModelFile.GetIData(newModelSection, "Insp_Top_Cut_Pixel" + i.ToString());
                galo.DarkArea.BottomCutPixel = StaticConfig.ModelFile.GetIData(newModelSection, "Insp_Bottom_Cut_Pixel" + i.ToString());
                galo.DarkArea.IgnoreSize = StaticConfig.ModelFile.GetIData(newModelSection, "Insp_Ignore_Size" + i.ToString());
                galo.DarkArea.MaskingValue = StaticConfig.ModelFile.GetIData(newModelSection, "Insp_Masking_Value" + i.ToString());
                galo.DarkArea.EdgeCaliperThreshold = StaticConfig.ModelFile.GetIData(newModelSection, "Insp_Edge_Caliper_TH" + i.ToString());
                galo.DarkArea.EdgeCaliperFilterSize = StaticConfig.ModelFile.GetIData(newModelSection, "Insp_Edge_Caliper_Filter_Size" + i.ToString());


                var lineVppFileName = Path.Combine(modelDir, $"{LineVppTitleName}_{i}.vpp");
                if (File.Exists(lineVppFileName))
                {
                    var tool = CogSerializer.LoadObjectFromFile(lineVppFileName) as CogFindLineTool;
                    galo.SetLineTool(tool);
                }

                var circleVppFileName = Path.Combine(modelDir, $"{CircleVppTitleName}_{i}.vpp");
                if (File.Exists(circleVppFileName))
                {
                    var tool = CogSerializer.LoadObjectFromFile(circleVppFileName) as CogFindCircleTool;
                    galo.SetCircleTool(tool);
                }
                GaloInspToolList.Add(galo);
            }
        }

        public void Save(string modelDir)
        {
            string newModelSection = ModelSection + "_0";
            Count = GaloInspToolList.Count;
            StaticConfig.ModelFile.SetData(newModelSection, "COUNT", Count);
            for (int i = 0; i < Count; i++)
            {
                var galo = GaloInspToolList[i];

                newModelSection = $"{ModelSection}_{i}";

                StaticConfig.ModelFile.SetData(newModelSection, $"INSPECTION TYPE" + i.ToString(), (int)galo.Type);
                StaticConfig.ModelFile.SetData(newModelSection, "Insp_Dist_Ingnore" + i.ToString(), galo.Distgnore);
                
                StaticConfig.ModelFile.SetData(newModelSection, "Insp_Spec_Dist" + i.ToString(), galo.SpecDistance);
                StaticConfig.ModelFile.SetData(newModelSection, "Insp_Spec_Dist_Max" + i.ToString(), galo.SpecDistanceMax);

                StaticConfig.ModelFile.SetData(newModelSection, "Insp_Edge_Threshold_Use" + i.ToString(), galo.DarkArea.ThresholdUse);
                StaticConfig.ModelFile.SetData(newModelSection, "Insp_Edge_Threshold" + i.ToString(), galo.DarkArea.Threshold);
                StaticConfig.ModelFile.SetData(newModelSection, "Insp_Top_Cut_Pixel" + i.ToString(), galo.DarkArea.TopCutPixel);
                StaticConfig.ModelFile.SetData(newModelSection, "Insp_Bottom_Cut_Pixel" + i.ToString(), galo.DarkArea.BottomCutPixel);
                StaticConfig.ModelFile.SetData(newModelSection, "Insp_Ignore_Size" + i.ToString(), galo.DarkArea.IgnoreSize);
                StaticConfig.ModelFile.SetData(newModelSection, "Insp_Masking_Value" + i.ToString(), galo.DarkArea.MaskingValue);
                StaticConfig.ModelFile.SetData(newModelSection, "Insp_Edge_Caliper_TH" + i.ToString(), galo.DarkArea.EdgeCaliperThreshold);
                StaticConfig.ModelFile.SetData(newModelSection, "Insp_Edge_Caliper_Filter_Size" + i.ToString(), galo.DarkArea.EdgeCaliperFilterSize);

                var lineVppFileName = Path.Combine(modelDir, $"{LineVppTitleName}_{i}.vpp");
                if (File.Exists(lineVppFileName))
                {
                    CogSerializer.SaveObjectToFile(galo.FindLineTool, lineVppFileName,
                                       typeof(System.Runtime.Serialization.Formatters.Binary.BinaryFormatter),
                                        CogSerializationOptionsConstants.ExcludeDataBindings);
                }

                var circleVppFileName = Path.Combine(modelDir, $"{CircleVppTitleName}_{i}.vpp");
                if (File.Exists(circleVppFileName))
                {
                    CogSerializer.SaveObjectToFile(galo.FindCircleTool, circleVppFileName,
                                    typeof(System.Runtime.Serialization.Formatters.Binary.BinaryFormatter),
                                     CogSerializationOptionsConstants.ExcludeDataBindings);
                }
            }
        }
    }

   
}
