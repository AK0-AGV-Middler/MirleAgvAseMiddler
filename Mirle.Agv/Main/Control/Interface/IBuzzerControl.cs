using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.AgvAseMiddler.Model;

namespace Mirle.AgvAseMiddler.Controller
{
    public interface IBuzzerControl
    {
        bool SetAlarm(Alarm alarm);
        bool SetAlarmStatus(bool hasAlarm, bool hasWarn);
        bool ResetAllAlarm();
        void StopBuzzer();
    }
}
