using COG.Class.Core;
using COG.Class.Units;
using Cognex.VisionPro;
using Cognex.VisionPro.SearchMax;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Class
{
    public class Algorithm
    {
        public MarkResult FindMark(CogImage8Grey cogImage, List<MarkTool> markToolList, double score)
        {
            foreach (var markTool in markToolList)
            {
                CogSearchMaxTool cogSearchMaxTool = new CogSearchMaxTool(markTool.SearchMaxTool);
                cogSearchMaxTool.InputImage = cogImage;
                cogSearchMaxTool.Run();

                if (cogSearchMaxTool.Results.Count > 0)
                {
                    var foundResult = cogSearchMaxTool.Results[0];
                    if (foundResult.Score > score)
                    {
                        CogRectangle trainRoi = cogSearchMaxTool.Pattern.TrainRegion as CogRectangle;
                        var trainOrigin = cogSearchMaxTool.Pattern.Origin;

                        MarkResult markResult = new MarkResult();
                        markResult.ReferencePos = new PointF((float)trainOrigin.TranslationX, (float)trainOrigin.TranslationY);
                        markResult.ReferenceWidth = (float)trainRoi.Width;
                        markResult.ReferenceHeight = (float)trainRoi.Height;

                        markResult.FoundPos = new PointF((float)foundResult.GetPose().TranslationX, (float)foundResult.GetPose().TranslationY);
                        markResult.Score = (float)foundResult.Score;
                        markResult.Angle = (float)foundResult.GetPose().Rotation;
                        markResult.Scale = (float)foundResult.GetPose().Scaling;
                        markResult.ResultGraphics = foundResult.CreateResultGraphics(CogSearchMaxResultGraphicConstants.MatchRegion
                                                                                | CogSearchMaxResultGraphicConstants.Origin);
                        return markResult;
                    }
                }
            }
            return null;
        }

    }
}
