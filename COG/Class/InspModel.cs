using COG.Class.Data;
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

                    #region Film Align
                    var leftAmpFilmTool = CreateFilmAlignTool();
                    stageUnit.Left.FilmAlign.ModelSection = stageUnit.Name + "_0_CAMERA___UP";
                    stageUnit.Left.FilmAlign.VppTitleName = "TrackingLine" + stageUnit.Name + "_0_CAMERA___UP";
                    stageUnit.Left.FilmAlign.ToolList = leftAmpFilmTool;


                    var rightAmpFilmTool = CreateFilmAlignTool();
                    stageUnit.Right.FilmAlign.ModelSection = stageUnit.Name + "_1_CAMERA___UP";
                    stageUnit.Right.FilmAlign.VppTitleName = "TrackingLine" + stageUnit.Name + "_1_CAMERA___UP";
                    stageUnit.Right.FilmAlign.ToolList = rightAmpFilmTool;
                    #endregion

                    #region Inspection
                    stageUnit.Left.Insp.ModelSection = stageUnit.Name + "_0_CAMERA___UP";
                    stageUnit.Left.Insp.LineVppTitleName = "FindLine_" + stageUnit.Name + "_0_CAMERA___UP";
                    stageUnit.Left.Insp.CircleVppTitleName = "Circle_" + stageUnit.Name + "_0_CAMERA___UP";

                    stageUnit.Right.Insp.ModelSection = stageUnit.Name + "_1_CAMERA___UP";
                    stageUnit.Right.Insp.LineVppTitleName = "FindLine_" + stageUnit.Name + "_1_CAMERA___UP";
                    stageUnit.Right.Insp.CircleVppTitleName = "Circle_" + stageUnit.Name + "_1_CAMERA___UP";
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
                Load(modelDir, stageIndex);

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


        private List<FilmAlignTool> CreateFilmAlignTool()
        {
            List<FilmAlignTool> lineToolList = new List<FilmAlignTool>();

            for (int subIndex = 0; subIndex < StaticConfig.FILM_ALIGN_MAX_COUNT; subIndex++)
            {
                FilmAlignTool lineTool = new FilmAlignTool();
                lineTool.Index = subIndex;
                lineToolList.Add(lineTool);
            }

            return lineToolList;
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

        private void Load(string modelDir, int stageIndex)
        {
            var stageUnit = StageUnitList[stageIndex];

            stageUnit.m_GD_ImageSave_Use = StaticConfig.ModelFile.GetBData(stageUnit.Name, "GD_IMAGE");
            stageUnit.m_NG_ImageSave_Use = StaticConfig.ModelFile.GetBData(stageUnit.Name, "NG_IMAGE");

            stageUnit.Left.Mark.Load(modelDir);
            stageUnit.Right.Mark.Load(modelDir);

            stageUnit.Left.FilmAlign.Load(modelDir);
            stageUnit.Right.FilmAlign.Load(modelDir);

            stageUnit.Left.Insp.Load(modelDir);
            stageUnit.Right.Insp.Load(modelDir);
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
}
