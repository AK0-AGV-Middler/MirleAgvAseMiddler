using Mirle.Agv.Controller.Tools;
using Mirle.Agv.Model;
using Mirle.Agv.Model.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mirle.Tools;

namespace Mirle.Agv.Controller
{
    public class AgvMoveRevise
    {
        private ReviseParameter reviseParameter;
        private OntimeReviseConfig ontimeReviseConfig = null;
        private ElmoDriver elmoDriver = null;
        private List<Sr2000Driver> DriverSr2000List = null;
        private OneTimeReviseParameter oneTimeReviseParameter = new OneTimeReviseParameter();
        Dictionary<EnumMoveControlSafetyType, SafetyData> safety;
        private ComputeFunction computeFunction = new ComputeFunction();
        private MirleLogger mirleLogger = MirleLogger.Instance;
        private string device = "AgvMoveRevise";
        private uint lastCount = 0;
        private int lastSR2000Index = -1;

        public AgvMoveRevise(OntimeReviseConfig ontimeReviseConfig, ElmoDriver elmoDriver, List<Sr2000Driver> DriverSr2000List)
        {
            this.ontimeReviseConfig = ontimeReviseConfig;
            this.elmoDriver = elmoDriver;
            this.DriverSr2000List = DriverSr2000List;
            SettingReviseData(100, true);
        }

        public void SettingReviseData(double velocity, bool dirFlag)
        {
            reviseParameter = new ReviseParameter(ontimeReviseConfig, velocity, dirFlag);
        }

