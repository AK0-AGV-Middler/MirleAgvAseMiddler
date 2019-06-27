using System.Collections.Generic;

namespace Mirle.Agv.Model
{
    public class VehLocation
    {
        public MapSection Section { get; set; } = new MapSection();
        public MapAddress Address { get; set; } = new MapAddress();

        public MapPosition EncoderGxPosition { get; set; } = new MapPosition();
        public MapPosition BarcodePosition { get; set; } = new MapPosition();
        public MapPosition Delta { get; set; } = new MapPosition();
        public MapPosition RealPosition { get; set; } = new MapPosition();

        public void SetBarcodePosition(int barcodeNum)
        {
            MapBarcode mapBarcode = MapInfo.Instance.dicBarcodes[barcodeNum];
            BarcodePosition = new MapPosition(mapBarcode.PositionX, mapBarcode.PositionY);           
        }
    }
}