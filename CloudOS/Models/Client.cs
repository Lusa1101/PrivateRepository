using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudOS.Models
{
    public class Client
    {
        public decimal Client_id { get; set; }
        public string? Client_type { get; set; }
        public string? Password { get; set; }
        public bool Approved { get; set; }
    }
}
