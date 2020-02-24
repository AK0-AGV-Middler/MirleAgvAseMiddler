﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSDriver.PSDriver;
using Mirle.Tools;
using System.Reflection;
using Mirle.AgvAseMiddler.Model.Configs;
using System.IO;
using Mirle.AgvAseMiddler.Model;
using Mirle.AgvAseMiddler.Model.TransferSteps;

namespace Mirle.AgvAseMiddler.Controller
{
    public class AsePackage
    {
        public PSWrapperXClass psWrapper;
        public AseMoveControl aseMoveControl;
        public AseRobotControl aseRobotControl;
        public AseBatteryControl aseBatteryControl;
        public AseBuzzerControl aseBuzzerControl;
        private MirleLogger mirleLogger = MirleLogger.Instance;
        private PspConnectionConfig pspConnectionConfig = new PspConnectionConfig();
        public string PspConnectionConfigFilePath { get; set; } = "PspConnectionConfig.xml";
        public string AutoReplyFilePath { get; set; } = "AutoReply.csv";
        private Vehicle theVehicle = Vehicle.Instance;
        private Dictionary<string, PSMessageXClass> psMessageMap = new Dictionary<string, PSMessageXClass>();

        public event EventHandler<string> OnMessageShowEvent;
        public event EventHandler<bool> OnConnectionChangeEvent;

        public AsePackage(Dictionary<string, string> portNumberMap)
        {
            InitialWrapper();
            aseMoveControl = new AseMoveControl(psWrapper);
            aseRobotControl = new AseRobotControl(psWrapper, portNumberMap);
            aseBatteryControl = new AseBatteryControl(psWrapper);
            aseBuzzerControl = new AseBuzzerControl(psWrapper);
        }

