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
        //DB Access
        DBManager dbMananger = new();

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
        List<string> options = new() { "Approve", "Tenants" };
        [ObservableProperty]
        string? selectedOption;

        //We will need their DataTemplates
        [ObservableProperty]
        DataTemplate? dataTemplat;   //For the selected data template
        [ObservableProperty]
        ObservableCollection<Object>? list;

        ObservableCollection<Client_view>? clients;        //Waiting for approval
        ObservableCollection<Tenant_view>? tenants;          //Manage Tenants

        //Commands
        public ICommand? DeclineCommand { get; set; }
        public ICommand? ApproveCommand { get; set; }

        /***        End of Tenant Management    **/


        /***        Resource Management     **/
        /***        VM Monitoring   **/
        [ObservableProperty]
        ObservableCollection<string>? osTypes;
        ObservableCollection<VM_view>? vms;

        public AdminViewModel()
        {
            //Setting the ICommands
            CoordinatorCommand = new Command<Button>(SetLayouts);
            LoginCommand = new Command(Login);
            ApproveCommand = new Command<Client_view>(Approve);
            DeclineCommand = new Command<Client_view>(Decline);

            //Set the current layout
            LoginLayout = true;
        }

        async void Approve(Client_view client)
        {
            if (await dbMananger.ApproveClient(client.Client_id))
            {
                //We need to update the lists
                SetData();
                if (clients != null)
                    List = new ObservableCollection<object>(clients);

                Debug.WriteLine(client.Name + " was approved.");
            }
            else
                Debug.WriteLine(client.Name + " was not approved.");
        }

        void Decline(Client_view client)
        {
            Debug.WriteLine(client.Name + " was declined.");
        }

        void Login()
        {
            if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
            {
                //Authentication
                dbMananger = new DBManager(username: Username, password: Password);

                //Fetch data
                SetData();

                //Set login layout to false if authorized
                LoginLayout = false;

                //Current Layout is homeLayout
                HomeLayout = true;
                CurrentLayout = "Home";

                //Clear the entries
                Username = null;
                Password = null;
            }
            else
                Debug.WriteLine("Please fill in your username and/or the password.");

                
        }

        partial void OnLoginLayoutChanged(bool value)
        {
            //Disable the options
            HomeLayout = !value;

            if(!HomeLayout)
            {
                clients = new();
                tenants = new();
                vms = new();
            }
        }

        partial void OnVmLayoutChanged(bool value)
        {
            if (value)
                SetData();

            if (vms != null)
                List = new ObservableCollection<Object>(vms);
        }

        partial void OnTenantLayoutChanged(bool value)
        {
            tenants = new();
        }

        partial void OnSelectedOptionChanged(string? value)
        {
            //Set data
            SetData();

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

        async void SetData()
        {
            try
            {
                clients = new ObservableCollection<Client_view>(await dbMananger.ReturnClientViews());
                tenants = new ObservableCollection<Tenant_view>(await dbMananger.ReturnTenantViews());
                vms = new ObservableCollection<VM_view>(await dbMananger.ReturnVMViews());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                //Do not allow login
                LoginLayout = true;
            }

            //Set os_Types and machines
            OsTypes = new ObservableCollection<string>() { "Ubuntu_64", "Windows_64", "Debian_64", "Redhat_64" };
            
        }

    }
}
