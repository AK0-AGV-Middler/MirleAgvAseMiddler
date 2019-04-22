using NUnit.Framework;
using Mirle.Agv.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Control.Tools.Logger;
using System.IO;
using Mirle.Agv.Control.Tools;
using Mirle.Agv.Model.Configs;
using Mirle.Agv.Model.TransferCmds;
using Mirle.Agv.Control.Handler;


namespace Mirle.Agv.Control.Tests
{
    [TestFixture()]
    public class MainFlowHandlerTests
    {
        [Test()]
        public void MainFlowHandlerTest()
        {
            MainFlowHandler mainFlow = new MainFlowHandler();

            mainFlow.DebugLog("ABCDE");

            Console.WriteLine();

            Assert.AreEqual(1, 1);
        }

        [Test()]
        public void XXXTest()
        {

        }
    }
}