        private void WriteLog(string category, string logLevel, string device, string carrierId, string message,
                             [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            string classMethodName = GetType().Name + ":" + memberName;
            LogFormat logFormat = new LogFormat(category, logLevel, classMethodName, device, carrierId, message);

            mirleLogger.Log( logFormat);
        }
        
        #region 即時修正
        private bool LineRevise(ref double[] wheelTheta, double theta, double sectionDeviation, bool isOldCompute = true)
        {
            if ((reviseParameter.ReviseType == EnumLineReviseType.Theta || theta > reviseParameter.ModifyTheta || theta < -reviseParameter.ModifyTheta) &&
                sectionDeviation < reviseParameter.ModifySectionDeviation * ontimeReviseConfig.Return0ThetaPriority.SectionDeviation &&
                sectionDeviation > -reviseParameter.ModifySectionDeviation * ontimeReviseConfig.Return0ThetaPriority.SectionDeviation)
            {
                if ((theta < reviseParameter.ModifyTheta / ontimeReviseConfig.Return0ThetaPriority.Theta &&
                     theta > -reviseParameter.ModifyTheta / ontimeReviseConfig.Return0ThetaPriority.Theta))
                {
                    reviseParameter.ReviseType = EnumLineReviseType.None;
                    wheelTheta = new double[4] { 0, 0, 0, 0 };
                    return true;
                }
                else
                {
                    reviseParameter.ReviseType = EnumLineReviseType.Theta;
                    reviseParameter.ReviseValue = theta;
                    double turnTheta = theta / reviseParameter.ModifyTheta / ontimeReviseConfig.LinePriority.Theta * reviseParameter.MaxTheta;

                    if (turnTheta > reviseParameter.MaxTheta)
                        turnTheta = reviseParameter.MaxTheta;
                    else if (turnTheta < -reviseParameter.MaxTheta)
                        turnTheta = -reviseParameter.MaxTheta;

                    turnTheta = reviseParameter.DirFlag ? -turnTheta : turnTheta;
                    wheelTheta = new double[4] { turnTheta, turnTheta, -turnTheta, -turnTheta };
                    return true;
                }
            }
            else if (reviseParameter.ReviseType == EnumLineReviseType.SectionDeviation || sectionDeviation > reviseParameter.ModifySectionDeviation
                                                                                   || sectionDeviation < -reviseParameter.ModifySectionDeviation)
            {
                if (sectionDeviation < reviseParameter.ModifySectionDeviation / ontimeReviseConfig.Return0ThetaPriority.SectionDeviation &&
                    sectionDeviation > -reviseParameter.ModifySectionDeviation / ontimeReviseConfig.Return0ThetaPriority.SectionDeviation)
                {
                    reviseParameter.ReviseType = EnumLineReviseType.None;
                    wheelTheta = new double[4] { 0, 0, 0, 0 };
                    return true;
                }
                else
                {
                    reviseParameter.ReviseType = EnumLineReviseType.SectionDeviation;
                    reviseParameter.ReviseValue = sectionDeviation;
                    double turnTheta = sectionDeviation / reviseParameter.ModifySectionDeviation / ontimeReviseConfig.LinePriority.SectionDeviation * reviseParameter.MaxTheta;

                    if (turnTheta > reviseParameter.MaxTheta)
                        turnTheta = reviseParameter.MaxTheta;
                    else if (turnTheta < -reviseParameter.MaxTheta)
                        turnTheta = -reviseParameter.MaxTheta;

                    if (isOldCompute)
                        turnTheta = reviseParameter.DirFlag ? turnTheta : -turnTheta;

                    wheelTheta = new double[4] { turnTheta, turnTheta, turnTheta, turnTheta };
                    return true;
                }
            }
            else
            {
                reviseParameter.ReviseType = EnumLineReviseType.None;
                wheelTheta = new double[4] { 0, 0, 0, 0 };
                return true;
            }
        }

        private bool HorizontalRevise(ref double[] wheelTheta, double theta, double sectionDeviation, int wheelAngle, bool isOldCompute = true)
        {
            if (reviseParameter.ReviseType == EnumLineReviseType.SectionDeviation || sectionDeviation > reviseParameter.ModifySectionDeviation
                                                                                  || sectionDeviation < -reviseParameter.ModifySectionDeviation)
            {
                if (sectionDeviation < reviseParameter.ModifySectionDeviation / ontimeReviseConfig.Return0ThetaPriority.SectionDeviation &&
                    sectionDeviation > -reviseParameter.ModifySectionDeviation / ontimeReviseConfig.Return0ThetaPriority.SectionDeviation)
                {
                    reviseParameter.ReviseType = EnumLineReviseType.None;
                    wheelTheta = new double[4] { wheelAngle, wheelAngle, wheelAngle, wheelAngle };
                    return true;
                }
                else
                {
                    reviseParameter.ReviseType = EnumLineReviseType.SectionDeviation;
                    reviseParameter.ReviseValue = sectionDeviation;
                    double turnTheta = sectionDeviation / reviseParameter.ModifySectionDeviation / ontimeReviseConfig.LinePriority.SectionDeviation * reviseParameter.MaxTheta;

                    if (turnTheta > reviseParameter.MaxTheta)
                        turnTheta = reviseParameter.MaxTheta;
                    else if (turnTheta < -reviseParameter.MaxTheta)
                        turnTheta = -reviseParameter.MaxTheta;

                    if (isOldCompute)
                    {
                        turnTheta = reviseParameter.DirFlag ? turnTheta : -turnTheta;
                        turnTheta = (wheelAngle == -90) ? -turnTheta : turnTheta;
                    }

                    wheelTheta = new double[4] { wheelAngle + turnTheta, wheelAngle + turnTheta, wheelAngle + turnTheta, wheelAngle + turnTheta };
                    return true;
                }
            }
            else if ((reviseParameter.ReviseType == EnumLineReviseType.Theta || theta > reviseParameter.ModifyTheta || theta < -reviseParameter.ModifyTheta))
            {
                if ((theta < reviseParameter.ModifyTheta / ontimeReviseConfig.Return0ThetaPriority.Theta &&
                     theta > -reviseParameter.ModifyTheta / ontimeReviseConfig.Return0ThetaPriority.Theta))
                {
                    reviseParameter.ReviseType = EnumLineReviseType.None;
                    wheelTheta = new double[4] { wheelAngle, wheelAngle, wheelAngle, wheelAngle };
                    return true;
                }
                else
                {
                    reviseParameter.ReviseType = EnumLineReviseType.Theta;
                    reviseParameter.ReviseValue = theta;
                    double turnTheta = theta / reviseParameter.ModifyTheta / ontimeReviseConfig.LinePriority.Theta * reviseParameter.MaxTheta;

                    if (turnTheta > reviseParameter.MaxTheta)
                        turnTheta = reviseParameter.MaxTheta;
                    else if (turnTheta < -reviseParameter.MaxTheta)
                        turnTheta = -reviseParameter.MaxTheta;

                    double leftTheta;

                    if ((reviseParameter.DirFlag && wheelAngle == 90) ||
                        (!reviseParameter.DirFlag && wheelAngle == -90))
                        leftTheta = -turnTheta;
                    else
                        leftTheta = turnTheta;

                    double rightTheta = -leftTheta;

                    wheelTheta = new double[4] { wheelAngle + leftTheta, wheelAngle + rightTheta, wheelAngle + leftTheta, wheelAngle + rightTheta };
                    return true;
                }
            }
            else
            {
                reviseParameter.ReviseType = EnumLineReviseType.None;
                wheelTheta = new double[4] { wheelAngle, wheelAngle, wheelAngle, wheelAngle };
                return true;
            }
        }

        private void UpdateParameter(double velocity)
        {
            if (velocity < 0)
                velocity = -velocity;

            if (reviseParameter.Velocity > velocity)
                velocity = reviseParameter.Velocity;

            if (velocity == reviseParameter.Velocity)
                return;

            reviseParameter.MaxTheta = 1;
            for (int i = 0; i < ontimeReviseConfig.SpeedToMaxTheta.Count; i++)
            {
                if (velocity < ontimeReviseConfig.SpeedToMaxTheta[i].Speed)
                {
                    reviseParameter.MaxTheta = ontimeReviseConfig.SpeedToMaxTheta[i].MaxTheta;
                    break;
                }
            }

            if (velocity > ontimeReviseConfig.MaxVelocity)
                velocity = ontimeReviseConfig.MaxVelocity;
            else if (velocity < ontimeReviseConfig.MinVelocity)
                velocity = ontimeReviseConfig.MinVelocity;

            reviseParameter.ModifyTheta = velocity / ontimeReviseConfig.ModifyPriority.Theta;
            reviseParameter.ModifySectionDeviation = velocity / ontimeReviseConfig.ModifyPriority.SectionDeviation;
            reviseParameter.ReviseType = EnumLineReviseType.None;
            reviseParameter.ReviseValue = 0;
            reviseParameter.ThetaCommandSpeed = 10;
        }
        #endregion
        
        #region 角度偏差檢查
        private bool CheckTehtaSectionDeviationSafe(double wheelAngle, double theta, double sectionDeviation, ref string safetyMessage)
        {
            if (safety == null)
                return true;

            if (safety[EnumMoveControlSafetyType.OntimeReviseTheta].Enable)
            {
                if (Math.Abs(theta) > safety[EnumMoveControlSafetyType.OntimeReviseTheta].Range)
                {
                    safetyMessage = "角度偏差" + theta.ToString("0.0") +
                        "度,已超過安全設置的" +
                        safety[EnumMoveControlSafetyType.OntimeReviseTheta].Range.ToString("0.0") +
                        "度,因此啟動EMS!";

                    return false;
                }
            }

            if (safety[EnumMoveControlSafetyType.OntimeReviseSectionDeviationLine].Enable)
            {
                if (Math.Abs(sectionDeviation) > safety[EnumMoveControlSafetyType.OntimeReviseSectionDeviationLine].Range)
                {
                    safetyMessage = "軌道偏差" + sectionDeviation.ToString("0") +
                        "mm,已超過安全設置的" +
                        safety[EnumMoveControlSafetyType.OntimeReviseSectionDeviationLine].Range.ToString("0") +
                        "mm,因此啟動EMS!";

                    return false;
                }
            }

            if (safety[EnumMoveControlSafetyType.OntimeReviseSectionDeviationLine].Enable)
            {
                if (wheelAngle != 0 && safety[EnumMoveControlSafetyType.OntimeReviseSectionDeviationHorizontal].Enable)
                {
                    if (Math.Abs(sectionDeviation) > safety[EnumMoveControlSafetyType.OntimeReviseSectionDeviationHorizontal].Range)
                    {
                        safetyMessage = "橫移偏差" + sectionDeviation.ToString("0") +
                            "mm,已超過安全設置的" +
                            safety[EnumMoveControlSafetyType.OntimeReviseSectionDeviationHorizontal].Range.ToString("0") +
                            "mm,因此啟動EMS!";

                        return true;
                    }
                }
                else
                {
                    if (Math.Abs(sectionDeviation) > safety[EnumMoveControlSafetyType.OntimeReviseSectionDeviationLine].Range)
                    {
                        safetyMessage = "軌道偏差" + sectionDeviation.ToString("0") +
                            "mm,已超過安全設置的" +
                            safety[EnumMoveControlSafetyType.OntimeReviseSectionDeviationLine].Range.ToString("0") +
                            "mm,因此啟動EMS!";

                        return true;
                    }
                }
            }

            return true;
        }
        #endregion
        
        public bool OntimeRevise(ref double[] wheelTheta, int wheelAngle, double velocity, ref string safetyMessage)
        {
            ThetaSectionDeviation reviseData = null;

            int index = -1;

            for (int i = 0; i < DriverSr2000List.Count; i++)
            {
                reviseData = DriverSr2000List[i].GetThetaSectionDeviation();
                if (reviseData != null)
                {
                    if (computeFunction.IsSameAngle(reviseData.BarcodeAngleInMap, reviseData.AGVAngleInMap, wheelAngle))
                    {
                        index = i;
                        break;
                    }
                    else
                        reviseData = null;
                }
            }
            
            if (!elmoDriver.MoveCompelete(EnumAxis.GT))
                return false;

            if (reviseData == null)
            {
                wheelTheta = new double[4] { wheelAngle, wheelAngle, wheelAngle, wheelAngle };
                return true;
            }
            else
            {
                uint count = reviseData.Count;

                if (count == lastCount && index == lastSR2000Index)
                {
                    return false;
                }
                else
                {
                    lastCount = count;
                    lastSR2000Index = index;

                    UpdateParameter(velocity);

                    if (wheelAngle == 0)
                        return LineRevise(ref wheelTheta, reviseData.Theta, reviseData.SectionDeviation);
                    else
                        return HorizontalRevise(ref wheelTheta, reviseData.Theta, reviseData.SectionDeviation, wheelAngle);
                }
            }
        }

        public bool OntimeReviseByAGVPositionAndSection(ref double[] wheelTheta, ThetaSectionDeviation thetaAndSectionDeviation, int wheelAngle, double velocity)
        {
            if (thetaAndSectionDeviation == null)
            {
                wheelTheta = new double[4] { wheelAngle, wheelAngle, wheelAngle, wheelAngle };
                return true;
            }
            else
            {
                UpdateParameter(velocity);

                if (wheelAngle == 0)
                    return LineRevise(ref wheelTheta, thetaAndSectionDeviation.Theta, thetaAndSectionDeviation.SectionDeviation, false);
                else
                    return HorizontalRevise(ref wheelTheta, thetaAndSectionDeviation.Theta, thetaAndSectionDeviation.SectionDeviation, wheelAngle, false);
            }
        } // OntimeReviseByAGVPositionAndSection()
    }
}
