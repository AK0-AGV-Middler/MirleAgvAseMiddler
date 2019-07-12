using NUnit.Framework;
using Mirle.Agv.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model.Tests
{
    [TestFixture()]
    public class VehicleTests
    {
        [Test()]
        public void UpdateStatusTest()
        {
            var theVehicle = Vehicle.Instance;
            var location = theVehicle.GetVehLoacation();
            location.Section.Distance = 123.45f;
            var distance = Vehicle.Instance.GetVehLoacation().Section.Distance;

            Assert.AreEqual(distance, 123.45f);
        }

        [Test()]
        public void ReferenceTest()
        {
            List<string> list001 = new List<string>();
            list001.Add("Apple");
            list001.Add("Book");
            Assert.AreEqual(2, list001.Count);

            List<string> list002 = list001;
            Assert.AreEqual(2, list002.Count);

            list001 = new List<string>();
            Assert.AreEqual(0, list001.Count);
            Assert.AreEqual(2, list002.Count);

            List<string> list003 = list002;
            Assert.AreEqual(2, list003.Count);

            list002.Clear();
            Assert.AreEqual(0, list002.Count);
            Assert.AreEqual(0, list003.Count);

        }
    }
}