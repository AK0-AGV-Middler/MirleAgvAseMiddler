using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Mirle.Agv.Model;
using System.Collections.Concurrent;

namespace Mirle.Agv.Control
{
    public class MainFlowHandler
    {
        private List<PartialJob> allPartialJobs;

        private ConcurrentQueue<PartialJob> quePartialJobs;

        private ConcurrentQueue<string> queAskReserve;

        //private ConcurrentQueue<PartialJob> readyToDoPartialJobs;

        //private EnumMainFlowState state;

        private MoveControlHandler moveControlHandler;

        private MiddleInterface middleHandler;

        private PlcInterface plcHandler;

        private CoupleHandler coupleHandler;



        public MainFlowHandler()
        {
            moveControlHandler = new MoveControlHandler();
            middleHandler = new MiddleInterface();

            allPartialJobs = new List<PartialJob>();
            quePartialJobs = new ConcurrentQueue<PartialJob>();
            queAskReserve = new ConcurrentQueue<string>();
        }

        private void MainFlowHandlerOn()
        {
            try
            {
                Thread thdGetAllPartialJobs = new Thread(new ThreadStart(GetAllPartialJobs));
                thdGetAllPartialJobs.IsBackground = true;
                thdGetAllPartialJobs.Start();

                Thread thdAskReserve = new Thread(new ThreadStart(AskReserve));
                thdAskReserve.IsBackground = true;
                thdAskReserve.Start();
            }
            catch (Exception ex)
            {
                //log ex
                throw;
            }
        }

        private void GetAllPartialJobs()
        {
            while (middleHandler.partialJobs.Count>0)
            {
                allPartialJobs = middleHandler.partialJobs.ToList();
                middleHandler.partialJobs.Clear();
                DoAllPartialJobs();
                Thread.Sleep(10);//can config
            }
        }

        private void DoAllPartialJobs()
        {
            for (int i = 0; i < allPartialJobs.Count; i++)
            {
                PartialJob partialJob = allPartialJobs[i].Clone();
                quePartialJobs.Enqueue(partialJob);
            }

            while (quePartialJobs.Count > 0)
            {
                if (!CanDoNextPartialJob())
                {
                    continue;
                }
                quePartialJobs.TryDequeue(out PartialJob partialJob);
                DoPartialJobs(partialJob);
                Thread.Sleep(10); //can config 10
            }
        }

        private bool CanDoNextPartialJob()
        {
            switch (allPartialJobs[0].partialJobType)
            {
                case EnumPartialJobType.Move:
                    return CanVehMove();
                case EnumPartialJobType.Load:
                    return CanVehLoad();
                case EnumPartialJobType.Unload:
                    return CanVehUnload();
                default:
                    return false;
            }
        }

        private bool CanVehUnload()
        {
            throw new NotImplementedException();
        }

        private bool CanVehLoad()
        {
            throw new NotImplementedException();
        }

        private bool CanVehMove()
        {
            //battery/emo/beam/etc/reserve
            throw new NotImplementedException();
        }

        private void DoPartialJobs(PartialJob partialJob)
        {
            switch (partialJob.partialJobType)
            {
                case EnumPartialJobType.Move:
                    MovePartialJob movePartialJob = (MovePartialJob)partialJob;
                    queAskReserve.Enqueue(movePartialJob.moveCmdInfo.GetSectionId());
                    break;
                case EnumPartialJobType.Load:
                    break;
                case EnumPartialJobType.Unload:
                    break;
                default:
                    break;
            }
        }

        private void AskReserve()
        {
            while (queAskReserve.Count>0)
            {
                queAskReserve.TryPeek(out string sectionId);
                if (middleHandler.GetReserveFromAgvc(sectionId))
                {
                    //log sectionId get reserved.
                    queAskReserve.TryDequeue(out string acceptSectionId);
                    GoThisSection(sectionId);
                }

                Thread.Sleep(123); //can config 123
            }
        }

        private void GoThisSection(string sectionId)
        {
            //find the partialjob in allpartialjobs
            //send moveinfo to MoveControlHandler;
            foreach (PartialJob partialJob in allPartialJobs)
            {               
                if (partialJob.partialJobType== EnumPartialJobType.Move)
                {
                    MovePartialJob movePartialJob = (MovePartialJob)partialJob;

                    if (movePartialJob.moveCmdInfo.GetSectionId()==sectionId)
                    {
                        //Convert this MovePartialJob into aMoveCmdInfo.
                        //MoveControlHandler.queReadyCmds.enqueue(aMoveCmdInfo)
                    }
                }
            }
        }

        public void Pause()
        {

        }

        public void Stop()
        {

        }
    }
}
