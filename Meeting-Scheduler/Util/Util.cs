using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Meeting_Scheduler.Util
{
    //
    // Summary:
    //      All the Utilities required for processing the request.
    public static class Util
    {
        public static List<User> users { get; set; }
        public static readonly HashSet<string> AllTimeZoneIds =
    new HashSet<string>(TimeZoneInfo.GetSystemTimeZones()
                                    .Select(tz => tz.Id.ToLower()));

        //
        // Summary:
        //     Converts the raw parameters into relevant variables.
        //
        // Parameters:
        //   ids:
        //      String containing ids of required employees.
        //
        //   length:
        //      Length of the meeting.
        //
        //   dates:
        //      String containing the start and end dates in the yyyyMMddHHmmss Format
        //
        //   timeZone:
        //      String containing the timeZone
        //
        // Returns:
        //      A tuple containing List of Users, length, Office Opening Time, Office Closing Time, Desired start date, Desired end date, and a TimeZoneInfo objects.
        public static (List<User>, int, int, int, DateTime, DateTime, TimeZoneInfo) Seggregator(string ids, int length, string dates, string hours, string timeZone)
        {
            if (ids == null || length == 0 || dates == null || hours == null)
            {
                throw new CustomException("Parameters Insufficient!", "Provide all Parameters correctly!");
            }
            List<User> users = Util.users;
            string[] eLTimes = dates.Split(';');
            string[] officeHours = hours.Split(';');
            TimeZoneInfo currentTimeZone;

            if (timeZone == null)
            {
                currentTimeZone = TimeZoneInfo.Local;
            }
            else
            {
                timeZone=timeZone.Replace('+',' ').ToLower();
                if (!AllTimeZoneIds.Contains(timeZone))
                {
                    throw new CustomException("Invalid Timezone!", "Try Entering a Valid Timezone!");
                }
                else
                {
                    currentTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                }
            }
            DateTime earliest_dt = TimeZoneInfo.ConvertTimeToUtc(Util.ParseDates(eLTimes[0]), currentTimeZone);
            DateTime latest_dt = TimeZoneInfo.ConvertTimeToUtc(Util.ParseDates(eLTimes[1]), currentTimeZone);
            DateTime defaultDate = TimeZoneInfo.ConvertTimeToUtc(new DateTime(), currentTimeZone);

            if (earliest_dt.Equals(defaultDate) || latest_dt.Equals(defaultDate))
            {
                throw new CustomException("Invalid Date-Time Format!", "Invalid Date-Time Format of 'Earliest Meeting Time' or 'Latest Meeting Time'! Enter them in the format 'yyyyMMddHHmmss', where HH is in 24-Hour Format.");
            }

            if (latest_dt <= earliest_dt)
            {
                throw new CustomException("Earliest Meeting Time later than Latest Meeting Time!", "Enter 'Latest Meeting Time' which is Later than 'Earliest Meeting Time'!");
            }

            if (officeHours[0].Length != 4 || officeHours[1].Length != 4)
            {
                throw new CustomException("Invalid Office Hours!", "Enter Office Hours in HHmm format!");
            }

            defaultDate= earliest_dt.Date + DateTime.ParseExact(officeHours[0],"HHmm", null).TimeOfDay;
            defaultDate=DateTime.SpecifyKind(defaultDate,DateTimeKind.Unspecified);
            //Convert office hours to int after checking if their format is valid
            int office_opening = Convert.ToInt32(TimeZoneInfo.ConvertTimeToUtc(defaultDate,currentTimeZone).ToString("HHmm"));
            defaultDate = latest_dt.Date + DateTime.ParseExact(officeHours[1], "HHmm", null).TimeOfDay;
            defaultDate = DateTime.SpecifyKind(defaultDate, DateTimeKind.Unspecified);
            int office_closing = Convert.ToInt32(TimeZoneInfo.ConvertTimeToUtc(defaultDate, currentTimeZone).ToString("HHmm"));

            if (office_closing <= office_opening)
            {
                throw new CustomException("Invalid Office Hours!", "Office close time earlier than Office close time!");
            }


            List<string> emp_ids = ids.Split(';').ToList();
            int meeting_length = length;

            List<User> meeting_users = users.Where(o => emp_ids.Contains(o.id)).ToList();
            if (meeting_users.Count != emp_ids.Count)
            {
                throw new CustomException("Invalid Employee id!", "Enter Correct Employee IDs!");
            }

            return (meeting_users, meeting_length, office_opening, office_closing, earliest_dt, latest_dt, currentTimeZone);
        }

        //
        // Summary:
        //     Computes the next timeSlot for the next day at office opening time.
        //
        // Parameters:
        //   dt:
        //      current DateTime.
        //
        //   opening_time:
        //      Office opening hours.
        //      
        // Returns:
        //     Computed DateTime object for the next day at office opening time.
        public static DateTime NextDay(DateTime dt, int opening_time)
        {
            return dt.AddDays(1).Date + new TimeSpan(opening_time / 100, opening_time % 100, 00);
        }

        //
        // Summary:
        //     Returns a list of all the meetings between earliest and latest dates.
        //
        // Parameters:
        //   meetings:
        //     List of all meetings of the required employees.
        //
        //   earliest:
        //     Earliest Date and Time.
        //
        //   latest:
        //     Latest Date and Time.
        //
        // Returns:
        //     List of meetings between earliest DateTime and latest DateTime.
        public static List<DateTime> BusySlots(List<List<DateTime>> meetings, DateTime earliest, DateTime latest)
        {

            List<DateTime> busy_slots = new List<DateTime>();
            foreach (List<DateTime> meet in meetings)
            {
                for (DateTime i = meet[0]; DateTime.Compare(i, meet[1]) < 0; i = i.AddHours(0.5))
                {
                    busy_slots.Add(i);
                }

            }
            return busy_slots.Distinct().ToList();
        }

        //
        // Summary:
        //     Computes a list of available slots after removing all the meeting slots between opening_time and
        //     closing_time from st date to et datecand converting to relevant TimeZone.
        //
        // Parameters:
        //   opening_time:
        //     Office Opening Hours in Hmm format.
        //
        //   closing_time:
        //     Office Closing Hours in Hmm format.
        //
        //   st:
        //     Desired start date and time.
        //
        //   et:
        //     Desired end date and time.
        //
        // Returns:
        //     List of suitable meeting times in string format.
        public static List<string> FreeSlots(List<List<DateTime>> meetings, int opening_time, int closing_time, DateTime st, DateTime et, int minutes, TimeZoneInfo currentTimeZone)
        {
            List<string> free_slots = new List<string>();
            List<DateTime> busy_slots = Util.BusySlots(meetings, st, et);
            if (Convert.ToInt32(st.ToString("HHmm")) < opening_time)
            {
                st = st.Date + new TimeSpan(opening_time / 100, opening_time % 100, 00);
            }
            DateTime time_slot = st;
            int slots_required = Convert.ToInt32(Math.Ceiling(minutes / 30d));
            if (Convert.ToInt32(et.ToString("HHmm")) > closing_time)
            {
                et = et.Date + new TimeSpan(closing_time / 100, closing_time % 100, 00);
            }
            while (DateTime.Compare(time_slot, et) < 0)
            {
                bool flag = false;
                if (!busy_slots.Contains(time_slot))
                {
                    DateTime temp_dt = time_slot;
                    for (int i = 1; i < slots_required; i++)
                    {
                        temp_dt = temp_dt.AddHours(0.5);
                        if (Convert.ToInt32(temp_dt.ToString("HHmm")) < opening_time || Convert.ToInt32(temp_dt.ToString("HHmm")) >= closing_time)
                        {

                            time_slot = Util.NextDay(time_slot, opening_time);
                            flag = true;
                            break;
                        }
                        if (DateTime.Compare(temp_dt, et) >= 0)
                        {
                            time_slot = time_slot.AddHours(0.5);
                            flag = true;
                            break;
                        }
                        if (busy_slots.Contains(temp_dt))
                        {
                            time_slot = temp_dt.AddHours(0.5);
                            flag = true;
                            break;
                        }

                    }
                    if (!flag)
                        free_slots.Add(TimeZoneInfo.ConvertTimeFromUtc(time_slot,currentTimeZone).ToString("MMMM dd, yyyy - HH:mm")+" To "+ TimeZoneInfo.ConvertTimeFromUtc(time_slot.AddMinutes(minutes), currentTimeZone).ToString("HH:mm"));
                }
                if (flag)
                {
                    flag = false;
                    continue;
                }
                if (Convert.ToInt32(time_slot.ToString("HHmm")) < closing_time && Convert.ToInt32(time_slot.ToString("HHmm")) >= opening_time)
                {
                    time_slot = time_slot.AddHours(0.5);
                }
                else
                {
                    time_slot = Util.NextDay(time_slot, opening_time);
                }
            }
            return free_slots;
        }

        //
        // Summary:
        //     Converts the dateString into a DateTime object. Returns object with '1/1/0001 12:00:00 AM' if in invalid
        //     format.
        //
        // Parameters:
        //   dateString:
        //      The string containing the date to be parsed
        //
        // Returns:
        //     Created DateTime object.
        public static DateTime ParseDates(string dateString)
        {
            string[] formats = { "M/d/yyyy h:mm:ss tt", "M/dd/yyyy h:mm:ss tt", "yyyyMMddHHmmss" };
            DateTime parseDate = new DateTime();
            if (!DateTime.TryParse(dateString, out parseDate))
            {
                foreach (string format in formats)
                {
                    if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out parseDate))
                    {
                        break;
                    }
                }
            }
            return parseDate;
        }
    }
}
