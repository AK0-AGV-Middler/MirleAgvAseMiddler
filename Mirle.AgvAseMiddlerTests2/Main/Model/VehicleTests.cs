using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mirle.AgvAseMiddler.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.AgvAseMiddler.Model.Tests
{
    [TestClass()]
    public class VehicleTests
    {
        [TestMethod()]
        public void CreateVehicleIntegrateStatusTest()
        {
            Assert.IsTrue(true);
        }

        [TestMethod()]
        public void EnumToIntToStringTest0219()
        {
            EnumPioDirection pioDirection = EnumPioDirection.Right;
            string xx = ((int)pioDirection).ToString();
            Assert.AreEqual("2", xx);
        }


        [TestMethod()]
        public void NameOfToStringTest0222()
        {
            EnumSlotNumber slotNumber = EnumSlotNumber.B;
            string x1 = nameof(slotNumber).PadLeft(2, '0');
            Assert.AreEqual("slotNumber", x1);
            string x2 = slotNumber.ToString().PadLeft(2, '0');
            Assert.AreEqual("0B", x2);
        }

        [TestMethod()]
        public void ListInsertTest0222()
        {
            List<string> xx = new List<string>();
            xx.Add("A");
            xx.Add("B");
            xx.Add("C");
            xx.Insert(1, "D");           
            string xxStr = "";
            foreach (var item in xx)
            {
                xxStr += item;
            }
            Assert.AreEqual("ADBC", xxStr);
        }

        [TestMethod()]
        public void GetAseCarrierSlotStatusTest0222()
        {
            var veh = Vehicle.Instance;
            veh.AseCarrierSlotA.CarrierId = "ABC";
            veh.AseCarrierSlotA.CarrierSlotStatus = EnumAseCarrierSlotStatus.Loading;

            Assert.AreEqual("ABC", veh.AseCarrierSlotA.CarrierId);
            Assert.AreEqual(EnumAseCarrierSlotStatus.Loading, veh.AseCarrierSlotA.CarrierSlotStatus);

            var slot = veh.GetAseCarrierSlotStatus(EnumSlotNumber.A);
            slot.CarrierId = "PQR";
            slot.CarrierSlotStatus = EnumAseCarrierSlotStatus.ReadFail;
            Assert.AreEqual("PQR", veh.AseCarrierSlotA.CarrierId);
            Assert.AreEqual(EnumAseCarrierSlotStatus.ReadFail, veh.AseCarrierSlotA.CarrierSlotStatus);
        }


    }
}