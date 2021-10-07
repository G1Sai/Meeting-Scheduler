using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;


namespace Meeting_Scheduler.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MeetingSchedulerController : ControllerBase
    {
        private readonly ILogger<MeetingSchedulerController> _logger;

        public MeetingSchedulerController(ILogger<MeetingSchedulerController> logger)
        {
            _logger = logger;
        }

        [System.Web.Http.HttpGet]
        public IEnumerable<DateTime> Get(string ids, int length, string dates, string hours)
        {

            List<User> users = Util.Util.users;
            string[] eLTimes = dates.Split(';');
            string[] officeHours = hours.Split(';');

            DateTime earliest_dt = Util.Util.ParseDates(eLTimes[0]);
            DateTime latest_dt = Util.Util.ParseDates(eLTimes[1]);
            DateTime defaultDate = new DateTime();

            if(earliest_dt.Equals(defaultDate)||latest_dt.Equals(defaultDate))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("Invalid Date-Time Format!"), ReasonPhrase = "Invalid Date-Time Format of 'Earliest Meeting Time' or 'Latest Meeting Time'! Enter them in the format 'yyyyMMddHHmmss', where HH is in 24-Hour Format." });
            }

            if (latest_dt<=earliest_dt)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("Earliest Meeting Time later than Latest Meeting Time!"), ReasonPhrase = "Enter 'Latest Meeting Time' which is Later than 'Earliest Meeting Time'!" });
            }

            if (officeHours[0].Length!=4|| officeHours[1].Length != 4)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("Invalid Office Hours!"), ReasonPhrase = "Enter Office Opening and Closing Hours in HHmm Format!" });
            }

            int office_opening = Convert.ToInt32(officeHours[0]);
            int office_closing = Convert.ToInt32(officeHours[1]);

            if (office_closing<=office_opening)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest){ Content = new StringContent("Invalid Office Hours!"), ReasonPhrase = "Office close time earlier than Office close time!" });
            }

            
            List<string> emp_ids = ids.Split(';').ToList();
            int meeting_length = length;
            
            List<User> meeting_users = users.Where(o => emp_ids.Contains(o.id)).ToList();
            if (meeting_users.Count!=emp_ids.Count)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("Invalid Employee id!"), ReasonPhrase = "Enter Correct Employee IDs!" });
            }
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
            return Util.Util.FreeSlots(busy_slots_adjusted, office_opening, office_closing, earliest_dt, latest_dt, meeting_length);
        }
    }
}
