using NUnit.Framework;
using Mirle.Agv.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Mirle.Agv.Model.TransferCmds;
using System.Reflection;

namespace Mirle.Agv.Model.Tests
{
    [TestFixture()]
    public class CarrierTests
    {
        [Test()]
        public void CloneTest()
        {
            Carrier ca1 = new Carrier();
            ca1.Id = "ca1_id";
            ca1.Size = 10;

            var ca2 = ca1.DeepClone();

            Console.WriteLine($"ca1: id={ca1.Id}, size ={ca1.Size}, stageNum={ca1.StageNum}");
            Console.WriteLine($"ca2: id={ca2.Id}, size ={ca2.Size}, stageNum={ca2.StageNum}");

            ca2.Id = "Ca2Id";
            ca2.StageNum = 3;
            Console.WriteLine();

            Console.WriteLine($"ca1: id={ca1.Id}, size ={ca1.Size}, stageNum={ca1.StageNum}");
            Console.WriteLine($"ca2: id={ca2.Id}, size ={ca2.Size}, stageNum={ca2.StageNum}");

            Battery battery1 = new Battery();
            var b2 = battery1.DeepClone();
        }

        [Test()]
        public void YYYTest()
        {
            Queue tempQ = Queue.Synchronized(new Queue());
            Queue q1 = Queue.Synchronized(new Queue());
            Queue q2 = Queue.Synchronized(new Queue());
            tempQ = q1;
            Assert.AreEqual(0, tempQ.Count);

            q1.Enqueue("msg1");
            Assert.AreEqual(1, tempQ.Count);
            q1.Enqueue("msg2");
            Assert.AreEqual(2, tempQ.Count);

            tempQ = q2;
            Assert.AreEqual(0, tempQ.Count);

            q1.Dequeue();
            Assert.AreEqual(0, tempQ.Count);
            Assert.AreEqual(1, q1.Count);
            q2.Enqueue("xxx1");
            Assert.AreEqual(1, tempQ.Count);
            q2.Enqueue("xxx2");
            Assert.AreEqual(2, tempQ.Count);
            q1.Dequeue();
            Assert.AreEqual(2, tempQ.Count);
            Assert.AreEqual(0, q1.Count);

            tempQ = q1;
            q2.Dequeue();
            Assert.AreEqual(0, tempQ.Count);
            Assert.AreEqual(1, q2.Count);


            //q2 = q1;
            //Assert.AreEqual(2, q2.Count);
            //q1.Enqueue("msg3");
            //Assert.AreEqual(3, q1.Count);
            //Assert.AreEqual(3, q2.Count);
            //q1=new Queue();
            //Assert.AreEqual(0, q1.Count);
            //Assert.AreEqual(3, q2.Count);
            //q1.Clear();
            //Assert.AreEqual(3, q2.Count);

        }

        public class SomeClass
        {
            public string AWord { get; set; }
        }

        [Test()]
        public void ZZZTest()
        {
            object tempObj;
            tempObj = new SomeClass();
            var getType = tempObj.GetType().ToString();
            var infos = tempObj.GetType().GetProperties();


            var timeStamp = DateTime.Now.ToString();
        }

        [Test()]
        public void Test001()
        {
            string temp = "Movi";
            string temp2 = null;
            //return (ErrorStatus)Enum.Parse(typeof(ErrorStatus), v);

            var result1 = typeof(EnumMoveState);
            var result2 = Enum.Parse(typeof(EnumMoveState), temp);
            var result3 = (EnumMoveState)Enum.Parse(typeof(EnumMoveState), temp);


            var timeStamp = DateTime.Now.ToString();
        }

        [Test()]
        public void Test002()
        {
            AgvcTransCmd agvcTransCmd = new AgvcTransCmd();
            List<string> aList = new List<string>();
            aList.Add("sec123");
            agvcTransCmd.ToLoadSections = aList;
            PropertyInfo[] infos = agvcTransCmd.GetType().GetProperties();

            foreach (var info in infos)
            {
                var name = info.Name;
                var value = info.GetValue(agvcTransCmd);
                List<string> valueInList = new List<string>();
                if (info.PropertyType==typeof(List<string>))
                {
                    valueInList = (List<string>)value;
                    var xx1 = valueInList.ToString();
                    for (int i = 0; i < valueInList.Count; i++)
                    {
                        var xx = valueInList[i]; 
                    }
                }
                var valueToString = value.ToString();               

            }

            Assert.True(true);
        }

    }
}