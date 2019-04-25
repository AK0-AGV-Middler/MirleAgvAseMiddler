using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class MapBarcodeValues
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Theta { get; set; }

        public MapBarcodeValues()
        {

        }

        public MapBarcodeValues(double X, double Y, double Theta)
        {
            this.X = X;
            this.Y = Y;
            this.Theta = Theta;
        }

        public MapBarcodeValues Clone(MapBarcodeValues mapBarcodeValues)
        {
            return new MapBarcodeValues(mapBarcodeValues.X, mapBarcodeValues.Y, mapBarcodeValues.Theta);
        }

        public bool Equals(MapBarcodeValues mapBarcode)
        {
            return (mapBarcode.X == X) && (mapBarcode.Y == Y) && (mapBarcode.Theta == Theta);
        }
    }

    public class MapBarcodeValuesWithEvent
    {
        public event EventHandler<MapBarcodeValues> OnMapBarcodeValuesChange;

        private MapBarcodeValues mapBarcodeValues;

        public MapBarcodeValues MapBarcodeValues
        {
            get { return mapBarcodeValues; }
            set
            {
                var oldValues = mapBarcodeValues;
                if (!oldValues.Equals(value))
                {
                    mapBarcodeValues = value;
                    if (OnMapBarcodeValuesChange != null)
                    {
                        OnMapBarcodeValuesChange(this, value);
                    }
                }
            }
        }
    }
}
