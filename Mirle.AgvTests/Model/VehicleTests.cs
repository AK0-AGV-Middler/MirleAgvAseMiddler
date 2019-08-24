using Mirle.Agv.Model.TransferCmds;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Mirle.Agv.Model.Tests
{
    [TestFixture()]
    public class VehicleTests
    {
        [Test()]
        public void UpdateStatusTest()
        {
            var theVehicle = Vehicle.Instance;
            var location = theVehicle.CurVehiclePosition;
            location.LastSection.Distance = 123.45f;
            var distance = Vehicle.Instance.CurVehiclePosition.LastSection.Distance;

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
        public void EnumCmdNumsTest()
        {
            EnumCmdNum cmdNum = (EnumCmdNum)int.Parse("31");
            Console.WriteLine();
        }

        [Test()]
        public void AgvcTransCmdCloneTest()
        {
            AgvcTransCmd transCmd = new AgvcTransCmd();
            transCmd.CommandId = "ABCDE";
            AgvcTransCmd bCmd = transCmd.DeepClone();
            Assert.AreEqual(bCmd.CommandId, transCmd.CommandId);
        }

        [Test()]
        public void NewListTest()
        {
            List<string> vs = new List<string>();
            vs.Add("abc");
            vs.Add("def");
            string xx = vs[0];
            Assert.AreEqual("abc", xx);

            vs = new List<string>();
            Assert.AreEqual("abc", xx);
        }

        [Test()]
        public void EnumReferenceTest()
        {
            EnumThreadStatus status = EnumThreadStatus.None;
            Vehicle.Instance.VisitTransferStepsStatus = status;
            Assert.AreEqual(EnumThreadStatus.None, Vehicle.Instance.VisitTransferStepsStatus);
            status = EnumThreadStatus.Working;
            Assert.AreEqual(EnumThreadStatus.None, Vehicle.Instance.VisitTransferStepsStatus);           
        }

    }
}