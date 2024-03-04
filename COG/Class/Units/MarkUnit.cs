using COG.Class.Data;
using COG.Settings;
using Cognex.VisionPro;
using Cognex.VisionPro.SearchMax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Class.Units
{
    public class MarkUnit
    {
        public int CamNo { get; set; } = 0;

        public bool[] Use { get; set; } = new bool[StaticConfig.PATTERN_MAX_COUNT];

        public AmpMark Amp = new AmpMark();

        public BondingMark Bonding = new BondingMark();

        public void Dispose()
        {
            Amp.Dispose();
            Bonding.Dispose();
        }

        public MarkUnit DeepCopy()
        {
            MarkUnit markUnit = new MarkUnit();
            markUnit.CamNo = CamNo;
            markUnit.Use = Use;
            markUnit.Amp = Amp.DeepCopy();
            markUnit.Bonding = Bonding.DeepCopy();

            return markUnit;
        }

        public void Load(string modelDir)
        {
            #region Amp Mark
            Amp.Score = StaticConfig.ModelFile.GetFData(Amp.ModelSection, "ACCEPT_SCORE");


            for (int i = 0; i < StaticConfig.PATTERN_MAX_COUNT; i++)
            {
                string key = "PATUSE" + i.ToString();
                Use[i] = StaticConfig.ModelFile.GetBData(Amp.VppTitleName, key);
            }

            foreach (var markTool in Amp.MarkToolList)
            {
                var vppFileName = Path.Combine(modelDir, $"{Amp.VppTitleName}_{markTool.Index}.vpp");
                if (File.Exists(vppFileName))
                {
                    var tool = CogSerializer.LoadObjectFromFile(vppFileName) as CogSearchMaxTool;
                    markTool.SetTool(tool);
                }
            }
            #endregion

            #region Bonding Mark
            Bonding.Score = StaticConfig.ModelFile.GetFData(Bonding.ModelSection, "ROIFinealign_MarkScore");

            foreach (var upMarkTool in Bonding.UpMarkToolList)
            {
                var vppFileName = Path.Combine(modelDir, $"{Bonding.VppTitleName}_0{upMarkTool.Index}.vpp");
                if (File.Exists(vppFileName))
                {
                    var tool = CogSerializer.LoadObjectFromFile(vppFileName) as CogSearchMaxTool;
                    upMarkTool.SetTool(tool);
                }
            }

            foreach (var downMarkTool in Bonding.DownMarkToolList)
            {
                var vppFileName = Path.Combine(modelDir, $"{Bonding.VppTitleName}_1{downMarkTool.Index}.vpp");
                if (File.Exists(vppFileName))
                {
                    var tool = CogSerializer.LoadObjectFromFile(vppFileName) as CogSearchMaxTool;
                    downMarkTool.SetTool(tool);
                }
            }
            #endregion
        }

        public void Save(string modelDir)
        {
            #region Amp Mark
            StaticConfig.ModelFile.SetData(Amp.ModelSection, "ACCEPT_SCORE", Amp.Score);

            for (int i = 0; i < StaticConfig.PATTERN_MAX_COUNT; i++)
            {
                string key = "PATUSE" + i.ToString();
                StaticConfig.ModelFile.SetData(Amp.VppTitleName, key, Use[i]);
            }
            
            foreach (var markTool in Amp.MarkToolList)
            {
                var vppFileName = Path.Combine(modelDir, $"{Amp.VppTitleName}_{markTool.Index}.vpp");
                if (File.Exists(vppFileName))
                {
                    CogSerializer.SaveObjectToFile(markTool.SearchMaxTool, vppFileName, typeof(System.Runtime.Serialization.Formatters.Binary.BinaryFormatter),
                                         CogSerializationOptionsConstants.ExcludeDataBindings);
                }
            }
            #endregion

            #region Bonding Mark
            StaticConfig.ModelFile.SetData(Bonding.ModelSection, "ROIFinealign_MarkScore", Bonding.Score);

            foreach (var upMarkTool in Bonding.UpMarkToolList)
            {
                var vppFileName = Path.Combine(modelDir, $"{Bonding.VppTitleName}_0{upMarkTool.Index}.vpp");
                if (File.Exists(vppFileName))
                {
                    CogSerializer.SaveObjectToFile(upMarkTool.SearchMaxTool, vppFileName, typeof(System.Runtime.Serialization.Formatters.Binary.BinaryFormatter),
                                     CogSerializationOptionsConstants.ExcludeDataBindings);
                }
            }

            foreach (var downMarkTool in Bonding.DownMarkToolList)
            {
                var vppFileName = Path.Combine(modelDir, $"{Bonding.VppTitleName}_1{downMarkTool.Index}.vpp");
                if (File.Exists(vppFileName))
                {
                    CogSerializer.SaveObjectToFile(downMarkTool.SearchMaxTool, vppFileName, typeof(System.Runtime.Serialization.Formatters.Binary.BinaryFormatter),
                                     CogSerializationOptionsConstants.ExcludeDataBindings);
                }
            }
            #endregion
        }
    }

    public class AmpMark : Mark
    {
        public List<MarkTool> MarkToolList { get; set; } = new List<MarkTool>();

        public void Dispose()
        {
            MarkToolList?.ForEach(x => x.Dispose());
            MarkToolList?.Clear();
        }

        public AmpMark DeepCopy()
        {
            AmpMark mark = new AmpMark();

            mark.ModelSection = ModelSection;
            mark.VppTitleName = VppTitleName;
            mark.CamNo = CamNo;
            mark.AlignType = AlignType;
            mark.Score = Score;

            for (int i = 0; i < MarkToolList?.Count(); i++)
                mark.MarkToolList.Add(MarkToolList[i].DeepCopy());

            return mark;
        }
    }

    public class BondingMark : Mark
    {
        public List<MarkTool> UpMarkToolList { get; set; } = new List<MarkTool>();

        public List<MarkTool> DownMarkToolList { get; set; } = new List<MarkTool>();

        public void Dispose()
        {
            UpMarkToolList.ForEach(x => x.Dispose());
            UpMarkToolList.Clear();

            DownMarkToolList.ForEach(x => x.Dispose());
            DownMarkToolList.Clear();
        }

        public BondingMark DeepCopy()
        {
            BondingMark mark = new BondingMark();

            mark.ModelSection = ModelSection;
            mark.VppTitleName = VppTitleName;
            mark.CamNo = CamNo;
            mark.AlignType = AlignType;
            mark.Score = Score;

            for (int i = 0; i < UpMarkToolList.Count(); i++)
                mark.UpMarkToolList.Add(UpMarkToolList[i].DeepCopy());

            for (int i = 0; i < DownMarkToolList.Count(); i++)
                mark.DownMarkToolList.Add(DownMarkToolList[i].DeepCopy());

            return mark;
        }
    }
}
