using NUnit.Framework;
using Mirle.Agv.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mirle.iibg3k0.ttc.Common;
using com.mirle.iibg3k0.ttc.Common.TCPIP;
using TcpIpClientSample;


namespace Mirle.Agv.Control.Tests
{
    [TestFixture()]
    public class MiddleAgentTests
    {
        [Test()]
        public void Receive_Cmd31MoveTest()
        {
            MainFlowHandler mainFlowHandler = new MainFlowHandler(@"D:\CsProject\Mirle.Agv\Mirle.Agv\bin\Debug");
            mainFlowHandler.InitialMainFlowHandler();
            MiddleAgent middleAgent = mainFlowHandler.GetMiddleAgent();

            object sender = new object();
            ID_31_TRANS_REQUEST request = new ID_31_TRANS_REQUEST();
            request.ActType = ActiveType.Move;
            request.CmdID = "cmdId";
            request.CSTID = "CstId";
            request.DestinationAdr = "destAddress";
            request.SecDistance = 23;
            
            TcpIpEventArgs e = new TcpIpEventArgs("packId",12,request);

            middleAgent.Receive_Cmd31(sender, e);


            Assert.True(true);
        }
    }
}