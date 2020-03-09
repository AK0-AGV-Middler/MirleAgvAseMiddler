using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mirle.Agv.AseMiddler.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Mirle.Agv.AseMiddler.Controller.Tests
{
    [TestClass()]
    public class AsePackageTests
    {
        [TestMethod()]
        public void LogPsWrapperTest()
        {
            AsePackage asePackage = new AsePackage(new Dictionary<string, string>());

            var xx = asePackage.mirleLogger;

            asePackage.LogPsWrapper("ABC", "DEF");

            asePackage.LogDebug("PQR", "ZYX");

            Assert.IsTrue(true);
        }
    }
}