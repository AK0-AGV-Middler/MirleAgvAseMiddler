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
        public event EventHandler<List<TransCmd>> OnAgvcTransCmdGotEvent;
        private List<TransCmd> transCmds;
        private VehLocation vehLocation;

        public MiddleAgent()
        {
            transCmds = new List<TransCmd>();
            vehLocation = new VehLocation();
        }

        public void WhenAgvcTransCmdGot(AgvcTransCmd agvcTransCmd)
        {
            if (AgvcTransCmdPreCheck())
            {
                ConvertAgvcTransCmdIntoList(agvcTransCmd);
                if (OnAgvcTransCmdGotEvent != null)
                {
                    OnAgvcTransCmdGotEvent.Invoke(this, transCmds);
                }
            }            
        }

        private bool AgvcTransCmdPreCheck()
        {
            //根據命令內容、當前車輛位置、當前車輛電量等資訊判斷這筆命令是否可行
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
