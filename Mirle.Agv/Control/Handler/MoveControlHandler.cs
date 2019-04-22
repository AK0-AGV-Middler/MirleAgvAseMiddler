using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Mirle.Agv.Model;
using Mirle.Agv.Model.Configs;
using Mirle.Agv.Model.TransferCmds;
using Mirle.Agv.Control;


namespace Mirle.Agv.Control.Handler
{
    public class MoveControlHandler : ITransferHandler, IMapBarcodeSender
    {
        private ConcurrentQueue<MoveCmdInfo> queReadyCmds;
        private EnumMoveState moveState;
        private VehLocation vehLocation;
        private List<IMapBarcodeTaker> mapBarcodeTakers;

        public MoveControlHandler()
        {
            mapBarcodeObservers = new List<IMapBarcodeTaker>();
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
            vehLocation.SetMapBarcodeValues(mapBarcodeValues);
            SendBarcodeValues(mapBarcodeValues);
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
                MapBarcodeValues mapBarcodeValues = new MapBarcodeValues();
                //TODO : get new mapBarcodeValues from driver
                UpdateMapBarcodeValues(mapBarcodeValues);
                Thread.Sleep(100);
            } while (moveState == EnumMoveState.Moving);
        }

        public void DoTransfer(TransCmd transCmd)
        {
            MoveCmdInfo moveCmd = (MoveCmdInfo)transCmd;
            queReadyCmds.Enqueue(moveCmd);
        }

        public void AddMapBarcodeTakerInList(IMapBarcodeTaker mapBarcodeTaker)
        {
            mapBarcodeTakers.Add(mapBarcodeTaker);
        }

        public void RemoveMapBarcodeTakerInList(IMapBarcodeTaker mapBarcodeTaker)
        {
            mapBarcodeTakers.Remove(mapBarcodeTaker);
        }

        public void SendBarcodeValues(MapBarcodeValues mapBarcodeValues)
        {
            foreach (var mapBarcodeTaker in mapBarcodeTakers)
            {
                mapBarcodeTaker.UpdateMapBarcode(mapBarcodeValues);
            }
        }
    }
}
