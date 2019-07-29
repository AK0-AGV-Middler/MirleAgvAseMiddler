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

    }
}