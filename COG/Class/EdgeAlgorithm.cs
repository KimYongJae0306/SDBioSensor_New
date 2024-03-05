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
    /* Sample
         * [Top] : 분주 상단
         *  EdgeAlgorithm algo = new EdgeAlgorithm();
         *  algo.Threshold = 30;
         *  algo.IgnoreSize = 10;
         *  algo.MaskingValue = 180;
         *  algo.Inspect(cogImage, ref tool, EdgeDirection.Top);
         *  
         *  
         * [Left] : 이미지 왼쪽 하단
         *  EdgeAlgorithm algo = new EdgeAlgorithm();
         *  algo.Threshold = 8;
         *  algo.IgnoreSize = 10;
         *  algo.MaskingValue = 180;
         *  algo.Inspect(cogImage, ref tool, EdgeDirection.Left);
         */
    public class EdgeAlgorithm
    {
        // Top -> Threshold : 30, IgnoreSize : 10, MaskingValue : 180
        // Left -> Threshold : 8 , IgnoreSize : 10

        public int Threshold { get; set; } = 30;

        public int IgnoreSize { get; set; } = 10;

        public int MaskingValue { get; set; } = 180;

        public Mat Inspect(CogImage8Grey cogImage, ref CogFindLineTool tool, EdgeDirection direction, CogTransform2DLinear transform, CogRectangle cropRect)
        {
            Mat matImage = GetConvertMatImage(cogImage);

            Mat edgeEnhanceCogImage = GetEdgeProcessingImage(matImage, direction, Threshold, IgnoreSize);
            matImage.Dispose();


            return edgeEnhanceCogImage;
        }

        //public Mat Inspect(CogImage8Grey cogImage, CogFindLineTool tool)
        //{
        //    Mat matImage = GetConvertMatImage(cogImage);


        //}

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

        //private Rectangle GetROI(CogFindLineTool tool)
        //{
        //    var lineSegment = tool.RunParams.ExpectedLineSegment;
        //    lineSegment.GetStartEnd(out double startX, out double startY, out double endX, out double endY);
        //    lineSegment.GetStartLengthRotation(out double startX_temp, out double startY_temp, out double length, out double rotation);

        //    if(rotation > 0)
        //    {

        //    }
        //    else
        //    {
        //        if(derection > 0)
        //        {

        //        }
        //        else
        //        {

        //        }
        //    }
        //}


        public List<int> GetVerticalMinEdgeTopPosY(Mat mat, int nTopCutpixel, int nBottomCutPixel)
        {
            int searchGap = 3;

            unsafe
            {
                List<int> valueList = new List<int>();

                IntPtr ptrData = mat.DataPointer;
                int stride = mat.Step;
                byte* data = (byte*)(void*)ptrData;

                for (int w = 0; w < mat.Width; w += searchGap)
                {
                    for (int h = 0; h < mat.Height; h++)
                    {
                        int index = (stride * h) + w;
                        int value = Convert.ToInt32(data[index]);
                        if (h < nTopCutpixel)
                            continue;

                        if (h > mat.Height - nBottomCutPixel)
                            continue;
                        if (value == 0)
                        {
                            valueList.Add(h);
                            break;
                        }
                    }
                }
                if (valueList.Count > 0)
                {
                    return valueList;
                }
                else
                {
                    return new List<int>();
                }
            }
        }

        public List<EdgePoint> GetVerticalEdgeBottomPos(Mat mat, int nTopCutpixel, int nBottomCutPixel)
        {
            int searchGap = 3;

            unsafe
            {
                List<EdgePoint> pointList = new List<EdgePoint>();

                IntPtr ptrData = mat.DataPointer;
                int stride = mat.Step;
                byte* data = (byte*)(void*)ptrData;

                for (int w = 0; w < mat.Width; w += searchGap)
                {
                    for (int h = mat.Height - 1; h > 0; h--)
                    {
                        int index = (stride * h) + w;
                        int value = Convert.ToInt32(data[index]);
                        int temp = mat.Height - 1 - h;
                        if (mat.Height - 1 - h < nBottomCutPixel)
                            continue;

                        if (h < nTopCutpixel)
                            continue;
                        if (value == 0)
                        {
                            pointList.Add(new EdgePoint(w, h));
                            break;
                        }
                    }
                }

                return pointList;
            }
        }

        public int GetHorizontalMinEdgePosY(Mat mat, int nTopCutpixel, int nBottomCutPixel)
        {
            int searchGap = 3;

            unsafe
            {
                List<int> valueList = new List<int>();

                IntPtr ptrData = mat.DataPointer;
                int stride = mat.Step;
                byte* data = (byte*)(void*)ptrData;

                for (int h = 0; h < mat.Height; h += searchGap)
                {
                    for (int w = 0; w < mat.Width; w++)
                    {
                        int index = (stride * h) + w;
                        int value = Convert.ToInt32(data[index]);
                        if (w < nTopCutpixel)
                            continue;

                        if (w > mat.Width - nBottomCutPixel)
                            continue;
                        if (value == 0)
                        {
                            valueList.Add(w);
                            break;
                        }
                    }
                }
                if (valueList.Count > 0)
                {
                    return valueList.Min();
                }
                else
                {
                    return -1;
                }
            }
        }

        public List<EdgePoint> GetHorizontalEdgePos(Mat mat, int nTopCutpixel, int nBottomCutPixel)
        {
            int searchGap = 3;

            unsafe
            {
                List<EdgePoint> valueList = new List<EdgePoint>();

                IntPtr ptrData = mat.DataPointer;
                int stride = mat.Step;
                byte* data = (byte*)(void*)ptrData;

                for (int h = 0; h < mat.Height; h += searchGap)
                {
                    for (int w = mat.Width - 1; w > 0; w--)
                    {
                        int index = (stride * h) + w;
                        int value = Convert.ToInt32(data[index]);

                        int temp = mat.Width - 1 - w;
                        if (mat.Width - 1 - w < nTopCutpixel)
                            continue;

                        if (w > mat.Width - nBottomCutPixel)
                            continue;
                        if (value == 0)
                        {
                            valueList.Add(new EdgePoint(w, h));
                            break;
                        }
                    }
                }
                return valueList;
            }
        }

        public Mat CropRoi(Mat mat, Rectangle roi)
        {
            int padLeft = 0 - roi.X;
            int padTop = 0 - roi.Y;
            int padRight = (roi.X + roi.Width) - mat.Width;
            int padBottom = (roi.Y + roi.Height) - mat.Height;
            Rectangle nonPadRect = new Rectangle(
                roi.X + (padLeft > 0 ? padLeft : 0),
                roi.Y + (padTop > 0 ? padTop : 0),
                roi.Width - (padRight > 0 ? padRight : 0) - (padLeft > 0 ? padLeft : 0),
                roi.Height - (padBottom > 0 ? padBottom : 0) - (padTop > 0 ? padTop : 0));

            Rectangle matRect = new Rectangle(0, 0, mat.Width, mat.Height);
            matRect.Intersect(nonPadRect);
            if (matRect.IsEmpty)
                return Mat.Zeros(roi.Height, roi.Width, DepthType.Cv8U, mat.NumberOfChannels);

            Mat boundMatOrigin = new Mat(mat, nonPadRect).Clone();
            if (padLeft > 0 || padTop > 0 || padRight > 0 || padBottom > 0)
            {
                CvInvoke.CopyMakeBorder(boundMatOrigin, boundMatOrigin,
                    padTop > 0 ? padTop : 0,
                    padBottom > 0 ? padBottom : 0,
                    padLeft > 0 ? padLeft : 0,
                    padRight > 0 ? padRight : 0,
                    BorderType.Constant, new MCvScalar(0));
            }
            return boundMatOrigin;
        }

        private List<double> GetEdgeYPos(CogImage8Grey cogImage, CogFindLineTool tool, CogTransform2DLinear transform, CogRectangle cropRect)
        {
            CogFindLineTool mappingTool = new CogFindLineTool(tool);
            List<double> foundPosYList = new List<double>();

            double startX = tool.RunParams.ExpectedLineSegment.StartX;
            double startY = tool.RunParams.ExpectedLineSegment.StartY;
            double endX = tool.RunParams.ExpectedLineSegment.EndX;
            double endY = tool.RunParams.ExpectedLineSegment.EndY;
            var liner = mappingTool.RunParams.ExpectedLineSegment.GetParentFromChildTransform() as CogTransform2DLinear;
            liner.MapPoint(startX, startY, out double mapX, out double mapY);

            var liner2 = cogImage.PixelFromRootTransform as CogTransform2DLinear;
            liner2.MapPoint(mapX, mapY, out double mapX33, out double mapY333);
            transform.MapPoint(startX, startY, out double x1, out double x2);


            mappingTool.InputImage = cogImage;
            mappingTool.Run();

            if (mappingTool.Results != null)
            {
                foreach (CogFindLineResult result in mappingTool.Results)
                {
                    if (result.Found)
                        foundPosYList.Add(result.Y);
                }
            }

            return foundPosYList;
        }

        public Mat GetEdgeProcessingImage(Mat mat, EdgeDirection direction, int threshold, int ignoreSize)
        {
            CvInvoke.GaussianBlur(mat, mat, new Size(5, 5), 2);

            Mat shadowMat = AddShadow(mat, direction);
            //shadowMat.Save(@"D:\shadowMat.bmp");

            Mat dest = new Mat();
            CvInvoke.Threshold(shadowMat, dest, threshold, 255, ThresholdType.Binary);
            //dest.Save(@"D:\dest.bmp");
            var maskImage = GetSizeFilterImage(dest, ignoreSize);

            shadowMat.Dispose();
            dest.Dispose();

            return maskImage;
        }

        private Mat GetSizeFilterImage(Mat mat, int ignoreSize)
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

                    if (area >= ignoreSize)
                        filteredContourList.Add(contour);
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

        public Mat AddShadow(Mat mat, EdgeDirection direction)
        {
            Mat outputMat = new Mat(mat.Size, DepthType.Cv8U, 1);
            CvInvoke.Filter2D(mat, outputMat, GetEdgeShadowKernel(direction), new Point(0, 0));

            return outputMat;
        }

        private ConvolutionKernelF GetEdgeShadowKernel(EdgeDirection direction)
        {
            float[,] matrix;
            if (direction == EdgeDirection.Top)
            {
                matrix = new float[3, 3] {
                      { 1, 1, 1 },
                      { 1, 1, -1},
                      { -1, -1, -1 }
                    };
            }
            else if (direction == EdgeDirection.Left)
            {
                matrix = new float[3, 3] {
                      { 0, 1, 2 },
                      { -1, 1, 1},
                      { -2, -1, 0 }
                    };
            }
            else
            {
                matrix = new float[3, 3] {
                      { 1, 1, 1 },
                      { 1, 1, -1},
                      { -1, -1, -1 }
                    };
            }

            return new ConvolutionKernelF(matrix);
        }
        public CogImage8Grey GetConvertCogImage(Mat mat)
        {
            CogImage8Root root = new CogImage8Root();
            root.Initialize(mat.Width, mat.Height, mat.DataPointer, mat.Step, null);
            var cogImage = new CogImage8Grey();
            cogImage.SetRoot(root);

            return cogImage;
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
    }

    public enum EdgeDirection
    {
        Top,
        Left,
    }

    public class EdgePoint
    {
        public int PointX;
        public int PointY;

        public EdgePoint(int x, int y)
        {
            PointX = x;
            PointY = y;
        }
    }
}
