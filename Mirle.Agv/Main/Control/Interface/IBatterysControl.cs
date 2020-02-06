using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Controller
{
    public interface IBatterysControl
    {
        void SetPercentage(double percentage);
        bool StopCharge();
        bool StartCharge(EnumChargeDirection chargeDirection);

        event EventHandler<double> OnBatteryPercentageChangeEvent;
    }
}
