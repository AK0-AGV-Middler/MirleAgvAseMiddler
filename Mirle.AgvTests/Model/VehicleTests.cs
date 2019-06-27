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
    }
}