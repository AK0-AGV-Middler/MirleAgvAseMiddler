using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Mirle.Agv.Controller.Tests
{
    [TestFixture()]
    public class MainFlowHandlerTests
    {
        private List<string> Works { get; } = new List<string>();

        private void Foo(List<string> array)
        {
            List<string> somethings = new List<string>();
            somethings.Add("pqr");
            somethings.Add("xyz");
            array.AddRange(somethings);
        }

        private string Foo2(string str)
        {
            if (str == null)
            {
                return "It is null";
            }
            else
            {
                return str + " is not null";
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
            string a = "xx";
            string b = null;

            var result1 = Foo2(a);
            var result2 = Foo2(b);

            Assert.AreEqual(result1, a + " is not null");

            
        }
    }
}