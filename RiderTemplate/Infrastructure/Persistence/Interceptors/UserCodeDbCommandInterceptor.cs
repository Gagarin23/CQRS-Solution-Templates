using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Persistence.Interceptors
{
    /// <summary>
    /// Помечаем запросы, которые были сгенерированы пользовательским кодом
    /// </summary>
    public class UserCodeDbCommandInterceptor : DbCommandInterceptor
    {
        public const string UserCodeQueryComment = "--UserQuery\n";

        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            command.CommandText = UserCodeQueryComment + command.CommandText;
            return base.ReaderExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = new CancellationToken())
        {
            command.CommandText = UserCodeQueryComment + command.CommandText;
            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
        {
            command.CommandText = UserCodeQueryComment + command.CommandText;
            return base.NonQueryExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = new CancellationToken())
        {
            command.CommandText = UserCodeQueryComment + command.CommandText;
            return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
        }
    }
}
