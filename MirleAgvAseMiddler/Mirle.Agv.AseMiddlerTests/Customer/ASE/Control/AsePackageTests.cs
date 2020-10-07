using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mirle.Agv.AseMiddler.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.AseMiddler.Model.TransferSteps;
using System.Collections.Concurrent;


namespace Mirle.Agv.AseMiddler.Controller.Tests
{
  [TestClass()]
  public class AsePackageTests
  {
    [TestMethod()]
    public void LogPsWrapperTest()
    {
      AsePackage asePackage = new AsePackage();

      var xx = asePackage.mirleLogger;


      Assert.IsTrue(true);
    }

    [TestMethod()]
    public void SubStringTest0327()
    {
      string word = "ABCDEF";

      string the3rd = word.Substring(2, 1);

      Assert.AreEqual("C", the3rd);
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

    [TestMethod()]
    public void ConcurrentQueueToListTest0427()
    {
      ConcurrentQueue<string> words = new ConcurrentQueue<string>();
      words.Enqueue("101");
      words.Enqueue("102");
      words.Enqueue("103");

      List<string> aList = words.ToList();
      string xx = aList.Last();
      xx += "G";

      Assert.AreEqual(3, aList.Count);
    }

    [TestMethod()]
    public void ConcurrentBagToListTest0427()
    {
      ConcurrentBag<string> words = new ConcurrentBag<string>();
      words.Add("101");
      words.Add("102");
      words.Add("103");

      List<string> aList = words.ToList();
      string xx = aList.Last();
      xx += "G";

      Assert.AreEqual(3, aList.Count);
    }
  }
}