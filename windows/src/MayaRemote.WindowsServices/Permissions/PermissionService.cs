using System;
using System.Security.Principal;
using MayaRemote.Core.Interfaces;

namespace MayaRemote.WindowsServices.Permissions;

public sealed class PermissionService : IPermissionService
{
    public bool IsElevated
    {
        get
        {
            try
            {
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch { return false; }
        }
    }

    public Task<bool> RequestElevationAsync()
    {
        if (IsElevated) return Task.FromResult(true);
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty,
                Verb = "runas",
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}
