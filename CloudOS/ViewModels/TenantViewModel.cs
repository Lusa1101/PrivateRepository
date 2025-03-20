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
        bool registerLayout = false;
        [ObservableProperty]
        bool loginLayout = false;
        [ObservableProperty]
        bool tenantLayout = false;
        [ObservableProperty]
        bool vmLayout = false;

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

        //Variables for the Tenants
        [ObservableProperty]
        ObservableCollection<string> plans = new();
        [ObservableProperty]
        string? selectedPlan;

        [ObservableProperty]
        ObservableCollection<Tenant> tenants = new();

        [ObservableProperty]
        string? tenantName;

        public ICommand AddTenantCommand { get; }

        //Variables for Virtual Machines
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

        [ObservableProperty]
        ObservableCollection<Virtual_Machine> machines = new();

        public TenantViewModel()
        {
            //Set the commands
            RegisterCommand = new Command(Register);
            LoginCommand = new Command(Login);
            AddTenantCommand = new Command(AddTenant);
            AddVMCommand = new Command(AddVM);

            //Set plans and tenants
            _ = SetData();
        }

        async void AddVM()
        {
            Debug.WriteLine("Reached");
            if (String.IsNullOrEmpty(Name) || String.IsNullOrEmpty(SelectedOSType))
                return;

            Virtual_Machine vm = new Virtual_Machine();
            vm.Name = Name;
            vm.OS_type = SelectedOSType;
            vm.CPUs = int.Parse(String.IsNullOrEmpty(Cpus) ? "-1" : Cpus);

            //We need to get the UUID and tenant_id
            string vmReg = vbManager.CreateVM(vm.Name);
            if (!vmReg.Contains("Error"))
                vm.UUID = functions.ReturnVMUUID(vmReg);
            else
            {
                Debug.WriteLine("Failed to get UUID.");
                return;
            }

            //We can now add it to the DB
            if (await dbManager.AddVM(vm))
                Debug.WriteLine("Successfully created the vm.");
            else
                Debug.WriteLine("Unsuccessfully created the vm.");
        }

        async void AddTenant()
        {
            if (!string.IsNullOrEmpty(TenantName) && !string.IsNullOrEmpty(SelectedPlan))
            {
                Tenant tenant = new Tenant();
                tenant.Tenant_name = TenantName;
                tenant.Subscription_plan = SelectedPlan;

                //Add the tenant to the DB
                if (await dbManager.AddTenant(tenant))
                    Debug.WriteLine("Added tenant successfully");
                else
                    Debug.WriteLine("Added tenant unsuccessfully");

                //Add to the tenants
                Tenants.Add(tenant);
            }
            else
                Debug.WriteLine("Please fill in the TenantName and Select an option");

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
                    //Initiate the layout
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
                            Debug.WriteLine("Successfully registered. Please wait for approval.");
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
                            Debug.WriteLine("Successfully registered. Please wait for approval.");
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
            Machines = new ObservableCollection<Virtual_Machine>(await dbManager.ReturnVMs());
            if (Client_id > 0)
                Tenants = new ObservableCollection<Tenant>(await dbManager.ReturnTenantsById(Client_id));
        }

        //Changing through different layouts
        partial void OnSelectedOptionChanged(string? value)
        {
            if (Client_id > 0 || value == "Register" || value == "Login")
            {
                ResetLayouts();
                switch (value)
                {
                    case "Register":
                        RegisterLayout = true;
                        break;
                    case "Virtual Machines":
                        VmLayout = true;
                        break;
                    case "Tenants":
                        TenantLayout = true;
                        break;
                    default:
                        LoginLayout = true;
                        break;

                }
            }
            else
                Debug.WriteLine("Please login.");
            
        }
        void ResetLayouts()
        {
            RegisterLayout = false;
            LoginLayout = false;
            TenantLayout = false;
            VmLayout = false;
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
