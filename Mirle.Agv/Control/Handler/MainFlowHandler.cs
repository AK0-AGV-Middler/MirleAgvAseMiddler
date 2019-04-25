using Mirle.Agv.Control.Tools;
using Mirle.Agv.Control.Tools.Logger;
using Mirle.Agv.Model;
using Mirle.Agv.Model.Configs;
using Mirle.Agv.Model.TransferCmds;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Mirle.Agv.Control
{
    public class MainFlowHandler : IMapBarcodeValuesEvent
    {
        #region Configs

        private string configPath;
        private ConfigHandler configHandler;
        private MainFlowConfigs mainFlowConfigs;
        private MiddlerConfigs middlerConfigs;
        private MapConfigs mapConfigs;

        #endregion

        #region Loggers
        //TODO : restruct by some design pattern
        private Logger debugLogger;
        private Logger infoLogger;
        private Logger errorLogger;
        private Logger commLogger;
        private Dictionary<string, Logger> dicLoggers;

        #endregion

        private List<TransCmd> transCmds;
        private bool goNextTransCmd;
        private ConcurrentQueue<MoveCmdInfo> queWaitForReserve;

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
            transCmds = new List<TransCmd>();
            queWaitForReserve = new ConcurrentQueue<MoveCmdInfo>();

            VehicleInitial();

            EventInitial();
        }

        private void EventInitial()
        {
            moveControlHandler.mapBarcode.OnMapBarcodeValuesChange += OnMapBarcodeValuesChangedEvent;
        }

        private void ControllerInitial()
        {
            moveControlHandler = new MoveControlHandler();
            middleHandler = new MiddleInterface();
            mapHandler = new MapHandler(mapConfigs.SectionFilePath,mapConfigs.AddressFilePath);
        }

        private void ConfigsInitial()
        {
            configPath = Path.Combine(Environment.CurrentDirectory, "Configs.ini");
            configHandler = new ConfigHandler(configPath);

            mainFlowConfigs = new MainFlowConfigs();
            var tempLogConfigPath = configHandler.GetString("MainFlow", "LogConfigPath", "Log.ini");
            mainFlowConfigs.LogConfigPath = Path.Combine(Environment.CurrentDirectory, tempLogConfigPath);
            int.TryParse(configHandler.GetString("MainFlow", "TransCmdsCheckInterval", "15"), out int tempTransCmdsCheckInterval);
            mainFlowConfigs.TransCmdsCheckInterval = tempTransCmdsCheckInterval;
            int.TryParse(configHandler.GetString("MainFlow", "DoTransCmdsInterval", "15"), out int tempDoTransCmdsInterval);
            mainFlowConfigs.DoTransCmdsInterval = tempDoTransCmdsInterval;
            int.TryParse(configHandler.GetString("MainFlow", "ReserveLength", "3"), out int tempReserveLength);
            mainFlowConfigs.ReserveLength = tempReserveLength;
            int.TryParse(configHandler.GetString("MainFlow", "AskReserveInterval", "15"), out int tempAskReserveInterval);
            mainFlowConfigs.AskReserveInterval = tempAskReserveInterval;

            middlerConfigs = new MiddlerConfigs();
            middlerConfigs.Ip = configHandler.GetString("Middler", "Ip", "127.0.0.1");
            int.TryParse(configHandler.GetString("Middler", "Port", "5001"), out int tempPort);
            middlerConfigs.Port = tempPort;
            int.TryParse(configHandler.GetString("Middler", "SleepTime", "10"), out int tempSleepTime);
            middlerConfigs.SleepTime = tempSleepTime;

            mapConfigs = new MapConfigs();
            mapConfigs.SectionFilePath = configHandler.GetString("Map", "SectionFilePath", "XXX.csv");
            mapConfigs.AddressFilePath = configHandler.GetString("Map", "AddressFilePath", "YYY.csv");
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
                Thread thdGetAllPartialJobs = new Thread(new ThreadStart(TransCmdsCheck));
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

        private void TransCmdsCheck()
        {
            while (middleHandler.IsTransCmds())
            {
                transCmds = middleHandler.GetTransCmds();
                middleHandler.ClearTransCmds();
                DoTransCmds();
                Thread.Sleep(mainFlowConfigs.TransCmdsCheckInterval);//can config
            }
        }

        private void DoTransCmds()
        {
            int index = 0;
            goNextTransCmd = true;

            while (index < transCmds.Count)
            {
                if (goNextTransCmd)
                {
                    TransCmd transCmd = transCmds[index];
                    switch (transCmd.GetType())
                    {
                        case EnumTransCmdType.Move:
                            MoveCmdInfo moveCmd = (MoveCmdInfo)transCmd;
                            queWaitForReserve.Enqueue(moveCmd);
                            goNextTransCmd = !moveCmd.IsPrecisePositioning;         
                            //TODO
                            //MoveComplete(MoveToEnd will set goNextTransCmd into true and go on
                            break;
                        case EnumTransCmdType.Load:
                            LoadCmdInfo loadCmdInfo = (LoadCmdInfo)transCmd;
                            //TODO
                            //command PLC to DoLoad
                            //LoadComplete will set goNextTransCmd into true and go on
                            break;
                        case EnumTransCmdType.Unload:
                            UnloadCmdInfo unloadCmdInfo = (UnloadCmdInfo)transCmd;
                            //TODO
                            //command PLC to DoLoad
                            //LoadComplete will set goNextTransCmd into true and go on
                            break;
                        default:
                            break;
                    }

                    goNextTransCmd = false;
                    index++;
                }
                Thread.Sleep(mainFlowConfigs.DoTransCmdsInterval);
            }
        }

        private bool IsReadyToMoveCmdQueFull()
        {
            return moveControlHandler.GetAmountOfQueReadyCmds() >= mainFlowConfigs.ReserveLength;
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

        private void AskReserve()
        {
            while (true)
            {
                if (CanAskReserve())
                {
                    queWaitForReserve.TryPeek(out MoveCmdInfo peek);    
                    if (middleHandler.GetReserveFromAgvc(peek.Section.Id))
                    {
                        theVehicle.UpdateStatus(peek);
                        moveControlHandler.DoTransfer(peek);
                        queWaitForReserve.TryDequeue(out MoveCmdInfo aMoveCmd);
                    }
                }
                Thread.Sleep(mainFlowConfigs.AskReserveInterval);
            }
        }

        private bool CanAskReserve()
        {
            return CanVehMove() && !IsReadyToMoveCmdQueFull() && IsWaitForReserveQueNotEmpty();
        }

        private bool IsWaitForReserveQueNotEmpty()
        {
            return !queWaitForReserve.IsEmpty;
        }

        public void Pause()
        {

        }

        public void Resume()
        {

        }

        public void Stop()
        {

        }

        public void UpdateMapBarcode(MapBarcodeValues mapBarcode)
        {
            theVehicle.UpdateStatus(mapBarcode);
        }

        public void OnMapBarcodeValuesChangedEvent(object sender, MapBarcodeValues mapBarcodeValues)
        {
            theVehicle.UpdateStatus(mapBarcodeValues);
        }
    }
}
