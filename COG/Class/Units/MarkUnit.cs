using Cognex.VisionPro;
using Cognex.VisionPro.SearchMax;
using System;
using System.Collections.Generic;
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

        public List<MarkTag> TagList = new List<MarkTag>();

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

            foreach (var tag in TagList)
                unit.TagList.Add(tag.DeepCopy());

            return unit;
        }
    }

    public class MarkTag
    {
        public int Index { get; set; }

        public double Score { get; set; }

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

        public void Dispose()
        {
            Tool?.Dispose();
            Tool = null;
        }

        public MarkTag DeepCopy()
        {
            MarkTag tag = new MarkTag();
            tag.Index = Index;
            tag.Score = Score;
            if(Tool!=null)
            {
                tag.Tool = new CogSearchMaxTool(Tool);
            }
            return tag;
        }
    }
}
