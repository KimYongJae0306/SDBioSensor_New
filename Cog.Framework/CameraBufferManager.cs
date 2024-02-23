using Cog.Framework.Device.Cameras;
using Cog.Framework.Helper;
using Cog.Framework.Settings;
using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cog.Framework
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
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Source + ex.Message + ex.StackTrace);
                    }
                }
            }

        }

     
    }
}
