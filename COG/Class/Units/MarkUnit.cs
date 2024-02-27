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
        public string Name { get; set; } = "";

        public int CamNo { get; set; } = 0;

        public int AlignType { get; set; } = 0;

        public double Score { get; set; }

        public List<MarkTag> TagList = new List<MarkTag>();

        public void Load()
        {
            Score = StaticConfig.ModelFile.GetFData(Name, "ACCEPT_SCORE");
            for (int i = 0; i < StaticConfig.PATTERN_MAX_COUNT; i++)
            {
                TagList[i].Index = i;
                string key = "PATUSE" + i.ToString();
                TagList[i].Use = StaticConfig.ModelFile.GetBData(Name, key);
            }
              
        }

        public void Save()
        {
            StaticConfig.ModelFile.SetData(Name, "ACCEPT_SCORE", Score);
            for (int i = 0; i < StaticConfig.PATTERN_MAX_COUNT; i++)
            {
                string key = "PATUSE" + i.ToString();
                StaticConfig.ModelFile.SetData(Name, key, TagList[i].Use);
            }
        }

        public void Dispose()
        {
            TagList.ForEach(x => x.Dispose());
            TagList.Clear();
        }

        public MarkUnit DeepCopy()
        {
            MarkUnit unit = new MarkUnit();
            unit.Name = Name;
            unit.CamNo = CamNo;
            unit.AlignType = AlignType;
            unit.Score = Score;

            foreach (var tag in TagList)
                unit.TagList.Add(tag.DeepCopy());

            return unit;
        }
    }

    public class MarkTag
    {
        public int Index { get; set; }

        public bool Use { get; set; }

       

        public CogSearchMaxTool Tool { get; private set; } = null;

        public void SetTool(CogSearchMaxTool tool)
        {
            Tool?.Dispose();
            Tool = null;
            Tool = tool;
        }

        public void SaveTool(string filePath)
        {
            if(Tool != null)
            {
                if (Tool.InputImage is CogImage8Grey grey)
                    grey.Dispose();

                if (Tool.InputImage is CogImage24PlanarColor color)
                    color.Dispose();

                Tool.InputImage = null;
                CogSerializer.SaveObjectToFile(Tool, filePath);
            }
        }

        public void SetTrainRegion(CogRectangle roi)
        {
            if (Tool == null)
                return;

            CogRectangle rect = new CogRectangle(roi);

            Tool.Pattern.Origin.TranslationX = rect.CenterX;
            Tool.Pattern.Origin.TranslationY = rect.CenterY;
            Tool.Pattern.TrainRegion = rect;
        }

        public void SetSearchRegion(CogRectangle roi)
        {
            if (Tool == null)
                return;

            CogRectangle rect = new CogRectangle(roi);
            rect.Color = CogColorConstants.Green;
            rect.LineStyle = CogGraphicLineStyleConstants.Dot;
            Tool.SearchRegion = new CogRectangle(rect);
        }

        public void SetOrginMark(CogPointMarker originMarkPoint)
        {
            if (Tool == null)
                return;

            Tool.Pattern.Origin.TranslationX = originMarkPoint.X;
            Tool.Pattern.Origin.TranslationY = originMarkPoint.Y;
        }

        public void Dispose()
        {
            Tool?.Dispose();
            Tool = null;
        }

        public MarkTag DeepCopy()
        {
            MarkTag tag = new MarkTag();
            tag.Index = Index;
            tag.Use = Use;
            if(Tool != null)
            {
                tag.Tool = new CogSearchMaxTool(Tool);
            }
            return tag;
        }

     
    }
}
