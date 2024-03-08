using COG.Class.Core;
using COG.Class.Data;
using COG.Helper;
using COG.Settings;
using Cognex.VisionPro;
using Cognex.VisionPro.Caliper;
using Cognex.VisionPro.SearchMax;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Class
{
    public partial class AlgorithmTool
    {
        private LineAlgorithm _lineAlgorithm = new LineAlgorithm();

        private CircleAlgorithm _circleAlgorithm = new CircleAlgorithm();

        public GaloLineToolResult RunGaloLineInspection(CogImage8Grey cogImage, CogImage8Grey binaryImage, GaloInspTool inspTool, ref CogRectangleAffine affineRect, bool isDebug)
        {
            GaloLineToolResult result = new GaloLineToolResult();
            if (cogImage == null)
                return result;

            CogFindLineTool tool = inspTool.FindLineTool;

            RollBackLineTool rollbackValue = new RollBackLineTool();
            rollbackValue.SetValue(tool);

            try
            {
                if(inspTool.DarkArea.ThresholdUse)
                {
                    if (inspTool.DarkArea.MaskingDirection == DarkMaskingDirection.InSide)
                    {
                        tool.RunParams.CaliperRunParams.FilterHalfSizeInPixels = 5; // 확인 필요
                        result.InsideResult = _lineAlgorithm.InspectDarkArea(cogImage, inspTool, true, isDebug);

                        ShiftLineSeqment(ref tool);
                        result.OutsideResult = _lineAlgorithm.InspectLine(cogImage, tool, isDebug);
                    }
                    else if (inspTool.DarkArea.MaskingDirection == DarkMaskingDirection.OutSide)
                    {
                        result.InsideResult = _lineAlgorithm.InspectLine(cogImage, tool, isDebug);
                        ShiftLineSeqment(ref tool);

                        tool.RunParams.CaliperRunParams.FilterHalfSizeInPixels = 5; // 확인 필요
                        result.OutsideResult = _lineAlgorithm.InspectDarkArea(cogImage, inspTool, false, isDebug);

                    }
                    else
                    {
                        tool.RunParams.CaliperRunParams.FilterHalfSizeInPixels = 5; // 확인 필요
                        result.InsideResult = _lineAlgorithm.InspectDarkArea(cogImage, inspTool, true, isDebug);
                        ShiftLineSeqment(ref tool);
                        result.OutsideResult = _lineAlgorithm.InspectDarkArea(cogImage, inspTool, false, isDebug);
                    }
                }
                else
                {
                    result.InsideResult = _lineAlgorithm.InspectLine(cogImage, tool, isDebug);
                    ShiftLineSeqment(ref tool);
                    result.OutsideResult = _lineAlgorithm.InspectLine(cogImage, tool, isDebug);
                }

                rollbackValue.RollBack(ref tool);
            }
            catch (Exception err)
            {
                rollbackValue.RollBack(ref tool);
            }

            var points1 = result.InsideResult.PointList;
            var points2 = result.OutsideResult.PointList;

            result.Judgement = _lineAlgorithm.CheckSpec(points1, points2, inspTool);
            
            return result;
        }

        public GaloCircleToolResult RunGaloCircleInspection(CogImage8Grey cogImage, GaloInspTool inspTool, bool isDebug)
        {
            GaloCircleToolResult result = new GaloCircleToolResult();
            if (cogImage == null)
                return result;

            List<CogCompositeShape> resultGraphics = _circleAlgorithm.InspectCircle(cogImage, inspTool.FindCircleTool, ref result, isDebug, inspTool);

            result.Judgement = CheckSpec(result.GetDistance(), inspTool);
            if (result.Judgement == Judgement.OK)
                result.ResultGraphics.AddRange(resultGraphics);

            return result;
        }

        private Judgement CheckSpec(List<double> distanceList, GaloInspTool galoInspTool)
        {
            if (distanceList.Count == 0)
                return Judgement.FAIL;

            int count = 0;
            foreach (var distance in distanceList)
            {
                if (distance < galoInspTool.SpecDistance || galoInspTool.SpecDistanceMax < distance)
                {
                    if (count > galoInspTool.Distgnore)
                            return Judgement.NG;
                    count++;
                }
            }
            return Judgement.OK;
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

            if (result.FilmAlignResult.Count != filmParam.ToolList.Count)
            {
                result.Judgement = Judgement.FAIL;
                return result;
            }
            var value = result.GetDistanceX_mm();
            if (filmParam.AmpModuleDistanceX + filmParam.FilmAlignSpecX <= value)
                result.Judgement = Judgement.NG;
            else
                result.Judgement = Judgement.OK;

            return result;
        }

        public MarkResult FindMark(CogImage8Grey cogImage, List<MarkTool> markToolList, double score)
        {
            try
            {
                foreach (var markTool in markToolList)
                {
                    //CogSearchMaxTool cogSearchMaxTool = new CogSearchMaxTool(markTool.SearchMaxTool);
                    CogSearchMaxTool cogSearchMaxTool = markTool.SearchMaxTool;
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
            }
            catch (Exception err)
            {
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

                if (Math.Abs(specT) > result.FoundDegree)
                    result.Judgement = Judgement.OK;
                else
                    result.Judgement = Judgement.NG;
            }

            return result;
        }

        private void ShiftLineSeqment(ref CogFindLineTool tool)
        {
            var lineSegment = tool.RunParams.ExpectedLineSegment;

            tool.LastRunRecordDiagEnable = CogFindLineLastRunRecordDiagConstants.None;
            tool.RunParams.CaliperSearchDirection *= (-1);
            tool.RunParams.CaliperRunParams.Edge0Polarity = tool.RunParams.CaliperRunParams.Edge1Polarity;

            double length = tool.RunParams.CaliperSearchLength / 2;
            double direction = tool.RunParams.CaliperSearchDirection;

            double radian = lineSegment.Rotation;
            double startX = lineSegment.StartX;
            double startY = lineSegment.StartY;
            double endX = lineSegment.EndX;
            double endY = lineSegment.EndY;

            PointF startPoint = new PointF((float)startX, (float)startY);
            PointF endPoint = new PointF((float)endX, (float)endY);

            if (direction < 0)
            {
                calcMovePoint(startPoint, endPoint, -length, out PointF calcStart, out PointF calcEnd);
                lineSegment.StartX = calcStart.X;
                lineSegment.StartY = calcStart.Y;

                lineSegment.EndX = calcEnd.X;
                lineSegment.EndY = calcEnd.Y;
            }
            else
            {
                calcMovePoint(startPoint, endPoint, length, out PointF calcStart, out PointF calcEnd);
                lineSegment.StartX = calcStart.X;
                lineSegment.StartY = calcStart.Y;

                lineSegment.EndX = calcEnd.X;
                lineSegment.EndY = calcEnd.Y;
            }
        }

        private void calcMovePoint(PointF point1, PointF point2, double distance, out PointF calcPoint1, out PointF calcPoint2)
        {
            // 직선의 방정식을 기준으로 직교하는 방향 벡터 계산
            double deltaX = point2.X - point1.X;
            double deltaY = point2.Y - point1.Y;
            double magnitude = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            double unitVectorX = deltaX / magnitude;
            double unitVectorY = deltaY / magnitude;

            // 각 점에 대해 이동 벡터 계산 및 적용

            calcPoint1 = new PointF();
            calcPoint1.X = (float)(point1.X + (distance * unitVectorY));
            calcPoint1.Y = (float)(point1.Y - (distance * unitVectorX));

            calcPoint2 = new PointF();
            calcPoint2.X = (float)(point2.X + (distance * unitVectorY));
            calcPoint2.Y = (float)(point2.Y - (distance * unitVectorX));
        }
    }

}
