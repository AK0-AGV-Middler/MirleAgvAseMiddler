using Mirle.Agv.Model;
using Mirle.Agv.Model.TransferCmds;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;


namespace Mirle.Agv.Control
{
    public class MoveControlHandler : ITransferHandler
    {
        private ConcurrentQueue<MoveCmdInfo> queReadyCmds;
        private EnumMoveState moveState;
        private VehLocation vehLocation;
        //public MapBarcodeValuesWithEvent mapBarcode;

        //值傳遞的事件
        public event EventHandler<MapBarcodeValues> OnMapBarcodeValuesChange;
        private MapBarcodeValues mapBarcodeValues;
        public MapBarcodeValues MapBarcodeValues
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

        public MoveControlHandler()
        {
            queReadyCmds = new ConcurrentQueue<MoveCmdInfo>();
            moveState = EnumMoveState.Idle;
            RunThreads();
        }

        public void RunThreads()
        {
            try
            {
                Thread thdUpdateLocation = new Thread(new ThreadStart(Tracking2DcodeReader));
                thdUpdateLocation.IsBackground = true;
                thdUpdateLocation.Start();

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

        private void UpdateMapBarcodeValues(MapBarcodeValues mapBarcodeValues)
        {
            MapBarcodeValues = mapBarcodeValues;
            vehLocation.SetMapBarcodeValues(mapBarcodeValues);
        }

        private void MoveOn(MoveCmdInfo moveCmd)
        {
            //drive elmo to move the vehicle
            throw new NotImplementedException();
        }

        private void Tracking2DcodeReader()
        {
            do
            {
                //TODO : get new mapBarcodeValues from driver
                MapBarcodeValues mapBarcodeValues = new MapBarcodeValues();
                //mapBarcodeValues = GetFromDriver();               
                UpdateMapBarcodeValues(mapBarcodeValues);
                Thread.Sleep(100);
            } while (moveState == EnumMoveState.Moving);
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
