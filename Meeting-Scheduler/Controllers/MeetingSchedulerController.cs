using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;


namespace Meeting_Scheduler.Controllers
{
    [ApiController]
    [Route("[controller]")]
    //Summary: 
    //  Meeting Scheduler Controller.
    public class MeetingSchedulerController : ControllerBase
    {
        [System.Web.Http.HttpGet]
        public IEnumerable<string> Get(string ids, int length, string dates, string hours, string timeZone)
        {
            List<User> meeting_users;
            int meeting_length, office_closing, office_opening;
            DateTime earliest_dt, latest_dt;
            TimeZoneInfo currentTimeZone;

            (meeting_users, meeting_length, office_opening, office_closing, earliest_dt, latest_dt, currentTimeZone) = Util.Util.Seggregator(ids, length, dates, hours, timeZone);

            List<List<DateTime>> busy_slots_adjusted = new List<List<DateTime>>();
            foreach (User meeting_user in meeting_users)
            {
                List<List<DateTime>> busy_slots = meeting_user.meetings.Where(o => (latest_dt >= o[0] && earliest_dt <= o[0]) || (latest_dt >= o[1] && earliest_dt <= o[1])).ToList();
                foreach (List<DateTime> meet in busy_slots)
                {
                    if (meet[1] > earliest_dt)
                    {
                        if (meet[0] < earliest_dt)
                        {
                            busy_slots_adjusted.Add(new List<DateTime> { earliest_dt, meet[1] });
                        }
                        else
                        {
                            busy_slots_adjusted.Add(new List<DateTime> { meet[0], meet[1] });
                        }
                    }
                    else
                    if (meet[0] < latest_dt)
                    {
                        if (meet[1] > latest_dt)
                        {
                            busy_slots_adjusted.Add(new List<DateTime> { meet[0], latest_dt });
                        }
                        else
                        {
                            busy_slots_adjusted.Add(new List<DateTime> { meet[0], meet[1] });
                        }
                    }
                }
            }
            return Util.Util.FreeSlots(busy_slots_adjusted, office_opening, office_closing, earliest_dt, latest_dt, meeting_length, currentTimeZone);
        }
    }
}
