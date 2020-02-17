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
        private PSWrapperXClass psWrapperXClass = new PSWrapperXClass();
        private PspConnectionConfig pspConnectionConfig;

        private Thread thdSchedule;

        private ConcurrentQueue<PSTransactionXClass> receiveMessageQueue = new ConcurrentQueue<PSTransactionXClass>();
        private ConcurrentQueue<PSMessageXClass> primarySendQueue = new ConcurrentQueue<PSMessageXClass>();
        private ConcurrentQueue<PSTransactionXClass> secondarySendQueue = new ConcurrentQueue<PSTransactionXClass>();
        private Dictionary<uint, PSTransactionXClass> psTransactionXClassMap = new Dictionary<uint, PSTransactionXClass>();

        public event EventHandler<enumConnectState> OnPspConnectStateChangeEvent;
        public event EventHandler<PSTransactionXClass> OnPrimarySentEvent;
        public event EventHandler<PSTransactionXClass> OnPrimaryReceivedEvent;
        public event EventHandler<PSTransactionXClass> OnSecondarySendEvent;
        public event EventHandler<PSTransactionXClass> OnSecondaryReceivedEvent;


        public PspWrapper(PspConnectionConfig pspConnectionConfig)
        {
            this.pspConnectionConfig = pspConnectionConfig;

            InitialWrapperXClass();

            InitialThread();
        }

        private void InitialThread()
        {
            try
            {
                thdSchedule = new Thread(new ThreadStart(Schedule));
                thdSchedule.IsBackground = true;
                thdSchedule.Start();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void InitialWrapperXClass()
        {
            try
            {
                LoadAutoReplyXml();
                SetupWrapperXClass();
                BindEvents();
                psWrapperXClass.Open();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void SetupWrapperXClass()
        {
            try
            {
                psWrapperXClass.Address = pspConnectionConfig.Ip;
                psWrapperXClass.Port = pspConnectionConfig.Port;
                psWrapperXClass.ConnectMode = pspConnectionConfig.IsServer ? enumConnectMode.Passive : enumConnectMode.Active;

                string msg = "PspWrapperXClass參數設定完成";
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void LoadAutoReplyXml()
        {

        }

        private void Schedule()
        {
            try
            {
                do
                {
                    if (primarySendQueue.Count > 0)
                    {
                        if (primarySendQueue.TryDequeue(out PSMessageXClass psMessageXClass))
                        {
                            PSTransactionXClass psTransaction = new PSTransactionXClass();
                            psTransaction.PSPrimaryMessage = psMessageXClass;
                            psTransactionXClassMap.Add(psMessageXClass.SystemBytes, psTransaction);
                            psWrapperXClass.PrimarySent(ref psTransaction);                            
                        }
                    }

                    if (secondarySendQueue.Count > 0)
                    {
                        if (secondarySendQueue.TryDequeue(out PSTransactionXClass psTransaction))
                        {
                            uint systemByte = psTransaction.PSPrimaryMessage.SystemBytes;
                            if (psTransactionXClassMap.ContainsKey(systemByte))
                            {
                                psTransactionXClassMap[systemByte] = psTransaction;
                                psWrapperXClass.SecondarySent(ref psTransaction);                                
                            }
                            else
                            {
                                string exMsg = $"Can not found transaction in map.[{systemByte}]";
                                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, exMsg);
                            }
                        }
                    }

                    if (receiveMessageQueue.Count > 0)
                    {
                        if (receiveMessageQueue.TryDequeue(out PSTransactionXClass psTransaction))
                        {
                            ParseTransaction(psTransaction);
                        }
                    }

                    System.Threading.SpinWait.SpinUntil(() => false, 50);
                } while (psWrapperXClass.ConnectionState != enumConnectState.Quit);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void ParseTransaction(PSTransactionXClass psTransaction)
        {
            try
            {

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void BindEvents()
        {
            try
            {
                psWrapperXClass.OnPrimarySent += PspWrapper_OnPrimarySent;
                psWrapperXClass.OnPrimaryReceived += PspWrapper_OnPrimaryReceived;
                psWrapperXClass.OnSecondarySent += PspWrapper_OnSecondarySent;
                psWrapperXClass.OnSecondaryReceived += PspWrapper_OnSecondaryReceived;
                psWrapperXClass.OnTransactionError += PspWrapper_OnTransactionError;
                psWrapperXClass.OnConnectionStateChange += PspWrapper_OnConnectionStateChange;

                string msg = "PspWrapperXClass事件綁定";
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void PspWrapper_OnConnectionStateChange(enumConnectState state)
        {
            OnPspConnectStateChangeEvent?.Invoke(this, state);
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

                string msg = "設定";
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void PrimarySendEnqueue(string source, PSMessageXClass psMessageXClass)
        {
            try
            {
                primarySendQueue.Enqueue(psMessageXClass);
                LogPspMessage(source, "PSEND", psMessageXClass);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void SecondarySendEnqueue(string source, PSTransactionXClass psTransaction)
        {
            try
            {
                secondarySendQueue.Enqueue(psTransaction);
                LogPspMessage(source, "SSEND", psTransaction.PSSecondaryMessage);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void LogPspMessage(string source, string psType, PSMessageXClass psMessageXClass)
        {
            try
            {
                string msg = $"[{psType}] [{psMessageXClass.Type}{psMessageXClass.Number}][{psMessageXClass.SystemBytes}][{psMessageXClass.PSMessage}]";

                Mirle.Tools.MirleLogger.Instance.Log(new Mirle.Tools.LogFormat("PspWrapper", "5", source, "Device", "CarrierID", msg));
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

        private void LogDebug(string source, string msg)
        {
            Mirle.Tools.MirleLogger.Instance.Log(new Mirle.Tools.LogFormat("Debug", "5", source, "Device", "CarrierID", msg));
        }

        private void LogPspWrapper(string source, string msg)
        {
            Mirle.Tools.MirleLogger.Instance.Log(new Mirle.Tools.LogFormat("PspWrapper", "5", source, "Device", "CarrierID", msg));
        }
    }
}
