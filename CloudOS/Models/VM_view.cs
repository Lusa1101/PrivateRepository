using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudOS.Models
{
    public class VM_view
    {
        public int Tenant_id { get; set; }
        public string? Name { get; set; }
        public string? Subscription_plan { get; set; }
        public string? VM_names { get; set; }
        public string? OS_type { get; set; }
        public int Memory_size { get; set; }
        public int Cpus {  get; set; }
    }
}
