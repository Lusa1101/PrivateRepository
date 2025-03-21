using System;
using System.Diagnostics;

namespace CloudOS.Services
{
    public class VBManager
    {
        //Functions class
        Functions functions = new();
        public static string ExecuteVMCommand(string query)
        {
            Process vmProcess = new Process();
            vmProcess.StartInfo.FileName = @"C:\Program Files\Oracle\VirtualBox\VBoxManage.exe";
            vmProcess.StartInfo.Arguments = query;
            vmProcess.StartInfo.UseShellExecute = false;
            vmProcess.StartInfo.RedirectStandardOutput = true;
            vmProcess.StartInfo.RedirectStandardError = true;
            vmProcess.StartInfo.CreateNoWindow = true;

            vmProcess.Start();

            string output = vmProcess.StandardOutput.ReadToEnd();
            string error = vmProcess.StandardError.ReadToEnd();

            vmProcess.WaitForExit();

            if(!string.IsNullOrEmpty(error))
            {
                Console.WriteLine("Error: " + error);
            }

            return output;
        }

        public (string output, string uuid) CreateVM(string vmName, string vdiName = "CorePlus", string controllerName = "SATA Controller", string osType = "ubuntu_64", int memoryMB = 512, int cpus = 2, int hdSize = 5120)
        {
            string path = "C:\\Users\\omphu\\Documents\\Brony\\CorePlus\\";
            string vdiPath = Path.Combine(path, $"{vdiName}.vdi");
            string isoPath = Path.Combine(path, "CorePlus-current.iso");
            string basefolder = $"C:\\Users\\omphu\\Documents\\Brony\\VM\\";
            string output;

            // Helper function to detect specific error patterns
            bool HasErrors(string commandOutput)
            {
                return commandOutput.Contains("Failed") || commandOutput.Contains("Could not") || commandOutput.Contains("Error");
            }

            // 1. Validate ISO File Existence
            if (!File.Exists(isoPath))
            {
                return ($"Error: ISO file not found at {isoPath}", "");
            }

            // 2. Create the Virtual Machine
                //System.IO.Directory.CreateDirectory(path); if it does not exist
                //Add --basefolder "{path}/{tenant_id}"
                //Create the directory if it does not exist before the vm is created.
            if(!Directory.Exists(basefolder))
                Directory.CreateDirectory(basefolder);
            string createCommand = $"createvm --name \"{vmName}\" --basefolder {basefolder} --ostype {osType} --register";    
            output = ExecuteVMCommand(createCommand);
            if (HasErrors(output)) return ("Error: Failed to create VM.", "");

            //Get the UUID
            string UUID = functions.ReturnVMUUID(output);

            // 3. Configure VM Memory and CPUs
            string modifyCommand = $"modifyvm \"{vmName}\" --memory {memoryMB} --cpus {cpus} --boot1 dvd --nic1 nat"; // Basic settings
            output = ExecuteVMCommand(modifyCommand);
            if (HasErrors(output)) return (("Error: Failed to configure VM settings.", ""));

            // 4. Create Virtual Disk if It Doesn't Exist
            if (!File.Exists(vdiPath))
            {
                string createHDD = $"createhd --filename \"{vdiPath}\" --size {hdSize}";
                output = ExecuteVMCommand(createHDD);
                if (HasErrors(output)) return ("Error: Failed to create virtual disk.", "");
            }

            // 5. Add SATA Controller for Virtual Disk
            string attachCommand = $"storagectl \"{vmName}\" --name \"{controllerName}\" --add sata --controller IntelAhci";
            output = ExecuteVMCommand(attachCommand);
            if (HasErrors(output)) return ("Error: Failed to attach SATA controller.", "");

            // 6. Attach Virtual Disk to SATA Controller
            string attachDiskCommand = $"storageattach \"{vmName}\" --storagectl \"{controllerName}\" --port 0 --device 0 --type hdd --medium \"{vdiPath}\"";
            output = ExecuteVMCommand(attachDiskCommand);
            if (HasErrors(output)) return ("Error: Failed to attach virtual disk.", "");

            // 7. Add IDE Controller for ISO Attachment
            string ideCommand = $"storagectl \"{vmName}\" --name \"IDE Controller\" --add ide";
            output = ExecuteVMCommand(ideCommand);
            if (HasErrors(output)) return ("Error: Failed to attach IDE controller.", "");

            // 8. Attach ISO File to IDE Controller
            string ideAttachCommand = $"storageattach \"{vmName}\" --storagectl \"IDE Controller\" --port 0 --device 0 --type dvddrive --medium \"{isoPath}\"";
            output = ExecuteVMCommand(ideAttachCommand);
            if (HasErrors(output)) return ("Error: Failed to attach ISO file.", "");

            // 9. Set Boot Order to Boot from DVD First
            string bootCommand = $"modifyvm \"{vmName}\" --boot1 dvd --boot2 disk";
            output = ExecuteVMCommand(bootCommand);
            if (HasErrors(output)) return ("Error: Failed to set boot order.", "");

            return ("Success: Virtual Machine created successfully!", UUID);
        }

