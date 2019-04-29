using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Model;
using Mirle.Agv.Control.Tools;
using Mirle.Agv.Model.TransferCmds;

namespace Mirle.Agv.Control
{
    public class MiddleAgent : IMapBarcodeValuesEvent
    {
        public event EventHandler<List<TransCmd>> OnMiddlerGetsNewTransCmdsEvent;
        
        private List<TransCmd> transCmds;
        private VehLocation vehLocation;
        private LoggerAgent loggerAgent;

        public MiddleAgent()
        {
            transCmds = new List<TransCmd>();
            vehLocation = new VehLocation();
            loggerAgent = LoggerAgent.Instance;
        }

        public void WhenAgvcTransCmdGot(AgvcTransCmd agvcTransCmd)
        {
            if (CanVehDoTransfer())
            {
                ConvertAgvcTransCmdIntoList(agvcTransCmd);
                if (OnMiddlerGetsNewTransCmdsEvent != null)
                {
                    OnMiddlerGetsNewTransCmdsEvent.Invoke(this, transCmds);
                }
            }
        }

        private bool CanVehDoTransfer()
        {
            throw new NotImplementedException();
        }

        private void ConvertAgvcTransCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            //解析收到的AgvcTransCmd並且填入TransCmds(list)
            throw new NotImplementedException();
        }

        public bool GetReserveFromAgvc(string sectionId)
        {
            throw new NotImplementedException();
        }

        public void ClearTransCmds()
        {
            transCmds.Clear();
        }

        public bool IsTransCmds()
        {
            return transCmds.Count > 0;
        }

        public List<TransCmd> GetTransCmds()
        {
            List<TransCmd> tempTransCmds = transCmds.ToList();
            return tempTransCmds;
        }

        public void OnMapBarcodeValuesChangedEvent(object sender, MapBarcodeReader mapBarcodeValues)
        {            
            vehLocation.SetMapBarcodeValues(mapBarcodeValues);
            //TODO: Make a Position change report from mapBarcode and send to AGVC
        }

        public void OnTransCmdsFinishedEvent(object sender, EnumCompleteStatus status)
        {
            //Send Transfer Command Complete Report to Agvc
            throw new NotImplementedException();
        }
    }
}
