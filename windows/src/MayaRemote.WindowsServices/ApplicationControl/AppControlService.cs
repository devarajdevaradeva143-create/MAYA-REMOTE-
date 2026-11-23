using System;
using System.Collections.Generic;
using System.Diagnostics;
using MayaRemote.Core.Interfaces;
using MayaRemote.Core.Models;

namespace MayaRemote.WindowsServices.ApplicationControl;

public sealed class AppControlService : IApplicationControlService
{
    public Task StartProcessAsync(string fileName, string? arguments = null)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments ?? string.Empty,
            UseShellExecute = true
        };
        Process.Start(psi);
        return Task.CompletedTask;
    }

    public Task KillProcessAsync(int processId)
    {
        try
        {
            var process = Process.GetProcessById(processId);
            process.Kill();
        }
        catch { }
        return Task.CompletedTask;
    }

    public IReadOnlyList<ProcessInfo> GetRunningProcesses()
    {
        var list = new List<ProcessInfo>();
        foreach (var p in Process.GetProcesses())
        {
            try
            {
                list.Add(new ProcessInfo { ProcessId = p.Id, ProcessName = p.ProcessName, MainWindowTitle = p.MainWindowTitle });
            }
            catch { }
        }
        return list;
    }
}
