using COG.Class.Core;
using COG.Class.Data;
using COG.Helper;
using Cognex.VisionPro;
using Cognex.VisionPro.Caliper;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace COG.Class
{
    public partial class LineAlgorithm : Algorithm
    {
        private EdgeAlgorithm _edgeAlgorithm = new EdgeAlgorithm();

        public LineResult InspectDarkArea(CogImage8Grey cogImage, GaloInspTool inspTool, bool isInside, bool isDebug)
        {
            LineResult result = new LineResult();
            var lineSeqment = inspTool.FindLineTool.RunParams.ExpectedLineSegment;

            var boundingBox = VisionProHelper.GetBoundingRect(cogImage, inspTool.FindLineTool);
            var cropCogImage = VisionProHelper.CropImage(cogImage, boundingBox, 255);

            if (cropCogImage == null)
                return result;

            Mat cropMat = ImageHelper.GetConvertMatImage(cropCogImage as CogImage8Grey);
            var cropLeftTopPoint = VisionProHelper.GetCropLeftTop(boundingBox);

            inspTool.FindLineTool.RunParams.ExpectedLineSegment.GetStartEnd(out double startX, out double startY, out double endX, out double endY);
            double verticalSize = Math.Abs(startX - endX);
            double horizontalSize = Math.Abs(startY - endY);

            if (verticalSize > horizontalSize)
                result.EdgeEnhanceMat = _edgeAlgorithm.GetEdgeProcessingImage(cropMat, inspTool, true, isInside, isDebug);
            else
                result.EdgeEnhanceMat = _edgeAlgorithm.GetEdgeProcessingImage(cropMat, inspTool, false, isInside, isDebug);

            result.CropImage = cropCogImage as CogImage8Grey;
            result.EdgeEnhanceImage = VisionProHelper.CovertGreyImage(result.EdgeEnhanceMat.DataPointer, result.EdgeEnhanceMat.Width, result.EdgeEnhanceMat.Height, result.EdgeEnhanceMat.Step);

            
            SetLineSegmentOffset(ref lineSeqment, new PointF(-cropLeftTopPoint.X, -cropLeftTopPoint.Y));

            inspTool.FindLineTool.RunParams.CaliperRunParams.EdgeMode = CogCaliperEdgeModeConstants.SingleEdge;
            inspTool.FindLineTool.LastRunRecordDiagEnable = CogFindLineLastRunRecordDiagConstants.None;
            inspTool.FindLineTool.InputImage = result.EdgeEnhanceImage;
            inspTool.FindLineTool.Run();

            if(inspTool.FindLineTool.Results == null)
            {
                if(inspTool.FindLineTool.Results.Count == 0)
                {
                    inspTool.FindLineTool.InputImage = cropCogImage as CogImage8Grey;
                    inspTool.FindLineTool.Run();
                }
            }
            else
                result.DetectEdgeAlgorithm = true;

            SetLineSegmentOffset(ref lineSeqment, new PointF(cropLeftTopPoint.X, cropLeftTopPoint.Y));
            CreateResultGraphics(inspTool.FindLineTool.Results, cropLeftTopPoint, out List<PointF> edgePointList, out List<CogCompositeShape> cogCompositeShapes);
          
            result.PointList.AddRange(edgePointList);
            result.GraphicsList.AddRange(cogCompositeShapes);

            return result;
        }

        private void CreateResultGraphics(CogFindLineResults findLineResults, Point offsetPoint, out List<PointF> edgePointList, out List<CogCompositeShape> cogCompositeShapes)
        {
            cogCompositeShapes = new List<CogCompositeShape>();
            edgePointList = new List<PointF>();
            if (findLineResults == null)
                return;

            if (findLineResults.Count > 0)
            {
                for (int i = 0; i < findLineResults.Count; i++)
                {
                    if (findLineResults[i].CaliperResults.Count > 0)
                    {
                        var caliperResult = findLineResults[i].CaliperResults;
                        PointF edge0Point = new PointF((float)caliperResult[0].Edge0.PositionX + offsetPoint.X, (float)caliperResult[0].Edge0.PositionY + offsetPoint.Y);
                        edgePointList.Add(edge0Point);

                        var graphics = findLineResults[i].CreateResultGraphics(CogFindLineResultGraphicConstants.CaliperEdge);
                        foreach (var shape in graphics.Shapes)
                        {
                            var lineSegmentGraphics = shape as CogLineSegment;
                            lineSegmentGraphics.StartX += offsetPoint.X;
                            lineSegmentGraphics.StartY += offsetPoint.Y;
                            lineSegmentGraphics.EndX += offsetPoint.X;
                            lineSegmentGraphics.EndY += offsetPoint.Y;

                        }
                        cogCompositeShapes.Add(graphics);
                    }
                    else
                    {
                        edgePointList.Add(new PointF());
                    }
                }
            }
        }

        public LineResult InspectLine(CogImage8Grey cogImage, CogFindLineTool tool, bool isDebug)
        {
            LineResult result = new LineResult();
            var lineSeqment = tool.RunParams.ExpectedLineSegment;

            var boundingBox = VisionProHelper.GetBoundingRect(cogImage, tool);
            var cropCogImage = VisionProHelper.CropImage(cogImage, boundingBox, 0);
            var cropLeftTopPoint = VisionProHelper.GetCropLeftTop(boundingBox);

            boundingBox.CenterX -= cropLeftTopPoint.X;
            boundingBox.CenterY -= cropLeftTopPoint.Y;

            var cropBinaryCogImage = GetBinaryImage(cropCogImage as CogImage8Grey, boundingBox);

            SetLineSegmentOffset(ref lineSeqment, new PointF(-cropLeftTopPoint.X, -cropLeftTopPoint.Y));

            tool.RunParams.CaliperRunParams.EdgeMode = CogCaliperEdgeModeConstants.SingleEdge;
            tool.LastRunRecordDiagEnable = CogFindLineLastRunRecordDiagConstants.None;
            tool.InputImage = cropBinaryCogImage;
            tool.Run();

            SetLineSegmentOffset(ref lineSeqment, new PointF(cropLeftTopPoint.X, cropLeftTopPoint.Y));

            CreateResultGraphics(tool.Results, cropLeftTopPoint, out List<PointF> edgePointList, out List<CogCompositeShape> cogCompositeShapes);

            result.PointList.AddRange(edgePointList);
            result.GraphicsList.AddRange(cogCompositeShapes);

            return result;
        }

        private void SetLineSegmentOffset(ref CogLineSegment cogLineSegment, PointF offset)
        {
            cogLineSegment.StartX += offset.X;
            cogLineSegment.StartY += offset.Y;
            cogLineSegment.EndX += offset.X;
            cogLineSegment.EndY += offset.Y;
        }
    }
}
