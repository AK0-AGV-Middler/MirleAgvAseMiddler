using Mirle.Agv.Model;
using Mirle.Agv.Model.TransferCmds;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Mirle.Agv.Model.Configs;


namespace Mirle.Agv.Control
{
    public class MoveControlHandler : ITransferHandler
    {
        private ConcurrentQueue<MoveCmdInfo> queReadyCmds;
        private EnumMoveState moveState;
        private VehLocation vehLocation;
        public Sr2000Agent sr2000Agent;
        private MoveControlConfigs moveControlConfigs;
        private LoggerAgent loggerAgent;

        //值傳遞的事件
        public event EventHandler<MapBarcodeReader> OnMapBarcodeValuesChange;
        private MapBarcodeReader mapBarcodeValues;
        public MapBarcodeReader MapBarcodeValues
        {
            get { return mapBarcodeValues; }
            set
            {
                var oldValues = mapBarcodeValues;
                if (!oldValues.Equals(value))
                {
                    mapBarcodeValues = value;
                    vehLocation.SetMapBarcodeValues(value);

                    //通知其他實體MapBarcodeValues已變成新的value
                    if (OnMapBarcodeValuesChange != null)
                    {
                        OnMapBarcodeValuesChange(this, value);
                    }
                }
            }
        }

        public event EventHandler<EnumCompleteStatus> OnMoveFinished;

        public MoveControlHandler(MoveControlConfigs moveControlConfigs,Sr2000Configs sr2000Configs)
        {
            loggerAgent = LoggerAgent.Instance;
            queReadyCmds = new ConcurrentQueue<MoveCmdInfo>();
            this.moveControlConfigs = moveControlConfigs;           
            sr2000Agent = new Sr2000Agent(sr2000Configs);
            moveState = EnumMoveState.Idle;
            RunThreads();
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

                Thread.Sleep(10);
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

        public void OnMapBarcodeValuesChangedEvent(object sender, MapBarcodeReader mapBarcodeValues)
        {
            vehLocation.SetMapBarcodeValues(mapBarcodeValues);
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
            if (OnMoveFinished != null)
            {
                OnMoveFinished(this, status);
            }
        }



    }
}
