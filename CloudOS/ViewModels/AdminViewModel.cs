using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CloudOS.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CloudOS.ViewModels
{
    /***        Functions for this view     ***/
    //Login
    //Tenant management => Registration requests, logs,
    //Resource management
    //VM Monitioring
    partial class AdminViewModel : ObservableObject
    {
        /***        StackLayout Controllers     **/
        [ObservableProperty]
        bool loginLayout;
        [ObservableProperty]
        bool tenantLayout;
        [ObservableProperty]
        bool vmLayout;
        [ObservableProperty]
        bool homeLayout;

        public ICommand CoordinatorCommand { get; set; }

        string? CurrentLayout;

        /***        End of StackLayout Controllers      **/

        //For Login
        [ObservableProperty]
        string? username;
        [ObservableProperty]
        string? password;
        public ICommand LoginCommand { get; set; }

        /***            Tenant Management       **/
        //For registration approval
        [ObservableProperty]
        List<string> options = new() { "Approve", "Tenants", "Virtual Machines" };
        [ObservableProperty]
        string? selectedOption;

        //We will need their DataTemplates
        [ObservableProperty]
        DataTemplate? dataTemplat;   //For the selected data template
        [ObservableProperty]
        ObservableCollection<Object>? list;
        
        /***        For binding the DataTemplate declared in the App.xaml   ***/
        //For tenant management
        

        ObservableCollection<Client_view>? clients;        //Waiting for approval
        ObservableCollection<Tenant_view>? tenants;          //Manage Tenants
        ObservableCollection<VM_view>? vms;                  //Monitor Virtual Machines 

        /***        End of Tenant Management    **/


        /***        Resource Management     **/
        /***        VM Monitoring   **/
        [ObservableProperty]
        ObservableCollection<string>? osTypes;
        [ObservableProperty]
        ObservableCollection<Virtual_Machine> machines = new();

        public AdminViewModel()
        {
            //Setting the ICommands
            CoordinatorCommand = new Command<Button>(SetLayouts);
            LoginCommand = new Command(Login);

            //Dummy data
            SetData();

            //Initial layout
            CurrentLayout = "Login";
            LoginLayout = true;
            HomeLayout = false;
            TenantLayout = false;
            VmLayout = false;
        }

        void Login()
        {
            //Set login layout to false if authorized
            LoginLayout = false;

            //Current Layout is homeLayout
            HomeLayout = true;
            CurrentLayout = "Home";
        }

        partial void OnLoginLayoutChanged(bool value)
        {
            //Disable the options
            HomeLayout = false;
        }

        partial void OnSelectedOptionChanged(string? value)
        {
            switch (value)
            {
                case "Approve":
                    //DataTemplate
                    if (App.Current != null)
                        DataTemplat = (DataTemplate)App.Current.Resources["clientDT"];
                    //Source for the collection
                    if (clients != null)
                        List = new ObservableCollection<Object>(clients);
                    break;
                case "Tenants":
                    //DataTemplate
                    if (App.Current != null)
                        DataTemplat = (DataTemplate)App.Current.Resources["tenantDT"];
                    //Source for the collection
                    if (tenants != null)
                        List = new ObservableCollection<Object>(tenants);
                    break;

                case "Virtual Machines":
                    //DataTemplate
                    if (App.Current != null)
                        DataTemplat = (DataTemplate)App.Current.Resources["vmDT"];
                    //Source for the collection
                    if (vms != null)
                        List = new ObservableCollection<Object>(vms);
                    break;

            }
        }

        //Function to set the Layout to false
        void UnsetLayouts(string text)
        {
            switch (text)
            {
                case "Tenant Management":
                    TenantLayout = false;
                    break;
                case "VM Monitoring":
                    VmLayout = false;
                    break;
                case "Log out":
                    HomeLayout = false;
                    break;
                case "Home":
                    HomeLayout = false;
                    break;
            }
        }

        //Function to set the Layout to ture
        public void SetLayouts(Button button)
        {
            //Get the button's text
            string text = button.Text;
            Debug.WriteLine($"Text: {text}. \nCurrent layout: {CurrentLayout}");

            //Set @CurrentLayout to false
            if (CurrentLayout != null)
                UnsetLayouts(CurrentLayout);

            //Set the appropriate layout to true
            switch (text)
            {
                case "Tenant Management":
                    HomeLayout = false;
                    TenantLayout = true;
                    break;
                case "VM Monitoring":
                    HomeLayout = false;
                    VmLayout = true;
                    break;
                case "Log out":
                    HomeLayout = false;
                    LoginLayout = true;
                    break;
            }

            //Update the CurrentLayout
            CurrentLayout = text;
        }

        void SetData()
        {
            clients = new ObservableCollection<Client_view>()
            {
                new Client_view { Name="Omphulusa", Address="Diepkloof", Type="Company"},
                new Client_view { Name="Ndivhuwo Mashau", Address="Diepsloot", Type="Personal"}
            };
            tenants = new ObservableCollection<Tenant_view>()
            {
                new Tenant_view { Name="Omphulusa", Address="Diepkloof", Subscription_plan="Free", Tenant_type="Company", Tenant_id=1},
                new Tenant_view { Name="Ndivhuwo Mashau", Address="Diepsloot", Subscription_plan="Silver", Tenant_type="Personal", Tenant_id=3}
            };
            vms = new ObservableCollection<VM_view>()
            {
                new VM_view { Name="Vm1", Tenant_id = 1 },
                new VM_view { Name="Vm2", Tenant_id = 2 }
            };

            //Set os_Types and machines
            OsTypes = new ObservableCollection<string>() { "Ubuntu_64", "Windows_64", "Debian_64", "Redhat_64" };
            Machines = new ObservableCollection<Virtual_Machine>()
            {
                new Virtual_Machine { Name = "VM!", OS_type = OsTypes[0], CPUs=2, Memory_size=2048, UUID="kjcjabu83gei983", Tenant_id=1},
                new Virtual_Machine { Name = "VM2", OS_type = OsTypes[0], CPUs=2, Memory_size=2048, UUID="kjcjabu83gei93423", Tenant_id=1},
                new Virtual_Machine { Name = "VM3", OS_type = OsTypes[0], CPUs=2, Memory_size=2048, UUID="kjcjabu83gei3", Tenant_id=2},
                new Virtual_Machine { Name = "VM2", OS_type = OsTypes[0], CPUs=2, Memory_size=2048, UUID="kjcjabu83mdksgei983", Tenant_id=2}
            };
        }

    }
}
