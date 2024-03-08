using COG.Device.Cameras;
using COG.Settings;
using Cognex.VisionPro;
using Cognex.VisionPro.CalibFix;
using Cognex.VisionPro.Caliper;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.ToolBlock;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using System.Drawing;

namespace COG.Helper
{
    public static class VisionProHelper
    {
        public static ICogImage CropImage(ICogImage sourceImage, CogRectangle rect)
        {
            CogCopyRegionTool regionTool = new CogCopyRegionTool();
            regionTool.InputImage = sourceImage;
            regionTool.Region = rect;
            regionTool.Run();
            return regionTool.OutputImage;
        }

        public static ICogImage CropImage(ICogImage sourceImage, ICogRegion region, int definePixelValue = 0)
        {
            CogCopyRegionTool regionTool = new CogCopyRegionTool();
            regionTool.InputImage = sourceImage;
            regionTool.Region = region;
            regionTool.RunParams.FillBoundingBoxValue = definePixelValue;
            regionTool.Run();
            return regionTool.OutputImage;
        }

        public static CogRectangleAffine GetBoundingRect(CogImage8Grey cogImage, CogFindCircleTool cogFindCircleTool)
        {
            CogRectangleAffine boundingBox = new CogRectangleAffine();

            //boundingBox.CenterX = cogFindLineTool.RunParams.ExpectedLineSegment.MidpointX;
            //boundingBox.CenterY = cogFindLineTool.RunParams.ExpectedLineSegment.MidpointY;
            //boundingBox.Rotation = cogFindLineTool.RunParams.ExpectedLineSegment.Rotation;// + CogMisc.DegToRad(90);
            //boundingBox.SideXLength = cogFindLineTool.RunParams.ExpectedLineSegment.Length;
            //boundingBox.SideYLength = cogFindLineTool.RunParams.CaliperSearchLength;

            return boundingBox;
        }

        public static CogRectangleAffine GetBoundingRect(CogImage8Grey cogImage, CogFindLineTool cogFindLineTool)
        {
            CogRectangleAffine boundingBox = new CogRectangleAffine();

            boundingBox.CenterX = cogFindLineTool.RunParams.ExpectedLineSegment.MidpointX;
            boundingBox.CenterY = cogFindLineTool.RunParams.ExpectedLineSegment.MidpointY;
            boundingBox.Rotation = cogFindLineTool.RunParams.ExpectedLineSegment.Rotation;// + CogMisc.DegToRad(90);
            boundingBox.SideXLength = cogFindLineTool.RunParams.ExpectedLineSegment.Length;
            boundingBox.SideYLength = cogFindLineTool.RunParams.CaliperSearchLength;

            return boundingBox;
        }

        public static CogImage8Grey CovertGreyImage(IntPtr ptr, int width, int height, int stride)
        {
            CogImage8Root root = new CogImage8Root();
            root.Initialize(width, height, ptr, stride, null);
            var cogImage = new CogImage8Grey();
            cogImage.SetRoot(root);

            return cogImage;
        }

        public static ICogImage CogCopyRegionTool(ICogImage destImage, ICogImage inputImage, CogRectangleAffine affineRect, bool alignmentEnabled)
        {
            inputImage.PixelFromRootTransform = destImage.PixelFromRootTransform;

            CogCopyRegionTool regionTool = new CogCopyRegionTool();

            CogRectangle Region = new CogRectangle();
            Region.X = 0;
            Region.Y = 0;
            Region.Width = inputImage.Width;
            Region.Height = inputImage.Height;

            CogRectangle destRect = ConvertAffineRectToRect(affineRect);
            regionTool.DestinationImage = destImage;
            
            regionTool.InputImage = inputImage;
            regionTool.Region = Region;
            regionTool.Region.SelectedSpaceName = destImage.SelectedSpaceName;
            regionTool.RunParams.ImageAlignmentEnabled = alignmentEnabled;
            regionTool.RunParams.DestinationImageAlignmentX = destRect.X;
            regionTool.RunParams.DestinationImageAlignmentY = destRect.Y;
            regionTool.RunParams.FillBoundingBoxValue = 255;

            regionTool.Run();
            ICogImage cogImage = regionTool.OutputImage;//.CopyBase(CogImageCopyModeConstants.CopyPixels);

            return cogImage;
        }

