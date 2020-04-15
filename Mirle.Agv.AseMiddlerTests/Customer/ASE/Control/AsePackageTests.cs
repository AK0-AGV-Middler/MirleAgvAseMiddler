using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mirle.Agv.AseMiddler.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.AseMiddler.Model.TransferSteps;


namespace Mirle.Agv.AseMiddler.Controller.Tests
{
    [TestClass()]
    public class AsePackageTests
    {
        [TestMethod()]
        public void LogPsWrapperTest()
        {
            AsePackage asePackage = new AsePackage(new Dictionary<string, string>());

            var xx = asePackage.mirleLogger;

            asePackage.LogPsWrapper("ABC", "DEF");

            asePackage.LogDebug("PQR", "ZYX");

            Assert.IsTrue(true);
        }

        [TestMethod()]
        public void SubStringTest0327()
        {
            string word = "ABCDEF";

            string the3rd = word.Substring(2,1);

            Assert.AreEqual("C",the3rd);
        }

        [TestMethod()]
        public void DictionaryContainsKeyTest0330()
        {
            Dictionary<string, int> myDictionary = new Dictionary<string, int>();
            myDictionary.Add("PQR", 100);

            if (myDictionary.ContainsKey(""))
            {
                Assert.IsTrue(string.IsNullOrEmpty(""));
            }

            string nWord = null;

            Assert.IsTrue(string.IsNullOrEmpty(""));
            Assert.IsTrue(string.IsNullOrEmpty(nWord));

            if (string.IsNullOrEmpty(nWord) || !myDictionary.ContainsKey(nWord))
            {
                Assert.IsTrue(string.IsNullOrEmpty(""));
            }

            //if (myDictionary.ContainsKey(nWord))
            //{
            //    Assert.IsTrue(string.IsNullOrEmpty(""));
            //}          
        }

        [TestMethod()]
        public void ListStringToStringTest0403()
        {
            List<string> words = new List<string>();
            words.Add("A");
            words.Add("[B]");
            words.Add("C");
            string xx = string.Join(", ", words);
            Assert.AreEqual("A, [B], C", xx);
        }      
    }
}