        /***    End of Create VM    ***/


        /***   Manage VMs   ***/
        public string StartVM(string vmName)
        {
            string command = $"startvm \"{vmName}\"";
            return ExecuteVMCommand(command);
        }

        public string StopVM(string vmName)
        {
            string command = $"controlvm \"{vmName}\" poweroff";
            return ExecuteVMCommand(command);
        }

        public string SaveVM(string name)
        {
            string command = $"controlvm \"{name}\" savestate";
            return ExecuteVMCommand(command);
        }

        public string TakeSnapshot(string vmName, string snapshotName)
        {
            string command = $"snapshot \"{vmName}\" take \"{snapshotName}\"";
            return ExecuteVMCommand(command);
        }

        public string DeleteVM(string vmName)
        {
            string command = $"unregistervm {vmName} --delete";
            return ExecuteVMCommand(command);
        }

        /***    End of Manage VMs   ***/

        //Monitor VMs

        public string VMLists()
        {
            string command = "list -l vms";
            return ExecuteVMCommand(command);
        }

        public string OSTypes()
        {
            string command = "list ostypes";
            return ExecuteVMCommand(command);
        }

        /*
            VBoxMAnage Commands

            1. createvm --name "{}" --ostype {} --register
            2. list [-l/--long] vms
            3. showvminfo {name}
            4. modifyvm {name} --cpus 2 --memory 2048 --vram 12

        Creating VMs:
            createvm --name "<vmname>" --ostype "<ostype>" --register: Creates a new VM.
            clonevm "<vmname>" --name "<newvmname>" --register --options link/full: Clones an existing VM.
            
        Deleting VMs:
            unregistervm "<vmname>" --delete: Unregisters and deletes a VM.

        Modifying VMs:
            modifyvm "<vmname>" --memory <memorysize>: Changes the VM's memory.
            modifyvm "<vmname>" --cpus <number>: Changes the VM's CPU count.
            modifyvm "<vmname>" --nic1 bridged/nat/hostonly/internal: Changes the VM's network adapter type.
            modifyvm "<vmname>" --nic1 hostonlyadapter1: Sets the host only adapter.
            modifyvm "<vmname>" --boot1 disk/dvd/net/none: Changes the VM's boot order.

        Starting/Stopping VMs:
            startvm "<vmname>" --type gui/headless: Starts a VM.
            controlvm "<vmname>" poweroff/reset/pause/resume: Controls a running VM.

        Snapshots:
            snapshot "<vmname>" take "<snapshotname>": Takes a snapshot of a VM.
            snapshot "<vmname>" restore "<snapshotname>": Restores a snapshot.
            snapshot "<vmname>" delete "<snapshotname>": Deletes a snapshot.
            snapshot "<vmname>" list: List snapshots.

        Creating Virtual Hard Disks:
            createhd --filename "<filename>.vdi" --size <sizeMB>: Creates a new virtual hard disk.
            Attaching Disks:
            storageattach "<vmname>" --storagectl "SATA Controller" --port 0 --device 0 --type hdd --medium "<filename>.vdi": Attaches a virtual hard disk to a VM.
            Detaching Disks:
            storageattach "<vmname>" --storagectl "SATA Controller" --port 0 --device 0 --type hdd --medium none: Detaches a virtual hard disk.

        Creating Host-Only Networks:
            hostonlyif create: Creates a new host-only network.
            hostonlyif ipconfig "<ifacename>" --ip <ipaddress> --netmask <netmask>: Configures a host-only network.
            Listing Networks:
            list hostonlyifs: Lists host-only networks.
            list bridgedifs: Lists bridged interfaces.
            Network Adapter Configuration:
            See the modifyvm network commands above.

        Listing VMs:
            list vms: Lists registered VMs.
            list runningvms: Lists running VMs.
            Listing OSTypes:
            list ostypes: Lists available operating system types.
            Getting VM Information:
            showvminfo "<vmname>": Shows detailed information about a VM.
         */
    }
}
