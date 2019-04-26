using NUnit.Framework;
using System;
using System.Collections.Concurrent;


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
            ClassB temp1 = new ClassB(concreteIIA);
            ClassB temp2 = new ClassB(concreteIIA);
            temp1.MemberB = 7;

            ConcurrentQueue<ClassA> classAs = new ConcurrentQueue<ClassA>();
            //List<ClassA> classAs = new List<ClassA>();
            //classAs.Add(temp1);
            //classAs.Add(temp2);
            classAs.Enqueue(temp1);
            classAs.Enqueue(temp2);
            classAs.TryPeek(out ClassA peek1);

            var checek1 = peek1;
            ClassB peek2 = (ClassB)peek1;
            var check2 = peek2.MemberB;

            classAs.TryDequeue(out ClassA take1);
            var check3 = peek2.MemberB;
            var check4 = peek1;
            ClassB take2 = (ClassB)take1;
            var check5 = take2.MemberB;
            classAs.TryPeek(out ClassA peek3);




            Assert.AreEqual(1, 1);
        }
    }
}