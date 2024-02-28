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
    public class MarkTool
    {
        public int Index { get; set; }

        public CogSearchMaxTool SearchMaxTool { get; private set; } = null;

        public void SetTool(CogSearchMaxTool tool)
        {
            SearchMaxTool?.Dispose();
            SearchMaxTool = null;
            SearchMaxTool = tool;
        }

        public void SaveTool(string filePath)
        {
            if (SearchMaxTool != null)
            {
                if (SearchMaxTool.InputImage is CogImage8Grey grey)
                    grey.Dispose();

                if (SearchMaxTool.InputImage is CogImage24PlanarColor color)
                    color.Dispose();

                SearchMaxTool.InputImage = null;
                CogSerializer.SaveObjectToFile(SearchMaxTool, filePath);
            }
        }

        public void SetMaskingImage(CogImage8Grey cogImage)
        {
            if (SearchMaxTool == null | cogImage == null)
                return;
            SearchMaxTool.Pattern.TrainImageMask?.Dispose();
            SearchMaxTool.Pattern.TrainImageMask = null;

            SearchMaxTool.Pattern.TrainImageMask = new CogImage8Grey(cogImage);
        }

        public void SetTrainRegion(CogRectangle roi)
        {
            if (SearchMaxTool == null)
                return;

            CogRectangle rect = new CogRectangle(roi);

            SearchMaxTool.Pattern.Origin.TranslationX = rect.CenterX;
            SearchMaxTool.Pattern.Origin.TranslationY = rect.CenterY;
            SearchMaxTool.Pattern.TrainRegion = rect;
        }

        public void SetSearchRegion(CogRectangle roi)
        {
            if (SearchMaxTool == null)
                return;

            CogRectangle rect = new CogRectangle(roi);
            rect.Color = CogColorConstants.Green;
            rect.LineStyle = CogGraphicLineStyleConstants.Dot;
            SearchMaxTool.SearchRegion = new CogRectangle(rect);
        }

        public void SetOrginMark(CogPointMarker originMarkPoint)
        {
            if (SearchMaxTool == null || originMarkPoint == null)
                return;

            SearchMaxTool.Pattern.Origin.TranslationX = originMarkPoint.X;
            SearchMaxTool.Pattern.Origin.TranslationY = originMarkPoint.Y;
        }

        public void Dispose()
        {
            SearchMaxTool?.Dispose();
            SearchMaxTool = null;
        }

        public MarkTool DeepCopy()
        {
            MarkTool markTool = new MarkTool();

            markTool.Index = Index;
            if (SearchMaxTool != null)
                markTool.SearchMaxTool = new CogSearchMaxTool(SearchMaxTool);

            return markTool;
        }

        //public List<MarkTool> MarkList = new List<MarkTool>();

        //public void Load()
        //{
        //    Score = StaticConfig.ModelFile.GetFData(Name, "ACCEPT_SCORE");
        //    for (int i = 0; i < StaticConfig.PATTERN_MAX_COUNT; i++)
        //    {
        //        MarkList[i].Index = i;
        //        string key = "PATUSE" + i.ToString();
        //        MarkList[i].Use = StaticConfig.ModelFile.GetBData(Name, key);
        //    }

        //}

        //public void Save()
        //{
        //    StaticConfig.ModelFile.SetData(Name, "ACCEPT_SCORE", Score);
        //    for (int i = 0; i < StaticConfig.PATTERN_MAX_COUNT; i++)
        //    {
        //        string key = "PATUSE" + i.ToString();
        //        StaticConfig.ModelFile.SetData(Name, key, MarkList[i].Use);
        //    }
        //}

        //public void Dispose()
        //{
        //    //MarkList.ForEach(x => x.Dispose());
        //    //MarkList.Clear();
        //}

        //public Mark DeepCopy()
        //{
        //    Mark unit = new Mark();
        //    //unit.Name = Name;
        //    //unit.CamNo = CamNo;
        //    //unit.AlignType = AlignType;
        //    //unit.Score = Score;

        //    //foreach (var tag in MarkList)
        //    //    unit.MarkList.Add(tag.DeepCopy());

        //    return unit;
        //}
    }
}
