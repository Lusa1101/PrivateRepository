using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudOS.Models
{
    internal class Tenant
    {
        public int Tenant_id { get; set; }
        public string? Tenant_name { get; set; }
        public string? Subscription_plan { get; set; }

        public DateOnly DateCreated { get; set; }
    }
}
