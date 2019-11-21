using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Controller;

namespace Mirle.Agv.Model
{
    [Serializable]
    public class PlcVehicle
    {
        public PlcBatterys Batterys = new PlcBatterys();
        public PlcRobot Robot = new PlcRobot();
        public PlcOperation JogOperation = new PlcOperation();

        public bool Loading { get; set; }
        public string CassetteId { get; set; } = "";
        public string FakeCassetteId { get; set; } = "";
        public string RenameCassetteId { get; set; } = "";

        //以下屬性會影響方向燈,語音和Beam sensor sleep
        public bool Forward { get; set; }
        public bool Backward { get; set; }
        public bool SpinTurnLeft { get; set; }//左旋轉
        public bool SpinTurnRight { get; set; }//右旋轉
        public bool TraverseLeft { get; set; }//左橫移
        public bool TraverseRight { get; set; }//右橫移
        public bool SteeringFL { get; set; }
        public bool SteeringFR { get; set; }
        public bool SteeringBL { get; set; }
        public bool SteeringBR { get; set; }

        //dicBeamSensor key值為PLCNearSignalTagID and PLCFarSignalTagID, 兩個都會指到相同PLCBeamSensor物件
        public Dictionary<string, PlcBeamSensor> dicBeamSensor = new Dictionary<string, PlcBeamSensor>();
        public List<PlcBeamSensor> listFrontBeamSensor = new List<PlcBeamSensor>();
        public List<PlcBeamSensor> listBackBeamSensor = new List<PlcBeamSensor>();
        public List<PlcBeamSensor> listLeftBeamSensor = new List<PlcBeamSensor>();
        public List<PlcBeamSensor> listRightBeamSensor = new List<PlcBeamSensor>();

        //BeamSensor Auto Sleepd開關
        public bool BeamSensorAutoSleep { get; set; }

        //這四個值由Move Control給定,可以多個方向同時給true
        //會影響beam sensor的值
        public bool MoveFront { get; set; }
        public bool MoveBack { get; set; }
        public bool MoveLeft { get; set; }
        public bool MoveRight { get; set; }

        //由主流程依圖資給定
        public bool FrontBeamSensorDisable { get; set; }
        public bool BackBeamSensorDisable { get; set; }
        public bool LeftBeamSensorDisable { get; set; }
        public bool RightBeamSensorDisable { get; set; }

        public Dictionary<string, PlcBumper> dicBumper = new Dictionary<string, PlcBumper>();
        public List<PlcBumper> listBumper = new List<PlcBumper>();

        public bool BumperAlarmStatus { get; set; } = false;
        public bool PlcEmoStatus { get; set; } = false;

        public ushort IPcStatus { get; set; } = 0;

        public Dictionary<string, PlcEmo> dicPlcEmo = new Dictionary<string, PlcEmo>();
        public List<PlcEmo> listPlcEmo = new List<PlcEmo>();

        public bool SafetyDisable { get; set; }
        public EnumVehicleSafetyAction VehicleSafetyAction { get; set; } = EnumVehicleSafetyAction.Normal;
        //BeamSensorDisableNormalSpeed
        public Boolean BeamSensorDisableNormalSpeed { get; set; } = false;

        public int BatteryCellNum = 17;
        public int BatteryReplaceIndex = 17;

        public PlcVehicle()
        {
            InitialPLCBeamSensor();
            InitialPlcBumpers();
            InitialPlcEmos();
            InitialBatteryCells();
        }

        #region HardCode PlcBeamSensors/PlcBumpers/PlcEmos will fix in config.xml

