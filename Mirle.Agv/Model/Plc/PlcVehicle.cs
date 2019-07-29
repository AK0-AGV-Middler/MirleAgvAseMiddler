using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Controller;

namespace Mirle.Agv.Model
{

    public class PlcVehicle
    {
        public PlcBatterys Batterys { get; set; } = new PlcBatterys();
        public PLCRobot Robot { get; set; } = new PLCRobot();

        public bool Loading { get; set; }
        public string CassetteId { get; set; }

        //以下屬性會影響方向燈,語音和Beam sensor sleep
        public bool Forward { get; set; }
        public bool Backward { get; set; }
        public bool SpinTurnLeft { get; set; }
        public bool SpinTurnRight { get; set; }
        public bool TraverseLeft { get; set; }
        public bool TraverseRight { get; set; }
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

        public Dictionary<string, PlcEmo> dicPlcEmo = new Dictionary<string, PlcEmo>();
        public List<PlcEmo> listPlcEmo = new List<PlcEmo>();
        public bool SafetyDisable { get; set; }
        public EnumVehicleSafetyAction VehicleSafetyAction { get; set; } = EnumVehicleSafetyAction.Normal;

        public PlcVehicle()
        {
            InitialPLCBeamSensor();
            InitialPlcBumpers();
            InitialPlcEmos();
        }

        #region HardCode PlcBeamSensors/PlcBumpers/PlcEmos will fix in config.xml
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
            aPlcEmo = new PlcEmo("Total");
            listPlcEmo.Add(aPlcEmo);
            dicPlcEmo.Add(aPlcEmo.PlcSignalTagId, aPlcEmo);
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

        private void DictionaryAddBeamSensor(PlcBeamSensor aPLCBeamSensor)
        {
            dicBeamSensor.Add(aPLCBeamSensor.PlcNearSignalTagId, aPLCBeamSensor);
            dicBeamSensor.Add(aPLCBeamSensor.PlcFarSignalTagId, aPLCBeamSensor);
            dicBeamSensor.Add(aPLCBeamSensor.PlcReadSleepTagId, aPLCBeamSensor);
            dicBeamSensor.Add(aPLCBeamSensor.PlcWriteSleepTagId, aPLCBeamSensor);
        }
    }
}
