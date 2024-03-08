using COG.Device.Cameras;
using COG.Helper;
using COG.Settings;
using COG.UI.Forms;
using Cognex.VisionPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace COG
{
    public class CameraBufferManager
    {
        private static CameraBufferManager _instance = null;

        private List<CameraBuffer> CameraBufferList { get; set; } = new List<CameraBuffer>();

        public static CameraBufferManager Instance()
        {
            if (_instance == null)
            {
                _instance = new CameraBufferManager();
            }

            return _instance;
        }

        public void Initialize()
        {
            var camCount = StaticConfig.CAM_COUNT;

            ProgressBarForm form = new ProgressBarForm();
            form.Maximum = camCount;
            form.Message = "CAMERA";
            form.Show();

            for (int i = 0; i < camCount; i++)
            {
                if(StaticConfig.VirtualMode)
                {
                    CameraBuffer buffer = new CameraBuffer();

                    buffer.CogImgTool = new Cognex.VisionPro.ImageFile.CogImageFileTool();

                    VisionProHelper.GetImageFile(buffer.CogImgTool, StaticConfig.IMAGE_FILE);

                    buffer.Index = i;
                    buffer.IMAGE_SIZE_X = buffer.CogImgTool.OutputImage.Width;
                    buffer.IMAGE_SIZE_Y = buffer.CogImgTool.OutputImage.Height;
                    buffer.IMAGE_CENTER_X = buffer.IMAGE_SIZE_X / 2;
                    buffer.IMAGE_CENTER_Y = buffer.IMAGE_SIZE_Y / 2;
                    buffer.USE_CUSTOM_CROSS = false;
                    buffer.CogCamBuf = buffer.CogImgTool.OutputImage as ICogImage;
                }
                else
                {
                    try
                    {
                        CameraBuffer buffer = new CameraBuffer();
                        buffer.Index = i;

                        VisionProHelper.GetCamSetValue(ref buffer);
                        buffer.IMAGE_SIZE_X = (buffer.CogImageBlock.Outputs[0].Value as ICogImage).Width;
                        buffer.IMAGE_SIZE_Y = (buffer.CogImageBlock.Outputs[0].Value as ICogImage).Height;
                        buffer.IMAGE_CENTER_X = buffer.IMAGE_SIZE_X / 2;
                        buffer.IMAGE_CENTER_Y = buffer.IMAGE_SIZE_Y / 2;
                        buffer.USE_CUSTOM_CROSS = false;

                        CameraBufferList.Add(buffer);

                        buffer.Initialize();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Source + ex.Message + ex.StackTrace);
                    }
                }
            }

            form.Dispose();
        }

        public void Release()
        {
            if (StaticConfig.VirtualMode == false)
            {
                try
                {
                    CogFrameGrabbers frameGrabbers = new CogFrameGrabbers();
                    foreach (ICogFrameGrabber fg in frameGrabbers)
                        fg.Disconnect(false);

                    foreach (var buffer in CameraBufferList)
                        buffer.CogImageBlock?.Dispose();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Source + ex.Message + ex.StackTrace);
                }
            }
        }

        public void ImageGrab(int camNo)
        {
            var buffer = GetCameraBuffer(camNo);

            if(buffer != null)
            {
                buffer.CogImageBlock.Run();
            }
        }

        public CameraBuffer GetCameraBuffer(int camNo)
        {
            return CameraBufferList.Where(x => x.Index == camNo).FirstOrDefault();
        }
    }
}
