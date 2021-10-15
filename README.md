MeetingScheduler API

INTRODUCTION
MeetingScheduler API returns a list of suitable meeting times according to the availability of all participating people within a 
desired period and between office opening and closing hours. All the meeting times are adjusted according to the client's desired
TimeZone. If no TimeZone is provided, the client is assumed to be living in the same TimeZone as the Server.

Employee Meeting Information is updated in the background. The frequency of updation can be changed in Util.UserUpdate.StartAsync

FileName of the file containing all meetings information can be changed in Util.UserUpdate.ReadFile


REQUEST FORMAT:
https://<website_name>/meetingscheduler/?ids=<employee_id1>;<employee_id2>&length=<meeting_length>&dates=<desired_start_date>;<desired_end_date>&hours=<office_opening_time>;<office_closing_time>&timezone=<timezone>

employee_ids seperated by semi-colon(;)
dates seperated by semi-colon(;)
hours seperated by semi-colon(;)
timezone use plus(+) in place of space(' '). timezone is optional. default is System Time Zone.


SAMPLE REQUEST:
https://localhost:44387/meetingscheduler/?ids=69405402307022621117331807458848852688;115067134195675490853540122833412221063;192567945604795981125867006303457825642&length=60&dates=20150218020000;20150219190000&hours=0900;1800&timezone=India+Standard+Time


SAMPLE RESPONSE:
["February 18, 2015 - 09:00 To 10:00","February 18, 2015 - 09:30 To 10:30","February 18, 2015 - 10:00 To 11:00","February 18, 2015 - 10:30 To 11:30","February 18, 2015 - 11:00 To 12:00","February 18, 2015 - 11:30 To 12:30","February 18, 2015 - 12:00 To 13:00","February 19, 2015 - 09:00 To 10:00","February 19, 2015 - 09:30 To 10:30","February 19, 2015 - 10:00 To 11:00","February 19, 2015 - 10:30 To 11:30","February 19, 2015 - 11:00 To 12:00","February 19, 2015 - 11:30 To 12:30"]
