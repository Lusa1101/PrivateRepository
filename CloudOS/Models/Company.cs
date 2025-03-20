using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudOS.Models
{
    internal class Company
    {
        public decimal Company_id { get; set; }
        public string? Name { get; set; }
        public string? Registration_no { get; set; }
        public string? Tax_no { get; set; }
        public string? Address {  get; set; }
        public string? Contact_no { get; set; }
        public string? Email { get; set; }
    }
}
