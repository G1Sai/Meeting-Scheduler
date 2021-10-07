using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Meeting_Scheduler.Util
{
    //Background Service to update the "Util.users" variable periodically.
    public class UserUpdate : IHostedService, IDisposable
    {
        private Timer _timer;

        public Task StartAsync(CancellationToken stoppingToken)
        {         
            //Update frequency can be changed here.
            _timer = new Timer(Readfile, null, TimeSpan.Zero,
                TimeSpan.FromMinutes(1));
            return Task.CompletedTask;
        }

        public void Readfile(object state)
        {
            List<User> users = new List<User>();
            //User file name can be changed here.
            using (System.IO.StreamReader file = new System.IO.StreamReader(@"Static/TextFile.txt"))
            {
                DateTime d1;
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
            }
            Util.users = users;
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
