using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Mirle.Agv.Control.Tests
{
    interface IInterfaceA
    {
        void DoSomething();
    }

    class ConcreteIIA : IInterfaceA
    {
        public void DoSomething()
        {

        }
    }

    abstract class ClassA
    {
        protected IInterfaceA interfaceA;

        public ClassA(IInterfaceA interfaceA)
        {
            this.interfaceA = interfaceA;
        }
    }

    class ClassB : ClassA
    {
        public int MemberB { get; set; }
        public ClassB(IInterfaceA interfaceA) : base(interfaceA)
        {
        }
    }


    [TestFixture()]
    public class MainFlowHandlerTests
    {
        [Test()]
        public void MainFlowHandlerTest()
        {
            MainFlowHandler mainFlow = new MainFlowHandler();


            Assert.AreEqual(1, 1);
        }

        [Test()]
        public void XXXTest()
        {
            ConcreteIIA concreteIIA = new ConcreteIIA();
            ClassB ins1 = new ClassB(concreteIIA);
            ins1.MemberB = 123;
            ClassB ins2 = new ClassB(concreteIIA);
            ins2.MemberB = 456;
 
            List<ClassB> list001 = new List<ClassB>();
            list001.Add(ins1);
            list001.Add(ins2);

            Assert.AreEqual(2, list001.Count);

            List<ClassB> list002 = list001;

            Assert.AreEqual(2, list002.Count);
            Assert.AreEqual(123, list002[0].MemberB);

            List<ClassB> list003 = new List<ClassB>();
            for (int i = 0; i < list001.Count; i++)
            {
                list003.Add(list001[i]);
            }

            Assert.AreEqual(2, list003.Count);
            Assert.AreEqual(456, list003[1].MemberB);

            list001.Clear();

            Assert.AreEqual(0, list001.Count);

            Assert.AreEqual(0, list002.Count);

            Assert.AreEqual(2, list003.Count);
        }
    }
}