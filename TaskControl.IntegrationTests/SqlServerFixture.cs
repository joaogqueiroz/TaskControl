using System.Data.SqlClient;
using System.Text.RegularExpressions;
using Testcontainers.MsSql;

namespace TaskControl.IntegrationTests
{
    // Starts one SQL Server container for the whole test run and creates the tables from SQLQuerys.sql.
    public class SqlServerFixture : IAsyncLifetime
    {
        private readonly MsSqlContainer _container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();

        public string ConnectionString { get; private set; } = string.Empty;

        public async System.Threading.Tasks.Task InitializeAsync()
        {
            await _container.StartAsync();
            ConnectionString = _container.GetConnectionString();

            var script = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "SQLQuerys.sql"));
            var batches = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)
                .Where(batch => !string.IsNullOrWhiteSpace(batch));

            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            foreach (var batch in batches)
            {
                using var command = new SqlCommand(batch, connection);
                command.ExecuteNonQuery();
            }
        }

        public System.Threading.Tasks.Task DisposeAsync() => _container.DisposeAsync().AsTask();
    }

    [CollectionDefinition(Name)]
    public class SqlServerCollection : ICollectionFixture<SqlServerFixture>
    {
        public const string Name = "SqlServer";
    }
}
