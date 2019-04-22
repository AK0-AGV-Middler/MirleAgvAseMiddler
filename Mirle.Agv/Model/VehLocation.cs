using System.Collections.Generic;

namespace Mirle.Agv.Model
{
    public class VehLocation
    {
        private MapBarcodeValues mapBarcodeValues;
        private List<double> encoderValues;
       
        public void SetMapBarcodeValues(MapBarcodeValues mapBarcodeValues)
        {
            this.mapBarcodeValues = mapBarcodeValues;
        }

        public MapBarcodeValues GetBarcodeValues()
        {
            return mapBarcodeValues;
        }
    }
}