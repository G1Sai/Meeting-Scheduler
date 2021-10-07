using System;
using System.Collections.Generic;

namespace Meeting_Scheduler
{
    public class User
    {
        public string id { get; set; }
        public List<List<DateTime>> meetings { get; set; }

        public void AddMeeting(DateTime dt1, DateTime dt2)
        {
            this.meetings.Add(new List<DateTime> { dt1, dt2 });
        }
    }
}
