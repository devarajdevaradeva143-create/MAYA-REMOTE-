using MayaRemote.Core.Models;
using Xunit;

namespace MayaRemote.Core.Tests;

public class DeviceIdentityTests
{
    [Fact]
    public void Fingerprint_Null_When_NoPublicKey()
    {
        var identity = new DeviceIdentity { DeviceId = Guid.NewGuid(), DeviceName = "x" };
        Assert.Null(identity.Fingerprint);
    }
}
