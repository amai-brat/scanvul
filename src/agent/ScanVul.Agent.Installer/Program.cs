using System.CommandLine;
using System.IO.Compression;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;

namespace ScanVul.Agent.Installer;

internal static class Program
{
    private const string AppSettingsFileName = "appsettings.json";
    
    public static int Main(string[] args)
    {
        IPlatformInstaller installer = OperatingSystem.IsWindows() 
            ? new WindowsInstaller() 
            : new LinuxInstaller();

        Option<DirectoryInfo> pathOption = new("--path")
        {
            Description = "Installation path",
            DefaultValueFactory = _ => installer.DefaultInstallationPath
        };

        Option<Uri> addressOption = new("--server")
        {
            Description = "Server IP address/hostname with port. E.g. https://10.10.10.10:80/",
            Required = true,
            CustomParser = arg =>
            {
                if (Uri.TryCreate(arg.Tokens[0].Value, UriKind.Absolute, out var uri))
                    return uri;
        
                arg.AddError("Incorrect server address given");
                return null;
            }
        };
        
        RootCommand rootCommand = new("ScanVul.Agent installer");

        rootCommand.Options.Add(pathOption);
        rootCommand.Options.Add(addressOption);

        rootCommand.SetAction(async (parseResult, ct) =>
        {
            var serverAddress = parseResult.GetRequiredValue(addressOption);
            var path = parseResult.GetRequiredValue(pathOption);
            
            Console.WriteLine($"Installing to {path}...");
            var installResult = await InstallAgentAsync(path, installer.AgentZipResourceName, ct);
            if (installResult.IsFailure)
            {
                Console.WriteLine(installResult.Error);
                return;
            }
            
            Console.WriteLine($"Registering agent on server {serverAddress}...");
            var tokenResult = await RegisterAgentAsync(serverAddress, ct);
            if (tokenResult.IsFailure)
            {
                Console.WriteLine(tokenResult.Error);
                return;
            }
            
            Console.WriteLine("Creating agent's configuration file...");
            var settingsResult = InitAgentSettings(path, serverAddress, tokenResult.Value.ToString());
            if (settingsResult.IsFailure)
            {
                Console.WriteLine(settingsResult.Error);
                return;
            }
            
            Console.WriteLine("Adding agent to autostart...");
            var autostartResult = await installer.AddAgentToAutoStartAsync(path);
            if (autostartResult.IsFailure)
            {
                Console.WriteLine(autostartResult.Error);
                return;
            }
            
            Console.WriteLine("Successfully installed");
        });
        return rootCommand.Parse(args).Invoke();
    }

    private static async Task<Result> InstallAgentAsync(DirectoryInfo path, string resourceName, CancellationToken ct = default)
    {
        try
        {
            if (!path.Exists) path.Create();
        
            var assembly = Assembly.GetExecutingAssembly();
            await using var resource = assembly.GetManifestResourceStream(resourceName);

            var zipFileName = Path.GetTempFileName();
            await using (var file = File.Create(zipFileName))
            {
                await resource!.CopyToAsync(file, ct);
            }

            using (var archive = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
            {
                archive.ExtractToDirectory(path.FullName, overwriteFiles: true);
            }
        
            File.Delete(zipFileName);
        
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error when installing agent to path {path.FullName}", ex);
        }
    }

    private static async Task<Result<Guid>> RegisterAgentAsync(Uri serverAddress, CancellationToken ct = default)
    {
        try
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = serverAddress;

            var response = await httpClient.PostAsync("/api/v1/agents/register", null, ct);
            if (!response.IsSuccessStatusCode)
                return Result.Failure<Guid>(response.ReasonPhrase ?? "Error");
        
            var resp = await response.Content.ReadFromJsonAsync<RegisterResponse>(
                RegisterResponseContext.Default.RegisterResponse, 
                cancellationToken: ct);
            return resp!.Token;
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>($"Error when registering agent on server", ex);
        }
    }

    private static Result InitAgentSettings(DirectoryInfo path, Uri serverAddress, string token)
    {
        try
        {
            if (!path.Exists) path.Create();
            
            var settings = new AgentSettings
            {
                Server = new AgentSettings.ServerSettings
                {
                    BaseUrl = serverAddress.ToString(),
                    Token = token
                }
            };
            var settingsPath = Path.Combine(path.FullName, AppSettingsFileName);
            var bytes = JsonSerializer.SerializeToUtf8Bytes(settings, AgentSettingsContext.Default.AgentSettings);
            using var fs = File.Create(settingsPath);
            using var writer = new BinaryWriter(fs);
            writer.Write(bytes);
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error when creating {AppSettingsFileName}", ex);
        }
    }
}