
using System;

namespace Mirle.Agv.Model
{
    [Serializable]
    public class VehicleLocation
    {
        public MapSection LastSection { get; set; } = new MapSection();
        public MapAddress LastAddress { get; set; } = new MapAddress();
        public MapPosition BarcodePosition { get; set; } = new MapPosition();
        private MapPosition realPosition = new MapPosition();

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
        }

        public VehicleLocation()
        { }

        public MapPosition RealPosition
        {
            get
            {
                return realPosition;                
            }
            set
            {
                if (value == null)
                {
                    return;
                }
                lock (realPosition)
                {
                    if (SimpleDistance(value, realPosition) >= RealPositionRangeMm)
                    {
                        realPosition = value;
                    }
                }               
            }
        }
        //public MapPosition RealPosition { get; set; } = new MapPosition();
        public double VehicleAngle { get; set; } = 0;
        public int WheelAngle { get; set; } = 0;
        public int RealPositionRangeMm { get; set; } = 15;
        public MapAddress NeerlyAddress { get; set; } = new MapAddress();

        private int SimpleDistance(MapPosition aPos, MapPosition bPos)
        {
            return (int)(Math.Abs(aPos.X - bPos.X) + Math.Abs(aPos.Y - bPos.Y));
        }

        public void SetRealPos(MapPosition mapPosition)
        {
            realPosition = mapPosition;
        }

    }
}