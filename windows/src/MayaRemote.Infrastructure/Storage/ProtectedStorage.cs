using System;
using System.IO;
using System.Text.Json;
using MayaRemote.Core.Interfaces;
using MayaRemote.Core.Models;

namespace MayaRemote.Infrastructure.Storage;

public sealed class ProtectedStorage
{
    private readonly string _path;

    public ProtectedStorage(string fileName, string? basePath = null)
    {
        var folder = basePath ?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _path = Path.Combine(folder, "MayaRemote", fileName);
    }

    public void Save<T>(T value)
    {
        var json = JsonSerializer.Serialize(value);
        var protectedBytes = System.Security.Cryptography.ProtectedData.Protect(System.Text.Encoding.UTF8.GetBytes(json), null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        File.WriteAllBytes(_path, protectedBytes);
    }

    public T? Load<T>() where T : class
    {
        if (!File.Exists(_path)) return null;
        try
        {
            var protectedBytes = File.ReadAllBytes(_path);
            var jsonBytes = System.Security.Cryptography.ProtectedData.Unprotect(protectedBytes, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
            return JsonSerializer.Deserialize<T>(System.Text.Encoding.UTF8.GetString(jsonBytes));
        }
        catch
        {
            return null;
        }
    }

    public void Clear()
    {
        if (File.Exists(_path)) File.Delete(_path);
    }
}

public sealed class JsonStorage
{
    private readonly string _path;

    public JsonStorage(string fileName, string? basePath = null)
    {
        var folder = basePath ?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _path = Path.Combine(folder, "MayaRemote", fileName);
    }

    public void Save<T>(T value)
    {
        var json = JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true });
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        File.WriteAllText(_path, json);
    }

    public T? Load<T>() where T : class
    {
        if (!File.Exists(_path)) return null;
        return JsonSerializer.Deserialize<T>(File.ReadAllText(_path));
    }

    public void Clear()
    {
        if (File.Exists(_path)) File.Delete(_path);
    }
}
