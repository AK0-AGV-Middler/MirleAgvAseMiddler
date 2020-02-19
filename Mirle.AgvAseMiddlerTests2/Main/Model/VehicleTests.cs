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
    }
}