        public static CogRectangle ConvertAffineRectToRect(CogRectangleAffine affineRect)
        {
            List<double> xPointList = new List<double>();

            xPointList.Add(affineRect.CornerOriginX);
            xPointList.Add(affineRect.CornerXX);
            xPointList.Add(affineRect.CornerYX);
            xPointList.Add(affineRect.CornerOppositeX);

            List<double> yPointList = new List<double>();
            yPointList.Add(affineRect.CornerOriginY);
            yPointList.Add(affineRect.CornerXY);
            yPointList.Add(affineRect.CornerYY);
            yPointList.Add(affineRect.CornerOppositeY);

            double minimumX = xPointList.Min();
            double minimumY = yPointList.Min();
            double maximumX = xPointList.Max();
            double maximumY = yPointList.Max();

            double width = maximumX - minimumX;
            double height = maximumY - minimumY;

            CogRectangle rect = new CogRectangle();
            rect.X = (int)minimumX;
            rect.Y = (int)minimumY;
            rect.Width = (int)width;
            rect.Height = (int)height;

            return rect;
        }

        public static void GetImageFile(CogImageFileTool ImageFileTool, String FileName)
        {
            try
            {
                ImageFileTool.Operator.Open(FileName, CogImageFileModeConstants.Read);
                ImageFileTool.Run();
            }
            catch (System.Exception ex)
            {
                ImageFileTool.Operator.Open("D:\\SystemData\\QDIDB.idb", CogImageFileModeConstants.Read);
                ImageFileTool.Run();
            }
        }

        public static CogRectangle CreateRectangle(double centerX, double centerY, double width, double height, bool interactive = true, CogRectangleDOFConstants constants = CogRectangleDOFConstants.All)
        {
            CogRectangle roi = new CogRectangle();

            roi.SetCenterWidthHeight(centerX, centerY, width, height);
            roi.Interactive = interactive;
            roi.GraphicDOFEnable = constants;

            return roi;
        }

