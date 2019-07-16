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
        private MoveControlConfig moveControlConfig;
        private MapInfo theMapInfo = new MapInfo();

        public event EventHandler<EnumCompleteStatus> OnMoveFinished;

        public MapPosition EncoderPosition { get; set; }
        public MapPosition BarcodePosition { get; set; }
        public MapPosition DeltaPosition { get; set; }
        public MapPosition RealPosition { get; set; }

        public MoveControlHandler(MoveControlConfig moveControlConfig, MapInfo theMapInfo)
        {
            this.theMapInfo = theMapInfo;
            loggerAgent = LoggerAgent.Instance;
            this.moveControlConfig = moveControlConfig;
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
