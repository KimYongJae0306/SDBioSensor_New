using COG.Class.Core;
using COG.Class.Data;
using COG.Helper;
using Cognex.VisionPro;
using Cognex.VisionPro.Caliper;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Class
{
    public class CircleAlgorithm : Algorithm
    {
        public List<CogCompositeShape> InspectCircle(CogImage8Grey cogImage,  CogFindCircleTool tool, ref GaloCircleToolResult result, bool isDebug, GaloInspTool inspTool)
        {
            List<CogCompositeShape> resultGraphicsList = new List<CogCompositeShape>();

            tool.RunParams.CaliperRunParams.FilterHalfSizeInPixels = 5; //확인 필요
            var cogPolygonBoundingBox = VisionProHelper.GetBoundingPolygon(cogImage, inspTool.FindCircleTool);
            var cropCogImage = VisionProHelper.CropImage(cogImage, cogPolygonBoundingBox, 255);
            var cropLeftTopPoint = VisionProHelper.GetCropLeftTop(cogPolygonBoundingBox);

            SetCogPolygonOffset(ref cogPolygonBoundingBox, new Point(-cropLeftTopPoint.X, -cropLeftTopPoint.Y));
            var binaryImage = GetBinaryImage(cropCogImage as CogImage8Grey, cogPolygonBoundingBox);
            VisionProHelper.Save(binaryImage, @"D:\123.bmp");
            CogCircularArc arc = tool.RunParams.ExpectedCircularArc;
            SetCogCircularArcOffset(ref arc, new PointF(-cropLeftTopPoint.X, -cropLeftTopPoint.Y));

            tool.RunParams.CaliperRunParams.EdgeMode = CogCaliperEdgeModeConstants.Pair;
            tool.InputImage = binaryImage as CogImage8Grey;
            tool.Run();

            SetCogCircularArcOffset(ref arc, new PointF(cropLeftTopPoint.X, cropLeftTopPoint.Y));

            CreateResultGraphics(tool.Results, cropLeftTopPoint, out List<PointF> edge0PointList, out List<PointF> edge1PointList, out List<CogCompositeShape> cogCompositeShapes);
            result.Edge0PointList.AddRange(edge0PointList);
            result.Edge1PointList.AddRange(edge1PointList);

            resultGraphicsList.AddRange(cogCompositeShapes);

            return resultGraphicsList;
        }

        private void CreateResultGraphics(CogFindCircleResults findCircleResults, Point offsetPoint, out List<PointF> edge0PointList, out List<PointF> edge1PointList, out List<CogCompositeShape> cogCompositeShapes)
        {
            cogCompositeShapes = new List<CogCompositeShape>();
            edge0PointList = new List<PointF>();
            edge1PointList = new List<PointF>();
            if (findCircleResults == null)
                return;

            if (findCircleResults.Count > 0)
            {
                for (int i = 0; i < findCircleResults.Count; i++)
                {
                    if (findCircleResults[i].CaliperResults.Count > 0)
                    {
                        var caliperResult = findCircleResults[i].CaliperResults;
                        PointF edge0Point = new PointF((float)caliperResult[0].Edge0.PositionX + offsetPoint.X, (float)caliperResult[0].Edge0.PositionY + offsetPoint.Y);
                        edge0PointList.Add(edge0Point);

                        PointF edge1Point = new PointF((float)caliperResult[0].Edge1.PositionX + offsetPoint.X, (float)caliperResult[0].Edge1.PositionY + offsetPoint.Y);
                        edge1PointList.Add(edge1Point);

                        var graphics = findCircleResults[i].CreateResultGraphics(CogFindCircleResultGraphicConstants.CaliperEdge);
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
                        edge0PointList.Add(new PointF());
                        edge1PointList.Add(new PointF());
                    }
                }
            }
        }

        private void SetCogCircularArcOffset(ref CogCircularArc cogCircularArc, PointF offset)
        {
            cogCircularArc.CenterX += offset.X;
            cogCircularArc.CenterY += offset.Y;
        }

        public CogImage8Grey GetBinaryImage(CogImage8Grey cropImage, CogPolygon boundingBox)
        {
            Mat cropMat = ImageHelper.GetConvertMatImage(cropImage as CogImage8Grey);
            MCvScalar meanScalar = new MCvScalar();
            MCvScalar stddevScalar = new MCvScalar();
            //cropMat.Save(@"D:\123.bmp");

            Mat resultMat = cropMat + meanScalar;
            Mat maskingMat = CreateMaskingMat(cropMat, boundingBox);
            //resultMat.Save(@"D:\123.bmp");
            //maskingMat.Save(@"D:\123.bmp");
            CvInvoke.MeanStdDev(cropMat, ref meanScalar, ref stddevScalar, maskingMat);

            double th = CvInvoke.Threshold(resultMat, resultMat, meanScalar.V0, 255, ThresholdType.Binary); // 150

            var area = boundingBox.Area;
            //resultMat.Save(@"D:\123.bmp");
            resultMat = GetSizeFilterImage(resultMat, 10); // 10
            //resultMat.Save(@"D:\123.bmp");
            var binaryCogImage = VisionProHelper.CovertGreyImage(resultMat.DataPointer, resultMat.Width, resultMat.Height, resultMat.Step);

            return binaryCogImage;
        }

        private void SetCogPolygonOffset(ref CogPolygon cogPolygon, Point offset)
        {
            var vertices = cogPolygon.GetVertices();
            if (vertices == null)
                return;
            int count = vertices.Length / 2;

            for (int i = 0; i < count; i++)
            {
                var newX = vertices[i, 0] + offset.X;
                var newY = vertices[i, 1] + offset.Y;

                cogPolygon.SetVertex(i, newX, newY);
            }
        }
    }
}
