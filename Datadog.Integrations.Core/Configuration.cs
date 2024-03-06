using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Npgsql;

namespace Datadog.Integrations.Core
{
	public static class Configuration
	{
		public static class Datadog
		{
			public static string Env = Environment.GetEnvironmentVariable("DD_ENV") ?? "not_set";
			public static string Service = Environment.GetEnvironmentVariable("DD_SERVICE") ?? "not_set";
			public static string Version = Environment.GetEnvironmentVariable("DD_VERSION") ?? "not_set";

			public static bool ExtensionEnabled = Environment.GetEnvironmentVariable("DD_AZURE_APP_SERVICES") != null;

			/// <summary>
			/// This will ultimately be based on direct stats submission to decouple from the extension.
			/// This allows us to prevent a skew in first chance exceptions for stats in untraced deploys.
			/// TODO
			/// </summary>
			public static bool StatsEnabled = ExtensionEnabled;
		}

		public static class Dapper
		{
			public static string SqlServerTable => Environment.GetEnvironmentVariable("JUNKYARD_SQL_SERVER_TABLE") ?? "sqldev";
		}

		public static class SqlServer
		{
			public static string ConnectionString => Environment.GetEnvironmentVariable("JUNKYARD_SQL_SERVER_CONNECTION");

			public static SqlConnection CreateConnection()
			{
				var connection = new SqlConnection(ConnectionString);

				if (connection.State != ConnectionState.Open)
				{
					connection.Open();
				}

				return connection;
			}

			public static async Task<SqlConnection> CreateConnectionAsync()
			{
				var connection = new SqlConnection(ConnectionString);

				if (connection.State != ConnectionState.Open)
				{
					await connection.OpenAsync();
				}

				return connection;
			}
		}

		public static class NpgSql
		{
			public static string ConnectionString => Environment.GetEnvironmentVariable("JUNKYARD_POSTGRES_CONNECTION");

			public static NpgsqlConnection CreateConnection()
			{
				var connection = new NpgsqlConnection(ConnectionString);

				if (connection.State != ConnectionState.Open)
				{
					connection.Open();
				}

				return connection;
			}

			public static async Task<NpgsqlConnection> CreateConnectionAsync()
			{
				var connection = new NpgsqlConnection(ConnectionString);

				if (connection.State != ConnectionState.Open)
				{
					await connection.OpenAsync();
				}

				return connection;
			}
		}

		public static class AzureServiceBus
		{
			public static string ConnectionString => Environment.GetEnvironmentVariable("JUNKYARD_AZURE_SERVICE_BUS_CONNECTION");
			public static string QueueName => Environment.GetEnvironmentVariable("JUNKYARD_AZURE_SERVICE_BUS_QUEUE") ?? "junklinedev";
		}
	}
}