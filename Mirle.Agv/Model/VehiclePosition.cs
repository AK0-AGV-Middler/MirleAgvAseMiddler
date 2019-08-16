
using System;

namespace Mirle.Agv.Model
{
    [Serializable]
    public class VehiclePosition
    {
        private MapInfo theMapInfo = new MapInfo();
        public MapSection LastSection { get; set; } = new MapSection();
        public MapAddress LastAddress { get; set; } = new MapAddress();
        public MapPosition BarcodePosition { get; set; } = new MapPosition();
        public MapPosition RealPosition { get; set; } = new MapPosition();
        public double VehicleAngle { get; set; } = 0;
        public int PredictVehicleAngle { get; set; } = 0;

        public VehiclePosition(MapInfo theMapInfo)
        {
            this.theMapInfo = theMapInfo;
        }       
    }
}