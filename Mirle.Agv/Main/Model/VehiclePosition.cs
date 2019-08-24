
using System;

namespace Mirle.Agv.Model
{
    [Serializable]
    public class VehiclePosition
    {
        public MapSection LastSection { get; set; } = new MapSection();
        public MapAddress LastAddress { get; set; } = new MapAddress();
        public MapPosition BarcodePosition { get; set; } = new MapPosition();
        private MapPosition realPosition;
        public MapPosition RealPosition
        {
            get
            {
                if (realPosition == null)
                {
                    return new MapPosition();
                }
                else
                {
                    return realPosition;
                }
            }
            set
            {
                if (realPosition == null)
                {
                    realPosition = value;
                }
                if (SimpleDistance(value, realPosition) >= RealPositionRangeMm)
                {
                    realPosition = value;
                }
            }
        }
        public double VehicleAngle { get; set; } = 0;
        public int PredictVehicleAngle { get; set; } = 0;
        public int RealPositionRangeMm { get; set; } = 5;

        private int SimpleDistance(MapPosition aPos, MapPosition bPos)
        {
            return (int)(Math.Abs(aPos.X - bPos.X) + Math.Abs(aPos.Y - bPos.Y));
        }

    }
}