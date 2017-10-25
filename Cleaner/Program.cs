using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.ComponentModel;
using static System.Console;
using static Cleaner.Colors;
using System.Security.AccessControl;

namespace Cleaner
{
    class Program
    {
        static void Main(string[] args)
        {
            ForegroundColor = main_color;
            string systemroot = Environment.ExpandEnvironmentVariables("%systemroot%");
            bool wrong_args = false;
            List<string> System_Dirs = new List<string>{
                @"c:\users\default",
                Environment.ExpandEnvironmentVariables("%Public%").ToLower(),
                };
            string[] supported_args = { "/help", "/?", "/u", "/U", "/r", "/R", "/p", "/P", "/d", "/D", "/t", "/T" };

            foreach (string arg in args)
            {
                if (!Array.Exists(supported_args, sup_arg => sup_arg == arg))
                {
                    wrong_args = true;
                    ForegroundColor = error_color;
                    WriteLine($"Argument \"{arg}\" is not supported!");
                }
                ForegroundColor = main_color;
            }

            if (!RequestSeBackupPrivilege())
            {
                ForegroundColor = error_color;
                WriteLine("Cannot request privileges. Run application as administrator to access all folders.");
                ForegroundColor = main_color;
            }





            if (args.Length == 0)
            {
                WriteLine();
                ForegroundColor = info_color;
                WriteLine("Drives info:");
                ForegroundColor = main_color;
                foreach (var drive in DriveInfo.GetDrives())
                {
                    try
                    {
                        long freeSpace = drive.TotalFreeSpace;
                        long totalSpace = drive.TotalSize;
                        double percentFree = ((double)freeSpace / (double)totalSpace) * 100;

                        WriteLine($"{drive.Name}");
                        WriteLine($"Total space: {Converters.LongToString(totalSpace)}");
                        WriteLine($"Space used: {Converters.LongToString(totalSpace - freeSpace)}");
                        WriteLine($"Space remaining: {Converters.LongToString(freeSpace)}");
                        Write($"Percent free space: ");
                        if (percentFree > 15) ForegroundColor = success_color;
                        else if (percentFree <= 15 && percentFree > 5) ForegroundColor = warning_color;
                        else ForegroundColor = error_color;
                        WriteLine($"{percentFree.ToString("F0")}%");
                        ForegroundColor = main_color;
                        WriteLine();
                    }
                    catch (Exception ex)
                    {
                        ForegroundColor = error_color;
                        WriteLine(ex.Message);
                        ForegroundColor = main_color;
                    }
                }

                WriteLine("===============================");

                WriteLine();
                ForegroundColor = info_color;
                WriteLine("Users temps:");
                ForegroundColor = main_color;

                string[] Profiles = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList").GetSubKeyNames();
                List<string> Profiles_Paths = new List<string>();

                foreach (string profile in Profiles)
                {
                    if (profile.Length > 8)
                    {
                        string profile_path = Registry.GetValue($@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\{profile}", "ProfileImagePath", null).ToString();
                        Profiles_Paths.Add(profile_path);
                        DirWork Profile = new DirWork($@"{profile_path}\AppData\Local\Temp");
                        Profile.GetInfo();
                    }
                }

                string users_dir = @"C:\Users";
                if (Directory.Exists(users_dir))
                {
                    WriteLine();
                    ForegroundColor = info_color;
                    WriteLine("Nonsystem users profiles:");
                    ForegroundColor = main_color;
                    string[] dirs = Directory.GetDirectories(users_dir);
                    foreach (string dir in dirs)
                    {
                        if (!Profiles_Paths.Contains(dir))
                        {
                            DirWork Dir = new DirWork(dir);
                            if ((Dir.DirInfo.Attributes & FileAttributes.ReparsePoint) == 0 && !System_Dirs.Contains(dir.ToLower()))
                            {
                                Dir.GetInfo();
                            }
                        }
                    }
                }

                string software_distribution_dir = $@"{systemroot}\SoftwareDistribution";
                if (Directory.Exists(software_distribution_dir))
                {
                    WriteLine();
                    ForegroundColor = info_color;
                    WriteLine("SoftwareDistribution:");
                    ForegroundColor = main_color;

                    DirWork Distr = new DirWork(software_distribution_dir);
                    Distr.GetInfo();
                }

                WriteLine();
                ForegroundColor = info_color;
                WriteLine("System temp:");
                ForegroundColor = main_color;
                string[] temps = new string[2]
                {
                Registry.GetValue($@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment", "TEMP", null).ToString(),
                Registry.GetValue($@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment", "TMP", null).ToString()
                };
                if (temps[0] == temps[1])
                {
                    DirWork Temp = new DirWork(temps[0]);
                    Temp.GetInfo();
                }
                else
                {
                    foreach (string tmp in temps)
                    {
                        DirWork Temp = new DirWork(tmp);
                        Temp.GetInfo();
                    }
                }

                WriteLine();
                ForegroundColor = info_color;
                WriteLine("Recycle bin:");
                ForegroundColor = main_color;
                List<UInt64> recycle = GetRecycleBin();
                WriteLine($"{recycle[0]} files => {Converters.LongToString(Convert.ToInt64(recycle[1]))}");
            }

            if (Array.Exists(args, str => str == "/help") || Array.Exists(args, str => str == "/?") || wrong_args == true)
            {
                WriteLine();
                ForegroundColor = success_color;
                WriteLine("Supported arguments:");

                ForegroundColor = info_color;
                Write("/help");
                ForegroundColor = main_color;
                WriteLine(" - This help page.");

                ForegroundColor = info_color;
                Write("/?");
                ForegroundColor = main_color;
                WriteLine(" - This help page.");

                ForegroundColor = info_color;
                Write("/u");
                ForegroundColor = main_color;
                WriteLine(" - Clear temp folders in all users profiles.");

                ForegroundColor = info_color;
                Write("/p");
                ForegroundColor = main_color;
                WriteLine(" - Delete nonsystem users profiles.");

                ForegroundColor = info_color;
                Write("/d");
                ForegroundColor = main_color;
                WriteLine(" - Empty SoftwareDistribution folder.");

                ForegroundColor = info_color;
                Write("/t");
                ForegroundColor = main_color;
                WriteLine(" - Empty system temp folder.");

                ForegroundColor = info_color;
                Write("/r");
                ForegroundColor = main_color;
                WriteLine(" - Empty recycle bin.");
            }

            else
            {
                string[] Profiles = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList").GetSubKeyNames();
                List<string> Profiles_Paths = new List<string>();
                foreach (string profile in Profiles)
                {
                    if (profile.Length > 8)
                    {
                        string profile_path = Registry.GetValue($@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\{profile}", "ProfileImagePath", null).ToString();
                        Profiles_Paths.Add(profile_path);
                    }
                }

                if (Array.Exists(args, str => str.ToLower() == "/u"))
                {
                    WriteLine();
                    ForegroundColor = warning_color;
                    WriteLine("Users temps:");
                    ForegroundColor = main_color;
                    foreach (string profile in Profiles_Paths)
                    {
                        ForegroundColor = info_color;
                        WriteLine();
                        WriteLine($"{profile}:");
                        ForegroundColor = main_color;

                        DirWork Profile = new DirWork($@"{profile}\AppData\Local\Temp");
                        Profile.EmptyDir();
                    }

                }

                if (Array.Exists(args, str => str.ToLower() == "/r"))
                {
                    WriteLine();
                    ForegroundColor = warning_color;
                    WriteLine("Recycle bin:");
                    ForegroundColor = main_color;
                    try
                    {
                        // Execute the method with the required parameters
                        uint IsSuccess = SHEmptyRecycleBin(IntPtr.Zero, null, RecycleFlags.SHRB_NOCONFIRMATION);
                        WriteLine("Empty the RecycleBin successsfully.");
                    }
                    catch (Exception ex)
                    {
                        // Handle exceptions
                        WriteLine($"Empty the RecycleBin failed. {ex.Message}");
                    }
                }

                if (Array.Exists(args, str => str.ToLower() == "/p"))
                {
                    string users_dir = @"C:\Users";
                    if (Directory.Exists(users_dir))
                    {
                        WriteLine();
                        ForegroundColor = warning_color;
                        WriteLine("Nonsystem users profiles:");
                        ForegroundColor = main_color;
                        string[] dirs = Directory.GetDirectories(users_dir);
                        foreach (string dir in dirs)
                        {
                            if (!Profiles_Paths.Contains(dir))
                            {
                                DirectoryInfo DirInfo = new DirectoryInfo(dir);
                                if ((DirInfo.Attributes & FileAttributes.ReparsePoint) == 0 && !System_Dirs.Contains(dir.ToLower()))
                                {
                                    ForegroundColor = info_color;
                                    WriteLine(dir);
                                    ForegroundColor = main_color;
                                    try
                                    {
                                        DirInfo.Delete(true);
                                        WriteLine($"{dir} was successfully deleted.");
                                    }
                                    catch (Exception ex)
                                    {
                                        try
                                        {
                                            DirectorySecurity dSecurity = DirInfo.GetAccessControl();
                                            dSecurity.AddAccessRule(new FileSystemAccessRule(System.Security.Principal.WindowsIdentity.GetCurrent().Name, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.InheritOnly, AccessControlType.Allow));
                                            DirInfo.SetAccessControl(dSecurity);
                                            DirInfo.Delete(true);
                                        }
                                        catch
                                        {
                                            ForegroundColor = error_color;
                                            WriteLine($"Can not delete {dir}. {ex.Message}");
                                            ForegroundColor = main_color;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (Array.Exists(args, str => str.ToLower() == "/d"))
                    {
                        WriteLine();
                        ForegroundColor = warning_color;
                        WriteLine("SoftwareDistribution:");
                        ForegroundColor = main_color;
                        string software_distribution_dir = $@"{systemroot}\SoftwareDistribution";
                        if (Directory.Exists(software_distribution_dir))
                        {
                            DirWork Profile = new DirWork(software_distribution_dir);
                            Profile.EmptyDir();
                        }
                        else
                        {
                            ForegroundColor = error_color;
                            WriteLine("SoftwareDistribution folder not founded.");
                            ForegroundColor = main_color;
                        }
                    }

                    if (Array.Exists(args, str => str.ToLower() == "/t"))
                    {
                        WriteLine();
                        ForegroundColor = warning_color;
                        WriteLine("System temp:");
                        ForegroundColor = main_color;
                        string[] temps = new string[2]
                        {
                            Registry.GetValue($@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment", "TEMP", null).ToString(),
                            Registry.GetValue($@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment", "TMP", null).ToString()
                        };
                        if (temps[0] == temps[1])
                        {
                            DirWork Temp = new DirWork(temps[0]);
                            Temp.EmptyDir();
                        }
                        else
                        {
                            foreach (string tmp in temps)
                            {
                                DirWork Temp = new DirWork(tmp);
                                Temp.EmptyDir();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Request Backup Privileges
        /// </summary>
        /// <returns></returns>
        static bool RequestSeBackupPrivilege()
        {
            LUID luid;

            if (!LookupPrivilegeValue(null, "SeBackupPrivilege", out luid))
                return false;

            TOKEN_PRIVILEGES_SINGLE tp = new TOKEN_PRIVILEGES_SINGLE
            {
                PrivilegeCount = 1,
                Luid = luid,
                Attributes = SE_PRIVILEGE_ENABLED
            };

            IntPtr hToken;
            return
                OpenProcessToken(
                    GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out hToken) &&
                AdjustTokenPrivileges(
                    hToken, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero) &&
                (Marshal.GetLastWin32Error() != ERROR_NOT_ALL_ASSIGNED);
        }

        const int SE_PRIVILEGE_ENABLED = 0x00000002;
        const int TOKEN_QUERY = 0x00000008;
        const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        const int ERROR_NOT_ALL_ASSIGNED = 1300;

        [DllImport("kernel32.dll", ExactSpelling = true)]
        static extern IntPtr GetCurrentProcess();

        [StructLayout(LayoutKind.Sequential)]
        struct TOKEN_PRIVILEGES_SINGLE
        {
            public UInt32 PrivilegeCount;
            public LUID Luid;
            public UInt32 Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool LookupPrivilegeValue(
            string lpSystemName, string lpName, out LUID lpLuid);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool OpenProcessToken(
            IntPtr ProcessHandle, UInt32 DesiredAccess, out IntPtr TokenHandle);

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern bool AdjustTokenPrivileges(
            IntPtr htok, bool disall, ref TOKEN_PRIVILEGES_SINGLE newst,
            int len, IntPtr prev, IntPtr relen);




        //Begins work with Recycle bin
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        public struct SHQUERYRBINFO
        {
            public Int32 cbSize;
            public UInt64 i64Size;
            public UInt64 i64NumItems;
        }

        //Now the Win32 API
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern int SHQueryRecycleBin(string pszRootPath, ref SHQUERYRBINFO pSHQueryRBInfo);

        //Now the method that does the work
        /// <summary>
        /// method for getting total files in the recycle bin and it's overall size
        /// </summary>
        /// <returns></returns>
        static List<UInt64> GetRecycleBin()
        {
            SHQUERYRBINFO query = new SHQUERYRBINFO();
            List<UInt64> info = new List<UInt64>();
            query.cbSize = Marshal.SizeOf(typeof(SHQUERYRBINFO));
            try
            {
                int result = SHQueryRecycleBin(null, ref query);
                if (result == 0)
                {
                    info.Add(query.i64NumItems);
                    info.Add(query.i64Size);
                    return info;
                }
                else
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            catch (Exception ex)
            {
                WriteLine($"Error accessing Recycle Bin: {ex.Message}");
                return null;
            }
        }

        public enum RecycleFlags : uint
        {
            SHRB_NOCONFIRMATION = 0x00000001, // Don't ask confirmation
            SHRB_NOPROGRESSUI = 0x00000002, // Don't show any windows dialog
            SHRB_NOSOUND = 0x00000004 // Don't make sound, ninja mode enabled :v
        }

        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        static extern uint SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, RecycleFlags dwFlags);


    }
}
