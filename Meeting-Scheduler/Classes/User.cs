using System;
using System.Collections.Generic;

namespace Meeting_Scheduler
{
    //
    //Summary : 
    //      User object contains id and a List of meetings.
    //      A meeting has two DateTime objects - the meeting start time and meeting end time.
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
