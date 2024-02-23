using Cognex.VisionPro;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.ToolBlock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Device.Cameras
{
    public partial class CameraBuffer
    {
        public int Index { get; set; }
        public string CamName { get; set; }

        public ICogImage CogCamBuf { get; set; }            //카메라랑 연결 시킬 버퍼. 
        public CogImageFileTool CogImgTool { get; set; }    //Test할때 사용 이미지 가져오기위한.
        public CogToolBlock CogImageBlock { get; set; }     //카메라 관련 

        public int IMAGE_SIZE_X { get; set; } = 0;
        public int IMAGE_SIZE_Y { get; set; } = 0;
        public int IMAGE_CENTER_X { get; set; } = 0;
        public int IMAGE_CENTER_Y { get; set; } = 0;
        public int CUSTOM_CROSS_X { get; set; } = 0;
        public int CUSTOM_CROSS_Y { get; set; } = 0;
        public bool USE_CUSTOM_CROSS { get; set; } = false;

        public bool GrabRefresh_LiveView { get; set; } = false;
        public bool Grab_Flag_Start { get; set; } = false;
        public bool Grab_Flag_End { get; set; } = false;

        public string CoordinateSpaceName { get; set; } = ""; //DisplayStatusBar 에서 마우스 좌표 표시 할때 이미지의 회전에 따라서 제대루된 좌표값 뿌릴때. 사용함 "*\\#\\"기본 ,"*\\#\\@"-> Y 반전시
    }
}
