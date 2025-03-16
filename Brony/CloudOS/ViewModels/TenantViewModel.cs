using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CloudOS.Models;
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

        //Layouts' visibilities
        [ObservableProperty]
        bool registerLayout;
        [ObservableProperty]
        bool login;
        [ObservableProperty]
        bool vmLayout;

        //Tenant-Type based registration
        [ObservableProperty]
        bool companyType;
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
        public ICommand? RegisterCommand { get; }

        public TenantViewModel() 
        {
            //Set the commands
            RegisterCommand = new Command(Register);

            //Initial Layout
            RegisterLayout = true;
        }

        //Registration method
        void Register()
        {
            bool cont = false;
            cont = functions.CheckInput(Check1, Check2, Check3, Check4, Check5, Check6);
            if (cont)
               Debug.WriteLine("All clear to proceed.");

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
            if (!String.IsNullOrEmpty(value) && !(functions.NumberValidator(value)))
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
            if (!String.IsNullOrEmpty(value) && !(functions.EmailValidator(value)))
                Check6 = "Not a vaild email input.";
            else
                Check6 = string.Empty;
        }
        /***        End of Input Validation     **/
    }
}
