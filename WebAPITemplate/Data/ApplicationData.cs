﻿using System.Reflection;

namespace WebAPITemplate.Data;

public static class ApplicationData
{
    public static string ApplicationName { get; } = "WebAPITemplate";
    public static TimeSpan UpTime => DateTime.Now - ApplicationConfiguration.Instance.StartupTime;
    public static Assembly MainAssembly { get; } = Assembly.GetExecutingAssembly();
    public static AssemblyName? AssemblyName { get; } = MainAssembly.GetName();
    public static Version? Version { get; } = AssemblyName.Version;

    public static object GenerateApplicationData()
    {
        return new
        {
            ApplicationName,
            Version,
            UpTime,
            ApplicationConfiguration.Instance.StartupTime,
#if DEBUG
            Environment = "DEVELOPMENT",
#else
            Environment = "RELEASE",
#endif
            Config = ApplicationConfiguration.Instance,
        };
    }
}