        private void InitialBatteryCells()
        {
            for (int i = 0; i <= BatteryCellNum; i++)
            {
                BatteryCell batteryCell = new BatteryCell(i);
                this.Batterys.BatteryCells.Add(batteryCell);
            }
        }
        private void InitialPlcBumpers()
        {
            PlcBumper aPLCBumper;
            dicBumper.Clear();
            listBumper.Clear();

            //Front
            aPLCBumper = new PlcBumper("FUR");
            listBumper.Add(aPLCBumper);
            dicBumper.Add(aPLCBumper.PlcSignalTagId, aPLCBumper);
            aPLCBumper = new PlcBumper("FUL");
            listBumper.Add(aPLCBumper);
            dicBumper.Add(aPLCBumper.PlcSignalTagId, aPLCBumper);
            aPLCBumper = new PlcBumper("FDR");
            listBumper.Add(aPLCBumper);
            dicBumper.Add(aPLCBumper.PlcSignalTagId, aPLCBumper);
            aPLCBumper = new PlcBumper("FDL");
            listBumper.Add(aPLCBumper);
            dicBumper.Add(aPLCBumper.PlcSignalTagId, aPLCBumper);
            aPLCBumper = new PlcBumper("UF");
            listBumper.Add(aPLCBumper);
            dicBumper.Add(aPLCBumper.PlcSignalTagId, aPLCBumper);
            aPLCBumper = new PlcBumper("DF");
            listBumper.Add(aPLCBumper);
            dicBumper.Add(aPLCBumper.PlcSignalTagId, aPLCBumper);

            //Back
            aPLCBumper = new PlcBumper("BUR");
            listBumper.Add(aPLCBumper);
            dicBumper.Add(aPLCBumper.PlcSignalTagId, aPLCBumper);
            aPLCBumper = new PlcBumper("BUL");
            listBumper.Add(aPLCBumper);
            dicBumper.Add(aPLCBumper.PlcSignalTagId, aPLCBumper);
            aPLCBumper = new PlcBumper("BDR");
            listBumper.Add(aPLCBumper);
            dicBumper.Add(aPLCBumper.PlcSignalTagId, aPLCBumper);
            aPLCBumper = new PlcBumper("BDL");
            listBumper.Add(aPLCBumper);
            dicBumper.Add(aPLCBumper.PlcSignalTagId, aPLCBumper);
            aPLCBumper = new PlcBumper("UB");
            listBumper.Add(aPLCBumper);
            dicBumper.Add(aPLCBumper.PlcSignalTagId, aPLCBumper);
            aPLCBumper = new PlcBumper("DB");
            listBumper.Add(aPLCBumper);
            dicBumper.Add(aPLCBumper.PlcSignalTagId, aPLCBumper);

            //Left
            aPLCBumper = new PlcBumper("UL");
            listBumper.Add(aPLCBumper);
            dicBumper.Add(aPLCBumper.PlcSignalTagId, aPLCBumper);
            aPLCBumper = new PlcBumper("DL");
            listBumper.Add(aPLCBumper);
            dicBumper.Add(aPLCBumper.PlcSignalTagId, aPLCBumper);

            //Right
            aPLCBumper = new PlcBumper("UR");
            listBumper.Add(aPLCBumper);
            dicBumper.Add(aPLCBumper.PlcSignalTagId, aPLCBumper);
            aPLCBumper = new PlcBumper("DR");
            listBumper.Add(aPLCBumper);
            dicBumper.Add(aPLCBumper.PlcSignalTagId, aPLCBumper);

        }

        private void InitialPlcEmos()
        {
            PlcEmo aPlcEmo;
            dicPlcEmo.Clear();
            listPlcEmo.Clear();

            //Front
            aPlcEmo = new PlcEmo("FR");
            listPlcEmo.Add(aPlcEmo);
            dicPlcEmo.Add(aPlcEmo.PlcSignalTagId, aPlcEmo);

            aPlcEmo = new PlcEmo("FC");
            listPlcEmo.Add(aPlcEmo);
            dicPlcEmo.Add(aPlcEmo.PlcSignalTagId, aPlcEmo);

            aPlcEmo = new PlcEmo("FL");
            listPlcEmo.Add(aPlcEmo);
            dicPlcEmo.Add(aPlcEmo.PlcSignalTagId, aPlcEmo);

            //Back
            aPlcEmo = new PlcEmo("BR");
            listPlcEmo.Add(aPlcEmo);
            dicPlcEmo.Add(aPlcEmo.PlcSignalTagId, aPlcEmo);

            aPlcEmo = new PlcEmo("BC");
            listPlcEmo.Add(aPlcEmo);
            dicPlcEmo.Add(aPlcEmo.PlcSignalTagId, aPlcEmo);

            aPlcEmo = new PlcEmo("BL");
            listPlcEmo.Add(aPlcEmo);
            dicPlcEmo.Add(aPlcEmo.PlcSignalTagId, aPlcEmo);

            //EMO-Total
            //aPlcEmo = new PlcEmo("Total");
            //listPlcEmo.Add(aPlcEmo);
            //dicPlcEmo.Add(aPlcEmo.PlcSignalTagId, aPlcEmo);
        }

