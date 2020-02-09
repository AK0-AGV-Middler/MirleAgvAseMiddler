using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public abstract class VehicleIntegrateStatus
    {
        public bool Loading { get; set; }
        public string CarrierId { get; set; } = "";
        public string FakeCarrierId { get; set; } = "";
        public bool RobotHome { get; set; } = true;

        public Batterys Batterys { get; set; } = null;
    }

    public abstract class Batterys
    {
        public double Percentage { get; protected set; }//剩餘電量s分比
        public double PortAutoChargeLowSoc { get; set; }
        public double PortAutoChargeHighSoc { get; set; }
        public bool Charging { get; set; }
        public double BatteryTemperature { get; set; }
        public double MeterVoltage { get; set; }
    }

    public class VehicleIntegrateStatusFactory
    {
        public VehicleIntegrateStatus GetVehicleIntegrateStatus(string type)
        {
            VehicleIntegrateStatus vehicleIntegrateStatus = null;

            if (type == "AUO")
            {
                vehicleIntegrateStatus = new PlcVehicle();
            }
            else if (type == "ASE")
            {
                vehicleIntegrateStatus = new AseVehicleIntegrateStatus();
            }           

            return vehicleIntegrateStatus;
        }
    }
}
