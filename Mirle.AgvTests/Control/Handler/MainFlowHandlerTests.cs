using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Mirle.Agv.Control.Tests
{
    [TestFixture()]
    public class MainFlowHandlerTests
    {
        class AA
        {
            private BB bb;
            public string a = "AAaa";
        }

        class BB
        {
            private AA aa;
            public string b = "BBbb";

            public BB(AA aa)
            {
                this.aa = aa;
            }

            public int HashAA()
            {
                return aa.GetHashCode();
            }
        }

        [Test()]
        public void MainFlowHandlerTest()
        {
            MainFlowHandler mainFlow = new MainFlowHandler();


            Assert.AreEqual(1, 1);
        }

        [Test()]
        public void XXXTest()
        {
            AA obA = new AA();

            BB obB = new BB(obA);

            var temp1 = obA.GetHashCode();

            var temp2 = obB.HashAA();

            Console.Read();
            
        }
    }
}