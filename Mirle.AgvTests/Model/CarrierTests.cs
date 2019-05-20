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
        }
    }
}