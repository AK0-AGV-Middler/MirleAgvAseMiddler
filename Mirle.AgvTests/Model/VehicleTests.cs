﻿using NUnit.Framework;
using Mirle.Agv.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

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


            ConcurrentQueue<string> que004 = new ConcurrentQueue<string>();
            que004.Enqueue("Cat");
            que004.Enqueue("Dog");
            Assert.AreEqual(2, que004.Count);

            List<string> list005 = que004.ToList();
            Assert.AreEqual(2, list005.Count);

            string str001 = list005[1];
            Assert.AreEqual(str001.GetHashCode(), list005[1].GetHashCode());

            list005.RemoveAt(1);
            Assert.AreEqual(1, list005.Count);
            Assert.AreEqual(2, que004.Count);
        }

        [Test()]
        public void QueToListTest()
        {
            ConcurrentQueue<string> tempQue = new ConcurrentQueue<string>();
            string str001 = "str001";
            string str002 = "str002";
            string str003 = "str003";

            tempQue.Enqueue(str001);
            tempQue.Enqueue(str002);
            tempQue.Enqueue(str003);

            Assert.AreEqual(3, tempQue.Count);

            var array01 = tempQue.ToArray();
            Assert.AreEqual(3, array01.Length);

            Assert.AreEqual(str001, array01[0]);

            var list01 = tempQue.ToList();
            Assert.AreEqual(3, list01.Count);

            Assert.AreEqual(str001, list01[0]);
        }

        [Test()]
        public void StringReferenceTest()
        {
            string msg = "PQR";
            Assert.AreEqual("PQR", msg);

            Alarm alarm = new Alarm();
            alarm.TestChangeStrToABC(msg);

            Assert.AreEqual("ABC", msg);
        }

        //[Test()]
        //public void GetterSetterTest()
        //{
        //    Vehicle vehicle = Vehicle.Instance;
        //    vehicle.someTestVeh.Loading = false;
        //    vehicle.someTestVeh.Loading = true;
        //    vehicle.someTestVeh.Loading = false;
        //    Assert.IsTrue(true);
        //}
    }
}