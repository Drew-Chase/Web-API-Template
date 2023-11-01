#if (UseSerilogLogging)
using Serilog;
using Serilog.Events;
using System.IO.Compression;
#endif
using WebAPITemplate.Data;

namespace WebAPITemplate;

public class Program
{
    public static void Main(string[] args)
    {
        ApplicationConfiguration.Instance.Initialize(Files.ApplicationConfiguration);
#if (UseSerilogLogging)
        ConfigureLogging();
#endif
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
#if (EnableSwaggerSupport)
        builder.Services.AddSwaggerGen();
#endif
#if (UseSerilogLogging)
        builder.Services.AddSerilog();
#endif
        builder.Services.AddMvc(options => options.EnableEndpointRouting = false);
        builder.Services.AddRazorPages().WithRazorPagesRoot("/Pages");

        var app = builder.Build();


        // Configure the HTTP request pipeline.
#if (EnableSwaggerSupport)
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseStatusCodePagesWithRedirects("/error/{0}");
            app.UseForwardedHeaders();
            app.UseHttpsRedirection();
        }

#else
        if (!app.Environment.IsDevelopment())
        {
            app.UseStatusCodePagesWithRedirects("/error/{0}");
            app.UseForwardedHeaders();
            app.UseHttpsRedirection();
        }
#endif

        app.UseRouting();
        app.MapControllers();
        app.MapRazorPages();
        app.UseAuthorization();
        app.UseDefaultFiles();
        app.UseStaticFiles();

        AppDomain.CurrentDomain.ProcessExit += (s, e) =>
        {
            ApplicationConfiguration.Instance.Save();

#if (UseSerilogLogging)
            Log.Debug("Application exiting after {TIME}.", ApplicationData.UpTime);
            Log.CloseAndFlush();
#endif
        };

        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            if (e.ExceptionObject is Exception exception)
            {
#if (UseSerilogLogging)
                Log.Fatal(exception, "Unhandled exception: {REPORT}", CrashHandler.Report(exception));
#else
                Console.Error.WriteLine("Unhandled exception: {0}", CrashHandler.Report(exception));
#endif
            }
        };

        app.Run($"http://localhost:{ApplicationConfiguration.Instance.Port}");
    }

#if (UseSerilogLogging)
    private static void ConfigureLogging()
    {
        // Initialize Logging
        string[] logs = Directory.GetFiles(Directories.Logs, "*.log");
        if (logs.Any())
        {
            using ZipArchive archive = ZipFile.Open(Path.Combine(Directories.Logs, $"logs-{DateTime.Now:MM-dd-yyyy HH-mm-ss.ffff}.zip"), ZipArchiveMode.Create);
            foreach (string log in logs)
            {
                archive.CreateEntryFromFile(log, Path.GetFileName(log));
                File.Delete(log);
            }
        }

        TimeSpan flushTime = TimeSpan.FromSeconds(30);
        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Verbose()
#else
            .MinimumLevel.Information()
#endif
            .WriteTo.Console(ApplicationConfiguration.Instance.LogLevel, outputTemplate: $"[{ApplicationData.ApplicationName}] [{{Timestamp:HH:mm:ss}} {{Level:u3}}] {{Message:lj}}{{NewLine}}{{Exception}}")
            .WriteTo.File(Files.DebugLog, LogEventLevel.Verbose, buffered: true, flushToDiskInterval: flushTime)
            .WriteTo.File(Files.LatestLog, LogEventLevel.Information, buffered: true, flushToDiskInterval: flushTime)
            .WriteTo.File(Files.ErrorLog, LogEventLevel.Error, buffered: false)
            .CreateLogger();
    }
#endif
}