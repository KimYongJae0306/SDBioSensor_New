using COG.Class.Data;
using COG.Class.Units;
using COG.Core;
using COG.Device.PLC;
using COG.Helper;
using COG.Settings;
using Cognex.VisionPro;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace COG.Class.Core.AppTask
{
    public class InspProcessTask
    {
        private AlgorithmTool AlgorithmTool { get; set; } = new AlgorithmTool();

        public int StageNo { get; set; } = -1;

        public bool IsLeft { get; set; } = false;

        private Task InspTask { get; set; }

        private CancellationTokenSource InspTaskCancellationTokenSource { get; set; }

        public void Initalize(int stageNo)
        {
            StageNo = stageNo;
            StartInspTask();
        }

        private void StartInspTask()
        {
            if (InspTask != null)
                return;

            InspTaskCancellationTokenSource = new CancellationTokenSource();
            InspTask = new Task(RunInspTask, InspTaskCancellationTokenSource.Token);
            InspTask.Start();
        }

        private void RunInspTask()
        {
            while (true)
            {
                if (InspTaskCancellationTokenSource.IsCancellationRequested)
                    break;

                InspModel inspModel = ModelManager.Instance().CurrentModel;

                if (AppsStatus.Instance().MC_STATUS == MC_STATUS.RUN && StageNo >=0 && inspModel != null)
                {
                    var readData = PlcControlManager.Instance().GetReadData();
                    var stageAddress = AppsStatus.Instance().StageAddress[StageNo];

                    int cmd = readData[stageAddress + PlcAddressMap.PLC_Command];
                    int status = readData[stageAddress + PlcAddressMap.PC_Status];

                    if (cmd != 0 && status == 0 && cmd != 9000)
                    {
                        CheckCmdInspFormat(cmd, out int calcStageNo, out int calcCmd);

                        if((PlcCommand)cmd == PlcCommand.StartInspection && calcStageNo == StageNo)
                            StartInspection();
                    }
                }

                Thread.Sleep(50);
            }
        }

        private void StartInspection()
        {
            Stopwatch sw = new Stopwatch();

            SystemManager.Instance().AddLogDisplay(StageNo, $"<-WELDING_INSPECTION_START", true);
            SystemManager.Instance().AddLogDisplay(StageNo, $"===== INSPECTION {StageNo + 1} =====", true);

            if (Inspection(out InspResult inspResult))
            {
               //OK
            }
            else
            {
                //NG
            }
        }

        private bool Inspection(out InspResult inspResult)
        {
            InspModel inspModel = ModelManager.Instance().CurrentModel;

            inspResult = new InspResult();
            inspResult.StageNo = StageNo;
            inspResult.IsLeft = IsLeft;

            //Todo 조명 켜야함
            int camNo = IsLeft ? 0 : 1;
            var cameraBuffer = CameraBufferManager.Instance().GetCameraBuffer(camNo);
            cameraBuffer.GrabOnce();
            SystemManager.Instance().AddLogDisplay(StageNo, "Image Grab Complete", true);

            CogImage8Grey cogImage = cameraBuffer.CogCamBuf as CogImage8Grey;

            //Todo
            Unit unit = null;
            if(IsLeft)
                unit = inspModel.StageUnitList[StageNo].Left;
            else
                unit = inspModel.StageUnitList[StageNo].Right;

            if (InspectMark(cogImage, unit, ref inspResult) == false)
            {
                inspResult.Judgement = Judgement.NG;
                return false;
            }

            if(RunFileAlign(cogImage, unit, ref inspResult) == false)
            {
                inspResult.Judgement = Judgement.NG;
                return false;
            }

            if(RunGaloInspection(cogImage, unit, ref inspResult) == false)
            {
                inspResult.Judgement = Judgement.NG;
                return false;
            }

            inspResult.Judgement = Judgement.OK;
            return true;
        }

        private bool RunFileAlign(CogImage8Grey cogImage, Unit unit, ref InspResult inspResult)
        {
            bool isGood = true;

            AmpROITracking(unit, inspResult.AmpMarkResult, true);

            try
            {
                inspResult.AmpFilmAlignResult = AlgorithmTool.RunAmpFlimAlign(cogImage, unit.FilmAlign);
                var filmAlignResultDistaneX = inspResult.AmpFilmAlignResult.GetDistanceX_mm();
                var ampModuleDistanceX = unit.FilmAlign.AmpModuleDistanceX;
                var filmAlignSpecX = unit.FilmAlign.FilmAlignSpecX;

                if (inspResult.AmpFilmAlignResult.Judgement != Judgement.OK)
                {
                    SystemManager.Instance().AddLogDisplay(StageNo,
                        $"Film Align NG, X : {filmAlignResultDistaneX}(mm) / Spec : {ampModuleDistanceX} + {filmAlignSpecX}", true);

                    inspResult.Judgement = Judgement.NG;
                    return false;
                }
                else
                {
                    SystemManager.Instance().AddLogDisplay(StageNo,
                      $"Film Align OK, X : {filmAlignResultDistaneX}(mm) / Spec : {ampModuleDistanceX} + {filmAlignSpecX}", true);
                }
                isGood = true;
            }
            catch (Exception err)
            {
                isGood = false;
                LoggerHelper.Save_Command($"RunFileAlign Error :{err.Message}", LogType.Error, 0);
            }
            finally
            {
                AmpROITracking(unit, inspResult.AmpMarkResult, false);
            }
            return true;
        }

        private bool RunGaloInspection(CogImage8Grey cogImage, Unit unit, ref InspResult inspResult)
        {
            bool isGood = true;
            BondingROITracking(unit, inspResult.BondingMarkResult.UpMarkResult, inspResult.BondingMarkResult.DownMarkResult, true);

            try
            {
                CogImage8Grey binaryImage = cogImage.CopyBase(CogImageCopyModeConstants.CopyPixels) as CogImage8Grey;

                for (int i = 0; i < unit.Insp.GaloInspToolList.Count; i++)
                {
                    var inspTool = unit.Insp.GaloInspToolList[i];

                    if (inspTool.Type == GaloInspType.Line)
                    {
                        CogRectangleAffine rect = new CogRectangleAffine();

                        var lineResult = AlgorithmTool.RunGaloLineInspection(cogImage, binaryImage, inspTool, ref rect, false);

                        inspResult.GaloResult.LineResult.Add(lineResult);

                        if (lineResult.Judgement != Judgement.OK)
                            isGood = false;
                    }
                    else
                    {
                        var circleInspResult = AlgorithmTool.RunGaloCircleInspection(cogImage, inspTool, false);
                        inspResult.GaloResult.CircleResult.Add(circleInspResult);

                        if (circleInspResult.Judgement != Judgement.OK)
                            isGood = false;
                    }
                }
            }
            catch (Exception err)
            {
                isGood = false;
                LoggerHelper.Save_Command($"RunGaloInspection Error :{err.Message}", LogType.Error, 0);
            }
            finally
            {
                BondingROITracking(unit, inspResult.BondingMarkResult.UpMarkResult, inspResult.BondingMarkResult.DownMarkResult, false);
            }

            return isGood;
        }

        private bool InspectMark(CogImage8Grey cogImage, Unit unit, ref InspResult inspResult)
        {
            MarkResult ampMarkResult = AlgorithmTool.FindMark(cogImage, unit.Mark.Amp.MarkToolList, unit.Mark.Amp.Score);
            inspResult.AmpMarkResult = ampMarkResult;

            var upMarkToolList = unit.Mark.Bonding.UpMarkToolList;
            var downMarkToolList = unit.Mark.Bonding.DownMarkToolList;
            var bondingScore = unit.Mark.Bonding.Score;
            var bondingSpec_T = unit.Mark.Bonding.AlignSpec_T;
            BondingMarkResult bondingMarkResult = AlgorithmTool.FindBondingMark(cogImage, upMarkToolList, downMarkToolList, bondingScore, bondingSpec_T);
            inspResult.BondingMarkResult = bondingMarkResult;

            SystemManager.Instance().AddLogDisplay(StageNo, "Mark Search Complete", true);
            if(ampMarkResult == null)
            {
                SystemManager.Instance().AddLogDisplay(StageNo, "Mark Search NG", true);
                return false;
            }
            else if(bondingMarkResult.Judgement == Judgement.FAIL)
            {
                SystemManager.Instance().AddLogDisplay(StageNo, "Mark Search NG", true);
                return false;
            }
            else if(bondingMarkResult.Judgement == Judgement.NG)
            {
                SystemManager.Instance().AddLogDisplay(StageNo, "Mark Search OK", true);
                string message = string.Format("Theta Align NG : {0:F2}(°) / Spec : ±{1:F2}(°)", bondingMarkResult.FoundDegree, bondingSpec_T);
                SystemManager.Instance().AddLogDisplay(StageNo, message, true);
                return false;
            }
            else if(bondingMarkResult.Judgement == Judgement.OK)
            {
                SystemManager.Instance().AddLogDisplay(StageNo, "Mark Search OK", true);
                string message = string.Format("Theta Align OK : {0:F2}(°) / Spec : ±{1:F2}(°)", bondingMarkResult.FoundDegree, bondingSpec_T);
                SystemManager.Instance().AddLogDisplay(StageNo, message, true);
            }
            return true;
        }

        private void CheckCmdInspFormat(int cmd, out int stageNo, out int calcCmd)
        {
            calcCmd = Convert.ToInt16("1" + cmd.ToString().Substring(1, cmd.ToString().Length - 1));
            stageNo = (int)(cmd / 1000) - 1;
            if (stageNo >= StaticConfig.STAGE_COUNT)
                calcCmd = 0;
        }

        private void AmpROITracking(Unit unit, MarkResult markResult, bool isTracking)
        {
            if (isTracking)
            {
                var coordinate = TeachingData.Instance().AmpCoordinate;
                coordinate.SetReferenceData(markResult.ReferencePos);
                coordinate.SetTargetData(markResult.FoundPos);
                coordinate.ExecuteCoordinate(unit);
            }
            else
            {
                var coordinate = TeachingData.Instance().AmpCoordinate;
                coordinate.SetReferenceData(markResult.FoundPos);
                coordinate.SetTargetData(markResult.ReferencePos);
                coordinate.ExecuteCoordinate(unit);
            }
        }

        private void BondingROITracking(Unit unit, MarkResult upMarkResult, MarkResult downMarkResult, bool isTracking)
        {
            if (isTracking)
            {
                var coordinate = TeachingData.Instance().BondingCoordinate;
                coordinate.SetReferenceData(upMarkResult.ReferencePos, downMarkResult.ReferencePos);
                coordinate.SetTargetData(upMarkResult.FoundPos, downMarkResult.FoundPos);
                coordinate.ExecuteCoordinate();
            }
            else
            {
                var coordinate = TeachingData.Instance().BondingCoordinate;
                coordinate.SetReferenceData(upMarkResult.FoundPos, downMarkResult.FoundPos);
                coordinate.SetTargetData(upMarkResult.ReferencePos, downMarkResult.ReferencePos);
                coordinate.ExecuteCoordinate();
            }
        }
    }
}
