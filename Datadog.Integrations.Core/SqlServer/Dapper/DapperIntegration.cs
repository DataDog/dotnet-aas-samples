using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace Datadog.Integrations.Core.SqlServer.Dapper
{
	public class DapperIntegration
	{
		public static readonly int Id = IntegrationHelper.ClaimIntegrationId();
		private static readonly string TableText = Configuration.Dapper.SqlServerTable;

		private static readonly string RecreateTableCommandText =
			$"IF OBJECT_ID(N'dbo.{TableText}', N'U') IS NULL BEGIN   CREATE TABLE {TableText} (Id int, Text varchar(100)); END;";
		private static readonly string InsertCommandText = $"INSERT INTO {TableText} (Id, Text) VALUES (@Id, @Text);";
		private static readonly string SelectOneCommandText = $"SELECT TOP 1 Text FROM {TableText} WHERE Id=@Id;";
		private static readonly string UpdateCommandText = $"UPDATE {TableText} SET Text=@Text WHERE Id=@Id;";
		private static readonly string SelectManyCommandText = $"SELECT TOP 1  * FROM {TableText} WHERE Id=@Id;";
		private static readonly string DeleteCommandText = $"DELETE FROM {TableText} WHERE Id=@Id;";

		private static readonly string IntentionalFailureCommandText = $"SELECT * FROM dbo.ThisTableWillNeverExist WHERE Id=@Id;";

		private static readonly Random RandomGenerator = new Random(DateTime.UtcNow.Millisecond);

		public static async Task IntentionalFailure()
		{
			await using var connection = Configuration.SqlServer.CreateConnection();
			var command = new CommandDefinition(IntentionalFailureCommandText, new { Id = 1 });
			using (var reader = await connection.ExecuteReaderAsync(command))
			{
				var records = reader.AsDataRecords()
					.Select(
						r => new { Id = (int)r["Id"], Text = (string)r["Text"] })
					.ToList();

				Console.WriteLine($"Selected {records.Count} record(s).");
			}
			connection.Close();
		}

		public static async Task RunAsync()
		{
			await using var connection = await Configuration.SqlServer.CreateConnectionAsync();
			await RecreateTable(connection);
			var id = RandomGenerator.Next();
			await InsertRowAsync(connection, id);
			await SelectScalarAsync(connection, id);
			await QueryAsync(connection, id);
			await UpdateRowAsync(connection, id);
			await SelectRecordsAsync(connection, id);
			await DeleteRecordAsync(connection, id);
			await connection.CloseAsync();
		}

		public static async Task Run()
		{
			await using var connection = Configuration.SqlServer.CreateConnection();
			EnsureTableExists(connection);
			var id = RandomGenerator.Next();
			InsertRow(connection, id);
			SelectScalar(connection, id);
			Query(connection, id);
			UpdateRow(connection, id);
			SelectRecords(connection, id);
			DeleteRecord(connection, id);
			connection.Close();
		}

		private static void DeleteRecord(IDbConnection connection, int id)
		{
			int records = connection.Execute(DeleteCommandText, new { Id = id });
			Console.WriteLine($"Deleted {records} record(s).");
		}

		private static void SelectRecords(IDbConnection connection, int id)
		{
			var command = new CommandDefinition(SelectManyCommandText, new { Id = id });

			using (var reader = connection.ExecuteReader(command))
			{
				var records = reader.AsDataRecords()
									  .Select(
										   r => new { Id = (int)r["Id"], Text = (string)r["Text"] })
									  .ToList();

				Console.WriteLine($"Selected {records.Count} record(s).");
			}

			using (var reader = connection.ExecuteReader(command, CommandBehavior.Default))
			{
				var records = reader.AsDataRecords()
									  .Select(
										   r => new { Id = (int)r["Id"], Text = (string)r["Text"] })
									  .ToList();

				Console.WriteLine($"Selected {records.Count} record(s) with `CommandBehavior.Default`.");
			}
		}

		private static void Query(IDbConnection connection, int id)
		{
			var records = connection.Query(SelectManyCommandText, new { Id = id }).ToList();
			Console.WriteLine($"Selected {records.Count} record(s) with Query().");
		}

		private static void UpdateRow(IDbConnection connection, int id)
		{
			int records = connection.Execute(UpdateCommandText, new { Text = "Text2", Id = id });
			Console.WriteLine($"Updated {records} record(s).");
		}

		private static void SelectScalar(IDbConnection connection, int id)
		{
			var Text = connection.ExecuteScalar(SelectOneCommandText, new { Id = id });
			Console.WriteLine($"Selected scalar `{Text ?? "(null)"}`.");
		}

		private static void InsertRow(IDbConnection connection, int id)
		{
			int records = connection.Execute(InsertCommandText, new { Id = id, Text = "Text1" });
			Console.WriteLine($"Inserted {records} record(s).");
		}

		private static void EnsureTableExists(IDbConnection connection)
		{
			int records = connection.Execute(RecreateTableCommandText);
			Console.WriteLine($"Dropped and recreated table. {records} record(s) affected.");
		}

		private static async Task DeleteRecordAsync(IDbConnection connection, int id)
		{
			int records = await connection.ExecuteAsync(DeleteCommandText, new { Id = id });
			Console.WriteLine($"Deleted {records} record(s).");
		}

		private static async Task SelectRecordsAsync(IDbConnection connection, int id)
		{
			var command = new CommandDefinition(SelectManyCommandText, new { Id = id });

			using (var reader = await connection.ExecuteReaderAsync(command))
			{
				var records = reader.AsDataRecords()
									  .Select(
										   r => new { Id = (int)r["Id"], Text = (string)r["Text"] })
									  .ToList();

				Console.WriteLine($"Selected {records.Count} record(s).");
			}

			using (var reader = await connection.ExecuteReaderAsync(command, CommandBehavior.Default))
			{
				var records = reader.AsDataRecords()
									  .Select(
										   r => new { Id = (int)r["Id"], Text = (string)r["Text"] })
									  .ToList();
				Console.WriteLine($"Selected {records.Count} record(s) with `CommandBehavior.Default`.");
			}
		}

		private static async Task QueryAsync(IDbConnection connection, int id)
		{
			var records = (await connection.QueryAsync(SelectManyCommandText, new { Id = id })).ToList();
			Console.WriteLine($"Selected {records.Count} record(s) with Query().");
		}

		private static async Task UpdateRowAsync(IDbConnection connection, int id)
		{
			int records = await connection.ExecuteAsync(UpdateCommandText, new { Text = "Text2", Id = id });
			Console.WriteLine($"Updated {records} record(s).");
		}

		private static async Task SelectScalarAsync(IDbConnection connection, int id)
		{
			var Text = await connection.ExecuteScalarAsync<string>(SelectOneCommandText, new { Id = id });
			Console.WriteLine($"Selected scalar `{Text ?? "(null)"}`.");
		}

		private static async Task InsertRowAsync(IDbConnection connection, int id)
		{
			int records = await connection.ExecuteAsync(InsertCommandText, new { Text = "Text1", Id = id });
			Console.WriteLine($"Inserted {records} record(s).");
		}

		private static async Task RecreateTable(IDbConnection connection)
		{
			int records = await connection.ExecuteAsync(RecreateTableCommandText);
			Console.WriteLine($"Dropped and recreated table. {records} record(s) affected.");
		}
	}
}