        private void InitialPLCBeamSensor()
        {
            PlcBeamSensor aPLCBeamSensor;
            dicBeamSensor.Clear();
            listFrontBeamSensor.Clear();
            listBackBeamSensor.Clear();
            listLeftBeamSensor.Clear();
            listRightBeamSensor.Clear();

            //Front
            aPLCBeamSensor = new PlcBeamSensor("FUR");
            listFrontBeamSensor.Add(aPLCBeamSensor);
            DictionaryAddBeamSensor(aPLCBeamSensor);

            aPLCBeamSensor = new PlcBeamSensor("FUL");
            listFrontBeamSensor.Add(aPLCBeamSensor);
            DictionaryAddBeamSensor(aPLCBeamSensor);

            aPLCBeamSensor = new PlcBeamSensor("FDR");
            listFrontBeamSensor.Add(aPLCBeamSensor);
            DictionaryAddBeamSensor(aPLCBeamSensor);

            aPLCBeamSensor = new PlcBeamSensor("FDL");
            listFrontBeamSensor.Add(aPLCBeamSensor);
            DictionaryAddBeamSensor(aPLCBeamSensor);

            aPLCBeamSensor = new PlcBeamSensor("FDC");
            listFrontBeamSensor.Add(aPLCBeamSensor);
            DictionaryAddBeamSensor(aPLCBeamSensor);


            //Back
            aPLCBeamSensor = new PlcBeamSensor("BUR");
            listBackBeamSensor.Add(aPLCBeamSensor);
            DictionaryAddBeamSensor(aPLCBeamSensor);

            aPLCBeamSensor = new PlcBeamSensor("BUL");
            listBackBeamSensor.Add(aPLCBeamSensor);
            DictionaryAddBeamSensor(aPLCBeamSensor);

            aPLCBeamSensor = new PlcBeamSensor("BDR");
            listBackBeamSensor.Add(aPLCBeamSensor);
            DictionaryAddBeamSensor(aPLCBeamSensor);

            aPLCBeamSensor = new PlcBeamSensor("BDL");
            listBackBeamSensor.Add(aPLCBeamSensor);
            DictionaryAddBeamSensor(aPLCBeamSensor);

            aPLCBeamSensor = new PlcBeamSensor("BDC");
            listBackBeamSensor.Add(aPLCBeamSensor);
            DictionaryAddBeamSensor(aPLCBeamSensor);

            //Left
            aPLCBeamSensor = new PlcBeamSensor("LUR");
            listLeftBeamSensor.Add(aPLCBeamSensor);
            DictionaryAddBeamSensor(aPLCBeamSensor);

            aPLCBeamSensor = new PlcBeamSensor("LUL");
            listLeftBeamSensor.Add(aPLCBeamSensor);
            DictionaryAddBeamSensor(aPLCBeamSensor);

            aPLCBeamSensor = new PlcBeamSensor("LDR");
            listLeftBeamSensor.Add(aPLCBeamSensor);
            DictionaryAddBeamSensor(aPLCBeamSensor);

            aPLCBeamSensor = new PlcBeamSensor("LDL");
            listLeftBeamSensor.Add(aPLCBeamSensor);
            DictionaryAddBeamSensor(aPLCBeamSensor);

            aPLCBeamSensor = new PlcBeamSensor("LUC");
            listLeftBeamSensor.Add(aPLCBeamSensor);
            DictionaryAddBeamSensor(aPLCBeamSensor);


            //Right
            aPLCBeamSensor = new PlcBeamSensor("RUR");
            listRightBeamSensor.Add(aPLCBeamSensor);
            DictionaryAddBeamSensor(aPLCBeamSensor);

            aPLCBeamSensor = new PlcBeamSensor("RUL");
            listRightBeamSensor.Add(aPLCBeamSensor);
            DictionaryAddBeamSensor(aPLCBeamSensor);

            aPLCBeamSensor = new PlcBeamSensor("RDR");
            listRightBeamSensor.Add(aPLCBeamSensor);
            DictionaryAddBeamSensor(aPLCBeamSensor);

            aPLCBeamSensor = new PlcBeamSensor("RDL");
            listRightBeamSensor.Add(aPLCBeamSensor);
            DictionaryAddBeamSensor(aPLCBeamSensor);

            aPLCBeamSensor = new PlcBeamSensor("RUC");
            listRightBeamSensor.Add(aPLCBeamSensor);
            DictionaryAddBeamSensor(aPLCBeamSensor);

        }
        #endregion

        private void DictionaryAddBeamSensor(PlcBeamSensor aPlcBeamSensor)
        {
            dicBeamSensor.Add(aPlcBeamSensor.PlcNearSignalTagId, aPlcBeamSensor);
            dicBeamSensor.Add(aPlcBeamSensor.PlcFarSignalTagId, aPlcBeamSensor);
            dicBeamSensor.Add(aPlcBeamSensor.PlcReadSleepTagId, aPlcBeamSensor);
            dicBeamSensor.Add(aPlcBeamSensor.PlcWriteSleepTagId, aPlcBeamSensor);
        }

    }
}
