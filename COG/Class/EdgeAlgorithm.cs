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
    public class EdgeAlgorithm
    {
        public Mat GetEdgeProcessingImage(Mat mat, GaloInspTool galoInspTool, bool isVertical, bool isInside, bool isDebug)
        {
            var inspParam = galoInspTool.DarkArea;

            Mat tempMat = mat.Clone();
            CvInvoke.GaussianBlur(mat, tempMat, new Size(5, 5), 2);

            Mat shadowMat = AddShadow(tempMat, isVertical);
            if (isDebug)
                shadowMat.Save(@"D:\shadowMat.bmp");

            Mat dest = new Mat();
            CvInvoke.Threshold(shadowMat, dest, inspParam.Threshold, 255, ThresholdType.Binary);
            if (isDebug)
                dest.Save(@"D:\dest.bmp");

            var binaryImage = GetSizeFilterImage(dest, inspParam.IgnoreSize);
            if (isDebug)
                dest.Save(@"D:\binaryImage.bmp");

            List<Point> searchedPoints = SearchEdge(binaryImage, galoInspTool, isVertical, isInside);
            if(searchedPoints.Count > 0)
            {
                int MaskingValue = inspParam.MaskingValue;
                //MCvScalar maskingColor = new MCvScalar(141);
                MCvScalar maskingColor = new MCvScalar(MaskingValue);
                CvInvoke.FillPoly(mat, new VectorOfPoint(searchedPoints.ToArray()), maskingColor);
            }

            binaryImage.Dispose();
            shadowMat.Dispose();
            dest.Dispose();

            return mat;
        }

        private List<Point> SearchEdge(Mat image, GaloInspTool galoInspTool, bool isVertical, bool isInside)
        {
            var inspParam = galoInspTool.DarkArea;
            List<Point> searchedPointList = new List<Point>();
            SetCutPixelValue(inspParam, isInside, out int startCutPixel, out int endCutPixel);
            if (isVertical)
            {
                galoInspTool.FindLineTool.RunParams.ExpectedLineSegment.GetStartEnd(out double startX, out double startY, out double endX, out double endY);
                if(startX < endX)
                {
                    double direction = galoInspTool.FindLineTool.RunParams.CaliperSearchDirection * (-1);
                    if (direction < 0)
                    {
                        var edgePointList = GetHorizontalEdgePointList(image, startCutPixel, endCutPixel, true);
                        searchedPointList.AddRange(edgePointList);


                    }
                    else
                    {
                        var edgePointList = GetHorizontalEdgePointList(image, startCutPixel, endCutPixel, false);
                        searchedPointList.AddRange(edgePointList);
                    }
                }
                else
                {
                    if (galoInspTool.FindLineTool.RunParams.CaliperSearchDirection < 0)
                    {
                        var edgePointList = GetHorizontalEdgePointList(image, startCutPixel, endCutPixel, true);
                        searchedPointList.AddRange(edgePointList);
                    }
                    else
                    {
                        var edgePointList = GetHorizontalEdgePointList(image, startCutPixel, endCutPixel, false);
                        searchedPointList.AddRange(edgePointList);
                    }
                }
            }
            else
            {
                galoInspTool.FindLineTool.RunParams.ExpectedLineSegment.GetStartEnd(out double startX, out double startY, out double endX, out double endY);

                if (startY > endY)
                {
                    double direction = galoInspTool.FindLineTool.RunParams.CaliperSearchDirection * (-1);

                    if(direction < 0)
                    {
                        var edgePointList = GetVerticalEdgePointList(image, startCutPixel, endCutPixel, true);
                        searchedPointList.AddRange(edgePointList);
                    }
                    else
                    {
                        var edgePointList = GetVerticalEdgePointList(image, startCutPixel, endCutPixel, false);
                        searchedPointList.AddRange(edgePointList);
                    }
                }
                else
                {
                    if (galoInspTool.FindLineTool.RunParams.CaliperSearchDirection < 0)
                    {
                        var edgePointList = GetVerticalEdgePointList(image, startCutPixel, endCutPixel, true);
                        searchedPointList.AddRange(edgePointList);
                    }
                    else
                    {
                        var edgePointList = GetVerticalEdgePointList(image, startCutPixel, endCutPixel, false);
                        searchedPointList.AddRange(edgePointList);
                    }
                }
                
            }

            return searchedPointList;
        }

        private List<Point> GetVerticalEdgePointList(Mat image, int startCutPixel, int endCutPixel, bool isLeftToRight)
        {
            List<Point> searchedPointList = GetVerticalMinEdgeTopPosX(image, startCutPixel, endCutPixel, isLeftToRight);

            if (searchedPointList.Count > 0)
            {
                if(isLeftToRight)
                {//확인OK
                    if (searchedPointList[searchedPointList.Count - 1].Y != image.Height - 1)
                    {
                        if (searchedPointList.Count < 1)
                            searchedPointList.Add(new Point(searchedPointList[0].X, image.Width - 1));
                        else
                            searchedPointList.Add(new Point(searchedPointList[searchedPointList.Count - 1].X, image.Height - 1));
                    }

                    if (searchedPointList[0].Y != 0)
                        searchedPointList.Insert(0, new Point(searchedPointList[0].X, 0));

                    searchedPointList.Add(new Point(0, image.Height - 1));
                    searchedPointList.Add(new Point(0, 0));
                }
                else
                {//확인OK
                    if (searchedPointList[searchedPointList.Count - 1].Y != image.Height - 1)
                    {
                        if (searchedPointList.Count < 1)
                            searchedPointList.Add(new Point(searchedPointList[0].X, image.Width - 1));
                        else
                            searchedPointList.Add(new Point(searchedPointList[searchedPointList.Count - 1].X, image.Height - 1));
                    }

                    if (searchedPointList[0].Y != 0)
                        searchedPointList.Insert(0, new Point(searchedPointList[0].X, 0));

                    searchedPointList.Add(new Point(image.Width - 1, image.Height - 1));
                    searchedPointList.Add(new Point(image.Width - 1, 0));
                }
            }

            return searchedPointList;
        }

        private List<Point> GetHorizontalEdgePointList(Mat image, int startCutPixel, int endCutPixel, bool isTopToBottom)
        {
            List<Point> searchedPointList = GetVerticalMinEdgeTopPosY(image, startCutPixel, endCutPixel, isTopToBottom);
            if (searchedPointList.Count > 0)
            {
                if(isTopToBottom)
                {//확인OK
                    if (searchedPointList[searchedPointList.Count - 1].X != image.Width - 1)
                    {
                        if (searchedPointList.Count < 1)
                            searchedPointList.Add(new Point(image.Width - 1, searchedPointList[0].Y));
                        else
                            searchedPointList.Add(new Point(image.Width - 1, searchedPointList[searchedPointList.Count - 1].Y));
                    }

                    if (searchedPointList[0].X != 0)
                        searchedPointList.Insert(0, new Point(0, searchedPointList[0].Y));

                    searchedPointList.Add(new Point(image.Width - 1, 0));
                    searchedPointList.Add(new Point(0, 0));
                }
                else
                {
                    //확인OK
                    if (searchedPointList[searchedPointList.Count - 1].X != image.Width - 1)
                    {
                        if (searchedPointList.Count < 1)
                            searchedPointList.Add(new Point(image.Width - 1, searchedPointList[0].Y));
                        else
                            searchedPointList.Add(new Point(image.Width - 1, searchedPointList[searchedPointList.Count - 1].Y));
                    }

                    if (searchedPointList[0].X != 0)
                        searchedPointList.Insert(0, new Point(0, searchedPointList[0].Y));

                    searchedPointList.Add(new Point(image.Width - 1, image.Height - 1));
                    searchedPointList.Add(new Point(0, image.Height - 1));
                }
            }
            return searchedPointList;
        }

        private void SetCutPixelValue(DarkAreaInspParam inspParam, bool isInside, out int startCutPixel, out int endCutPixel)
        {
            if(isInside)
            {
                startCutPixel = inspParam.StartCutPixel;
                endCutPixel = inspParam.EndCutPixel;
            }
            else
            {
                startCutPixel = inspParam.OutsideStartCutPixel;
                endCutPixel = inspParam.OutsideEndCutPixel;
            }
        }
        
        public List<Point> GetVerticalMinEdgeTopPosY(Mat mat, int startCutpixel, int endCutPixel, bool isTopToBottom)
        {
            int searchGap = 3;
            int temp = 2;
            unsafe
            {
                List<Point> searchPointList = new List<Point>();

                IntPtr ptrData = mat.DataPointer;
                int stride = mat.Step;
                byte* data = (byte*)(void*)ptrData;

                for (int w = 0; w < mat.Width; w += searchGap)
                {
                    if(isTopToBottom)
                    {
                        for (int h = 0; h < mat.Height; h++)
                        {
                            int index = (stride * h) + w;
                            int value = Convert.ToInt32(data[index]);
                            if (h < startCutpixel)
                                continue;

                            if (h > mat.Height - endCutPixel)
                                continue;
                            if (value == 0)
                            {
                                if(searchPointList.Count != 0)
                                {
                                    var prevPoint = searchPointList.Last();
                                    if(Math.Abs(prevPoint.Y - h) <= temp)
                                    {
                                        searchPointList.Add(new Point(w, h));
                                        break;
                                    }
                                }
                                else
                                {
                                    searchPointList.Add(new Point(w, h));
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int h = mat.Height - 1; h >= 0; h--)
                        {
                            int index = (stride * h) + w;
                            int value = Convert.ToInt32(data[index]);
                            if (mat.Height -1 -h < startCutpixel)
                                continue;

                            if (h < endCutPixel)
                                continue;
                            if (value == 0)
                            {
                                if (searchPointList.Count != 0)
                                {
                                    var prevPoint = searchPointList.Last();
                                    if (Math.Abs(prevPoint.Y - h) <= temp)
                                    {
                                        searchPointList.Add(new Point(w, h));
                                        break;
                                    }
                                }
                                else
                                {
                                    searchPointList.Add(new Point(w, h));
                                    break;
                                }
                            }
                        }
                    }
                }
                return searchPointList;
            }
        }

        public List<Point> GetVerticalMinEdgeTopPosX(Mat mat, int startCutpixel, int endCutPixel, bool isLeftToRight)
        {
            int searchGap = 3;
            int temp = 2;
            unsafe
            {
                List<Point> searchPointList = new List<Point>();

                IntPtr ptrData = mat.DataPointer;
                int stride = mat.Step;
                byte* data = (byte*)(void*)ptrData;

                for (int h = 0; h < mat.Height; h += searchGap)
                {
                    if (isLeftToRight)
                    {
                        for (int w = 0; w < mat.Width; w++)
                        {
                            int index = (stride * h) + w;
                            int value = Convert.ToInt32(data[index]);
                            if (w < startCutpixel)
                                continue;

                            if (w > mat.Width - endCutPixel)
                                continue;
                            if (value == 0)
                            {
                                if (searchPointList.Count != 0)
                                {
                                    var prevPoint = searchPointList.Last();
                                    if (Math.Abs(prevPoint.X - w) <= temp)
                                    {
                                        searchPointList.Add(new Point(w, h));
                                        break;
                                    }
                                }
                                else
                                {
                                    searchPointList.Add(new Point(w, h));
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int w = mat.Width - 1; w >= 0; w--)
                        {
                            int index = (stride * h) + w;
                            int value = Convert.ToInt32(data[index]);

                            if (mat.Width - 1 - w < startCutpixel)
                                continue;

                            if (w < endCutPixel)
                                continue;
                            if (value == 0)
                            {
                                if (searchPointList.Count != 0)
                                {
                                    var prevPoint = searchPointList.Last();
                                    if (Math.Abs(prevPoint.X - w) <= temp)
                                    {
                                        searchPointList.Add(new Point(w, h));
                                        break;
                                    }
                                }
                                else
                                {
                                    searchPointList.Add(new Point(w, h));
                                    break;
                                }
                            }
                        }
                    }
                }
                return searchPointList;
            }
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

        public Mat AddShadow(Mat mat, bool isVertical)
        {
            Mat outputMat = new Mat(mat.Size, DepthType.Cv8U, 1);
            CvInvoke.Filter2D(mat, outputMat, GetEdgeShadowKernel(isVertical), new Point(0, 0));

            return outputMat;
        }

        private ConvolutionKernelF GetEdgeShadowKernel(bool isVertical)
        {
            float[,] matrix;
            
            if (isVertical)
            {
                matrix = new float[3, 3] {
                      { 1, 1, 1 },
                      { 1, 1, -1},
                      { -1, -1, -1 }
                    };
            }
            else
            {
                matrix = new float[3, 3] {
                      { 0, 1, 2 },
                      { -1, 1, 1},
                      { -2, -1, 0 }
                    };
            }
          
            return new ConvolutionKernelF(matrix);
        }
    }

    public enum DarkMaskingDirection
    {
        InSide = 0,
        OutSide = 1,
        Both = 2,
    }
}
