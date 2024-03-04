using COG.Class.Core;
using COG.Class.Data;
using COG.Class.Units;
using COG.Helper;
using Cognex.VisionPro;
using Cognex.VisionPro.Caliper;
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
        public GaloCircleInspResult RunGaloCircleInspection(CogImage8Grey cogImage, GaloInspTool inspTool)
        {
            if (cogImage == null)
                return null;

            CogFindCircleTool tool = new CogFindCircleTool(inspTool.FindCircleTool);
            tool.InputImage = cogImage as CogImage8Grey;
            tool.Run();

            GaloCircleInspResult result = new GaloCircleInspResult();
            if (tool.Results?.Count > 0)
            {
                for (int i = 0; i < tool.Results.Count; i++)
                {
                    if(tool.Results[i].CaliperResults.Count > 0)
                    {
                        var caliperResult = tool.Results[i].CaliperResults;
                        PointF edge0Point = new PointF((float)caliperResult[0].Edge0.PositionX, (float)caliperResult[0].Edge0.PositionX);
                        result.Edge0PointList.Add(edge0Point);

                        PointF edge1Point = new PointF((float)caliperResult[0].Edge1.PositionX, (float)caliperResult[0].Edge1.PositionX);
                        result.Edge1PointList.Add(edge1Point);
                    }
                    else
                    {
                        result.Edge0PointList.Add(new PointF());
                        result.Edge1PointList.Add(new PointF());
                    }
                }
            }

            if (result.Edge0PointList.Count <= 0)
                result.Judgement = Judgement.FAIL;
            else
            {

            }



            return null;
        }

        public AmpFilmAlignResult RunAmpFlimAlign(CogImage8Grey cogImage, FilmAlignParam filmParam)
        {
            if (cogImage == null)
                return null;

            AmpFilmAlignResult result = new AmpFilmAlignResult();

            foreach (var tool in filmParam.ToolList)
            {
                tool.FindLineTool.InputImage = cogImage;
                tool.FindLineTool.Run();


                FilmAlignResult lineResult = new FilmAlignResult();

                if (tool.FindLineTool.Results.Count > 0)
                {
                    lineResult.Found = true;
                    var expectedLineSegment = tool.FindLineTool.RunParams.ExpectedLineSegment;
                    var lineSegment = tool.FindLineTool.Results.GetLineSegment();

                    lineResult.Type = tool.FilmROIType;
                    lineResult.StartReferencePoint = new PointF((float)expectedLineSegment.StartX, (float)expectedLineSegment.StartY);
                    lineResult.EndReferencePoint = new PointF((float)expectedLineSegment.EndX, (float)expectedLineSegment.EndY);
                    lineResult.StartFoundPoint = new PointF((float)lineSegment.StartX, (float)lineSegment.StartY);
                    lineResult.EndFoundPoint = new PointF((float)lineSegment.EndX, (float)lineSegment.EndY);
                    lineResult.Line = tool.FindLineTool.Results.GetLine();
                }
                else
                {
                    lineResult.Found = false;
                }
                result.FilmAlignResult.Add(lineResult);
            }

            if(result.FilmAlignResult.Count != filmParam.ToolList.Count)
            {
                result.Judgement = Judgement.FAIL;
                return result;
            }
            var value = result.GetDistanceX_mm();
            if(value + filmParam.FilmAlignSpecX <= filmParam.AmpModuleDistanceX)
                result.Judgement = Judgement.NG;
            else
                result.Judgement = Judgement.OK;

            return result;
        }

        public MarkResult FindMark(CogImage8Grey cogImage, List<MarkTool> markToolList, double score)
        {
            foreach (var markTool in markToolList)
            {
                CogSearchMaxTool cogSearchMaxTool = new CogSearchMaxTool(markTool.SearchMaxTool);
                cogSearchMaxTool.InputImage = cogImage;
                cogSearchMaxTool.Run();

                if (cogSearchMaxTool.Results?.Count > 0)
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

        public MarkResult FindMark(CogImage8Grey cogImage, MarkTool markTool)
        {
            if (cogImage == null | markTool == null)
                return null;

            CogSearchMaxTool cogSearchMaxTool = markTool.SearchMaxTool;// new CogSearchMaxTool(markTool.SearchMaxTool);
            cogSearchMaxTool.InputImage = cogImage;
            cogSearchMaxTool.Run();

            if (cogSearchMaxTool.Results.Count > 0)
            {
                var foundResult = cogSearchMaxTool.Results[0];

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

            return null;
        }

        public BondingMarkResult FindBondingMark(CogImage8Grey cogImage, List<MarkTool> upMarkToolList, List<MarkTool> downMarkToolList, double score, double specT)
        {
            if (cogImage == null | upMarkToolList.Count <= 0 | downMarkToolList.Count <= 0)
                return new BondingMarkResult();

            var upMarkResult = FindMark(cogImage, upMarkToolList, score);
            var downMarkResult = FindMark(cogImage, downMarkToolList, score);

            BondingMarkResult result = new BondingMarkResult();
            result.UpMarkResult = upMarkResult;
            result.DownMarkResult = downMarkResult;

            if (upMarkResult == null || downMarkResult == null)
            {
                //검사 실패
                result.Judgement = Judgement.FAIL;
            }
            else
            {
                var originTheta = Math.Atan2(downMarkResult.ReferencePos.Y - upMarkResult.ReferencePos.Y, downMarkResult.ReferencePos.X - upMarkResult.ReferencePos.X);
                var foundTheta = Math.Atan2(downMarkResult.FoundPos.Y - upMarkResult.FoundPos.Y, downMarkResult.FoundPos.X - upMarkResult.FoundPos.X);

                result.FoundDegree = (foundTheta - originTheta) * 180 / Math.PI;

                if(Math.Abs(specT) > result.FoundDegree)
                    result.Judgement = Judgement.OK;
                else
                    result.Judgement = Judgement.NG;
            }

            return result;
        }
    }
}
