using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class MapBarcodeReader
    {
        private EnumMapBarcodeReaderSide side;

        public double X { get; set; }
        public double Y { get; set; }
        public double Theta { get; set; }

        public MapBarcodeReader():this(0,0,0)
        {

        }

        public MapBarcodeReader(double X, double Y, double Theta): this(EnumMapBarcodeReaderSide.None,X,Y,Theta)
        {
           
        }

        public MapBarcodeReader(EnumMapBarcodeReaderSide side, double X, double Y, double Theta)
        {
            this.side = side;
            this.X = X;
            this.Y = Y;
            this.Theta = Theta;
        }

        public EnumMapBarcodeReaderSide GetSide() { return side; }

        public MapBarcodeReader Clone(MapBarcodeReader mapBarcode)
        {
            return new MapBarcodeReader(mapBarcode.X, mapBarcode.Y, mapBarcode.Theta);
        }

        public bool Equals(MapBarcodeReader mapBarcode)
        {
            return (mapBarcode.X == X) && (mapBarcode.Y == Y) && (mapBarcode.Theta == Theta);
        }
    }
}
