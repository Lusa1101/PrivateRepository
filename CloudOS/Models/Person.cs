using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudOS.Models
{
    internal class Person
    {
        public long Id { get; set; }
        public string? Names { get; set; }
        public string? Surname { get; set; }
        public string? Address { get; set; }
        public string? Cell { get; set; }
        public string? Email { get; set; }

        public string? Type = "tenant";


    }
}
