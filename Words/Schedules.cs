using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Words
{
    public enum ScheduleType
    {
        Learning,
        Reviewing
    }

    public class Schedules
    {
        public DateTime Date { get; set; }
        public IList<(ScheduleType, (int, int))> Lists { get; set; } = new List<(ScheduleType, (int, int))>();
    }
}
