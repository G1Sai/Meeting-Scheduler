using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Meeting_Scheduler
{
    //Summary: 
    //  Used for creating Custom Exceptions.
    public class CustomException : Exception
    {
        public string Content { get; set; }
        public string Solution { get; set; }
        public CustomException(string Content, string Solution)
        {
            this.Content = Content;
            this.Solution = Solution;
        }
    }
}
