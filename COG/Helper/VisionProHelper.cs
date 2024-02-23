using COG.Device.Cameras;
using COG.Settings;
using Cognex.VisionPro;
using Cognex.VisionPro.CalibFix;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.ToolBlock;
using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace COG.Helper
{
    public static class VisionProHelper
    {
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
    }
}
