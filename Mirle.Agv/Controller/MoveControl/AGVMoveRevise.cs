using Mirle.Agv.Model;
using Mirle.Agv.Model.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Controller
{
    public class AGVMoveRevise
    {
        private ReviseParameter reviseParameter;
        private OntimeReviseConfig ontimeReviseConfig = null;
        private ElmoDriver elmoDriver = null;
        private List<Sr2000Driver> DriverSr2000List = null;
        private const int AllowableTheta = 10;

        public AGVMoveRevise(OntimeReviseConfig ontimeReviseConfig, ElmoDriver elmoDriver, List<Sr2000Driver> DriverSr2000List)
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

        private bool IsSameAngle(double barcodeAngleInMap, double agvAngleInMap, int wheelAngle)
        {
            if (Math.Abs(agvAngleInMap - 0) < AllowableTheta)
                agvAngleInMap = 0;
            else if (Math.Abs(agvAngleInMap - 90) < AllowableTheta)
                agvAngleInMap = 90;
            else if (Math.Abs(agvAngleInMap - -90) < AllowableTheta)
                agvAngleInMap = -90;
            else if (Math.Abs(agvAngleInMap - 180) < AllowableTheta || Math.Abs(agvAngleInMap - -180) < AllowableTheta)
                agvAngleInMap = 180;
            else
                return false;

            return (agvAngleInMap + barcodeAngleInMap + wheelAngle) % 180 == 0;
        }

        private bool LineRevise(ref double[] wheelTheta, double theta, double sectionDeviation)
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

        private bool HorizontalRevise(ref double[] wheelTheta, double theta, double sectionDeviation, int wheelAngle)
        {
            if ((reviseParameter.ReviseType == EnumLineReviseType.Theta || theta > reviseParameter.ModifyTheta || -theta < -reviseParameter.ModifyTheta) &&
                sectionDeviation < reviseParameter.ModifySectionDeviation * ontimeReviseConfig.Return0ThetaPriority.SectionDeviation &&
                sectionDeviation > -reviseParameter.ModifySectionDeviation * ontimeReviseConfig.Return0ThetaPriority.SectionDeviation)
            {
                if ((theta < reviseParameter.ModifyTheta / ontimeReviseConfig.Return0ThetaPriority.Theta &&
                    -theta > -reviseParameter.ModifyTheta / ontimeReviseConfig.Return0ThetaPriority.Theta))
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

                    turnTheta = reviseParameter.DirFlag ? -turnTheta : turnTheta;
                    wheelTheta = new double[4] { wheelAngle - turnTheta, wheelAngle + turnTheta, wheelAngle - turnTheta, wheelAngle + turnTheta };
                    return true;
                }
            }
            else if (reviseParameter.ReviseType == EnumLineReviseType.SectionDeviation || sectionDeviation > reviseParameter.ModifySectionDeviation
                                                                                   || -sectionDeviation < -reviseParameter.ModifySectionDeviation)
            {
                if (sectionDeviation < reviseParameter.ModifySectionDeviation / ontimeReviseConfig.Return0ThetaPriority.SectionDeviation &&
                    -sectionDeviation > -reviseParameter.ModifySectionDeviation / ontimeReviseConfig.Return0ThetaPriority.SectionDeviation)
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

                    turnTheta = reviseParameter.DirFlag ? turnTheta : -turnTheta;
                    turnTheta = (wheelAngle == -90) ? -turnTheta : turnTheta;
                    wheelTheta = new double[4] { wheelAngle + turnTheta, wheelAngle + turnTheta, wheelAngle + turnTheta, wheelAngle + turnTheta };
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

        public bool OntimeRevise(ref double[] wheelTheta, int wheelAngle = 0)
        {
            ThetaSectionDeviation reviseData = null;

            if (!elmoDriver.MoveCompelete(EnumAxis.GT))
                return false;

            for (int i = 0; i < DriverSr2000List.Count; i++)
            {
                reviseData = DriverSr2000List[i].GetThetaSectionDeviation();
                if (reviseData != null)
                {
                    if (IsSameAngle(reviseData.BarodeAngleInMap, reviseData.AGVAngleInMap, wheelAngle))
                        break;
                    else
                        reviseData = null;
                }
            }

            if (reviseData == null)
            {
                wheelTheta = new double[4] { wheelAngle, wheelAngle, wheelAngle, wheelAngle };
                return true;
            }
            else
            {
                if (wheelAngle == 0)
                    return LineRevise(ref wheelTheta, reviseData.Theta, reviseData.SectionDeviation);
                else
                    return HorizontalRevise(ref wheelTheta, reviseData.Theta, reviseData.SectionDeviation, wheelAngle);
            }
        }
    }
}
