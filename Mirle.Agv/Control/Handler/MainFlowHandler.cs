using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using Mirle.Agv.Model;
using Mirle.Agv.Control.Tools;
using System.Collections.Concurrent;
using Mirle.Agv.Control.Tools.Logger;
using Mirle.Agv.Model.Configs;
using Mirle.Agv.Control.Handler;
using System.Windows.Forms;

namespace Mirle.Agv.Control
{
    public class MainFlowHandler : IMapBarcodeTaker
    {
        #region Configs

        private string configPath;
        private ConfigHandler configHandler;
        private MainFlowConfigs mainFlowConfigs;
        private MiddlerConfigs middlerConfigs;

        #endregion

        #region Loggers
        //TODO : restruct by some design pattern
        private Logger debugLogger;
        private Logger infoLogger;
        private Logger errorLogger;
        private Logger commLogger;
        private Dictionary<string, Logger> dicLoggers;

        #endregion
        
        private List<PartialJob> allPartialJobs;

        private ConcurrentQueue<PartialJob> quePartialJobs;

        private ConcurrentQueue<string> queAskReserve;

        //private ConcurrentQueue<PartialJob> readyToDoPartialJobs;

        //private EnumMainFlowState state;

        private MoveControlHandler moveControlHandler;

        private MiddleInterface middleHandler;

        private MapHandler mapHandler;

        private PlcInterface plcHandler;

        private CoupleHandler coupleHandler;

        public Vehicle theVehicle;

        #region LogFunctions

        public void DebugLog(string msg)
        {
            if (debugLogger != null)
            {
                debugLogger.SaveLogFile("sCategory", "sLogLevel", "sClassFunctionName", "Device", "CarrierId", msg);
            }
        }

        public void InfoLog(string msg)
        {
            if (infoLogger != null)
            {
                infoLogger.SaveLogFile("sCategory", "sLogLevel", "sClassFunctionName", "Device", "CarrierId", msg);
            }
        }

        public void ErrorLog(string msg)
        {
            if (errorLogger != null)
            {
                errorLogger.SaveLogFile("sCategory", "sLogLevel", "sClassFunctionName", "Device", "CarrierId", msg);
            }
        }

        public void CommLog(string msg)
        {
            if (commLogger != null)
            {
                commLogger.SaveLogFile("sCategory", "sLogLevel", "sClassFunctionName", "Device", "CarrierId", msg);
            }
        }

        #endregion

        public MainFlowHandler()
        {
            ConfigsInitial();
            LoggersInitial();

            ControllerInitial();
            AddMapBarcodeTakerInList();

            allPartialJobs = new List<PartialJob>();
            quePartialJobs = new ConcurrentQueue<PartialJob>();
            queAskReserve = new ConcurrentQueue<string>();

            VehicleInitial();
        }

        private void AddMapBarcodeTakerInList()
        {
            moveControlHandler.AddMapBarcodeTakerInList(this);
            moveControlHandler.AddMapBarcodeTakerInList(middleHandler);
            moveControlHandler.AddMapBarcodeTakerInList(mapHandler);
        }

        private void ControllerInitial()
        {
            moveControlHandler = new MoveControlHandler();
            middleHandler = new MiddleInterface();
            mapHandler = new MapHandler();
        }

        private void ConfigsInitial()
        {
            configPath = Path.Combine(Environment.CurrentDirectory, "Configs.ini");
            configHandler = new ConfigHandler(configPath);

            mainFlowConfigs = new MainFlowConfigs();
            mainFlowConfigs.LogConfigPath = Path.Combine(Environment.CurrentDirectory, configHandler.GetString("MainFlow", "LogConfigPath", "Log.ini"));

            middlerConfigs = new MiddlerConfigs();
            middlerConfigs.Ip = configHandler.GetString("Middler", "Ip", "127.0.0.1");
            int.TryParse(configHandler.GetString("Middler", "Port", "5001"), out int tempPort);
            middlerConfigs.Port = tempPort;
            int.TryParse(configHandler.GetString("Middler", "SleepTime", "10"), out int tempSleepTime);
            middlerConfigs.SleepTime = tempSleepTime;

        }

        private void LoggersInitial()
        {
            //TODO : make abstract class with an logger and its bean and a function do log, make 4 level subclass imp this abstract class
            dicLoggers = new Dictionary<string, Logger>();

            List<CategoryTypeBean> listCategory = Logger.ReadLogIniFile(mainFlowConfigs.LogConfigPath);
            foreach (CategoryTypeBean bean in listCategory)
            {
                Logger logger = new Logger(bean);
                dicLoggers.Add(logger.LogFileName, logger);
            }

            if (dicLoggers.ContainsKey("Debug"))
            {
                debugLogger = dicLoggers["Debug"];
            }

            if (dicLoggers.ContainsKey("Info"))
            {
                infoLogger = dicLoggers["Info"];
            }

            if (dicLoggers.ContainsKey("Error"))
            {
                errorLogger = dicLoggers["Error"];
            }

            if (dicLoggers.ContainsKey("Comm"))
            {
                commLogger = dicLoggers["Comm"];
            }
        }

        private void VehicleInitial()
        {
            theVehicle = Vehicle.GetInstance();

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
            while (middleHandler.partialJobs.Count > 0)
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
                    //MovePartialJob movePartialJob = (MovePartialJob)partialJob;
                    //queAskReserve.Enqueue(movePartialJob.moveCmdInfo.GetSectionId());
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
            while (queAskReserve.Count > 0)
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
                if (partialJob.partialJobType == EnumPartialJobType.Move)
                {
                    MovePartialJob movePartialJob = (MovePartialJob)partialJob;

                    if (movePartialJob.moveCmdInfo.GetSectionId() == sectionId)
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

        public void UpdateMapBarcode(MapBarcodeValues mapBarcode)
        {
            theVehicle.UpdateStatus(mapBarcode);
        }

    }
}
