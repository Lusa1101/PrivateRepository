The preresquisites and some guidelines for using this application are:
  1. Install Visual Studio(VS) 2022 with the .NET MAUI UI development workload.
  2. Ensure that you have a .NET 8 installed. On your PowerShell, type ***dotnet --version***
  3. Install PostgesSQL (PSQL):
       a. Keep the default values, especially the 'PORT = 5432'.
       b. Make sure you note your PSQL credentials such as username and the password.
       b. You will need to set the environment variable.
           i. Open the ***edit the system environment variables*** by searching it using windows.
           ii. Then open the ***Environment Variables...*** on your bottom right.
           iii. Under user variables, select ***New...***.
           iv. Variable name = POSTGRES_CONNECTION_STRING;
               Variable value = Host=localhost;Port=5432;Database=cloud_os;Username=postgres;Password=postgres;
               ***Where there is postgres, use the credentials you used when setting up the PSQL.***
  4. Install the OracleVirtualBox by selecting **Windows Host** in the *VirtualBox Platform Packages* window from https://www.virtualbox.org/wiki/Downloads
  5. Create a directory under Documents for the project with your preferred name.
       a. In this folder, create a directory called CorePlus.
           i. You will have to download the **CorePlus** disk image for the OS which will used for the virtual machines from http://tinycorelinux.net/downloads.html
           ii. Move this file to the CorePlus folder.
       b. Create another directory called VirtualMachines.
       c. Clone this project. You should have 3 folders by now.
  6. Before running the app, there are few packages that should be downloaded and two lines of code that must be changed
	     a. Use the NuGet Manager in the VS 2022 to add the following packages
		       i. CommunityToolkit versions 8
		       ii. Microsoft.EntityFrameworkCore versions 8
		       iii. Npgsql versions 8
	     b. Find a file called VBManager located in the **Services** folder and go to line 37 and 40. Change the folders accordingly.
