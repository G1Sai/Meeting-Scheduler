using System;

public static class Util
{
    public static DateTime NextDay(DateTime dt, int opening_time)
    {
        return dt.AddDays(1).Date + new TimeSpan(opening_time, 00, 00);
    }

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

    public static List<DateTime> FreeSlots(List<List<DateTime>> meetings, int opening_time, int closing_time, DateTime st, DateTime et, int minutes)
    {
        List<DateTime> free_slots = new List<DateTime>();
        List<DateTime> busy_slots = Util.BusySlots(meetings, st, et);
        DateTime time_slot = st;
        int slots_required = Convert.ToInt32(Math.Ceiling(minutes / 30d));
        if (et.Hour > closing_time)
        {
            et = et.Date + new TimeSpan(closing_time, 00, 00);
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
                    if (temp_dt.Hour > closing_time)
                    {
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
                    free_slots.Add(time_slot);
            }
            if (flag)
            {
                flag = false;
                time_slot = time_slot.AddHours(0.5);
                continue;
            }
            if (time_slot.Hour < closing_time && time_slot.Hour >= opening_time)
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

    public static List<User> Readfile(string fil)
    {
        List<User> users = new List<User>();
        System.IO.StreamReader file = new System.IO.StreamReader(fil);
        DateTime d1;

        string[] idate = { "M/d/yyyy h:mm:ss tt", "M/dd/yyyy h:mm:ss tt" };
        DateTime d2;
        while (!file.EndOfStream)
        {
            string[] meet = file.ReadLine().Split(';');
            if (meet.Length < 3)
                continue;
            var r = users.Where(o => o.id == meet[0]).FirstOrDefault();
            if (r is not null)
            {
                d1 = Util.ParseDates(meet[1]);
                d2 = Util.ParseDates(meet[2]);
                r.AddMeeting(d1, d2);
            }
            else
            {
                d1 = Util.ParseDates(meet[1]);
                d2 = Util.ParseDates(meet[2]);
                users.Add(new User { id = meet[0], meetings = new List<List<DateTime>> { new List<DateTime> { d1, d2 } } });
            }
        }
        return users;
    }

    public static DateTime ParseDates(string dateString)
    {
        string[] formats = { "M/d/yyyy h:mm:ss tt", "M/dd/yyyy h:mm:ss tt" };
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
