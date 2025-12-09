namespace ScanVul.Server.Domain.AgentAggregate.Enums;

public enum OperatingSystem
{
    /// <summary>
    /// Unknown
    /// </summary>
    Unknown = 0,
    
    /// <summary>
    /// Windows versions (1000-1999)
    /// </summary>
    Windows = 1000,
    
    /// <summary>
    /// Linux distributions (2000-2999)
    /// </summary>
    Linux = 2000,
    
    /// <summary>
    /// Alt Linux (2100-2150)
    /// </summary>
    Alt = 2100,
    
    /// <summary>
    /// Arch-based (2200-2300)
    /// </summary>
    Arch = 2200,
    
    /// <summary>
    /// Debian-based (2500-2700)
    /// </summary>
    Debian = 2500,
    
    /// <summary>
    /// OSX (3000-3999)
    /// </summary>
    Osx = 3000,
    
    /// <summary>
    /// BSD (4000-4999)
    /// </summary>
    Bsd = 4000,
    
    /// <summary>
    /// Other
    /// </summary>
    Other = 5000
}