using COG.Class.Core;
using COG.Class.Data;
using COG.Class.Units;
using COG.Helper;
using COG.Settings;
using COG.UI.Forms;
using Cognex.VisionPro;
using Cognex.VisionPro.Caliper;
using Cognex.VisionPro.ImageProcessing;
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
    
    public partial class Algorithm
    {
        public CogImage8Grey GetBinaryImage(CogImage8Grey cropImage, CogRectangleAffine boundingBox)
        {
            Mat cropMat = ImageHelper.GetConvertMatImage(cropImage as CogImage8Grey);
            MCvScalar meanScalar = new MCvScalar();
            MCvScalar stddevScalar = new MCvScalar();
            //cropMat.Save(@"D:\123.bmp");

            Mat resultMat = cropMat + meanScalar;
            Mat maskingMat = CreateMaskingMat(cropMat, boundingBox);
            maskingMat.Save(@"D:\123.bmp");
            CvInvoke.MeanStdDev(cropMat, ref meanScalar, ref stddevScalar, maskingMat);

            double th = CvInvoke.Threshold(resultMat, resultMat, meanScalar.V0, 255, ThresholdType.Binary); // 150
            //resultMat.Save(@"D:\123.bmp");
            var area = boundingBox.Area;
            resultMat = GetSizeFilterImage(resultMat, 10); // 10
            var binaryCogImage = VisionProHelper.CovertGreyImage(resultMat.DataPointer, resultMat.Width, resultMat.Height, resultMat.Step);

            return binaryCogImage;
        }

        protected Mat CreateMaskingMat(Mat matImage, CogRectangleAffine boundingBox)
        {
            Mat maskImage = new Mat(matImage.Height, matImage.Width, DepthType.Cv8U, 1);
            maskImage.SetTo(new MCvScalar(0));

            int rightTopX = (int)boundingBox.CornerOriginX;
            int rightTopY = (int)boundingBox.CornerOriginY;

            int rightBottomX = (int)boundingBox.CornerXX;
            int rightBottomY = (int)boundingBox.CornerXY;

            int leftTopX = (int)boundingBox.CornerYX;
            int leftTopY = (int)boundingBox.CornerYY;

            int leftBottomX = (int)boundingBox.CornerOppositeX;
            int leftBottomY = (int)boundingBox.CornerOppositeY;

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            VectorOfPoint contour = new VectorOfPoint(new[]
            {
                new Point(rightTopX, rightTopY),
                new Point(rightBottomX, rightBottomY),
                new Point(leftBottomX, leftBottomY),
                new Point(leftTopX, leftTopY),
            });
            contours.Push(contour);

            CvInvoke.DrawContours(maskImage, contours, -1, new MCvScalar(255), -1);

            return maskImage;
        }

        protected Mat CreateMaskingMat(Mat matImage, CogPolygon boundingBox)
        {
            Mat maskImage = new Mat(matImage.Height, matImage.Width, DepthType.Cv8U, 1);
            maskImage.SetTo(new MCvScalar(0));

            var vertices = boundingBox.GetVertices();
            if (vertices == null)
                return null;

            int count = vertices.Length / 2;
            List<Point> points = new List<Point>();
            for (int i = 0; i < count; i++)
                points.Add(new Point((int)vertices[i, 0], (int)vertices[i, 1]));

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            VectorOfPoint contour = new VectorOfPoint(points.ToArray());
            contours.Push(contour);

            CvInvoke.DrawContours(maskImage, contours, -1, new MCvScalar(255), -1);

            return maskImage;
        }

        protected Mat GetSizeFilterImage(Mat mat, int ignoreSize)
        {
            var contours = new VectorOfVectorOfPoint();
            Mat hierarchy = new Mat();
            CvInvoke.FindContours(mat, contours, hierarchy, RetrType.Ccomp, ChainApproxMethod.ChainApproxSimple);

            List<VectorOfPoint> filteredContourList = new List<VectorOfPoint>();
            if (contours.Size != 0)
            {
                float[] hierarchyArray = MatToFloatArray(hierarchy);
                for (int idxContour = 0; idxContour < contours.Size; ++idxContour)
                {
                    //if (hierarchyArray[idxContour * 4 + 3] > -0.5)
                    //    continue;

                    var contour = contours[idxContour];
                    var hull = new VectorOfPoint();
                    CvInvoke.ConvexHull(contour, hull, true);

                    double area = CvInvoke.ContourArea(contour);

                    if (area > ignoreSize)
                    {
                        filteredContourList.Add(contour);
                    }

                }
            }
            Mat filteredImage = new Mat(new Size(mat.Width, mat.Height), DepthType.Cv8U, 1);
            byte[] tempArray = new byte[mat.Step * mat.Height];
            Marshal.Copy(tempArray, 0, filteredImage.DataPointer, mat.Step * mat.Height);

            IInputArrayOfArrays contoursArray = new VectorOfVectorOfPoint(filteredContourList.Select(vector => vector.ToArray()).ToArray());
            CvInvoke.DrawContours(filteredImage, contoursArray, -1, new MCvScalar(255), -1);

            return filteredImage;
        }

        public float[] MatToFloatArray(Mat mat)
        {
            float[] floatArray = new float[mat.Width * mat.Height * mat.NumberOfChannels];
            Marshal.Copy(mat.DataPointer, floatArray, 0, floatArray.Length);
            return floatArray;
        }

        public Judgement CheckSpec(List<PointF> points1, List<PointF> points2, GaloInspTool galoInspTool)
        {
            var distanceList = MathHelper.GetDistance(points1, points2);
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
