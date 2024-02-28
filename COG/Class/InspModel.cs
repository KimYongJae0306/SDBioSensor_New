using COG.Class.Units;
using COG.Device.PLC;
using COG.Settings;
using COG.UI.Forms;
using Cognex.VisionPro;
using Cognex.VisionPro.Blob;
using Cognex.VisionPro.Caliper;
using Cognex.VisionPro.SearchMax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace COG.Core
{
    public partial class InspModel
    {
        public string ModelName { get; set; } = "";

        public string ModelInfo { get; set; } = "";

        public string ModelPath { get; set; } = "";

        public List<StageUnit> StageUnitList = new List<StageUnit>();

        public StageUnit GetStageUnit(string name)
        {
            var unit = StageUnitList.Where(x => x.Name == name).FirstOrDefault();

            return unit;
        }

        public void New()
        {
            // Todo : 모델 dispose
            if (StaticConfig.PROGRAM_TYPE == "ATT_AREA_PC1")
            {
                for (int i = 0; i < StaticConfig.STAGE_MAX_COUNT; i++)
                {
                    StageUnit stageUnit = new StageUnit();
                    stageUnit.Name = "INSPECTION_" + (i + 1).ToString();

                    #region Amp Mark
                    var leftAmpMarkTool = CreateMarkTool();
                    stageUnit.Left.Mark.Amp.ModelSection = stageUnit.Name + "_0_CAMERA___UP";
                    stageUnit.Left.Mark.Amp.VppTitleName = stageUnit.Name + "_0_CAMERA___UP";
                    stageUnit.Left.Mark.Amp.MarkToolList = leftAmpMarkTool;

                    var rightAmpMarkTool = CreateMarkTool();
                    stageUnit.Right.Mark.Amp.ModelSection = stageUnit.Name + "_1_CAMERA___UP";
                    stageUnit.Right.Mark.Amp.VppTitleName = stageUnit.Name + "_1_CAMERA___UP";
                    stageUnit.Right.Mark.Amp.MarkToolList = rightAmpMarkTool;
                    #endregion

                    #region Bonding Mark
                    var leftUpBondingMarkTool = CreateMarkTool();
                    var leftDownBondingMarkTool = CreateMarkTool();

                    stageUnit.Left.Mark.Bonding.ModelSection = stageUnit.Name + "_0_CAMERA___UP";
                    stageUnit.Left.Mark.Bonding.VppTitleName = "ROIFineAlign" + stageUnit.Name + "_0_CAMERA___UP";
                    stageUnit.Left.Mark.Bonding.UpMarkToolList = leftUpBondingMarkTool;
                    stageUnit.Left.Mark.Bonding.DownMarkToolList = leftDownBondingMarkTool;

                    var rightUpBondingMarkTool = CreateMarkTool();
                    var rightDownBondingMarkTool = CreateMarkTool();

                    stageUnit.Right.Mark.Bonding.ModelSection = stageUnit.Name + "_1_CAMERA___UP";
                    stageUnit.Right.Mark.Bonding.VppTitleName = "ROIFineAlign" + stageUnit.Name + "_1_CAMERA___UP";
                    stageUnit.Right.Mark.Bonding.UpMarkToolList = leftUpBondingMarkTool;
                    stageUnit.Right.Mark.Bonding.DownMarkToolList = leftDownBondingMarkTool;
                    #endregion

                    StageUnitList.Add(stageUnit);
                }
            }
            else
            {

            }
        }

        public bool Load(string newModelName)
        {
            string modelPath = StaticConfig.ModelPath;

            New();

            AppsConfig.Instance().ProjectName = newModelName;
            StaticConfig.SystemFile.SetData("SYSTEM", "LAST_PROJECT", AppsConfig.Instance().ProjectName);

            ModelName = newModelName;

            string buf = modelPath + AppsConfig.Instance().ProjectName + "\\Model.ini";
            StaticConfig.ModelFile.SetFileName(buf);
            AppsConfig.Instance().ProjectInfo = StaticConfig.ModelFile.GetSData("PROJECT", "NAME");
            ModelInfo = AppsConfig.Instance().ProjectInfo;

            ModelPath = buf;

            //ToDo: PLC
            //Main.WriteDevice(PLCDataTag.BASE_RW_ADDR + DEFINE.CURRENT_MODEL_CODE, Convert.ToInt16(Main.ProjectName));

            SystemManager.Instance().ShowProgerssBar(StaticConfig.STAGE_COUNT, true, 0);

            string modelDir = modelPath + AppsConfig.Instance().ProjectName;
            for (int stageIndex = 0; stageIndex < StaticConfig.STAGE_COUNT; stageIndex++)
            {
                LoadUnitParam(stageIndex);
                LoadMark(modelDir, stageIndex);

                SystemManager.Instance().ShowProgerssBar(StaticConfig.STAGE_COUNT, true, stageIndex + 1);
            }

            ModelManager.Instance().CurrentModel = this;

            return true;
        }

        public bool Save(string modelDir)
        {
            StaticConfig.SystemFile.SetData("SYSTEM", "LAST_PROJECT", ModelName);
            StaticConfig.ModelFile.SetData("PROJECT", "NAME", ModelInfo);

            for (int i = 0; i < StageUnitList.Count; i++)
            {
                SaveAmpMark(modelDir, i);
                SaveBondingMark(modelDir, i);
                SaveUnitParam(StageUnitList[i]);
            }

            return true;
        }
    }

    public partial class InspModel
    {
        private List<MarkTool> CreateMarkTool()
        {
            List<MarkTool> markToolList = new List<MarkTool>();

            for (int subIndex = 0; subIndex < StaticConfig.PATTERN_MAX_COUNT; subIndex++)
            {
                MarkTool markTool = new MarkTool();
                markTool.Index = subIndex;
                markToolList.Add(markTool);
            }

            return markToolList;
        }

        private void LoadMark(string modelDir, int stageIndex)
        {
            var stageUnit = StageUnitList[stageIndex];
                #region Left
                stageUnit.Left.Mark.Load(modelDir);
                stageUnit.Right.Mark.Load(modelDir);
                //stageUnit.Left.Mark.Amp.MarkToolList.Load();
                //var leftAmpMark = stageUnit.LeftUnit.AmpMark;
                //var leftVppFileName = Path.Combine(modelDir, $"{leftAmpMark.Mark.Name}_{i}.vpp");

                //if (File.Exists(leftVppFileName))
                //{
                //    var markTag = leftAmpMark.Mark.TagList[i];
                //    var tool = CogSerializer.LoadObjectFromFile(leftVppFileName) as CogSearchMaxTool;
                //    markTag.SetTool(tool);
                //}
                #endregion

                #region Right

                //stageUnit.RightUnit.AmpMark.Mark.Load();
                //var rightAmpMark = stageUnit.RightUnit.AmpMark;
                //var rightVppFileName = Path.Combine(modelDir, $"{rightAmpMark.Mark.Name}_{i}.vpp");

                //if (File.Exists(rightVppFileName))
                //{
                //    var markTag = rightAmpMark.Mark.TagList[i];
                //    var tool = CogSerializer.LoadObjectFromFile(rightVppFileName) as CogSearchMaxTool;
                //    markTag.SetTool(tool);
                //}
                #endregion
        }

        private void SaveAmpMark(string modelDir, int stageIndex)
        {
            var stageUnit = StageUnitList[stageIndex];
            for (int i = 0; i < StaticConfig.PATTERN_MAX_COUNT; i++)
            {
                //#region Left
                //var leftMarkUnit = stageUnit.LeftUnit.AmpMark;
                //var leftVppFileName = Path.Combine(modelDir, $"{leftMarkUnit.Mark.Name}_{i}.vpp");

                //var leftMarkTag = leftMarkUnit.Mark.TagList[i];
                //leftMarkTag.SaveTool(leftVppFileName);
                //#endregion

                //#region Right
                //var rightMarkUnit = stageUnit.RightUnit.AmpMark;
                //var rightVppFileName = Path.Combine(modelDir, $"{rightMarkUnit.Mark.Name}_{i}.vpp");

                //var rightMarkTag = rightMarkUnit.Mark.TagList[i];
                //rightMarkTag.SaveTool(rightVppFileName);
                //#endregion
            }
        }

        private void SaveBondingMark(string modelDir, int stageIndex)
        {
            var stageUnit = StageUnitList[stageIndex];
            for (int i = 0; i < StaticConfig.PATTERN_MAX_COUNT; i++)
            {
                //
                //#region Left
                //var leftMarkUnit = stageUnit.LeftUnit.BondingMark;
                //var leftVppFileName = Path.Combine(modelDir, $"{leftMarkUnit.Name}_{i:D2}.vpp");

                //var leftMarkTag = leftMarkUnit.TagList[i];
                //leftMarkTag.SaveTool(leftVppFileName);
                //#endregion

                //#region Right
                //var rightMarkUnit = stageUnit.RightUnit.BondingMark;
                //var rightVppFileName = Path.Combine(modelDir, $"{rightMarkUnit.Name}_{i:D2}.vpp");

                //var rightMarkTag = rightMarkUnit.TagList[i];
                //rightMarkTag.SaveTool(rightVppFileName);
                //#endregion
            }
        }

        private void LoadUnitParam(int stageIndex)
        {
            var stageUnit = StageUnitList[stageIndex];

            stageUnit.m_GD_ImageSave_Use = StaticConfig.ModelFile.GetBData(stageUnit.Name, "GD_IMAGE");
            stageUnit.m_NG_ImageSave_Use = StaticConfig.ModelFile.GetBData(stageUnit.Name, "NG_IMAGE");
        }

        private void SaveUnitParam(StageUnit stageUnit)
        {
            StaticConfig.ModelFile.SetData(stageUnit.Name, "GD_IMAGE", stageUnit.m_GD_ImageSave_Use);
            StaticConfig.ModelFile.SetData(stageUnit.Name, "NG_IMAGE", stageUnit.m_NG_ImageSave_Use);
        }
    }
  
    public class LightCtrlParamemter
    {
    }

    public class ETC
    {
        public bool m_Manu_Match_Use { get; set; }
        public bool m_UseLineMax { get; set; }

        public bool m_UseCustomCross { get; set; }
        public int m_CustomCross_X { get; set; }
        public int m_CustomCross_Y { get; set; }

        public void Load(string spection)
        {
            var systemFile = StaticConfig.SystemFile;
            var modelFile = StaticConfig.ModelFile;

            m_Manu_Match_Use = modelFile.GetBData(spection, "MANU_MATCH_USE");
            m_UseLineMax = modelFile.GetBData(spection, "LINEMAX_USE");

            m_UseCustomCross = modelFile.GetBData(spection, "CUSTOM_CROSS_USE");
            m_CustomCross_X = modelFile.GetIData(spection, "CUSTOM_CROSS_X");
            m_CustomCross_Y = modelFile.GetIData(spection, "CUSTOM_CROSS_Y");
        }

        public void Save(string spection)
        {
            var modelFile = StaticConfig.ModelFile;
            modelFile.SetData(spection, "MANU_MATCH_USE", m_Manu_Match_Use);
            modelFile.SetData(spection, "LINEMAX_USE", m_UseLineMax);

            modelFile.SetData(spection, "CUSTOM_CROSS_USE", m_UseCustomCross);
            modelFile.SetData(spection, "CUSTOM_CROSS_X", m_CustomCross_X);
            modelFile.SetData(spection, "CUSTOM_CROSS_Y", m_CustomCross_Y);
        }
    }

    public class StageUnit 
    {
        public string Name { get; set; }

        public bool m_GD_ImageSave_Use { get; set; }

        public bool m_NG_ImageSave_Use { get; set; }

        public Unit Left { get; set; } = new Unit();

        public Unit Right { get; set; } = new Unit();

        public StageUnit DeepCopy()
        {
            StageUnit unit = new StageUnit();
            unit.Name = Name;
            unit.m_GD_ImageSave_Use = m_GD_ImageSave_Use;
            unit.m_NG_ImageSave_Use = m_NG_ImageSave_Use;
            unit.Left = Left.DeepCopy();
            unit.Right = Right.DeepCopy();

            return unit;
        }

        public void Dispose()
        {
            Left.Dispose();
            Right.Dispose();
        }
    }

    public class Test
    {
        public AmpMark AmpMark = new AmpMark();
    }

    public class Unit
    {
        public MarkUnit Mark = new MarkUnit();

        public List<InspUnit> InspParamList = new List<InspUnit>();

        public Unit DeepCopy()
        {
            Unit unit = new Unit();

            unit.Mark = Mark.DeepCopy();
            foreach (var param in InspParamList)
                unit.InspParamList.Add(param.DeepCopy());

            return unit;
        }

        public void Dispose()
        {
            Mark.Dispose();

            InspParamList.ForEach(x => x.Dispose());
            InspParamList.Clear();
        }
    }

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
            #region Amp
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

            #region Bonding
            Bonding.Score = StaticConfig.ModelFile.GetFData(Bonding.ModelSection, "ROIFinealign_MarkScore");
            Bonding.AlignSpec_T = StaticConfig.ModelFile.GetFData(Bonding.ModelSection, "ROIFinealign_T_Spec");

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

        public void Save()
        {

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
        public double AlignSpec_T { get; set; } = 0;

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

            mark.AlignSpec_T = AlignSpec_T;

            for (int i = 0; i < UpMarkToolList.Count(); i++)
                mark.UpMarkToolList.Add(UpMarkToolList[i].DeepCopy());

            for (int i = 0; i < DownMarkToolList.Count(); i++)
                mark.DownMarkToolList.Add(DownMarkToolList[i].DeepCopy());

            return mark;
        }
    }



    public enum CamDirection
    {
        Left,
        Right,
    }

    public enum InspType
    {
        Line = 0,
        Circle = 1
    };
}
