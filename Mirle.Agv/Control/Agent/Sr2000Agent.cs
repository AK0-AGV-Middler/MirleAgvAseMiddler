using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Model;
using Mirle.Agv.Model.Configs;
using System.Threading;

namespace Mirle.Agv.Control
{
    public class Sr2000Agent
    {
        private Dictionary<EnumMapBarcodeReaderSide, MapBarcodeReader> dicMapBarcodeReaders;
        private Sr2000Configs sr2000Configs;
        private LoggerAgent loggerAgent;

        public Sr2000Agent(Sr2000Configs sr2000Configs)
        {
            loggerAgent = LoggerAgent.Instance;
            //TODO: fill dicMapBarcodeReaders from sr2000Configs setting.
            this.sr2000Configs = sr2000Configs;
            dicMapBarcodeReaders = new Dictionary<EnumMapBarcodeReaderSide, MapBarcodeReader>();
            RunThreads();
        }

        private void RunThreads()
        {
            Thread thdTrackingMapBarcode = new Thread(new ThreadStart(TrackingMapBarcode));
            thdTrackingMapBarcode.IsBackground = true;
            thdTrackingMapBarcode.Start();
        }

        private void TrackingMapBarcode()
        {
            while (true)
            {
                //TODO : get new mapBarcodeValues from driver
                MapBarcodeReader mapBarcodeValues = new MapBarcodeReader();
                //mapBarcodeValues = GetFromDriver();               
                UpdateMapBarcode(EnumMapBarcodeReaderSide.None, mapBarcodeValues);
                Thread.Sleep(sr2000Configs.TrackingInterval);
            }
        }

        public event EventHandler<MapBarcodeReader> OnMapBarcodeValuesChange;

        private void UpdateMapBarcode(EnumMapBarcodeReaderSide side,MapBarcodeReader mapBarcode)
        {
            if (IsMapBarcodeReaderExist(side))
            {
                var oldValues = dicMapBarcodeReaders[side];
                if (!oldValues.Equals(mapBarcode))
                {
                    dicMapBarcodeReaders[side] = mapBarcode;

                    //通知其他實體MapBarcodeValues已變成新的value
                    if (OnMapBarcodeValuesChange != null)
                    {
                        OnMapBarcodeValuesChange(this, mapBarcode);
                    }
                }
            }            
        }

        public bool IsMapBarcodeReaderExist(EnumMapBarcodeReaderSide side)
        {
            return dicMapBarcodeReaders.ContainsKey(side);
        }

        public MapBarcodeReader GetMapBarcodeReader(EnumMapBarcodeReaderSide side)
        {
            MapBarcodeReader result = new MapBarcodeReader();
            if (IsMapBarcodeReaderExist(side))
            {
                result = dicMapBarcodeReaders[side];
            }
            return result;
        }

        //增加調整Sr2000角度的方法

    }
}
