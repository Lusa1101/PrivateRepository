using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;
using CloudOS.Services;
using CloudOS.Models;
using System.Diagnostics;

namespace CloudOS.ViewModels
{
    class MainViewModel : ObservableObject
    {
        public TenantViewModel? Tenant1 { get; }
        public TenantViewModel? Tenant2 { get; }

        public MainViewModel() 
        {
            Tenant1 = new TenantViewModel();
            Tenant2 = new TenantViewModel();
        }
    }
}
