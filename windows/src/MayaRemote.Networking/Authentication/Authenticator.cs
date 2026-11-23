using System;
using System.Security.Cryptography;
using MayaRemote.Core.Interfaces;

namespace MayaRemote.Networking.Authentication;

public sealed class Authenticator : IAuthenticationService
{
    private readonly Dictionary<byte[], byte[]> _challenges = new(new ByteArrayComparer());
    private readonly object _lock = new();

    public byte[] CreateChallenge()
    {
        var challenge = RandomNumberGenerator.GetBytes(32);
        lock (_lock) _challenges[challenge] = challenge;
        return challenge;
    }

    public bool VerifyResponse(byte[] challenge, byte[] response, byte[] expectedKey)
    {
        lock (_lock)
        {
            if (!_challenges.ContainsKey(challenge)) return false;
            _challenges.Remove(challenge);
        }

        using var hmac = new HMACSHA256(expectedKey);
        var expected = hmac.ComputeHash(challenge);
        return CryptographicOperations.FixedTimeEquals(expected, response);
    }

    public Task AuthenticateAsync(Guid connectionId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    private sealed class ByteArrayComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[]? x, byte[]? y)
        {
            if (x == null || y == null) return false;
            if (x.Length != y.Length) return false;
            return CryptographicOperations.FixedTimeEquals(x, y);
        }

        public int GetHashCode(byte[] obj)
        {
            var hash = new System.Security.Cryptography.SHA256Managed();
            return BitConverter.ToInt32(hash.ComputeHash(obj), 0);
        }
    }
}
