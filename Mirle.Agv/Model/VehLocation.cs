using System.Collections.Generic;

namespace Mirle.Agv.Model
{
    public class VehLocation
    {
        private MapBarcodeReader mapBarcodeValues;
        private List<double> encoderValues;
       
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