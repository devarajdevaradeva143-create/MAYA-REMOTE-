using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MayaRemote.Core.Interfaces;
using MayaRemote.Core.Models;

namespace MayaRemote.Core.Security;

public sealed class CryptoHelper
{
    private const int NonceSize = 12;
    private const int TagSize = 16;

    public static byte[] GenerateKey(int sizeInBytes = 32)
    {
        var key = new byte[sizeInBytes];
        RandomNumberGenerator.Fill(key);
        return key;
    }

    public static byte[] Encrypt(byte[] plaintext, byte[] key)
    {
        using var aes = new AesGcm(key);
        var nonce = new byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[TagSize];
        aes.Encrypt(nonce, plaintext, ciphertext, tag);
        var output = new byte[NonceSize + ciphertext.Length + TagSize];
        Buffer.BlockCopy(nonce, 0, output, 0, NonceSize);
        Buffer.BlockCopy(ciphertext, 0, output, NonceSize, ciphertext.Length);
        Buffer.BlockCopy(tag, 0, output, NonceSize + ciphertext.Length, TagSize);
        return output;
    }

    public static byte[] Decrypt(byte[] ciphertextWithNonceAndTag, byte[] key)
    {
        if (ciphertextWithNonceAndTag.Length < NonceSize + TagSize)
            throw new CryptographicException("Ciphertext too short.");
        using var aes = new AesGcm(key);
        var nonce = new byte[NonceSize];
        var ciphertext = new byte[ciphertextWithNonceAndTag.Length - NonceSize - TagSize];
        var tag = new byte[TagSize];
        Buffer.BlockCopy(ciphertextWithNonceAndTag, 0, nonce, 0, NonceSize);
        Buffer.BlockCopy(ciphertextWithNonceAndTag, NonceSize, ciphertext, 0, ciphertext.Length);
        Buffer.BlockCopy(ciphertextWithNonceAndTag, NonceSize + ciphertext.Length, tag, 0, TagSize);
        var plaintext = new byte[ciphertext.Length];
        aes.Decrypt(nonce, ciphertext, tag, plaintext);
        return plaintext;
    }

    public static string ToHex(byte[] data) => Convert.ToHexString(data);
    public static byte[] FromHex(string hex) => Convert.FromHexString(hex);
}

public sealed class DeviceIdentityStore : IDeviceStorage
{
    private readonly string _path;

    public DeviceIdentityStore(string? basePath = null)
    {
        var folder = basePath ?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _path = Path.Combine(folder, "MayaRemote", "device.identity");
    }

    public void Save(DeviceIdentity identity)
    {
        var json = JsonSerializer.Serialize(identity);
        var protectedBytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(json), null, DataProtectionScope.CurrentUser);
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        File.WriteAllBytes(_path, protectedBytes);
    }

    public DeviceIdentity? Load()
    {
        if (!File.Exists(_path)) return null;
        try
        {
            var protectedBytes = File.ReadAllBytes(_path);
            var jsonBytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
            return JsonSerializer.Deserialize<DeviceIdentity>(Encoding.UTF8.GetString(jsonBytes));
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
