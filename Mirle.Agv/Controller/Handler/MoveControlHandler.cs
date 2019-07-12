using Mirle.Agv.Model;
using Mirle.Agv.Model.TransferCmds;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Mirle.Agv.Model.Configs;
using Mirle.Agv.Controller.Tools;
using System.IO;

namespace Mirle.Agv.Controller
{
    public class MoveControlHandler
    {
        private LoggerAgent loggerAgent;
        private ConcurrentQueue<MoveCmdInfo> queReadyCmds;
        private EnumMoveState moveState;
        private VehLocation vehLocation;
        public Sr2000Agent sr2000Agent;
        private MoveControlConfig moveControlConfig;
        private Dictionary<string, ElmoSingleAxisConfig> dicElmoSingleAxisConfigs;
        private ElmoAxisConfig elmoAxisConfig;
        private MapInfo theMapInfo = new MapInfo();

        public event EventHandler<EnumCompleteStatus> OnMoveFinished;

        public MapPosition EncoderPosition { get; set; }
        public MapPosition BarcodePosition { get; set; }
        public MapPosition DeltaPosition { get; set; }
        public MapPosition RealPosition { get; set; }

        public MoveControlHandler(MoveControlConfig moveControlConfig, Sr2000Config sr2000Config, MapInfo theMapInfo)
        {
            this.theMapInfo = theMapInfo;
            loggerAgent = LoggerAgent.Instance;
            queReadyCmds = new ConcurrentQueue<MoveCmdInfo>();
            this.moveControlConfig = moveControlConfig;
            sr2000Agent = new Sr2000Agent(sr2000Config);
            AxisInitial();

            moveState = EnumMoveState.Idle;
        }

        private void AxisInitial()
        {
            dicElmoSingleAxisConfigs = new Dictionary<string, ElmoSingleAxisConfig>();

            var path = Path.Combine(Environment.CurrentDirectory, "AxisConfig.ini");
            ConfigHandler configHandler = new ConfigHandler(path);

            elmoAxisConfig = new ElmoAxisConfig();
            int.TryParse(configHandler.GetString("ElmoAxis", "AxisNum", "18"), out int tempAxisNum);
            elmoAxisConfig.AxisNum = tempAxisNum;
            elmoAxisConfig.SectionName = configHandler.GetString("ElmoAxis", "SectionName", "Axis");

            for (int i = 0; i < elmoAxisConfig.AxisNum; i++)
            {
                int index = i + 1;
                var sectionName = elmoAxisConfig.SectionName + index.ToString();

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

        public int GetAmountOfQueReadyCmds()
        {
            return queReadyCmds.Count;
        }

        /// <summary>
        ///  when move finished, call this function to notice other class instance that move is finished with status
        /// </summary>
        private void MoveFinished(EnumCompleteStatus status)
        {
            OnMoveFinished?.Invoke(this, status);
        }

        public bool TransferMove(MoveCmdInfo moveCmd)
        {
            return true;
        }

        public bool AddReservedOkMapPosition(MapPosition mapPosition)
        {
            return true;
        }
    }
}
