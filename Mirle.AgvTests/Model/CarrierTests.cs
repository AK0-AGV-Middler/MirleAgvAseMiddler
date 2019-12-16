using NUnit.Framework;
using Mirle.Agv.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Mirle.Agv.Model.TransferSteps;
using System.Reflection;

namespace Mirle.Agv.Model.Tests
{
    [TestFixture()]
    public class CarrierTests
    {
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
            agvcTransCmd.ToLoadSectionIds = aList;
            PropertyInfo[] infos = agvcTransCmd.GetType().GetProperties();

            foreach (var info in infos)
            {
                var name = info.Name;
                var value = info.GetValue(agvcTransCmd);
                List<string> valueInList = new List<string>();
                if (info.PropertyType == typeof(List<string>))
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

        [Test()]
        public void NullTrimTest()
        {
            //string xx = null;
            //var xx1 = string.IsNullOrEmpty(xx);
            //var xx2 = xx.Trim();

            string xx3 = "";
            var xx4 = xx3.Trim();

            Assert.True(true);
        }

        [Test()]
        public void AsyncAwaitTest()
        {
            Task<int> xx1 = foo2();
            var xx2 = xx1.Result;


            Assert.AreEqual(2,xx2);
        }

        public int foo1()
        {
            return 1;
        }

        public async Task<int> foo2()
        {
            int xxx = await Task.Run(() => foo1());
            return 2;
        }

    }
}