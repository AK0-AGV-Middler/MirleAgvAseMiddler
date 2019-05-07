using System.Collections.Generic;

namespace Mirle.Agv.Model
{
    public class VehLocation
    {
        private MapBarcodeReader mapBarcodeValues;
        private List<double> encoderValues;

        public MapSection Section { get; set; }
        public MapAddress Address { get; set; }

        public VehLocation()
        {
            Section = new MapSection();
            Address = new MapAddress();
        }

        public void SetMapBarcodeValues(MapBarcodeReader mapBarcodeValues)
        {
            this.mapBarcodeValues = mapBarcodeValues;
        }

        public MapBarcodeReader GetBarcodeValues()
        {
            return mapBarcodeValues;
        }
    }
}