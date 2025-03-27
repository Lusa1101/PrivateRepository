using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CloudOS.Models;
using CloudOS.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CloudOS.ViewModels
{
    /***        Functions of the Tenant **/
    //Registration
    //Login
    //VM Management

    partial class TenantViewModel : ObservableObject
    {
        //Function for input validation
        Functions functions = new();

        //Class for data retrieval
        DBManager dbManager = new();

        //For VBoxManage
        VBManager vbManager = new VBManager();

        //Layouts' visibilities
        [ObservableProperty]
        bool registerLayout;
        [ObservableProperty]
        bool loginLayout;
        [ObservableProperty]
        bool tenantLayout;
        [ObservableProperty]
        bool vmLayout;

        [ObservableProperty]
        string? selectedOption;

        //Tenant-Type based registration
        [ObservableProperty]
        string? companyType;
        [ObservableProperty]
        bool personalReg = false;
        [ObservableProperty]
        bool companyReg = false;

        //Variables for entries
        [ObservableProperty]
        string? text1;
        [ObservableProperty]
        string? text2;
        [ObservableProperty]
        string? text3;
        [ObservableProperty]
        string? text4;
        [ObservableProperty]
        string? text5;
        [ObservableProperty]
        string? text6;

        //Variables for validations of entries
        [ObservableProperty]
        string? check1;
        [ObservableProperty]
        string? check2;
        [ObservableProperty]
        string? check3;
        [ObservableProperty]
        string? check4;
        [ObservableProperty]
        string? check5;
        [ObservableProperty]
        string? check6;

        //The Registration Command
        public ICommand RegisterCommand { get; }

        /***        Login   ***/
        [ObservableProperty]
        string? email;
        [ObservableProperty]
        string? password;

        //Checker for password
        [ObservableProperty]
        string? passwordCheck;

        //ICommand for Login
        public ICommand LoginCommand { get; }

        //The logged-in user
        private decimal Client_id;

        /***        End of Login    ***/

        //***       Variables for the Tenants       ***/
        [ObservableProperty]
        ObservableCollection<string> plans = new();
        [ObservableProperty]
        string? selectedPlan;

        [ObservableProperty]
        ObservableCollection<Tenant> tenants = new();

        [ObservableProperty]
        string? tenantName;

        //Selected tenant
        [ObservableProperty]
        Tenant currentTenant = new();

        public ICommand AddTenantCommand { get; }
        public ICommand DeleteTenantCommand { get; }

        /***        End of Tenants      ***/

        //***       Variables for Virtual Machines          ***/
        [ObservableProperty]
        ObservableCollection<string> osTypes = new();
        [ObservableProperty]
        string? selectedOSType;

        [ObservableProperty]
        string? name;
        [ObservableProperty]
        string? memory;
        [ObservableProperty]
        string? cpus;

        public ICommand AddVMCommand { get; }
        public ICommand DeleteVMCommand { get; }
        public ICommand RunVMCommand { get; }
        public ICommand SaveVMCommand { get; }

        [ObservableProperty]
        ObservableCollection<Virtual_Machine> machines = new();

        //VMs for the current tenant


        /***        End of Virtual Machines     ***/

        public TenantViewModel()
        {
            //Set the commands
            RegisterCommand = new Command(Register);
            LoginCommand = new Command(Login);

            AddTenantCommand = new Command(AddTenant);
            DeleteTenantCommand = new Command<Tenant>(DeleteTenant);

            AddVMCommand = new Command(AddVM);
            DeleteVMCommand = new Command<Virtual_Machine>(DeleteVM);
            RunVMCommand = new Command<Virtual_Machine>(RunVM);
            SaveVMCommand = new Command<Virtual_Machine>(SaveVM);

            //Layouts to false
            ResetLayouts();

            //Set plans and tenants
            _ = SetData();

            //For testing
            Email = "thendo.mash@gmail.com";
            Password = "@Th3ndo1";
        }

        async void AddVM()
        {
            Debug.WriteLine("Reached");
            if (String.IsNullOrEmpty(Name) || String.IsNullOrEmpty(SelectedOSType))
                return;

            Virtual_Machine vm = new Virtual_Machine();
            vm.Name = Name;
            vm.OS_type = SelectedOSType;
            vm.CPUs = int.Parse(String.IsNullOrEmpty(Cpus) ? "2" : Cpus);
            vm.Memory_size = int.Parse(String.IsNullOrEmpty(Memory) ? "256" : Memory);
            vm.Tenant_id = CurrentTenant.Tenant_id;

            //We need to get the UUID and tenant_id
            string vmReg = vbManager.CreateVM(vm.Name).uuid;
            if (vmReg != "")
                vm.UUID = vmReg;
            else
            {
                Debug.WriteLine("Failed to get UUID.");
                return;
            }

            //We can now add it to the DB
            if (await dbManager.AddVM(vm))
            {
                //Update the Machines list
                Machines.Add(vm);

                //Clear out the entries
                Name = null;
                Memory = null;
                Cpus = null;

                Debug.WriteLine("Successfully created the vm.");
            }
            else
                Debug.WriteLine("Unsuccessfully created the vm.");
        }

        async void DeleteVM(Virtual_Machine vm)
        {
            //Need to delete from the VB first
            string result = "";
            if (vm.Name != null)
                result = vbManager.DeleteVM(vm.Name);

            //Now delete from the DB
            if (!result.Contains("Successful") && await dbManager.DeleteVM(vm))
            {
                //Remove from the list
                Machines.Remove(vm);

                Debug.WriteLine("VM was deleted: " + vm.Name);
            }
            else
                Debug.WriteLine("Failed to delete VM: " + vm.Name);

        }

        void RunVM(Virtual_Machine vm)
        {
            //Simply run through VB
            if (vm.Name != null)
                vbManager.StartVM(vm.Name);
            else
                Debug.WriteLine("The vm name was null.");
        }

        void SaveVM(Virtual_Machine vm)
        {
            //Save the current state of the vm and close it
            if(vm.Name != null)
                vbManager.SaveVM(vm.Name);
            else
                Debug.WriteLine("The vm name was null.");
        }

        async void AddTenant()
        {
            if (!string.IsNullOrEmpty(TenantName) && !string.IsNullOrEmpty(SelectedPlan))
            {
                Tenant tenant = new Tenant();
                tenant.Tenant_name = TenantName;
                tenant.Subscription_plan = SelectedPlan;
                tenant.Client_id = Client_id;

                //Add the tenant to the DB
                if (await dbManager.AddTenant(tenant))
                {
                    //Clear out the entriy
                    TenantName = null;

                    //Refresh the list
                    //Add to the tenants
                    Tenants.Add(tenant);

                    Debug.WriteLine("Added tenant successfully");
                }
                else
                    Debug.WriteLine("Added tenant unsuccessfully");                
            }
            else
                Debug.WriteLine("Please fill in the TenantName and Select an option");

        }

        async void DeleteTenant(Tenant tenant)
        {
            if (await dbManager.DeleteTenant(tenant.Tenant_id))
            {
                //Remove from the list
                Tenants.Remove(tenant);
            }
            else
                Debug.WriteLine("Failed to delete.");
        }

        //Login method
        async void Login()
        {
            if (!string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(Password))
            {
                //Authenticate and get the current cliwnt details
                Client_id = await dbManager.Authenticate(Email, Password);

                if (Client_id > 0)
                {
                    //Clear out the entries
                    Email = null;
                    Password = null;

                    //Set data
                    await SetData();

                    //Reset all the layouts
                    ResetLayouts();

                    //Initial layout to be tenants
                    TenantLayout = true;
                    SelectedOption = "Tenants";

                    Debug.WriteLine("Login successful.");
                }
                else
                    Debug.WriteLine("Failed to login.");
            }
            else
                Debug.WriteLine("Failed to login. Fill in the username and/or password.");
        }

        //Registration method
        async void Register()
        {
            if (!string.IsNullOrEmpty(Text1) && !string.IsNullOrEmpty(Text2) && !string.IsNullOrEmpty(Text3) && !string.IsNullOrEmpty(Text4) && !string.IsNullOrEmpty(Text5) && !string.IsNullOrEmpty(Text6) && !string.IsNullOrEmpty(Password))
                if (string.IsNullOrEmpty(Check1) && string.IsNullOrEmpty(Check2) && string.IsNullOrEmpty(Check3) && string.IsNullOrEmpty(Check4) && string.IsNullOrEmpty(Check5) && string.IsNullOrEmpty(Check6) && string.IsNullOrEmpty(PasswordCheck))
                {
                    if (PersonalReg)
                    {
                        //To register the Person
                        Person person = new Person();
                        person.Names = Text1;
                        person.Surname = Text2;
                        person.Id = long.Parse(String.IsNullOrEmpty(Text3) ? "-1" : Text3);
                        person.Address = Text4;
                        person.Cell = Text5;
                        person.Email = Text6;

                        //Remember to validate the values
                        Debug.WriteLine("Personal Registration");

                        //Add to the database
                        if (await dbManager.AddPerson(person, Password))
                        {
                            Debug.WriteLine("Successfully registered. Please wait for approval.");

                            //Clear out the entries
                            Text1 = null;
                            Text2 = null;
                            Text3 = null;
                            Text4 = null;
                            Text5 = null;
                            Text6 = null;
                            Password = null;
                        }
                        else
                            Debug.WriteLine("Registration was unsuccessful.");
                    }
                    else
                    {
                        //To register the company
                        Company company = new Company();
                        company.Name = Text1;
                        company.Registration_no = Text2;
                        company.Tax_no = Text3;
                        company.Address = Text4;
                        company.Contact_no = Text5;
                        company.Email = Text6;

                        //Remember to validate the values
                        Debug.WriteLine("Company Registreation");

                        //Add to the DB
                        if (await dbManager.AddCompany(company, Password))
                        {
                            Debug.WriteLine("Successfully registered. Please wait for approval.");

                            //Clear out the entries
                            Text1 = null;
                            Text2 = null;
                            Text3 = null;
                            Text4 = null;
                            Text5 = null;
                            Text6 = null;
                            Password = null;
                        }
                        else
                            Debug.WriteLine("Registration was unsuccessful.");

                    }
                }
                else
                    Debug.WriteLine("Please complete all the entries.");

            
        }

        //To set the data for the collections
        async Task SetData()
        {
            //For the Pickers
            Plans = new ObservableCollection<string>() { "Free-tier", "Gold", "Platinum" };
            OsTypes = new ObservableCollection<string>() { "Ubuntu_64", "Windows_64", "Debian_64", "Redhat_64" };

            //Retrieve required data from the database            
            if (Client_id > 0)
            {
                Tenants = new ObservableCollection<Tenant>(await dbManager.ReturnTenantsById(Client_id));
                Tenants.Add(new Tenant());
            }

            if(CurrentTenant.Tenant_id > 0)
                Machines = new ObservableCollection<Virtual_Machine>(await dbManager.ReturnVMById(CurrentTenant.Tenant_id));
        }

        void ResetLayouts()
        {
            RegisterLayout = false;
            LoginLayout = false;
            TenantLayout = false;
            VmLayout = false;
        }

        /***        Changes made on the Observable-properties ***/

        //Changing through different layouts
        void ResetValues()
        {
            //This is the same as login out, so we need to restart the class
            Client_id = -1;
            //CurrentTenant = Tenants.FirstOrDefault(temp => temp.Tenant_id == -1);
        }
        partial void OnSelectedOptionChanged(string? value)
        {
            //Reset the layouts
            ResetLayouts();

            switch (value)
            {
                case "Register":
                    RegisterLayout = true;
                    ResetValues();
                    break;
                case "Virtual Machines":
                    if (CurrentTenant.Tenant_id > 0)
                    {
                        VmLayout = true;
                    }
                    else
                        Debug.WriteLine("Please login or select a tenant.");
                    break;
                case "Tenants":
                    if (Client_id > 0)
                    {
                        TenantLayout = true;
                    }
                    else
                        Debug.WriteLine("Please login.");
                    break;
                default:
                    LoginLayout = true;
                    ResetValues();
                    break;

            }

        }
        partial void OnCompanyTypeChanged(string? value)
        {
            if (value == "Personal")
            {
                CompanyReg = false;
                PersonalReg = true;
            }
            else
            {
                PersonalReg = !PersonalReg;
                CompanyReg = true;
            }
        }

        //To listen to the changes in the Type
        partial void OnCompanyRegChanged(bool value)
            => PersonalReg = !value;

        partial void OnPersonalRegChanged(bool value)
            => CompanyReg = !value;

        partial void OnCurrentTenantChanged(Tenant? oldValue, Tenant newValue)
        {
            if (oldValue == newValue || oldValue != newValue)
            {
                SelectedOption = "Virtual Machines";
                Machines = new ObservableCollection<Virtual_Machine>(dbManager.ReturnVMById(CurrentTenant.Tenant_id).Result);
            }
        }

        /***        End of Changes made on the Observable-properties        ***/


        /***        Input Validation    ***/

        partial void OnText1Changed(string? value)
        {
            if (!String.IsNullOrEmpty(value) && !(functions.StringValidator(value)))
                Check1 = "Not a valid name.";
            else
                Check1 = string.Empty;
        }

        partial void OnText2Changed(string? value)
        {
            if (!String.IsNullOrEmpty(value) && PersonalReg && !(functions.StringValidator(value)))
                Check2 = "Not a valid surname.";
            else
                Check2 = string.Empty;

        }

        partial void OnText3Changed(string? value)
        {
            if (!String.IsNullOrEmpty(value) && !(functions.NumberValidator(value, PersonalReg)))
                Check3 = "Not a vaild number input.";
            else
                Check3 = string.Empty;
        }

        partial void OnText4Changed(string? value)
        {
            if (!String.IsNullOrEmpty(value) && !(functions.SpecialCharChecker(value)))
                Check4 = "Not a valid address.";
            else
                Check4 = string.Empty;
        }

        partial void OnText5Changed(string? value)
        {
            if (!String.IsNullOrEmpty(value) && !(functions.CellValidator(value)))
                Check5 = "Not a vaild number input.";
            else
                Check5 = string.Empty;
        }

        partial void OnText6Changed(string? value)
        {
            if (!LoginLayout && !String.IsNullOrEmpty(value) && !(functions.EmailValidator(value)))
                Check6 = "Not a vaild email input.";
            else
                Check6 = string.Empty;
        }

        partial void OnPasswordChanged(string? value)
        {
            if (!LoginLayout && !string.IsNullOrEmpty(value))
                PasswordCheck = functions.PasswordChecker(value);
        }

        partial void OnCpusChanged(string? value)
        {
            if (!String.IsNullOrEmpty(value) && !functions.NumberValidator(value))
                Check2 = "Enter valid number of CPUS.";
            else
                Check2 = string.Empty;
        }

        partial void OnMemoryChanged(string? value)
        {
            if (!String.IsNullOrEmpty(value) && !functions.NumberValidator(value))
                Check2 = "Enter valid number of Memory.";
            else
                Check2 = string.Empty;
        }
        /***        End of Input Validation     **/
    }
}
