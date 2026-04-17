using PermissionSaver.Models;
using System;
using System.IO;
using System.Security.AccessControl;

namespace PermissionSaver.Services
{
    public static class PermissionService
    {
        public static PermissionData CapturePermissions(string path)
        {
            var data = new PermissionData();

            if (OperatingSystem.IsWindows())
            {
                try
                {
                    if (File.Exists(path))
                    {
                        var fileInfo = new FileInfo(path);
                        var security = fileInfo.GetAccessControl(AccessControlSections.All);
                        data.WindowsAclSddl = security.GetSecurityDescriptorSddlForm(AccessControlSections.All);
                    }
                    else if (Directory.Exists(path))
                    {
                        var dirInfo = new DirectoryInfo(path);
                        var security = dirInfo.GetAccessControl(AccessControlSections.All);
                        data.WindowsAclSddl = security.GetSecurityDescriptorSddlForm(AccessControlSections.All);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not read Windows ACL for {path}: {ex.Message}");
                }
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                try
                {
                    var mode = File.GetUnixFileMode(path);
                    data.UnixFileModeInt = (int)mode;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not read Unix mode for {path}: {ex.Message}");
                }
            }

            return data;
        }

        public static void ApplyPermissions(string path, PermissionData data)
        {
            if (OperatingSystem.IsWindows() && !string.IsNullOrEmpty(data.WindowsAclSddl))
            {
                try
                {
                    if (File.Exists(path))
                    {
                        var fileInfo = new FileInfo(path);
                        var security = new FileSecurity();
                        security.SetSecurityDescriptorSddlForm(data.WindowsAclSddl);
                        security.SetAccessRuleProtection(true, true);
                        fileInfo.SetAccessControl(security);
                    }
                    else if (Directory.Exists(path))
                    {
                        var dirInfo = new DirectoryInfo(path);
                        var security = new DirectorySecurity();
                        security.SetSecurityDescriptorSddlForm(data.WindowsAclSddl);
                        security.SetAccessRuleProtection(true, true);
                        dirInfo.SetAccessControl(security);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Ошибка применения прав к {path}: {ex.Message}");
                }
            }
            else if ((OperatingSystem.IsLinux() || OperatingSystem.IsMacOS()) && data.UnixFileModeInt.HasValue)
            {
                try
                {
                    var mode = (UnixFileMode)data.UnixFileModeInt.Value;
                    File.SetUnixFileMode(path, mode);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not apply Unix mode to {path}: {ex.Message}");
                }
            }
        }
    }
}
