namespace ScanVul.Agent.Services.PackageManagers;

public class WingetCommandBuilder
{
    public WingetCommandBuilder(string executablePath, string commandName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(commandName);
        
        ExecutablePath = $"& '{executablePath}'";
        CommandName = commandName;
    }

    public string ExecutablePath { get; private set; }
    public string CommandName { get; private set; }

    public HashSet<string> Params { get; private set; } = [];

    public WingetCommandBuilder WithDefaultParams()
    {
        return WithSilent()
            .WithMachineScope()
            .WithAcceptSourceAgreements()
            .WithAcceptPackageAgreements();
    }

    public string Build()
    {
        return string.Join(" ", [ExecutablePath, CommandName, ..Params, "2>&1; $LASTEXITCODE"]);
    }
    
    /// <summary>
    /// Add package id
    /// </summary>
    public WingetCommandBuilder WithId(string id)
    {
        Params.Add($"--id {id}");
        return this;
    }
    
    /// <summary>
    /// Hides the installer UI
    /// </summary>
    public WingetCommandBuilder WithSilent()
    {
        Params.Add("--silent");
        return this;
    }
    
    /// <summary>
    /// Exact match
    /// </summary>
    public WingetCommandBuilder WithExactMatch()
    {
        Params.Add("--exact");
        return this;
    }
    
    /// <summary>
    /// Auto-accepts EULAs
    /// </summary>
    public WingetCommandBuilder WithAcceptPackageAgreements()
    {
        Params.Add("--accept-package-agreements");
        return this;
    }
    
    /// <summary>
    /// Auto-accepts source terms
    /// </summary>
    public WingetCommandBuilder WithAcceptSourceAgreements()
    {
        Params.Add("--accept-source-agreements");
        return this;
    }
    
    /// <summary>
    /// System-wide context
    /// </summary>
    public WingetCommandBuilder WithMachineScope()
    {
        Params.Add("--scope machine");
        return this;
    }
    
    /// <summary>
    /// Winget source
    /// </summary>
    public WingetCommandBuilder WithWingetSource()
    {
        Params.Add("--source winget");
        return this;
    }
    
    /// <summary>
    /// Upgrade even if current version is not available
    /// </summary>
    public WingetCommandBuilder WithIncludeUnknown()
    {
        Params.Add("--include-unknown");
        return this;
    }
}