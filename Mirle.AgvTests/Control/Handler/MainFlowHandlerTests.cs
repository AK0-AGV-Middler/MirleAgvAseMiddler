using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Mirle.Agv.Control.Tests
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

        [Test()]
        public void MainFlowHandlerTest()
        {
            MainFlowHandler mainFlow = new MainFlowHandler();


            Assert.AreEqual(1, 1);
        }

        [Test()]
        public void XXXTest()
        {
            string[] words = { "abc", "def" };

            Assert.AreEqual(words.Length, 2);
            
        }
    }
}