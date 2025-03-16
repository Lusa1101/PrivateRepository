using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudOS.Models
{
    internal class Virtual_Machine
    {
        public int Tenant_id { get; set; }
        public string? UUID { get; set; } //created by the VB; Universal Unique Identifier
        public string? Name { get; set; }
        public string? OS_type { get; set; } = "ubuntu_64";
        public int Memory_size { get; set; } = 256;
        public int CPUs { get; set; } = 2;
        public DateTime DateCreated { get; set; } = DateTime.Now;

    }
}
