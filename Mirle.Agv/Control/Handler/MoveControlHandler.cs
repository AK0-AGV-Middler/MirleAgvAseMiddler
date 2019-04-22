using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Mirle.Agv.Model;

namespace Mirle.Agv.Control
{
    public class MoveControlHandler
    {
        private MainFlowHandler mainFlowHandler;

        private ConcurrentQueue<MoveCmdInfo> queReadyCmds;
        private EnumMoveState moveState;
        
        private VehLocation vehLocation;

        public MoveControlHandler(MainFlowHandler mainFlowHandler)
        {
            this.mainFlowHandler = mainFlowHandler;
            queReadyCmds = new ConcurrentQueue<MoveCmdInfo>();
            moveState = EnumMoveState.Idle;
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

        public void AddQueReadyCmds(MoveCmdInfo moveCmd)
        {
            queReadyCmds.Enqueue(moveCmd);
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

        private void MoveOn(MoveCmdInfo moveCmd)
        {
            //drive elmo to move the vehicle
            throw new NotImplementedException();
        }

        private void Tracking2DcodeReader()
        {
            do
            {
                UpdateLoacation();
                SendLocationReportToMainFlow();
                Thread.Sleep(100);
            } while (moveState == EnumMoveState.Moving);
        }

        private void SendLocationReportToMainFlow()
        {
            throw new NotImplementedException();
        }
    }
}
