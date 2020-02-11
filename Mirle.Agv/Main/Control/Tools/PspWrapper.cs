using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSDriver.PSDriver;
using Mirle.Agv.Model.Configs;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;

namespace Mirle.Agv.Controller.Tools
{
    public class PspWrapper
    {
        private PSWrapperXClass psWrapperXClass;
        private PspConnectionConfig pspConnectionConfig;

        private ConcurrentQueue<PSTransactionXClass> receiveMessageQueue = new ConcurrentQueue<PSTransactionXClass>();
        private ConcurrentQueue<PSMessageXClass> primarySendQueue = new ConcurrentQueue<PSMessageXClass>();
        private ConcurrentQueue<PSTransactionXClass> secondarySendQueue = new ConcurrentQueue<PSTransactionXClass>();
        private static ConcurrentDictionary<string, PSTransactionXClass> sendTransactionMap = new ConcurrentDictionary<string, PSTransactionXClass>();
        private static ConcurrentDictionary<string, PSTransactionXClass> receiveTransactionMap = new ConcurrentDictionary<string, PSTransactionXClass>();

        public EnumPspConnectionState EnumPspConnectionState { get; set; } = EnumPspConnectionState.Offline;

        public event EventHandler<EnumPspConnectionState> OnPspConnectStateChangeEvent;
        public event EventHandler<PSTransactionXClass> OnPrimarySentEvent;
        public event EventHandler<PSTransactionXClass> OnPrimaryReceivedEvent;
        public event EventHandler<PSTransactionXClass> OnSecondarySendEvent;
        public event EventHandler<PSTransactionXClass> OnSecondaryReceivedEvent;


        public PspWrapper(PspConnectionConfig pspConnectionConfig)
        {
            this.pspConnectionConfig = pspConnectionConfig;
            InitialWrapperXClass();
        }

        private void InitialWrapperXClass()
        {
            try
            {
                LoadAutoReplyXml();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void LoadAutoReplyXml()
        {

        }

        public void Open()
        {
            EnumPspConnectionState = EnumPspConnectionState.Offline;
            OnPspConnectStateChangeEvent?.Invoke(this, EnumPspConnectionState);
            WatchPspConnectionStatus();
        }

        private void WatchPspConnectionStatus()
        {
            try
            {
                PspConnect();

                Thread thdSchedule = new Thread(new ThreadStart(Schedule));
                thdSchedule.IsBackground = true;
                thdSchedule.Start();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void Schedule()
        {
            try
            {
                do
                {

                } while (EnumPspConnectionState.Offline != EnumPspConnectionState);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void PspConnect()
        {
            try
            {
                SetupPspWrapeer();
                BindEvents();
                psWrapperXClass.Open();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void BindEvents()
        {
            psWrapperXClass.OnPrimarySent += PspWrapper_OnPrimarySent;
            psWrapperXClass.OnPrimaryReceived += PspWrapper_OnPrimaryReceived;
            psWrapperXClass.OnSecondarySent += PspWrapper_OnSecondarySent;
            psWrapperXClass.OnSecondaryReceived += PspWrapper_OnSecondaryReceived;
            psWrapperXClass.OnTransactionError += PspWrapper_OnTransactionError;
            psWrapperXClass.OnConnectionStateChange += PspWrapper_OnConnectionStateChange;
        }

        private void PspWrapper_OnConnectionStateChange(enumConnectState state)
        {

            switch (state)
            {
                case enumConnectState.CheckConnectMode:
                    EnumPspConnectionState = EnumPspConnectionState.CheckCheckConnectMode;
                    break;
                case enumConnectState.ActiveWaitConnected:
                    EnumPspConnectionState = EnumPspConnectionState.Online;
                    break;
                case enumConnectState.PassiveWaitConnected:
                    EnumPspConnectionState = EnumPspConnectionState.Online;

                    break;
                case enumConnectState.Connected:
                    EnumPspConnectionState = EnumPspConnectionState.Online;

                    break;
                case enumConnectState.Quit:
                    EnumPspConnectionState = EnumPspConnectionState.Offline;
                    break;
                default:
                    EnumPspConnectionState = EnumPspConnectionState.Offline;
                    break;
            }

            OnPspConnectStateChangeEvent?.Invoke(this, EnumPspConnectionState);
        }

        private void PspWrapper_OnTransactionError(string errorString, ref PSMessageXClass msg)
        {
            throw new NotImplementedException();
        }

        private void PspWrapper_OnSecondaryReceived(ref PSTransactionXClass transaction)
        {
            receiveMessageQueue.Enqueue(transaction);
            OnSecondaryReceivedEvent?.Invoke(this, transaction);
        }

        private void PspWrapper_OnSecondarySent(ref PSTransactionXClass transaction)
        {
            OnSecondarySendEvent?.Invoke(this, transaction);
        }

        private void PspWrapper_OnPrimaryReceived(ref PSTransactionXClass transaction)
        {
            receiveMessageQueue.Enqueue(transaction);
            OnPrimaryReceivedEvent?.Invoke(this, transaction);
        }

        private void PspWrapper_OnPrimarySent(ref PSTransactionXClass transaction)
        {
            OnPrimarySentEvent?.Invoke(this, transaction);
        }

        private void SetupPspWrapeer()
        {
            try
            {
                psWrapperXClass.Address = pspConnectionConfig.Ip;
                psWrapperXClass.Port = pspConnectionConfig.Port;
                psWrapperXClass.ConnectMode = pspConnectionConfig.IsServer ? enumConnectMode.Passive : enumConnectMode.Active;

                string msg = "Setup PspConfig done.";
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void LogException(string source, string exMsg)
        {
            Mirle.Tools.MirleLogger.Instance.Log(new Mirle.Tools.LogFormat("Error", "5", source, "Device", "CarrierID", exMsg));
        }

        private void LogDebug(string classMethodName, string msg)
        {
            Mirle.Tools.MirleLogger.Instance.Log(new Mirle.Tools.LogFormat("Debug", "5", classMethodName, "Device", "CarrierID", msg));
        }
    }
}