        private void InitialWrapper()
        {
            try
            {
                LoadAutoReply();
                LoadPspConnectionConfig();
                BindPsWrapperEvent();

                if (!theVehicle.IsSimulation)
                {
                    psWrapper.Open();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void BindPsWrapperEvent()
        {
            try
            {
                psWrapper.OnConnectionStateChange += PsWrapper_OnConnectionStateChange;
                psWrapper.OnPrimarySent += PsWrapper_OnPrimarySent;
                psWrapper.OnPrimaryReceived += PsWrapper_OnPrimaryReceived;
                psWrapper.OnSecondarySent += PsWrapper_OnSecondarySent;
                psWrapper.OnSecondaryReceived += PsWrapper_OnSecondaryReceived;
                psWrapper.OnTransactionError += PsWrapper_OnTransactionError;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void PsWrapper_OnTransactionError(string errorString, ref PSMessageXClass psMessage)
        {
            LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, psMessage.ToString());
        }

        private void PsWrapper_OnSecondarySent(ref PSTransactionXClass transaction)
        {
            try
            {
                string msg = $"SecodarySent : [{transaction.ToString()}]";
                LogPsWrapper(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void PsWrapper_OnSecondaryReceived(ref PSTransactionXClass transaction)
        {
            try
            {
                string msg = $"SecodaryReceived : [{transaction.ToString()}]";
                LogPsWrapper(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);

                OnSecondaryReceived(transaction);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void OnSecondaryReceived(PSTransactionXClass transaction)
        {
            try
            {
                if (transaction.PSSecondaryMessage == null)
                {
                    throw new Exception("Secondary message is empty.");
                }

                if (transaction.PSSecondaryMessage.Type.ToUpper() != "S")
                {
                    throw new Exception("Secondary message type is not S.");
                }


                switch (transaction.PSSecondaryMessage.Number)
                {
                    case "34":
                        ReceivePositionReportRequestAck(transaction.PSSecondaryMessage.PSMessage);
                        break;
                    case "36":
                        ReceiveBatteryStatusRequestAck(transaction.PSSecondaryMessage.PSMessage);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void ReceiveBatteryStatusRequestAck(string psMessage)
        {
            try
            {
                AseBatteryStatus aseBatteryStatus = new AseBatteryStatus(theVehicle.AseBatteryStatus);
                aseBatteryStatus.Ah = GetAhFromPsMessage(psMessage.Substring(0, 9));
                aseBatteryStatus.Voltage = double.Parse(psMessage.Substring(9, 4)) * 0.01;
                aseBatteryStatus.Temperature = double.Parse(psMessage.Substring(13, 3));
                theVehicle.AseBatteryStatus = aseBatteryStatus;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private double GetAhFromPsMessage(string v)
        {
            string isPositive = v.Substring(0, 1);
            double value = double.Parse(v.Substring(1, 8)) * 0.01;
            return IsValuePositive(isPositive) ? value : -value;
        }

        private void ReceivePositionReportRequestAck(string psMessage)
        {
            try
            {
                AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
                aseMoveStatus.AseMoveState = (EnumAseMoveState)Enum.Parse(typeof(EnumAseMoveState), psMessage.Substring(0, 1));
                aseMoveStatus.LastMapPosition.X = GetPositionFromPsMessage(psMessage.Substring(1, 9));
                aseMoveStatus.LastMapPosition.Y = GetPositionFromPsMessage(psMessage.Substring(10, 18));
                aseMoveStatus.HeadDirection = int.Parse(psMessage.Substring(19, 3));
                aseMoveStatus.MovingDirection = int.Parse(psMessage.Substring(22, 3));
                aseMoveStatus.Speed = int.Parse(psMessage.Substring(25, 4));
                theVehicle.AseMoveStatus = aseMoveStatus;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private double GetPositionFromPsMessage(string v)
        {
            string isPositive = v.Substring(0, 1);
            double value = double.Parse(v.Substring(1, 8));
            return IsValuePositive(isPositive) ? value : -value;
        }

        private bool IsValuePositive(string v)
        {
            switch (v)
            {
                case "P":
                    return true;
                case "N":
                    return false;
                default:
                    throw new Exception($"字串無法辨別正負.{v}");
            }
        }

        private void PsWrapper_OnPrimaryReceived(ref PSTransactionXClass transaction)
        {
            try
            {
                string msg = $"PrimaryReceived : [{transaction.PSPrimaryMessage.ToString()}]";
                LogPsWrapper(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);

                OnPrimaryReceived(transaction);
                AutoReplyFromPsMessageMap(transaction);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        internal void Mainflow_OnTransferCommandCheckedEvent(object sender, AgvcTransCmd e)
        {
            throw new NotImplementedException();
        }

        private void OnPrimaryReceived(PSTransactionXClass transaction)
        {
            try
            {
                if (transaction.PSPrimaryMessage == null)
                {
                    throw new Exception("Primary message is empty.");
                }

                if (transaction.PSPrimaryMessage.Type.ToUpper() != "P")
                {
                    throw new Exception("Primary message type is not P.");
                }

                switch (transaction.PSPrimaryMessage.Number)
                {
                    case "11":
                        SetVehicleAuto();
                        break;
                    case "13":
                        SetVehicleManual();
                        break;
                    case "21":
                        UpdateMoveStatus(transaction.PSPrimaryMessage.PSMessage);
                        break;
                    case "23":
                        UpdateRobotStatus(transaction.PSPrimaryMessage.PSMessage);
                        break;
                    case "25":
                        UpdateCarrierSlotStatus(transaction.PSPrimaryMessage.PSMessage);
                        break;
                    case "29":
                        UpdateChargeStatus(transaction.PSPrimaryMessage.PSMessage);
                        break;
                    case "43":
                        ReceiveMoveAppendArrivalReport(transaction.PSPrimaryMessage.PSMessage);
                        break;
                    case "61":
                        AlarmReport(transaction.PSPrimaryMessage.PSMessage);
                        break;
                    case "63":
                        AllAlarmReset();
                        break;
                    case "97":
                        ShowTestMsg();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void ShowTestMsg()
        {
            OnMessageShowEvent?.Invoke(this, "A test msg from AGVL.");
        }

        private void ReceiveMoveAppendArrivalReport(string psMessage)
        {
            try
            {
                EnumAseArrival aseArrival = (EnumAseArrival)Enum.Parse(typeof(EnumAseArrival), psMessage.Substring(0, 1));
                switch (aseArrival)
                {
                    case EnumAseArrival.Fail:
                        aseMoveControl.OnMoveFinish(EnumMoveComplete.Fail);
                        break;
                    case EnumAseArrival.Arrival:
                        ArrivalPosition(psMessage);
                        break;
                    case EnumAseArrival.EndArrival:
                        aseMoveControl.OnMoveFinish(EnumMoveComplete.Success);
                        break;
                    default:
                        break;
                }

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void ArrivalPosition(string psMessage)
        {
            try
            {
                AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
                aseMoveStatus.LastMapPosition.X = GetPositionFromPsMessage(psMessage.Substring(1, 9));
                aseMoveStatus.LastMapPosition.Y = GetPositionFromPsMessage(psMessage.Substring(10, 18));
                aseMoveStatus.HeadDirection = int.Parse(psMessage.Substring(19, 3));
                aseMoveStatus.MovingDirection = int.Parse(psMessage.Substring(22, 3));
                aseMoveStatus.Speed = int.Parse(psMessage.Substring(25, 4));
                theVehicle.AseMoveStatus = aseMoveStatus;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void AllAlarmReset()
        {
            try
            {
                aseBuzzerControl.OnAlarmCodeAllReset();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void AlarmReport(string psMessage)
        {
            try
            {
                string isAlarmSet = psMessage.Substring(0, 1).ToUpper().Trim();
                int alarmCode = int.Parse(psMessage.Substring(1, 6));

                aseBuzzerControl.OnAlarmCodeSet(alarmCode, IsValueTrue(isAlarmSet));
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void UpdateChargeStatus(string psMessage)
        {
            try
            {
                string isCharging = psMessage.Substring(0, 1).ToUpper().Trim();
                theVehicle.IsCharging = IsValueTrue(isCharging);

                string msg = $"充電狀態改變[{isCharging}]";
                OnMessageShowEvent?.Invoke(this, msg);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private bool IsValueTrue(string v)
        {
            switch (v)
            {
                case "1":
                    return true;
                case "0":
                    return false;
                default:
                    throw new Exception($"字串無法辨別真假.{v}");
            }
        }

        private void UpdateCarrierSlotStatus(string psMessage)
        {
            try
            {
                AseCarrierSlotStatus aseCarrierSlotStatus = new AseCarrierSlotStatus();
                aseCarrierSlotStatus.CarrierSlotStatus = (EnumAseCarrierSlotStatus)Enum.Parse(typeof(EnumAseCarrierSlotStatus), psMessage.Substring(1, 1));
                aseCarrierSlotStatus.CarrierId = psMessage.Substring(2);
                EnumSlotNumber slotNumber = (EnumSlotNumber)Enum.Parse(typeof(EnumSlotNumber), psMessage.Substring(0, 1));
                switch (slotNumber)
                {
                    case EnumSlotNumber.A:
                        theVehicle.AseCarrierSlotA = aseCarrierSlotStatus;
                        break;
                    case EnumSlotNumber.B:
                        theVehicle.AseCarrierSlotB = aseCarrierSlotStatus;
                        break;
                    default:
                        throw new Exception($"PsMessage slot number error.[{psMessage.Substring(0, 1)}]");
                }

                aseRobotControl.OnReadCarrierIdFinish(slotNumber);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void UpdateRobotStatus(string psMessage)
        {
            try
            {
                AseRobotStatus aseRobotStatus = new AseRobotStatus();
                aseRobotStatus.RobotState = (EnumAseRobotState)Enum.Parse(typeof(EnumAseRobotState), psMessage.Substring(0, 1));
                aseRobotStatus.IsHome = IsValueTrue(psMessage.Substring(1, 1));

                if (theVehicle.AseRobotStatus.RobotState == EnumAseRobotState.Idle && aseRobotStatus.RobotState == EnumAseRobotState.Error)
                {
                    theVehicle.AseRobotStatus = aseRobotStatus;
                    aseRobotControl.OnRobotInterlockError();
                }
                else if (theVehicle.AseRobotStatus.RobotState == EnumAseRobotState.Busy && aseRobotStatus.RobotState == EnumAseRobotState.Error)
                {
                    theVehicle.AseRobotStatus = aseRobotStatus;
                    aseRobotControl.OnRobotCommandError();
                }
                else if (theVehicle.AseRobotStatus.RobotState == EnumAseRobotState.Busy && aseRobotStatus.RobotState == EnumAseRobotState.Idle)
                {
                    theVehicle.AseRobotStatus = aseRobotStatus;
                    aseRobotControl.OnRobotCommandFinish();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void UpdateMoveStatus(string psMessage)
        {
            try
            {
                AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
                aseMoveStatus.AseMoveState = (EnumAseMoveState)Enum.Parse(typeof(EnumAseMoveState), psMessage.Substring(0, 1));
                aseMoveStatus.HeadDirection = int.Parse(psMessage.Substring(1, 3));
                theVehicle.AseMoveStatus = aseMoveStatus;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void SetVehicleManual()
        {
            theVehicle.AutoState = EnumAutoState.Manual;
            string msg = "車輛狀態切換 : Manual";
            OnMessageShowEvent?.Invoke(this, msg);
        }

        private void SetVehicleAuto()
        {
            theVehicle.AutoState = EnumAutoState.Auto;
            string msg = "車輛狀態切換 : Auto";
            OnMessageShowEvent?.Invoke(this, msg);
        }

        private void AutoReplyFromPsMessageMap(PSTransactionXClass psTransaction)
        {
            try
            {
                if (!IsPrimaryEmpty(psTransaction))
                {
                    if (IsPrimaryInMessageMap(psTransaction))
                    {
                        ReplyFromMsgMap(psTransaction);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void ReplyFromMsgMap(PSTransactionXClass psTransaction)
        {
            try
            {
                PSMessageXClass primaryMessage = psTransaction.PSPrimaryMessage;
                int primaryNumber = int.Parse(primaryMessage.Number);
                string secondaryTypeNumber = "S" + (primaryNumber + 1).ToString("00");

                if (psMessageMap.ContainsKey(secondaryTypeNumber))
                {
                    psTransaction.PSSecondaryMessage = psMessageMap[secondaryTypeNumber];
                    psWrapper.SecondarySent(ref psTransaction);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private bool IsPrimaryInMessageMap(PSTransactionXClass psTransaction)
        {
            try
            {
                if (psMessageMap.Count <= 0)
                {
                    throw new Exception("PsMessageMap is empty");
                }

                PSMessageXClass primaryMessage = psTransaction.PSPrimaryMessage;
                if (primaryMessage.Type.ToUpper() != "P")
                {
                    throw new Exception($"PrimaryReceive transaction.primaryMessage.type is not P.[{primaryMessage.Type}]");
                }

                return psMessageMap.ContainsKey(primaryMessage.Type + primaryMessage.Number);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return false;
            }
        }

        private bool IsPrimaryEmpty(PSTransactionXClass psTransaction)
        {
            return psTransaction.PSPrimaryMessage == null;
        }

        private void PsWrapper_OnPrimarySent(ref PSTransactionXClass transaction)
        {
            try
            {
                string msg = $"PrimarySent : [{transaction.PSPrimaryMessage.ToString()}]";
                LogPsWrapper(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void PsWrapper_OnConnectionStateChange(enumConnectState state)
        {
            try
            {
                string msg = $"PsWrapper connection state changed.[{state}]";
                LogPsWrapper(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
                OnMessageShowEvent?.Invoke(this, msg);
                OnConnectionChangeEvent?.Invoke(this, psWrapper.IsConnected());
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }        

        private void LoadPspConnectionConfig()
        {
            try
            {
                psWrapper = new PSWrapperXClass();
                if (File.Exists(PspConnectionConfigFilePath))
                {
                    pspConnectionConfig = new XmlHandler().ReadXml<PspConnectionConfig>(PspConnectionConfigFilePath);
                }
                psWrapper.Address = pspConnectionConfig.Ip;
                psWrapper.Port = pspConnectionConfig.Port;
                psWrapper.ConnectMode = pspConnectionConfig.IsServer ? enumConnectMode.Passive : enumConnectMode.Active;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void LoadAutoReply()
        {
            try
            {
                if (File.Exists(AutoReplyFilePath))
                {
                    LoadAutoReplyFileToMyMessageMap();
                    //FitPspMessageList();
                    //InitialSingleMsgType();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void LoadAutoReplyFileToMyMessageMap()
        {
            try
            {
                string[] allRows = File.ReadAllLines(AutoReplyFilePath);

                foreach (string oneRow in allRows)
                {
                    var parts = oneRow.Split(',');
                    string key = parts[0];
                    string description = parts[1];
                    string autoMessage = parts[2];

                    PSMessageXClass psMessage = new PSMessageXClass();
                    psMessage.Type = key.Substring(0, 1);
                    psMessage.Number = key.Substring(1, 2);
                    psMessage.Description = description;
                    psMessage.PSMessage = autoMessage;

                    psMessageMap.Add(key, psMessage);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void InstallTransferCommand(AgvcTransCmd agvcTransCmd)
        {
            try
            {
                string cmdId = agvcTransCmd.CommandId.Substring(0, 20);
                string fromPortNumber = agvcTransCmd.LoadAddressId.Substring(0, 4);
                string toPortNumber = agvcTransCmd.UnloadAddressId.Substring(0, 4);
                string lotId = agvcTransCmd.LotId.PadLeft(39, '0');
                string cstId = agvcTransCmd.CassetteId;

                string message = string.Concat(cmdId, fromPortNumber, toPortNumber, lotId, cstId);
                PrimarySend("P37", message);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private PSTransactionXClass PrimarySend(string index, string message)
        {
            try
            {
                PSMessageXClass psMessage = new PSMessageXClass();
                psMessage.Type = index.Substring(0, 1);
                psMessage.Number = index.Substring(1, 2);
                psMessage.PSMessage = message;
                PSTransactionXClass psTransaction = new PSTransactionXClass();
                psTransaction.PSPrimaryMessage = psMessage;

                psWrapper.PrimarySent(ref psTransaction);
                return psTransaction;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return null;
            }
        }

        private void LogException(string classMethodName, string exMsg)
        {
            mirleLogger.Log(new LogFormat("Error", "5", classMethodName, "Device", "CarrierID", exMsg));
        }

        private void LogDebug(string classMethodName, string msg)
        {
            mirleLogger.Log(new LogFormat("Debug", "5", classMethodName, "Device", "CarrierID", msg));
        }

        private void LogPsWrapper(string classMethodName, string msg)
        {
            mirleLogger.Log(new LogFormat("PsWrapper", "5", classMethodName, "Device", "CarrierID", msg));
        }

        public bool IsConnected()
        {
            return psWrapper.IsConnected();
        }

        public void Connect()
        {
            psWrapper.Open();
        }

        public void DisConnect()
        {
            psWrapper.Close();
        }
    }
}