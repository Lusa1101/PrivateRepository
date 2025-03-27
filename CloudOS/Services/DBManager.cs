using Npgsql;
using System.Collections;
using Microsoft.Extensions.Configuration;
using CloudOS.Models;
using System.Diagnostics;

namespace CloudOS
{
    internal class DBManager
    {
        private string? _connectionString;

        public DBManager(string username = "application", string password = "application")
        {
            //Use the admin's PSQL credentials for accessing the db
            //Must have login credentials that are default for the application
            _connectionString = $"Host=localhost;Port=5432;Database=cloud_os;Username={username};Password={password};";

        }

        public NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        async Task<long> ExecuteScalarQueryAsync(string query)
        {
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(query, connection))
                {
                    try
                    {
                        var result = await command.ExecuteScalarAsync();
                        return (long)(result != null ? result : -1);
                    }
                    catch(Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        return -1;
                    }
                }
            }
        }

        async Task<decimal> ExecuteScalarQueryAsyncDec(string query)
        {
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(query, connection))
                {
                    try
                    {
                        decimal result = Convert.ToDecimal(await command.ExecuteScalarAsync());
                        return result > 0 ? result : -1;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        return -1;
                    }
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
                {
                    try
                    {
                        return await command.ExecuteNonQueryAsync();
                    }
                    catch(Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        return -1;
                    }
                }
            }
        }

        public async Task<bool> AddTenant(Tenant tenant)
        {
            string query = $"insert into tenant(client_id, tenant_name, subscription_plan) values ({tenant.Client_id}, '{tenant.Tenant_name}', '{tenant.Subscription_plan}');";
            int result = await ExecuteNonQueryAsync(query);

            return result > 0;
        }
        async Task<int> ReturnNewTenantID()
        {
            string query = "select max(tenant_id) from tenant;";

            return (int)(await ExecuteScalarQueryAsync(query));
        }
        async Task<decimal> ReturnNewCompanyID(string contact)
        {
            string query = $"select company_id from company where contact_no = '{contact}';";

            return await ExecuteScalarQueryAsyncDec(query);
        }

        public async Task<bool> AddCompany(Company company, string password)
        {
            int result = 0;
            //Add the company
            string companyQuery = $"insert into company(name, registration_no, tax_no, address, contact_no, email) values ('{company.Name}', '{company.Registration_no}', '{company.Tax_no}', '{company.Address}', '{company.Contact_no}', '{company.Email}')";
            result = await ExecuteNonQueryAsync(companyQuery);

            //Add the client
            decimal company_id = (company.Contact_no != null)? (await ReturnNewCompanyID(company.Contact_no)) : 0;
            string clientQuery = $"insert into client (client_id, client_type, password) values ({company_id}, 'company', '{password}'); ";
            if (result > 0)
                result = await ExecuteNonQueryAsync(clientQuery);
            else
                await DeleteCompany(company);

                return result > 0;
        }

        public async Task<bool> AddPerson(Person person, string password)
        {
            int result = 0;
            //Add the person
            string personQuery = "insert into person(id, names, surname, address, cell, email, type)" +
                        $" values ({person.Id}, '{person.Names}', '{person.Surname}', '{person.Address}', '{person.Cell}', '{person.Email}', '{person.Type}');";
            result = await ExecuteNonQueryAsync(personQuery);

            //Add the client
            string clientQuery = $"insert into client (client_id, client_type, password) values ({person.Id}, 'personal', '{password}'); ";
            if (result > 0)
                result = await ExecuteNonQueryAsync(clientQuery);
            else
                await DeletePerson(person);

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

        async Task<bool> DeleteCompany(Company company)
        {
            int result = 0;
            string query = $"delete from company where company_id = {company.Company_id}";

            result = await ExecuteNonQueryAsync(query);

            return result > 0;
        }

        async Task<bool> DeletePerson(Person person)
        {
            int result = 0;
            string query = $"delete from person where id = {person.Id}";

            result = await ExecuteNonQueryAsync(query);

            return result > 0;
        }

        public async Task<bool> DeleteVM(Virtual_Machine vm)
        {
            int result = 0;
            string query = $"delete from virtual_machine where UUID = '{vm.UUID}'";

            result = await ExecuteNonQueryAsync(query);

            return result > 0;
        }

        public async Task<bool> DeleteTenant(int tenant_id)
        {
            int result = 0;
            string query = $"delete from tenant where tenant_id = {tenant_id}";

            result = await ExecuteNonQueryAsync(query);

            return result > 0;
        }

        public async Task<bool> ApproveClient(decimal client_id)
        {
            string query = $"update client set approved = '1' where client_id = {client_id};";
            int result = await ExecuteNonQueryAsync(query);

            return result > 0;
        }

        public async Task DeclineClient(int client_id)
        {
            await Task.Delay(10);
        }

        public async Task<decimal> Authenticate(string email, string password)
        {
            decimal result = -1;
            Client client = new();
            string query = $"select client_id from client_auth_view where approved = '1' AND email = '{email}' AND password = '{password}';";
            result = await ExecuteScalarQueryAsyncDec(query);

            return result;
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
        public async Task<List<Tenant>> ReturnTenantsById(decimal client_id)
        {
            List<Tenant> tenants = new List<Tenant>();
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();
                string query = $"select tenant_id, tenant_name, subscription_plan from tenant where client_id = {client_id};";
                var command = new NpgsqlCommand(query, connection);
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Tenant temp = new();
                        temp.Tenant_id = reader.GetInt32(0);
                        temp.Tenant_name = reader.GetString(1);
                        temp.Subscription_plan = reader.GetString(2);

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

        public async Task<List<Virtual_Machine>> ReturnVMById(int tenant_id)
        {
            List<Virtual_Machine> vms = new();
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();
                string query = $"select uuid, name, os_type, memory_size, cpus from virtual_machine where tenant_id = {tenant_id};";
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

        public async Task<List<Client_view>> ReturnClientViews()
        {
            List<Client_view> clients = new List<Client_view>();
            using(var connection = CreateConnection())
            {
                await connection.OpenAsync();
                string query = "select name, address, type, client_id from client_view where approved = '0';";
                var command = new NpgsqlCommand(query, connection); 

                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Client_view client = new();
                        client.Name = reader.GetString(0);
                        client.Address = reader.GetString(1);
                        client.Type = reader.GetString(2);
                        client.Client_id = reader.GetInt64(3);

                        //Add to clients
                        clients.Add(client);
                    }
                }
            }
            return clients;
        }

        public async Task<List<Tenant_view>> ReturnTenantViews()
        {
            List<Tenant_view> tenants = new();
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();
                string query = "select tenant_id, name, type, address, subscription_plan from tenant_view";
                var command = new NpgsqlCommand(query, connection);
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var tenant = new Tenant_view();
                        tenant.Name = reader.GetString(1);
                        tenant.Tenant_id = reader.GetInt32(0);
                        tenant.Tenant_type = reader.GetString(2);
                        tenant.Address = reader.GetString(3);
                        tenant.Subscription_plan = reader.GetString(4);

                        //Add to tenants
                        tenants.Add(tenant);
                    }
                }
            }

            return tenants;
        }

        public async Task<List<VM_view>> ReturnVMViews()
        {
            List<VM_view> vms = new();
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();
                string query = "select tenant_id, name, subscription_plan, vm_names, os_type, memory_size, cpus from vm_view;";
                var command = new NpgsqlCommand(query, connection);
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var vm = new VM_view();
                        vm.Tenant_id = reader.GetInt32(0);
                        vm.Name = reader.GetString(1);
                        vm.Subscription_plan = reader.GetString(2);
                        vm.VM_names = reader.GetString(3);
                        vm.OS_type = reader.GetString(4);
                        vm.Memory_size = reader.GetInt32(5);
                        vm.Cpus = reader.GetInt32(6);

                        //Add to vms
                        vms.Add(vm);
                    }
                }
            }
            return vms;
        }
    }
}
