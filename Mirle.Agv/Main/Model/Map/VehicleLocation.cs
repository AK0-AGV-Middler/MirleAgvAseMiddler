
using System;

namespace Mirle.Agv.Model
{
    [Serializable]
    public class VehicleLocation
    {
        public MapSection LastSection { get; set; } = new MapSection();
        public MapAddress LastAddress { get; set; } = new MapAddress();
        public MapPosition BarcodePosition { get; set; } = new MapPosition();        
        public double Speed { get; set; }
        public double MoveDirectionAngle { get; set; }
        public MapPosition AgvcPosition { get; set; } = new MapPosition();
        public MapPosition RealPosition { get; set; } = new MapPosition();
        public double VehicleAngle { get; set; } = 0;
        public int WheelAngle { get; set; } = 0;
        public int RealPositionRangeMm { get; set; } = 15;
        public MapAddress NeerlyAddress { get; set; } = new MapAddress();
        public EnumVehicleLocation WhereAmI { get; set; } = EnumVehicleLocation.None;

        public VehicleLocation(VehicleLocation vehicleLocation)
        {
            LastSection = vehicleLocation.LastSection;
            LastAddress = vehicleLocation.LastAddress;
            BarcodePosition = vehicleLocation.BarcodePosition;
            RealPosition = vehicleLocation.RealPosition;
            VehicleAngle = vehicleLocation.VehicleAngle;
            WheelAngle = vehicleLocation.WheelAngle;
            RealPositionRangeMm = vehicleLocation.RealPositionRangeMm;
            NeerlyAddress = vehicleLocation.NeerlyAddress;
            WhereAmI = vehicleLocation.WhereAmI;
        }

        public VehicleLocation()
        { }

        //public MapPosition RealPosition
        //{
        //    get
        //    {
        //        return realPosition;                
        //    }
        //    set
        //    {
        //        if (value == null)
        //        {
        //            return;
        //        }
        //        lock (realPosition)
        //        {
        //            if (SimpleDistance(value, realPosition) >= RealPositionRangeMm)
        //            {
        //                realPosition = value;
        //            }
        //        }               
        //    }
        //}
        

        private int SimpleDistance(MapPosition aPos, MapPosition bPos)
        {
            return (int)(Math.Abs(aPos.X - bPos.X) + Math.Abs(aPos.Y - bPos.Y));
        }       
    }
}