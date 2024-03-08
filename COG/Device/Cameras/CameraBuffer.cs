using Cognex.VisionPro;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.ToolBlock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace COG.Device.Cameras
{
    public partial class CameraBuffer
    {
        private object _lock = new object();

        public bool IsGrabbing { get; private set; } = false;

        private GrabSeq _grabSeq { get; set; } = GrabSeq.None;

        private bool _isLiveMode { get; set; } = false;

        private bool EnableGrab { get; set; } = false;

        private GrabStatus GrabStatus { get; set; } = GrabStatus.Stop;

        private Task _grabTask { get; set; } = null;

        private CancellationTokenSource _cancelGrabTask { get; set; }
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

        //public string CoordinateSpaceName { get; set; } = ""; //DisplayStatusBar 에서 마우스 좌표 표시 할때 이미지의 회전에 따라서 제대루된 좌표값 뿌릴때. 사용함 "*\\#\\"기본 ,"*\\#\\@"-> Y 반전시
        public void Initialize()
        {
            _cancelGrabTask = new CancellationTokenSource();
            _grabTask = new Task(GrabThread, _cancelGrabTask.Token);
            _grabTask.Start();
        }

        public void StopGrab()
        {
            EnableGrab = false;
            _isLiveMode = false;
        }

        public void GrabLive()
        {
            IsGrabbing = true;

            EnableGrab = true;
            _isLiveMode = true;
        }

        public void GrabOnce()
        {
            IsGrabbing = true;

            EnableGrab = true;
            _isLiveMode = false;

            while (IsGrabbing)
                Thread.Sleep(10);
        }

        private void GrabThread()
        {
            while (true)
            {
                if (_cancelGrabTask.IsCancellationRequested)
                    break;

                if (EnableGrab)
                {
                    ImageGrab();
                }
                    
                Thread.Sleep(50);
            }
        }

        public void ImageGrab()
        {
            GrabSeq seq = GrabSeq.None;
            bool LoopFlag = true;

            while (LoopFlag)
            {
                if (EnableGrab == false && seq == GrabSeq.GrabDone)
                {
                    ((CogAcqFifoTool)CogImageBlock.Tools["CogAcqFifoTool1"]).Operator.Flush();
                    LoopFlag = false;
                    IsGrabbing = false;
                    break;
                }

                switch (seq)
                {
                    case GrabSeq.SendCommand:
                        CogImageBlock.Run();
                        
                        seq = GrabSeq.WaitImage;
                        break;
                    case GrabSeq.WaitImage:
                        if (CogImageBlock.RunStatus.Result == CogToolResultConstants.Accept)
                        {
                            CogCamBuf = CogImageBlock.Outputs[0].Value as CogImage8Grey;
                            seq = GrabSeq.GrabDone;
                        }
                        break;

                    case GrabSeq.GrabDone:

                        ((CogAcqFifoTool)CogImageBlock.Tools["CogAcqFifoTool1"]).Operator.Flush();
                        seq = GrabSeq.Complete;
                        break;

                    case GrabSeq.Complete:

                        if(_isLiveMode)
                            seq = GrabSeq.SendCommand;
                        else
                            LoopFlag = false;
                        break;
                }
                Thread.Sleep(1);
            }
        }
    }

    public enum GrabStatus
    {
        Stop,
        OnceGrab,
        Live,
    }

    public enum GrabSeq
    {
        None,
        SendCommand,
        WaitImage,
        GrabDone,
        Complete,
    }

}
