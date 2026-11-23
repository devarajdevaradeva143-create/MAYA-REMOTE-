namespace MayaRemote.Agent;

public sealed class AgentConfiguration
{
    public string DeviceName { get; set; } = "MayaRemote Agent";
    public int TcpPort { get; set; } = 47110;
    public int DiscoveryPort { get; set; } = 47111;
    public bool AutoStart { get; set; } = true;
    public string LogFilePath { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MayaRemote", "agent.log");
    public int KeepAliveIntervalSeconds { get; set; } = 30;
    public string? AllowedPublicKeyHex { get; set; }
}
