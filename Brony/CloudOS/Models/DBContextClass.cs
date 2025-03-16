using Microsoft.EntityFrameworkCore;
using CloudOS.Models;

namespace CloudOS.Models
{
    internal class DBContextClass : DbContext
    {
        public DBContextClass(DbContextOptions dbCO) :base(dbCO)
        {

        }
        public DbSet<Virtual_Machine> VirtualMachines { get; set; } = null!;
        public DbSet<Company> Companies { get; set; }
        public DbSet<Person> People { get; set; }
        //public DbSet<Tenant_view>
    }
}
