using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Model.TransferCmds;

namespace Mirle.Agv.Model
{
    public class Vehicle
    {
        private static Vehicle theVehicle;
        private static object theLock = new object();

        private Battery battery;
        private VehLocation vehLoacation;
        private Dictionary<string, Carrier> dicCarriersById;
        private Dictionary<int, Carrier> dicCarriersByStageNum;
        private PlcRobot PlcRobot;
        private TransCmd transCmd;


        public Vehicle()
        {
            battery = new Battery(50, 100);   //50,100 can config
            vehLoacation = new VehLocation();
            dicCarriersById = new Dictionary<string, Carrier>();
            dicCarriersByStageNum = new Dictionary<int, Carrier>();
        }

        public static Vehicle GetInstance()
        {
            if (theVehicle == null)
            {
                lock (theLock)
                {
                    if (theVehicle == null)
                    {
                        theVehicle = new Vehicle();
                    }
                }
            }

            return theVehicle;
        }

        #region Setter

        public void UpdateStatus(Battery battery)
        {
            this.battery = battery;
        }

        public void UpdateStatus(VehLocation vehLoacation)
        {
            this.vehLoacation = vehLoacation;
        }

        public void UpdateStatus(MapBarcodeValues mapBarcode)
        {
            this.vehLoacation.SetMapBarcodeValues(mapBarcode);
        }

        public void UpdateStatus(TransCmd  transCmd)
        {
            this.transCmd = transCmd;
        }

        public void AddCarrier(Carrier aCarrier)
        {
            Carrier tempCarrier = aCarrier.Clone();
            dicCarriersById.Add(tempCarrier.GetId(), tempCarrier);
            dicCarriersByStageNum.Add(tempCarrier.GetStageNum(), tempCarrier);
        }

        #endregion

        #region Getter

        public Battery GetBattery()
        {
            return this.battery;
        }

        public VehLocation GetVehLoacation()
        {
            return this.vehLoacation;
        }

        public TransCmd GetTransCmd()
        {
            return transCmd;
        }

        public Carrier GetCarrierById(string id)
        {
            try
            {
                if (dicCarriersById.ContainsKey(id))
                {
                    return dicCarriersById[id];
                }
                return new Carrier();
            }
            catch (Exception ex)
            {
                //log ex
                return new Carrier();
            }
        }

        public Carrier GetCarrierByStageNum(int stagenum)
        {
            try
            {
                if (dicCarriersByStageNum.ContainsKey(stagenum))
                {
                    return dicCarriersByStageNum[stagenum];
                }
                return new Carrier();
            }
            catch (Exception ex)
            {
                //log ex
                return new Carrier();
            }
        }

        #endregion
    }
}
