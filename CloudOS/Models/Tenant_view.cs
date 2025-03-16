using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudOS.Models
{
    public class Tenant_view
    {
        public int Tenant_id { get; set; }
        public string? Name { get; set; }
        public string? Tenant_type { get; set; }
        public string? Address { get; set; }
        public string? Subscription_plan { get; set; }
    }
}
