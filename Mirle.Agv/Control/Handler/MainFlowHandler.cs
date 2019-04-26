﻿using Mirle.Agv.Control.Tools;
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
    public class MainFlowHandler : IMapBarcodeValuesEvent, ICmdFinished
    {
        #region Configs

        private string configPath;
        private ConfigHandler configHandler;
        private MiddlerConfigs middlerConfigs;
        private Sr2000Configs sr2000Configs;
        private MainFlowConfigs mainFlowConfigs;
        private MapConfigs mapConfigs;
        private MoveControlConfigs moveControlConfigs;

        #endregion

        private List<TransCmd> transCmds;
        private bool goNextTransCmd;
        private ConcurrentQueue<MoveCmdInfo> queWaitForReserve;

        #region Agent

        private BmsAgent bmsAgent;
        private ElmoAgent elmoAgent;
        private MiddleAgent middleAgent;
        private PlcAgent plcAgent;
        private LoggerAgent loggerAgent;

        #endregion

        #region Handler

        private BatteryHandler batteryHandler;
        private CoupleHandler coupleHandler;
        private MapHandler mapHandler;
        private MoveControlHandler moveControlHandler;
        private RobotControlHandler robotControlHandler;

        #endregion

        public Vehicle theVehicle;

        public MainFlowHandler()
        {
            ConfigsInitial();
            LoggersInitial();

            AgentInitial();
            HandlerInitial();

            transCmds = new List<TransCmd>();
            queWaitForReserve = new ConcurrentQueue<MoveCmdInfo>();

            VehicleInitial();

            EventInitial();

            RunThreads();
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

            sr2000Configs = new Sr2000Configs();
            int.TryParse(configHandler.GetString("Sr2000", "TrackingInterval", "10"), out int tempTrackingInterval);
            sr2000Configs.TrackingInterval = tempTrackingInterval;

            moveControlConfigs = new MoveControlConfigs();
        }

        private void LoggersInitial()
        {
            //TODO : make abstract class with an logger and its bean and a function do log, make 4 level subclass imp this abstract class
            loggerAgent = LoggerAgent.Instance;
        }

        private void AgentInitial()
        {
            bmsAgent = new BmsAgent();
            elmoAgent = new ElmoAgent();
            middleAgent = new MiddleAgent();
            plcAgent = new PlcAgent();
        }

        private void HandlerInitial()
        {
            batteryHandler = new BatteryHandler();
            coupleHandler = new CoupleHandler();
            mapHandler = new MapHandler(mapConfigs.SectionFilePath, mapConfigs.AddressFilePath);
            moveControlHandler = new MoveControlHandler(moveControlConfigs, sr2000Configs);
            robotControlHandler = new RobotControlHandler();
        }

        private void VehicleInitial()
        {
            theVehicle = Vehicle.GetInstance();
        }

        private void EventInitial()
        {
            //來自MoveControl的Barcode更新訊息，通知MainFlow(this)'middleAgent'mapHandler
            moveControlHandler.sr2000Agent.OnMapBarcodeValuesChange += OnMapBarcodeValuesChangedEvent;
            moveControlHandler.sr2000Agent.OnMapBarcodeValuesChange += middleAgent.OnMapBarcodeValuesChangedEvent;
            moveControlHandler.sr2000Agent.OnMapBarcodeValuesChange += mapHandler.OnMapBarcodeValuesChangedEvent;
            moveControlHandler.sr2000Agent.OnMapBarcodeValuesChange += moveControlHandler.OnMapBarcodeValuesChangedEvent;


            //來自MoveControl的移動結束訊息，通知MainFlow(this)'middleAgent'mapHandler
            moveControlHandler.OnMoveFinished += OnTransCmdsFinishedEvent;
            moveControlHandler.OnMoveFinished += middleAgent.OnTransCmdsFinishedEvent;
            moveControlHandler.OnMoveFinished += mapHandler.OnTransCmdsFinishedEvent;

            //來自RobotControl的取貨結束訊息，通知MainFlow(this)'middleAgent'mapHandler
            robotControlHandler.OnLoadFinished += OnTransCmdsFinishedEvent;
            robotControlHandler.OnLoadFinished += middleAgent.OnTransCmdsFinishedEvent;
            robotControlHandler.OnLoadFinished += mapHandler.OnTransCmdsFinishedEvent;

            //來自RobotControl的放貨結束訊息，通知MainFlow(this)'middleAgent'mapHandler
            robotControlHandler.OnUnloadFinished += OnTransCmdsFinishedEvent;
            robotControlHandler.OnUnloadFinished += middleAgent.OnTransCmdsFinishedEvent;
            robotControlHandler.OnUnloadFinished += mapHandler.OnTransCmdsFinishedEvent;
        }

        private void RunThreads()
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
                string className = GetType().Name;
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                string classMethodName = className + ":" + methodName;
                LogFormat logFormat = new LogFormat("Error", "1", classMethodName, "Device", "CarrierID", ex.StackTrace);
                loggerAgent.LogError(logFormat);
            }
        }

        private void OnAgvcTransCmdGotEvent()
        {
            transCmds = middleAgent.GetTransCmds();
            middleAgent.ClearTransCmds();
            DoTransCmds();
        }

        private void TransCmdsCheck()
        {
            while (middleAgent.IsTransCmds())
            {
                transCmds = middleAgent.GetTransCmds();
                middleAgent.ClearTransCmds();
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
                    if (middleAgent.GetReserveFromAgvc(peek.Section.Id))
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

        public void UpdateMapBarcode(MapBarcodeReader mapBarcode)
        {
            theVehicle.UpdateStatus(mapBarcode);
        }

        public void OnMapBarcodeValuesChangedEvent(object sender, MapBarcodeReader mapBarcodeValues)
        {
            theVehicle.UpdateStatus(mapBarcodeValues);
        }

        public void OnTransCmdsFinishedEvent(object sender, EnumCompleteStatus status)
        {
            throw new NotImplementedException();
        }

    }
}
