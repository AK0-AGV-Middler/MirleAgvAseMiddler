using Mirle.Agv.Model;
using Mirle.Agv.Model.TransferCmds;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Mirle.Agv.Model.Configs;
using Mirle.Agv.Control.Tools;
using System.IO;

namespace Mirle.Agv.Control
{
    public class MoveControlHandler
    {
        private LoggerAgent loggerAgent;
        private ConcurrentQueue<MoveCmdInfo> queReadyCmds;
        private EnumMoveState moveState;
        private VehLocation vehLocation;
        public Sr2000Agent sr2000Agent;
        private MoveControlConfigs moveControlConfigs;
        private Dictionary<string, ElmoSingleAxisConfig> dicElmoSingleAxisConfigs;
        private ElmoAxisConfigs elmoAxisConfigs;

        public event EventHandler<EnumCompleteStatus> OnMoveFinished;

        public MoveControlHandler(MoveControlConfigs moveControlConfigs, Sr2000Configs sr2000Configs)
        {
            loggerAgent = LoggerAgent.Instance;
            queReadyCmds = new ConcurrentQueue<MoveCmdInfo>();
            this.moveControlConfigs = moveControlConfigs;
            sr2000Agent = new Sr2000Agent(sr2000Configs);
            AxisInitial();

            moveState = EnumMoveState.Idle;
            RunThreads();
        }

        private void AxisInitial()
        {
            dicElmoSingleAxisConfigs = new Dictionary<string, ElmoSingleAxisConfig>();

            var path = Path.Combine(Environment.CurrentDirectory, "AxisConfig.ini");
            ConfigHandler configHandler = new ConfigHandler(path);

            elmoAxisConfigs = new ElmoAxisConfigs();
            int.TryParse(configHandler.GetString("ElmoAxis", "AxisNum", "18"), out int tempAxisNum);
            elmoAxisConfigs.AxisNum = tempAxisNum;
            elmoAxisConfigs.SectionName = configHandler.GetString("ElmoAxis", "SectionName", "Axis");

            for (int i = 0; i < elmoAxisConfigs.AxisNum; i++)
            {
                int index = i + 1;
                var sectionName = elmoAxisConfigs.SectionName + index.ToString();

                ElmoSingleAxisConfig elmoSingleAxisConfigs = new ElmoSingleAxisConfig();
                elmoSingleAxisConfigs.AxisAlias = configHandler.GetString(sectionName, "AxisAlias", "XXX");
                elmoSingleAxisConfigs.AxisName = configHandler.GetString(sectionName, "AxisName", "XXX");
                elmoSingleAxisConfigs.IsGroup = bool.Parse(configHandler.GetString(sectionName, "IsGroup", "False"));
                elmoSingleAxisConfigs.AxisID = int.Parse(configHandler.GetString(sectionName, "AxisID", "100"));
                elmoSingleAxisConfigs.MotorResolution = double.Parse(configHandler.GetString(sectionName, "MotorResolution", "2.3"));
                elmoSingleAxisConfigs.PulseUnit = double.Parse(configHandler.GetString(sectionName, "PulseUnit", "4.5"));
                elmoSingleAxisConfigs.Velocity = double.Parse(configHandler.GetString(sectionName, "Velocity", "6.7"));
                elmoSingleAxisConfigs.Acceleration = double.Parse(configHandler.GetString(sectionName, "Acceleration", "8.9"));
                elmoSingleAxisConfigs.Deceleration = double.Parse(configHandler.GetString(sectionName, "Deceleration", "0.1"));
                elmoSingleAxisConfigs.Jerk = double.Parse(configHandler.GetString(sectionName, "Jerk", "2.3"));


                dicElmoSingleAxisConfigs.Add(elmoSingleAxisConfigs.AxisName, elmoSingleAxisConfigs);
            }
        }

        public void RunThreads()
        {
            try
            {
                Thread thdTryDeQueReadyCmds = new Thread(new ThreadStart(TryDeQueReadyCmds));
                thdTryDeQueReadyCmds.IsBackground = true;
                thdTryDeQueReadyCmds.Start();
            }
            catch (Exception ex)
            {
                //log ex
                throw;
            }
        }

        public int GetAmountOfQueReadyCmds()
        {
            return queReadyCmds.Count;
        }

        private void TryDeQueReadyCmds()
        {
            do
            {
                if (queReadyCmds.Count > 0)
                {
                    MoveCmdInfo moveCmd;
                    queReadyCmds.TryDequeue(out moveCmd);
                    MoveOn(moveCmd);
                }

                if (moveState == EnumMoveState.MoveComplete)
                {
                    UpdateLoacation();
                    SendMoveCompleteReportToMainFlow();
                }

                SpinWait.SpinUntil(() => false, 10);
            } while (moveState == EnumMoveState.Moving);
        }

        private void SendMoveCompleteReportToMainFlow()
        {
            throw new NotImplementedException();
        }

        private void UpdateLoacation()
        {
            throw new NotImplementedException();
        }

        private void MoveOn(MoveCmdInfo moveCmd)
        {
            //drive elmo to move the vehicle
            throw new NotImplementedException();
        }

        public void DoTransfer(TransCmd transCmd)
        {
            MoveCmdInfo moveCmd = (MoveCmdInfo)transCmd;
            queReadyCmds.Enqueue(moveCmd);
        }

        /// <summary>
        ///  when move finished, call this function to notice other class instance that move is finished with status
        /// </summary>
        private void MoveFinished(EnumCompleteStatus status)
        {
            OnMoveFinished?.Invoke(this, status);
        }

        public void MainFlow_OnTransferMoveEven(object sender, MoveCmdInfo moveCmd)
        {
            throw new NotImplementedException();
        }
    }
}
