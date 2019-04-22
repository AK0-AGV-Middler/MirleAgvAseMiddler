using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Model;

namespace Mirle.Agv.Control
{
    public interface IMapBarcodeTaker
    {
        void UpdateMapBarcode(MapBarcodeValues mapBarcode);
    }
}
