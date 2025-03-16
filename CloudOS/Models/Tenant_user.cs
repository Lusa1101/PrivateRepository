using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudOS.Models
{
    internal class Tenant_user
    {
        public int Tenant_id { get; set; }
        public int Id { get; set; }
        public DateOnly DateCreated { get; set;}
    }
}
