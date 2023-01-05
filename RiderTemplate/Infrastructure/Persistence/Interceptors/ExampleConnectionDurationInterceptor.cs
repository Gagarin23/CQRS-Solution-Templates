using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Options;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Persistence.Interceptors;

public class ExampleConnectionDurationInterceptor : IDbCommandInterceptor
{
    private readonly ILogger<ExampleConnectionDurationInterceptor> _logger;
    private readonly bool _isConnectionDurationMonitoringOn;
    private readonly TimeSpan _connectionDurationWarningInMilliseconds;

    public ExampleConnectionDurationInterceptor(IOptions<ApplicationOptions> options, ILogger<ExampleConnectionDurationInterceptor> logger)
    {
        _logger = logger;
        _isConnectionDurationMonitoringOn = options.Value.IsConnectionDurationMonitoringOn;
        _connectionDurationWarningInMilliseconds = options.Value.ConnectionDurationWarningInMilliseconds > 0 ?
            TimeSpan.FromMilliseconds(options.Value.ConnectionDurationWarningInMilliseconds) : 
            TimeSpan.MaxValue;
    }

    public ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        WriteLog(command, eventData);
        return new(result);
    }

    public ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        WriteLog(command, eventData);
        return new(result);
    }

    public ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
    {
        WriteLog(command, eventData);
        return new(result);
    }

    private void WriteLog(DbCommand command, CommandExecutedEventData eventData)
    {
        if (_isConnectionDurationMonitoringOn && eventData.Duration >= _connectionDurationWarningInMilliseconds)
        {
            _logger.LogWarning(eventData.EventId, $"Connection duration was {eventData.Duration.Milliseconds}ms\n" +
                                                  $"Current duration warning set to {_connectionDurationWarningInMilliseconds.Milliseconds}ms" +
                                                  $"Query: {command.CommandText}");
        }
    }
}
