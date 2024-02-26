using COG.Class.Units;
using COG.Settings;
using COG.UI.Forms;
using Cognex.VisionPro;
using Cognex.VisionPro.Blob;
using Cognex.VisionPro.Caliper;
using Cognex.VisionPro.SearchMax;
using System;
using System.Collections.Generic;
using System.IO;
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
                    var leftAmpMark = CreateMark(stageUnit.Name + "_0_CAMERA___UP");
                    stageUnit.LeftCamUnit.AmpMarkList.AddRange(leftAmpMark);

                    var rightAmpMark = CreateMark(stageUnit.Name + "_1_CAMERA___UP");
                    stageUnit.RightCamUnit.AmpMarkList.AddRange(rightAmpMark);
                    #endregion

                    #region Bonding Mark
                    var leftBondingMark = CreateMark("ROIFineAlign" + stageUnit.Name + "_0_CAMERA___UP");
                    stageUnit.LeftCamUnit.BondingMarkList.AddRange(leftBondingMark);

                    var rightBondingMark = CreateMark("ROIFineAlign" + stageUnit.Name + "_1_CAMERA___UP");
                    stageUnit.RightCamUnit.BondingMarkList.AddRange(rightBondingMark);
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
                LoadAmpMark(modelDir, stageIndex);
                LoadBondingMark(modelDir, stageIndex);

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
        private List<MarkUnit> CreateMark(string name)
        {
            List<MarkUnit> markList = new List<MarkUnit>();

            MarkUnit markUnit = new MarkUnit();
            markUnit.Name = name;
            markUnit.AlignType = StaticConfig.M_1CAM2SHOT;

            for (int subIndex = 0; subIndex < StaticConfig.PATTERN_MAX_COUNT; subIndex++)
            {
                MarkTag markTag = new MarkTag();
                markTag.Index = subIndex;
                markUnit.TagList.Add(markTag);
            }
            markList.Add(markUnit);

            return markList;
        }

        private void LoadAmpMark(string modelDir, int stageIndex)
        {
            for (int subIndex = 0; subIndex < StaticConfig.PATTERN_MAX_COUNT; subIndex++)
            {
                var stageUnit = StageUnitList[stageIndex];
                for (int i = 0; i < stageUnit.LeftCamUnit.AmpMarkList.Count; i++)
                {
                    #region Left
                    var leftMarkUnit = stageUnit.LeftCamUnit.AmpMarkList[i];
                    var leftVppFileName = Path.Combine(modelDir, $"{leftMarkUnit.Name}_{subIndex}.vpp");

                    if (File.Exists(leftVppFileName))
                    {
                        var markTag = leftMarkUnit.TagList[subIndex];
                        var tool = CogSerializer.LoadObjectFromFile(leftVppFileName) as CogSearchMaxTool;
                        markTag.SetTool(tool);
                    }
                    #endregion

                    #region Right
                    var rightMarkUnit = stageUnit.RightCamUnit.AmpMarkList[i];
                    var rightVppFileName = Path.Combine(modelDir, $"{rightMarkUnit.Name}_{subIndex}.vpp");

                    if (File.Exists(rightVppFileName))
                    {
                        var markTag = rightMarkUnit.TagList[subIndex];
                        var tool = CogSerializer.LoadObjectFromFile(rightVppFileName) as CogSearchMaxTool;
                        markTag.SetTool(tool);
                    }
                    #endregion
                }
            }
        }

        private void LoadBondingMark(string modelDir, int stageIndex)
        {
            for (int subIndex = 0; subIndex < StaticConfig.PATTERN_MAX_COUNT; subIndex++)
            {
                var stageUnit = StageUnitList[stageIndex];
                for (int i = 0; i < stageUnit.LeftCamUnit.BondingMarkList.Count; i++)
                {
                    #region Left
                    var leftMarkUnit = stageUnit.LeftCamUnit.BondingMarkList[i];
                    var leftVppFileName = Path.Combine(modelDir, $"{leftMarkUnit.Name}_{subIndex:D2}.vpp");

                    if (File.Exists(leftVppFileName))
                    {
                        var markTag = leftMarkUnit.TagList[subIndex];
                        var tool = CogSerializer.LoadObjectFromFile(leftVppFileName) as CogSearchMaxTool;
                        markTag.SetTool(tool);
                    }
                    #endregion

                    #region Right
                    var rightMarkUnit = stageUnit.RightCamUnit.BondingMarkList[i];
                    var rightVppFileName = Path.Combine(modelDir, $"{rightMarkUnit.Name}_{subIndex:D2}.vpp");

                    if (File.Exists(rightVppFileName))
                    {
                        var markTag = rightMarkUnit.TagList[subIndex];
                        var tool = CogSerializer.LoadObjectFromFile(rightVppFileName) as CogSearchMaxTool;
                        markTag.SetTool(tool);
                    }
                    #endregion
                }
            }
        }

        private void SaveAmpMark(string modelDir, int stageIndex)
        {
            for (int subIndex = 0; subIndex < StaticConfig.PATTERN_MAX_COUNT; subIndex++)
            {
                var stageUnit = StageUnitList[stageIndex];
                for (int i = 0; i < stageUnit.LeftCamUnit.AmpMarkList.Count; i++)
                {
                    #region Left
                    var leftMarkUnit = stageUnit.LeftCamUnit.AmpMarkList[i];
                    var leftVppFileName = Path.Combine(modelDir, $"{leftMarkUnit.Name}_{subIndex}.vpp");

                    var leftMarkTag = leftMarkUnit.TagList[subIndex];
                    leftMarkTag.SaveTool(leftVppFileName);
                    #endregion

                    #region Right
                    var rightMarkUnit = stageUnit.RightCamUnit.AmpMarkList[i];
                    var rightVppFileName = Path.Combine(modelDir, $"{rightMarkUnit.Name}_{subIndex}.vpp");

                    var rightMarkTag = rightMarkUnit.TagList[subIndex];
                    rightMarkTag.SaveTool(rightVppFileName);
                    #endregion
                }
            }
        }

        private void SaveBondingMark(string modelDir, int stageIndex)
        {
            for (int subIndex = 0; subIndex < StaticConfig.PATTERN_MAX_COUNT; subIndex++)
            {
                var stageUnit = StageUnitList[stageIndex];
                for (int i = 0; i < stageUnit.LeftCamUnit.BondingMarkList.Count; i++)
                {
                    #region Left
                    var leftMarkUnit = stageUnit.LeftCamUnit.BondingMarkList[i];
                    var leftVppFileName = Path.Combine(modelDir, $"{leftMarkUnit.Name}_{subIndex:D2}.vpp");

                    var leftMarkTag = leftMarkUnit.TagList[subIndex];
                    leftMarkTag.SaveTool(leftVppFileName);
                    #endregion

                    #region Right
                    var rightMarkUnit = stageUnit.RightCamUnit.BondingMarkList[i];
                    var rightVppFileName = Path.Combine(modelDir, $"{rightMarkUnit.Name}_{subIndex:D2}.vpp");

                    var rightMarkTag = rightMarkUnit.TagList[subIndex];
                    rightMarkTag.SaveTool(rightVppFileName);
                    #endregion
                }
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

        public Unit LeftCamUnit { get; set; } = new Unit();

        public Unit RightCamUnit { get; set; } = new Unit();

        public void Dispose()
        {
            LeftCamUnit.Dispose();
            RightCamUnit.Dispose();
        }
    }

    public class Unit
    {
        public List<MarkUnit> AmpMarkList = new List<MarkUnit>();

        public List<MarkUnit> BondingMarkList = new List<MarkUnit>();

        public List<InspUnit> InspParamList = new List<InspUnit>();

        public void Dispose()
        {
            AmpMarkList.ForEach(x => x.Dispose());
            AmpMarkList.Clear();

            BondingMarkList.ForEach(x => x.Dispose());
            BondingMarkList.Clear();

            InspParamList.ForEach(x => x.Dispose());
            InspParamList.Clear();
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
