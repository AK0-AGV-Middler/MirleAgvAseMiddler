
using System;

namespace Mirle.Agv.Model
{
    [Serializable]
    public class VehLocation
    {
        private MapInfo theMapInfo = new MapInfo();
        public MapSection Section { get; set; } = new MapSection();
        public MapAddress Address { get; set; } = new MapAddress();
        public MapPosition EncoderGxPosition { get; set; } = new MapPosition();
        public MapPosition BarcodePosition { get; set; } = new MapPosition();
        public MapPosition Delta { get; set; } = new MapPosition();
        public MapPosition RealPosition { get; set; } = new MapPosition();

        public VehLocation(MapInfo theMapInfo)
        {
            this.theMapInfo = theMapInfo;
        }

        public void SetBarcodePosition(int barcodeNum)
        {
            MapBarcode mapBarcode = theMapInfo.allBarcodes[barcodeNum];
            BarcodePosition = new MapPosition(mapBarcode.Position.X, mapBarcode.Position.Y);
        }
    }
}