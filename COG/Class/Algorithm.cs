using COG.Class.Core;
using COG.Class.Data;
using COG.Class.Units;
using COG.Helper;
using COG.Settings;
using Cognex.VisionPro;
using Cognex.VisionPro.Caliper;
using Cognex.VisionPro.SearchMax;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace COG.Class
{
    public class Algorithm
    {
        public void GetBinaryImage(CogImage8Grey cogImage, CogRectangleAffine boundingBox)
        {
            var cropImage = VisionProHelper.CropImage(cogImage, boundingBox, 255);

            Mat mat = GetConvertMatImage(cogImage);

            MCvScalar meanScalar = new MCvScalar();
            MCvScalar stddevScalar = new MCvScalar();

            CvInvoke.MeanStdDev(mat, ref meanScalar, ref stddevScalar);
            Mat resultMat = mat + meanScalar;

            double th = CvInvoke.Threshold(resultMat, resultMat, 0, 255, ThresholdType.Otsu);

            //VisionProHelper.convert
        }

        public void Test3(CogImage8Grey cogImage, GaloInspTool inspTool)
        {

        }

        public GaloLineToolResult RunGaloLineInspection(CogImage8Grey cogImage, GaloInspTool inspTool, ref CogRectangleAffine affineRect)
        {
            GaloLineToolResult result = new GaloLineToolResult();
            if (cogImage == null)
                return result;

            var boundingBox = VisionProHelper.GetBoundingRect(cogImage, inspTool.FindLineTool);
            GetBinaryImage(cogImage, boundingBox);


            CogFindLineTool tool = inspTool.FindLineTool;
            List<CogCompositeShape> resultGraphics0 = null;
            List<CogCompositeShape> resultGraphics1 = null;

            RollBackLineTool rollbackValue = new RollBackLineTool();
            rollbackValue.SetValue(tool);

            try
            {
                resultGraphics0 = InspectLine0(cogImage, tool, ref result);
                resultGraphics1 = InspectLine1(cogImage, tool, ref result);

                rollbackValue.RollBack(ref tool);
            }
            catch (Exception err)
            {
                rollbackValue.RollBack(ref tool);
            }

            var distanceList = result.GetDistance();
            if (distanceList.Count == 0)
            {
                result.Judgement = Judgement.FAIL;
                return result;
            }

            foreach (var distance in distanceList)
            {
                if (distance < inspTool.SpecDistance || inspTool.SpecDistanceMax < distance)
                {
                    result.Judgement = Judgement.NG;
                    return result;
                }
            }
            result.Judgement = Judgement.OK;
            result.Line0.ResultGraphics.AddRange(resultGraphics0);
            result.Line1.ResultGraphics.AddRange(resultGraphics1);

            return result;
        }
      
        public Mat GetConvertMatImage(CogImage8Grey cogImage)
        {
            IntPtr cogIntptr = GetIntptr(cogImage, out int stride);
            byte[] byteArray = new byte[stride * cogImage.Height];
            Marshal.Copy(cogIntptr, byteArray, 0, byteArray.Length);
            Mat matImage = new Mat(new Size(cogImage.Width, cogImage.Height), DepthType.Cv8U, 1, cogIntptr, stride);
            //Marshal.Copy(byteArray, 0, matImage.DataPointer, matImage.Step * matImage.Height);

            return matImage;
        }

        public IntPtr GetIntptr(CogImage8Grey image, out int stride)
        {
            unsafe
            {
                var cogPixelData = image.Get8GreyPixelMemory(CogImageDataModeConstants.Read, 0, 0, image.Width, image.Height);
                IntPtr ptrData = cogPixelData.Scan0;
                stride = cogPixelData.Stride;

                return ptrData;
            }
        }
        private CogRectangle GetROI(CogFindLineTool cogFindLine)
        {
            double length = cogFindLine.RunParams.CaliperSearchLength / 2;
            double startX = cogFindLine.RunParams.ExpectedLineSegment.StartX;
            double startY = cogFindLine.RunParams.ExpectedLineSegment.StartY;

            double endX = cogFindLine.RunParams.ExpectedLineSegment.EndX;
            double endY = cogFindLine.RunParams.ExpectedLineSegment.EndY;

            double left = startX < endX ? startX : endX;
            double top = startY < endY ? startY : endY;

            

            double right = startX > endX ? startX : endX;
            double bottom = startY > endY ? startY : endY;

            CogRectangle rect = new CogRectangle();

            rect.X = left;
            rect.Y = top;
            rect.Width = Math.Abs(left - right);
            rect.Height = Math.Abs(top- bottom);

            return rect;
        }

        private void InspectEdge(CogImage8Grey cogImage, GaloInspTool inspTool)
        {
            EdgeAlgorithm algorithm = new EdgeAlgorithm();
            algorithm.Threshold = inspTool.DarkArea.Threshold;
            algorithm.IgnoreSize = inspTool.DarkArea.IgnoreSize;
            algorithm.MaskingValue = inspTool.DarkArea.MaskingValue;
            //algorithm.Inspect()
        }


        private List<CogCompositeShape> InspectLine0(CogImage8Grey cogImage, CogFindLineTool tool, ref GaloLineToolResult result)
        {
            tool.InputImage = cogImage as CogImage8Grey;
            tool.RunParams.CaliperRunParams.EdgeMode = CogCaliperEdgeModeConstants.SingleEdge;
            tool.LastRunRecordDiagEnable = CogFindLineLastRunRecordDiagConstants.None;
            tool.Run();

            List<CogCompositeShape> resultGraphicsList = new List<CogCompositeShape>();
            if (tool.Results?.Count > 0)
            {
                for (int i = 0; i < tool.Results.Count; i++)
                {
                    if (tool.Results[i].CaliperResults.Count > 0)
                    {
                        var caliperResult = tool.Results[i].CaliperResults;
                        PointF edge0Point = new PointF((float)caliperResult[0].Edge0.PositionX, (float)caliperResult[0].Edge0.PositionY);
                        result.Line0.Edge0PointList.Add(edge0Point);

                        var graphics = tool.Results[i].CreateResultGraphics(CogFindLineResultGraphicConstants.CaliperEdge);
                        resultGraphicsList.Add(graphics);
                    }
                    else
                    {
                        result.Line0.Edge0PointList.Add(new PointF());
                    }
                }
            }
            return resultGraphicsList;
        }

        private List<CogCompositeShape> InspectLine1(CogImage8Grey cogImage, CogFindLineTool tool, ref GaloLineToolResult result)
        {
            tool.InputImage = cogImage as CogImage8Grey;
            tool.RunParams.CaliperRunParams.EdgeMode = CogCaliperEdgeModeConstants.SingleEdge;
            tool.LastRunRecordDiagEnable = CogFindLineLastRunRecordDiagConstants.None;
            tool.RunParams.CaliperSearchDirection *= (-1);
            tool.RunParams.CaliperRunParams.Edge0Polarity = tool.RunParams.CaliperRunParams.Edge1Polarity;
            //tool.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.DontCare;

            double length = tool.RunParams.CaliperSearchLength / 2;
            double radian = tool.RunParams.ExpectedLineSegment.Rotation;
            double direction = tool.RunParams.CaliperSearchDirection;

            double startX = tool.RunParams.ExpectedLineSegment.StartX;
            double startY = tool.RunParams.ExpectedLineSegment.StartY;
            double endX = tool.RunParams.ExpectedLineSegment.EndX;
            double endY = tool.RunParams.ExpectedLineSegment.EndY;

            PointF startPoint = new PointF((float)startX, (float)startY);
            PointF endPoint = new PointF((float)endX, (float)endY);

            calcMovePoint(startPoint, endPoint, -length, out PointF calcStart, out PointF calcEnd);
            tool.RunParams.ExpectedLineSegment.StartX = calcStart.X;
            tool.RunParams.ExpectedLineSegment.StartY = calcStart.Y;

            tool.RunParams.ExpectedLineSegment.EndX = calcEnd.X;
            tool.RunParams.ExpectedLineSegment.EndY = calcEnd.Y;

            tool.Run();
            List<CogCompositeShape> resultGraphicsList = new List<CogCompositeShape>();
            if (tool.Results?.Count > 0)
            {
                UI.Forms.PatternTeachForm.CogRecord = tool.CreateCurrentRecord();
                for (int i = 0; i < tool.Results.Count; i++)
                {
                    if (tool.Results[i].CaliperResults.Count > 0)
                    {
                        var caliperResult = tool.Results[i].CaliperResults;
                        PointF edge0Point = new PointF((float)caliperResult[0].Edge0.PositionX, (float)caliperResult[0].Edge0.PositionY);
                        result.Line1.Edge0PointList.Add(edge0Point);

                        var graphics = tool.Results[i].CreateResultGraphics(CogFindLineResultGraphicConstants.CaliperEdge);
                        resultGraphicsList.Add(graphics);
                    }
                    else
                    {
                        result.Line1.Edge0PointList.Add(new PointF());
                    }
                }
            }
            return resultGraphicsList;
        }

        void calcMovePoint(PointF point1, PointF point2, double distance, out PointF calcPoint1, out PointF calcPoint2)
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

        public GaloCircleToolResult RunGaloCircleInspection(CogImage8Grey cogImage, GaloInspTool inspTool)
        {
            GaloCircleToolResult result = new GaloCircleToolResult(); 
            if (cogImage == null)
                return result;

            Test3(cogImage, inspTool);
            //CogFindCircleTool tool = new CogFindCircleTool(inspTool.FindCircleTool);
            CogFindCircleTool tool = inspTool.FindCircleTool;
            tool.InputImage = cogImage as CogImage8Grey;
            tool.Run();

            if (tool.Results?.Count > 0)
            {
                for (int i = 0; i < tool.Results.Count; i++)
                {
                    if(tool.Results[i].CaliperResults.Count > 0)
                    {
                        var caliperResult = tool.Results[i].CaliperResults;
                        PointF edge0Point = new PointF((float)caliperResult[0].Edge0.PositionX, (float)caliperResult[0].Edge0.PositionY);
                        result.Edge0PointList.Add(edge0Point);

                        PointF edge1Point = new PointF((float)caliperResult[0].Edge1.PositionX, (float)caliperResult[0].Edge1.PositionY);
                        result.Edge1PointList.Add(edge1Point);

                        var graphics = tool.Results[i].CreateResultGraphics(CogFindCircleResultGraphicConstants.CaliperEdge);
                        result.ResultGraphics.Add(graphics);
                    }
                    else
                    {
                        result.Edge0PointList.Add(new PointF());
                        result.Edge1PointList.Add(new PointF());
                    }
                }
            }

            if (result.Edge0PointList.Count <= 0)
            {
                result.Judgement = Judgement.FAIL;
                return result;
            }
            var distanceList = result.GetDistance();

            for (int i = 0; i < distanceList.Count(); i++)
            {
                var distance = distanceList[i];
                distance *= (StaticConfig.PixelResolution / 1000);

                if (inspTool.SpecDistance > distance && distance > inspTool.SpecDistanceMax)
                {
                    result.Judgement = Judgement.NG;
                    return result;
                }
            }

            result.Judgement = Judgement.OK;

            return result;
        }

        private void Position_Calculate(double x, double y, double length, double radian, out double calcX, out double calcY)
        {
            calcX = x + (length * Math.Sin(radian));
            calcY = y + (length * Math.Cos(radian));
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

    public class RollBackLineTool
    {
        public double ContrastThreshold { get; set; }

        public int FilterHalfSizeInPixels { get; set; }

        public double CaliperSearchDirection { get; set; }

        public CogCaliperPolarityConstants Edge0Polarity { get; set; }

        public double StartX { get; set; }

        public double StartY { get; set; }

        public double EndX { get; set; }

        public double EndY { get; set; }

        public void SetValue(CogFindLineTool lineTool)
        {
            ContrastThreshold = lineTool.RunParams.CaliperRunParams.ContrastThreshold;
            FilterHalfSizeInPixels = lineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels;
            CaliperSearchDirection = lineTool.RunParams.CaliperSearchDirection;
            Edge0Polarity = lineTool.RunParams.CaliperRunParams.Edge0Polarity;
            StartX = lineTool.RunParams.ExpectedLineSegment.StartX;
            StartY = lineTool.RunParams.ExpectedLineSegment.StartY;
            EndX = lineTool.RunParams.ExpectedLineSegment.EndX;
            EndY = lineTool.RunParams.ExpectedLineSegment.EndY;
        }

        public void RollBack(ref CogFindLineTool lineTool)
        {
            lineTool.RunParams.CaliperRunParams.ContrastThreshold = ContrastThreshold;
            lineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels = FilterHalfSizeInPixels;
            lineTool.RunParams.CaliperSearchDirection = CaliperSearchDirection;
            lineTool.RunParams.CaliperRunParams.Edge0Polarity = Edge0Polarity;
            lineTool.RunParams.ExpectedLineSegment.StartX = StartX;
            lineTool.RunParams.ExpectedLineSegment.StartY = StartY;
            lineTool.RunParams.ExpectedLineSegment.EndX = EndX;
            lineTool.RunParams.ExpectedLineSegment.EndY = EndY;
        }
    }
} 
