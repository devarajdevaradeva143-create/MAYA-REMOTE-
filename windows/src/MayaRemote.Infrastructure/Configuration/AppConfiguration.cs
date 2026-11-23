using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MayaRemote.Core.Interfaces;

namespace MayaRemote.Infrastructure.Configuration;

public sealed class AppConfiguration : IConfiguration, IDisposable
{
    private readonly string _path;
    private readonly Dictionary<string, object?> _values = new(StringComparer.OrdinalIgnoreCase);
    private readonly FileSystemWatcher? _watcher;
    private bool _disposed;

    public AppConfiguration(string fileName, string? basePath = null)
    {
        var folder = basePath ?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _path = Path.Combine(folder, "MayaRemote", fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        Reload();

        try
        {
            _watcher = new FileSystemWatcher(Path.GetDirectoryName(_path)!, Path.GetFileName(_path))
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
            };
            _watcher.Changed += (_, _) => Reload();
            _watcher.EnableRaisingEvents = true;
        }
        catch { }
    }

    public T Get<T>(string key, T defaultValue = default)
    {
        if (!_values.TryGetValue(key, out var value)) return defaultValue;
        if (value is T t) return t;
        try { return (T)Convert.ChangeType(value, typeof(T)); } catch { return defaultValue; }
    }

    public void Set<T>(string key, T value)
    {
        _values[key] = value;
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        var json = JsonSerializer.Serialize(_values, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_path, json);
    }

    public void Reload()
    {
        _values.Clear();
        if (!File.Exists(_path)) return;
        try
        {
            var json = File.ReadAllText(_path);
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            if (dict == null) return;
            foreach (var kvp in dict)
            {
                _values[kvp.Key] = kvp.Value.ValueKind switch
                {
                    JsonValueKind.String => kvp.Value.GetString(),
                    JsonValueKind.Number => kvp.Value.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    _ => kvp.Value.GetRawText()
                };
            }
        }
        catch { }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _watcher?.Dispose();
        GC.SuppressFinalize(this);
    }
}
