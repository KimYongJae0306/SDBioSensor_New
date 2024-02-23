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
        public List<StageUnit> StageUnitList = new List<StageUnit>();
       
        public void New()
        {
            // Todo : 모델 dispose
            if (StaticConfig.PROGRAM_TYPE == "ATT_AREA_PC1")
            {
                for (int i = 0; i < StaticConfig.STAGE_COUNT; i++)
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

        public void Load()
        {
            New();
        }
    }

    public partial class InspModel
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
            New();

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

            SystemManager.Instance().ShowProgerssBar(StaticConfig.STAGE_COUNT, true, 0);

            string modelDir = modelPath + AppsConfig.Instance().ProjectName;
            for (int stageIndex = 0; stageIndex < StaticConfig.STAGE_COUNT; stageIndex++)
            {

                LoadAmpMark(modelDir, stageIndex);
                LoadBondingMark(modelDir, stageIndex);

                SystemManager.Instance().ShowProgerssBar(StaticConfig.STAGE_COUNT, true, stageIndex + 1);
            }
            return true;
        }

        public List<PatternUnit> CreateMark(string name)
        {
            List<PatternUnit> markList = new List<PatternUnit>();

            PatternUnit patternUnit = new PatternUnit();
            patternUnit.Name = name;
            patternUnit.AlignType = StaticConfig.M_1CAM2SHOT;

            for (int subIndex = 0; subIndex < StaticConfig.PATTERN_MAX; subIndex++)
            {
                PatternTag patternTag = new PatternTag();
                patternTag.Index = subIndex;
                patternUnit.TagList.Add(patternTag);
            }
            markList.Add(patternUnit);

            return markList;
        }

        private void LoadAmpMark(string modelDir,int stageIndex)
        {
            for (int subIndex = 0; subIndex < StaticConfig.PATTERN_MAX; subIndex++)
            {
                var stageUnit = StageUnitList[stageIndex];
                for (int i = 0; i < stageUnit.LeftCamUnit.AmpMarkList.Count; i++)
                {
                    #region Left
                    var patternLeftUnit = stageUnit.LeftCamUnit.AmpMarkList[i];
                    var leftVppFileName = Path.Combine(modelDir, $"{patternLeftUnit.Name}_{subIndex}.vpp");

                    if (File.Exists(leftVppFileName))
                    {
                        var leftTag = patternLeftUnit.TagList[subIndex];
                        var tool = CogSerializer.LoadObjectFromFile(leftVppFileName) as CogSearchMaxTool;
                        leftTag.SetTool(tool);
                    }
                    #endregion

                    #region Right
                    var patternRightUnit = stageUnit.RightCamUnit.AmpMarkList[i];
                    var rightVppFileName = Path.Combine(modelDir, $"{patternRightUnit.Name}_{subIndex}.vpp");

                    if (File.Exists(rightVppFileName))
                    {
                        var rightTag = patternRightUnit.TagList[subIndex];
                        var tool = CogSerializer.LoadObjectFromFile(rightVppFileName) as CogSearchMaxTool;
                        rightTag.SetTool(tool);
                    }
                    #endregion
                }
            }
        }

        private void LoadBondingMark(string modelDir, int stageIndex)
        {
            for (int subIndex = 0; subIndex < StaticConfig.PATTERN_MAX; subIndex++)
            {
                var stageUnit = StageUnitList[stageIndex];
                for (int i = 0; i < stageUnit.LeftCamUnit.BondingMarkList.Count; i++)
                {
                    #region Left
                    var patternLeftUnit = stageUnit.LeftCamUnit.BondingMarkList[i];
                    var leftVppFileName = Path.Combine(modelDir, $"{patternLeftUnit.Name}_{subIndex:D2}.vpp");

                    if (File.Exists(leftVppFileName))
                    {
                        var leftTag = patternLeftUnit.TagList[subIndex];
                        var tool = CogSerializer.LoadObjectFromFile(leftVppFileName) as CogSearchMaxTool;
                        leftTag.SetTool(tool);
                    }
                    #endregion

                    #region Right
                    var patternRightUnit = stageUnit.RightCamUnit.BondingMarkList[i];
                    var rightVppFileName = Path.Combine(modelDir, $"{patternRightUnit.Name}_{subIndex:D2}.vpp");

                    if (File.Exists(rightVppFileName))
                    {
                        var rightTag = patternRightUnit.TagList[subIndex];
                        var tool = CogSerializer.LoadObjectFromFile(rightVppFileName) as CogSearchMaxTool;
                        rightTag.SetTool(tool);
                    }
                    #endregion
                }
            }
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

    public class PatternUnit
    {
        public string Name { get; set; } = "";

        public int CamNo { get; set; } = 0;

        public int AlignType { get; set; } = 0;

        public List<PatternTag> TagList = new List<PatternTag>();
    }

    public class PatternTag
    {
        public int Index { get; set; }

        public CogSearchMaxTool Tool { get; private set; } = null;

        public void SetTool(CogSearchMaxTool tool)
        {
            Tool?.Dispose();
            Tool = null;
            Tool = tool;
        }
    }

    public class StageUnit 
    {
        public string Name { get; set; }

        public InspUnit LeftCamUnit { get; set; } = new InspUnit();

        public InspUnit RightCamUnit { get; set; } = new InspUnit();
    }

    public class InspUnit
    {
        public List<PatternUnit> AmpMarkList = new List<PatternUnit>();

        public List<PatternUnit> BondingMarkList = new List<PatternUnit>();

        public List<InspParamUnit> InspParamList = new List<InspParamUnit>();
    }

    public class InspParamUnit
    {
        public CogFitLineTool FindLineTool { get; set; } = null;
        public CogFindCircleTool FindCircleTool { get; set; } = null;

        public InspType InspType { get; set; }
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public double LenthX { get; set; }
        public double LenthY { get; set; }
        public double dSpecDistance { get; set; }
        public double dSpecDistanceMax { get; set; }
        public int Distgnore { get; set; }

        public int HistogramROICnt { get; set; }
        public int[] HistogramSpec { get; set; }

        public bool ThresholdUse { get; set; }
        public int Threshold { get; set; } = 32;
        public int TopCutPixel { get; set; } = 15;
        public int IgnoreSize { get; set; } = 20;
        public int BottomCutPixel { get; set; } = 10;
        public int MaskingValue { get; set; } = 210;
        public int EdgeCaliperThreshold { get; set; } = 55;
        public int EdgeCaliperFilterSize { get; set; } = 10;
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
