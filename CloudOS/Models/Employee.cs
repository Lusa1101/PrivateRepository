using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudOS.Models
{
    internal class Employee
    {
        public int Id { get; set; }
        public string? Department { get; set; }
        public string? Role { get; set; }
        private string? Password { get; set; }
    }
}
