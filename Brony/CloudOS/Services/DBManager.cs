using Npgsql;
using System.Collections;
using Microsoft.Extensions.Configuration;
using CloudOS.Models;

namespace CloudOS
{
    internal class DBManager
    {
        private string? _connectionString;

        public DBManager()
        {
            _connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING", EnvironmentVariableTarget.User);
            IDictionary variables = Environment.GetEnvironmentVariables();
            foreach (DictionaryEntry entry in variables )
            {
                Console.WriteLine($"{entry.Key}: {entry.Value}");
            }
        }

        public NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        async Task<int> ExecuteScalarQueryAsync(string query)
        {
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(query, connection))
                {
                    var result = await command.ExecuteScalarAsync();
                    return (int)(result != null ? result : -1);
                }
            }
        }

        //Execute non-queries
        //Returns -1 for CREATE and DROP table
        //Return number of affected rows for INSERT, DELETE and UPDATE
        async Task<int> ExecuteNonQueryAsync(string query)
        {
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(query, connection))
                    return await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<bool> AddTenant(string subscription_plan)
        {
            string query = $"insert into tenant(subscription_plan) values ('{subscription_plan}');";
            int result = await ExecuteNonQueryAsync(query);

            return result > 0;
        }
        async Task<int> ReturnNewTenantID()
        {
            string query = "select max(tenant_id) from tenant;";

            return await ExecuteScalarQueryAsync(query);
        }
        public async Task<bool> AddCompany(Company company, string subscription_plan)
        {
            int result = 0;
            //Add the tenant
            if (await AddTenant(subscription_plan))
            {
                //Add the company
                string companyQuery = "insert into company(name, registration_no, tax_no, address, contact_no, email)" +
                            $" values ('{company.Name}', '{company.Registration_no}', '{company.Tax_no}', '{company.Address}', '{company.Contact_no}', '{company.Email}')";
                result = await ExecuteNonQueryAsync(companyQuery);

                //Add the Tenant_company
                string companyTenantQuery = $"insert into tenant_company (tenant_id, company_id) values ({ReturnNewTenantID()}, {company.Company_id}); ";
                if (result > 0)
                    result = await ExecuteNonQueryAsync(companyTenantQuery);
            }

            return result > 0;
        }

        public async Task<bool> AddPerson(Person person, string subscription_plan)
        {
            int result = 0;
            //Add the tenant
            if (await AddTenant(subscription_plan))
            {
                //Add the company
                string companyQuery = "insert into person(id, names, surname, address, cell, email, type)" +
                            $" values ({person.Id}, '{person.Names}', '{person.Surname}', '{person.Address}', '{person.Cell}', '{person.Email}', '{person.Type}');";
                result = await ExecuteNonQueryAsync(companyQuery);

                //Add the Tenant_company
                string companyTenantQuery = $"insert into tenant_user (tenant_id, id) values ({ReturnNewTenantID()}, {person.Id}); ";
                if (result > 0)
                    result = await ExecuteNonQueryAsync(companyTenantQuery);
            }

            return result > 0;
        }

        public async Task<bool> AddVM(Virtual_Machine vm)
        {
            int result = 0;
            string query = "insert into virtual_machine (uuid, tenant_id, name, os_type, memory_size, cpus)" +
                            $" values ('{vm.UUID}', {vm.Tenant_id}, '{vm.Name}', '{vm.OS_type}', {vm.Memory_size}, {vm.CPUs});";

            result = await ExecuteNonQueryAsync(query);

            return result > 0;
        }

        public async Task<bool> AddUser(Tenant_user user, string subscription_plan)
        {
            int result = 0;
            if (await AddTenant(subscription_plan))
            {
                string query = $"insert into tenant_user(subscription) values ('{subscription_plan}');";
                result = await ExecuteNonQueryAsync(query);
            }

            return result > 0;
        }

        public async Task<List<Tenant>> ReturnTenants()
        {
            List<Tenant> tenants = new List<Tenant>();
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();
                string query = "select tenant_id, subscription_plan from tenant;";
                var command = new NpgsqlCommand(query, connection);
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Tenant temp = new();
                        temp.Tenant_id = reader.GetInt32(0);
                        temp.Subscription_plan = reader.GetString(1);

                        //Add to the list
                        tenants.Add(temp);
                    }
                }
            }

            return tenants;
        }

        public async Task<List<Virtual_Machine>> ReturnVMs()
        {
            List<Virtual_Machine> vms = new();
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();
                string query = $"select uuid, name, os_type, memory_size, cpus from virtual_machine;";
                var command = new NpgsqlCommand(query, connection);
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Virtual_Machine vm = new();
                        vm.UUID = reader.GetString(0);
                        vm.Name = reader.GetString(1);
                        vm.OS_type = reader.GetString(2);
                        vm.Memory_size = reader.GetInt32(3);
                        vm.CPUs = reader.GetInt32(4);

                        //Add to the list
                        vms.Add(vm);
                    }
                }
            }

            return vms;
        }
    }
}
