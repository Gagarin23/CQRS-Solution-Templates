using System;

namespace Infrastructure;

public static class EnvironmentExtension
{
    public static readonly string CurrentEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    public static readonly bool IsDevelopment = CurrentEnvironment == "Development";
}