        public static void GetCamSetValue(ref CameraBuffer cameraBuffer)
        {
            cameraBuffer.CogImageBlock = new CogToolBlock();

            string filename = StaticConfig.SYS_DATADIR + StaticConfig.CAM_SETDIR + "CCD_" + cameraBuffer.Index + ".vpp";
            try
            {
                if (File.Exists(filename))
                {
                    cameraBuffer.CogImageBlock = CogSerializer.LoadObjectFromFile(filename) as CogToolBlock;
                }
                else
                {
                    #region Camera VPP initialize
                    CogFrameGrabbers frameGrabberALL = new CogFrameGrabbers();

                    CogAcqFifoTool temptool = new CogAcqFifoTool();
                    CogIPOneImageTool temptool1 = new CogIPOneImageTool();
                    CogFixtureTool temptool2 = new CogFixtureTool();

                    temptool.Name = "CogAcqFifoTool1";
                    temptool1.Name = "CogIPOneImageTool1";
                    temptool2.Name = "CogFixtureTool1";

                    int idx = cameraBuffer.Index;

                    temptool.Operator = frameGrabberALL[idx].CreateAcqFifo("Generic GigEVision (Mono)", CogAcqFifoPixelFormatConstants.Format8Grey, 0, true);

                    cameraBuffer.CogImageBlock.Tools.Add(temptool);
                    cameraBuffer.CogImageBlock.Tools.Add(temptool1);
                    cameraBuffer.CogImageBlock.Tools.Add(temptool2);

                    cameraBuffer.CogImageBlock.Tools["CogIPOneImageTool1"].DataBindings.Add("InputImage",
                    cameraBuffer.CogImageBlock.Tools["CogAcqFifoTool1"],
                    "OutputImage");

                    cameraBuffer.CogImageBlock.Tools["CogFixtureTool1"].DataBindings.Add("InputImage",
                    cameraBuffer.CogImageBlock.Tools["CogIPOneImageTool1"],
                    "OutputImage");

                    Cognex.VisionPro.CogImage8Grey nOutPutImage = new CogImage8Grey();
                    CogToolBlockTerminal nOutPut = new CogToolBlockTerminal("OutputImage", nOutPutImage);
                    cameraBuffer.CogImageBlock.Outputs.Add(nOutPut);

                    string desPath = "";
                    desPath = DataBindingPath(cameraBuffer.CogImageBlock.Outputs["OutputImage"]);

                    cameraBuffer.CogImageBlock.Outputs.DataBindings.Add(desPath, cameraBuffer.CogImageBlock.Tools["CogFixtureTool1"], "OutputImage");
                    #endregion

                    #region 카메라 초기 셋팅
                    int nX, nY, nW, nH;
                    double Exposure, Brightness, Contrast;

                    CogAcqFifoTool nfifotool = new CogAcqFifoTool();
                    nfifotool = cameraBuffer.CogImageBlock.Tools[0] as CogAcqFifoTool;
                    nfifotool.Operator.OwnedROIParams.GetROIXYWidthHeight(out nX, out nY, out nW, out nH);
                    if (nW == 2592)  // 가압
                    {
                        Exposure = 120;
                        Brightness = 0.1;
                        Contrast = 0.4;
                    }
                    else
                    {
                        Exposure = 60;
                        Brightness = 0.1;
                        Contrast = 0.4;
                    }
                    try { nfifotool.Operator.OwnedExposureParams.Exposure = Exposure; } catch { }
                    try { nfifotool.Operator.OwnedBrightnessParams.Brightness = Brightness; } catch { }
                    try { nfifotool.Operator.OwnedContrastParams.Contrast = Contrast; } catch { }

                    #endregion

                    try
                    {
                        CogSerializer.SaveObjectToFile(cameraBuffer.CogImageBlock, filename);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show(ex.Source + ex.Message + ex.StackTrace);
                    }
                }
                cameraBuffer.CogImageBlock.Name = cameraBuffer.Index.ToString();
                //cameraBuffer.CogImageBlock.Ran += Main_Ran;
                cameraBuffer.CogImageBlock.Run();
                cameraBuffer.CogCamBuf = cameraBuffer.CogImageBlock.Outputs[0].Value as ICogImage;

                #region CAMERA NAME
                string CamToHostIP, CurLocalIP;

                CogAcqFifoTool fifotool = new CogAcqFifoTool();
                fifotool = cameraBuffer.CogImageBlock.Tools[0] as CogAcqFifoTool;

                NetworkInterface[] network = NetworkInterface.GetAllNetworkInterfaces();
                CamToHostIP = fifotool.Operator.FrameGrabber.OwnedGigEAccess.HostIPAddress;

                for (int i = 0; i < network.Length; i++)
                {
                    if (network[i].NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    {
                        for (int j = 0; j < network[i].GetIPProperties().UnicastAddresses.Count; j++)
                        {
                            CurLocalIP = network[i].GetIPProperties().UnicastAddresses[j].Address.ToString();
                            if (CurLocalIP == CamToHostIP)
                            {
                                cameraBuffer.CamName = network[i].Name;
                            }
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + ex.Message + ex.StackTrace);

            }
        }

        public static string DataBindingPath(Cognex.VisionPro.ToolBlock.CogToolBlockTerminal Terminal)
        {
            CogToolBlockTerminal inputTerminal;
            string sourcePath;

            try
            {
                inputTerminal = Terminal;
                sourcePath = "Item[\"" + inputTerminal.ID + "\"].Value.(" + inputTerminal.ValueType.FullName + ")";
                sourcePath = Cognex.VisionPro.Implementation.Internal.CogToolTerminals.RemoveExtraAssemblyInfoFromPath(sourcePath);
            }
            catch (System.Exception ex)
            {
                return sourcePath = null;
            }
            return sourcePath;
        }

        public static void Save(ICogImage image, string fileName)
        {
            try
            {
                if (image == null)
                    return;

                lock (image)
                {
                    string extension = Path.GetExtension(fileName);
                    if (extension == ".bmp")
                    {
                        CogImageFileBMP bmp = new CogImageFileBMP();
                        bmp.Open(fileName, CogImageFileModeConstants.Write);
                        bmp.Append(image);
                        bmp.Close();
                    }
                    else if (extension == ".jpg" || extension == ".jpeg")
                    {
                        CogImageFileJPEG jpg = new CogImageFileJPEG();
                        jpg.Open(fileName, CogImageFileModeConstants.Write);
                        jpg.Append(image);
                        jpg.Close();
                    }
                    else if (extension == ".png")
                    {
                        CogImageFilePNG png = new CogImageFilePNG();
                        png.Open(fileName, CogImageFileModeConstants.Write);
                        png.Append(image);
                        png.Close();
                    }
                }
            }
            catch (Exception)
            {
            }
           
        }

        public static CogPolygon GetBoundingPolygon(CogImage8Grey cogImage, CogFindCircleTool cogFindCircleTool)
        {
            CogPolygon boundingPolygon = new CogPolygon();

            var pointList = GetPolygonPoints(cogFindCircleTool);

            for (int pointIndex = 0; pointIndex < pointList.Count; pointIndex++)
                boundingPolygon.AddVertex(pointList[pointIndex].X, pointList[pointIndex].Y, pointIndex);

            return boundingPolygon;
        }

        private static List<PointF> GetPolygonPoints(CogFindCircleTool cogFindCircleTool)
        {
            List<PointF> points = new List<PointF>();

            double centerX = cogFindCircleTool.RunParams.ExpectedCircularArc.CenterX;
            double centerY = cogFindCircleTool.RunParams.ExpectedCircularArc.CenterY;
            double radius = cogFindCircleTool.RunParams.ExpectedCircularArc.Radius;
            double caliperLength = cogFindCircleTool.RunParams.CaliperSearchLength / 2;
            double angleStartTheta = cogFindCircleTool.RunParams.ExpectedCircularArc.AngleStart;
            double angleSpan = cogFindCircleTool.RunParams.ExpectedCircularArc.AngleSpan;

            int caliperCount = cogFindCircleTool.RunParams.NumCalipers + 2;     // 두개 더
            double perRadian = angleSpan / caliperCount;

            // 상단
            for (int index = 0; index < caliperCount + 1; index++)      // caliperCount + 1 : Caliper 끝 단 영역 확보를 위해 +1
            {
                double upperX = centerX + (radius + caliperLength) * Math.Cos(angleStartTheta + perRadian * index);
                double upperY = centerY + (radius + caliperLength) * Math.Sin(angleStartTheta + perRadian * index);

                points.Add(new PointF(Convert.ToSingle(upperX), Convert.ToSingle(upperY)));
            }

            // 하단
            List<PointF> temp = new List<PointF>();
            for (int index = 0; index < caliperCount + 1; index++)
            {
                double lowerX = centerX + (radius - caliperLength) * Math.Cos(angleStartTheta + perRadian * index);
                double lowerY = centerY + (radius - caliperLength) * Math.Sin(angleStartTheta + perRadian * index);

                temp.Add(new PointF(Convert.ToSingle(lowerX), Convert.ToSingle(lowerY)));
            }

            // 하단 뒤집어서
            temp.Reverse();

            // Add
            points.AddRange(temp);

            return points;
        }

        public static Point GetCropLeftTop(CogRectangleAffine affineRect)
        {
            List<double> xPointList = new List<double>();

            xPointList.Add(affineRect.CornerOriginX);
            xPointList.Add(affineRect.CornerXX);
            xPointList.Add(affineRect.CornerYX);
            xPointList.Add(affineRect.CornerOppositeX);

            List<double> yPointList = new List<double>();
            yPointList.Add(affineRect.CornerOriginY);
            yPointList.Add(affineRect.CornerXY);
            yPointList.Add(affineRect.CornerYY);
            yPointList.Add(affineRect.CornerOppositeY);

            double minimumX = xPointList.Min();
            double minimumY = yPointList.Min();

            return new Point((int)minimumX, (int)minimumY);
        }

        public static Point GetCropLeftTop(CogPolygon polygon)
        {
            var vertices = polygon.GetVertices();
            if (vertices == null)
                return new Point();

            List<double> xPointList = new List<double>();
            List<double> yPointList = new List<double>();

            int count = vertices.Length / 2;

            for (int i = 0; i < count; i++)
            {
                xPointList.Add(vertices[i, 0]);
                yPointList.Add(vertices[i, 1]);
            }
            double minimumX = xPointList.Min();
            double minimumY = yPointList.Min();

            return new Point((int)minimumX, (int)minimumY);
        }

        public static void DisposeDisplay(CogRecordDisplay display)
        {
            if (display.Image is CogImage8Grey grayImage)
            {
                grayImage.Dispose();
                grayImage = null;
            }
            if (display.Image is CogImage24PlanarColor colorImage)
            {
                colorImage.Dispose();
                colorImage = null;
            }
        }
    }
}
