using Cognex.VisionPro;
using Cognex.VisionPro.Caliper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Class.Data
{
    public class FilmAlignTool
    {
        private int _index { get; set; } = 0;

        public int Index
        {
            get
            {
                return _index;
            }
            set
            {
                FilmROIType = (FilmROIType)value;
                _index = value;
            }
        }
     
        public FilmROIType FilmROIType { get; private set; }

        public CogFindLineTool FindLineTool { get; private set; } = null;

        public void SetTool(CogFindLineTool tool)
        {
            FindLineTool?.Dispose();
            FindLineTool = null;
            FindLineTool = tool;
        }

        public void SaveTool(string filePath)
        {
            if (FindLineTool != null)
            {
                if (FindLineTool.InputImage is CogImage8Grey grey)
                    grey.Dispose();

                FindLineTool.InputImage = null;
                CogSerializer.SaveObjectToFile(FindLineTool, filePath);
            }
        }

        public void Dispose()
        {
            FindLineTool?.Dispose();
            FindLineTool = null;
        }

        public FilmAlignTool DeepCopy()
        {
            FilmAlignTool lineTool = new FilmAlignTool();

            lineTool.Index = Index;
            if (FindLineTool != null)
                lineTool.FindLineTool = new CogFindLineTool(FindLineTool);

            return lineTool;
        }
    }

    public enum FilmROIType
    {
        Left_Top,
        Left_Side,
        Right_Top,
        Right_Side,
    }
}
