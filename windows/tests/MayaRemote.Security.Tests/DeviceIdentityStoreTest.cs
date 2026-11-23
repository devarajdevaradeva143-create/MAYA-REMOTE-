using MayaRemote.Core.Security;
using MayaRemote.Core.Models;
using Xunit;

namespace MayaRemote.Security.Tests;

public class DeviceIdentityStoreTests
{
    [Fact]
    public void SaveAndLoad_Roundtrips_ViaDpapi()
    {
        var store = new DeviceIdentityStore(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MayaRemoteTests"));
        var identity = new DeviceIdentity
        {
            DeviceId = Guid.NewGuid(),
            DeviceName = "Test Device",
            PublicKey = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32),
            PairedAt = DateTime.UtcNow,
            IsPaired = true
        };

        store.Save(identity);
        var loaded = store.Load();

        Assert.NotNull(loaded);
        Assert.Equal(identity.DeviceId, loaded!.DeviceId);
        Assert.Equal(identity.DeviceName, loaded.DeviceName);
        Assert.Equal(identity.PublicKey, loaded.PublicKey);
        Assert.True(loaded.IsPaired);

        store.Clear();
        Assert.Null(store.Load());
    }

    [Fact]
    public void Fingerprint_Derived_From_PublicKey()
    {
        var key = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
        var identity = new DeviceIdentity { DeviceId = Guid.NewGuid(), PublicKey = key };
        Assert.NotNull(identity.Fingerprint);
        Assert.Equal(16, identity.Fingerprint.Length);
    }
}
