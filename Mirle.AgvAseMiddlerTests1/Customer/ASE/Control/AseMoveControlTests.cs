using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mirle.AgvAseMiddler.Controller;
using PSDriver.PSDriver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.AgvAseMiddler.Controller.Tests
{
    [TestClass()]
    public class AseMoveControlTests
    {
        [TestMethod()]
        public void GetPositionStringTest()
        {
            double x1 = 12345678;
            string x1String = new AseMoveControl(new PSWrapperXClass()).GetPositionString(x1);

            Assert.AreEqual("P12345678", x1String);
        }
    }
}