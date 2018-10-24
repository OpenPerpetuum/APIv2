using Microsoft.SqlServer.Server;
using OpenPerpetuum.Core.DataServices.Database.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace OpenPerpetuum.Core.DataServices.Database.MSSQL
{
	public class MicrosoftSqlDatabaseProvider : IDatabaseProvider
	{
		public string ProviderName { get; }
		public string ConnectionString { get; }

		public static string CreateConnectionString(string username, string password, string server, string defaultDatabase)
		{
			return $"Server={server};Database={defaultDatabase};Persist Security Info=False;User ID={username};Password={password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;";
		}

		public MicrosoftSqlDatabaseProvider(string providerName, string connectionString)
		{
			ProviderName = providerName;
			ConnectionString = connectionString;
		}

		public MicrosoftSqlDatabaseProvider(string providerName, string username, string password, string server, string defaultDatabase)
		{
			ProviderName = providerName;
			ConnectionString = CreateConnectionString(username, password, server, defaultDatabase);
		}

		public IResult<TResultObject> ExecuteProcedure<TResultObject>(string procedureName, IDictionary<string, object> inputParameters) where TResultObject : DatabaseResult, new()
		{
			using (var connection = new SqlConnection(ConnectionString))
			using (var command = new SqlCommand(procedureName, connection) { CommandType = CommandType.StoredProcedure })
			{
				var dataTables =
					inputParameters
						.Where(param => param.Value is DynamicDataTable)
						.Select(param => new KeyValuePair<string, DynamicDataTable>(param.Key, (DynamicDataTable)param.Value));

				if (dataTables.Any())
					ParseDataTables(command, dataTables);

				Dictionary<string, dynamic> precisionArgs = new Dictionary<string, dynamic>();
				// When I have the foundation finished I'll integrate this better as part of the command structure and meta data process
				using (var precisionCommand = new SqlCommand("select PARAMETER_NAME, NUMERIC_PRECISION, NUMERIC_SCALE from information_schema.parameters where specific_schema = @SchemaName and specific_name = @ProcedureName and DATA_TYPE = 'decimal'", connection) { CommandType = CommandType.Text })
				{
					connection.Open();

					string[] procedureParts = procedureName.Split(".");
					string schemaName = procedureParts.Length == 2 ? procedureParts[0] : "dbo";
					string procName = procedureParts.Length == 2 ? procedureParts[1] : procedureParts[0];

					var schemaParam = precisionCommand.CreateParameter();
					schemaParam.ParameterName = "SchemaName";
					schemaParam.Value = schemaName;

					var procParam = precisionCommand.CreateParameter();
					procParam.ParameterName = "ProcedureName";
					procParam.Value = procName;

					precisionCommand.Parameters.AddRange(new SqlParameter[]
					{
						schemaParam,
						procParam
					});

					using (var pReader = precisionCommand.ExecuteReader())
						while (pReader.Read())
						{
							string name = (string)pReader["PARAMETER_NAME"];
							byte precision = (byte)pReader["NUMERIC_PRECISION"];
							byte scale = (byte)(int)pReader["NUMERIC_SCALE"];

							dynamic precisionObject = new ExpandoObject();
							precisionObject.name = name.TrimStart('@');
							precisionObject.precision = precision;
							precisionObject.scale = scale;

							precisionArgs.Add(precisionObject.name, precisionObject);
						}
				}

				var parameters = inputParameters
					.Where(param => !(param.Value is DynamicDataTable))
					.Select(param =>
					{
						var newParam = command.CreateParameter();
						newParam.ParameterName = param.Key;
						newParam.Value = param.Value;

						if (precisionArgs.ContainsKey(param.Key))
						{
							newParam.Scale = precisionArgs[param.Key].scale;
							newParam.Precision = precisionArgs[param.Key].precision;
						}

						return newParam;
					})
					.ToArray();

				SqlParameter returnParam = command.Parameters.Add("RETURN_VALUE", SqlDbType.Int);
				returnParam.Direction = ParameterDirection.ReturnValue;

				command.Parameters.AddRange(parameters);

				try
				{
					if (connection.State != ConnectionState.Open)
						connection.Open();

					var reader = command.ExecuteReader();
					var results = CreateResult<TResultObject>(reader);

					var returnValue = 0;
					try
					{
						returnValue = (int)command.Parameters["RETURN_VALUE"].Value;
					}
					catch
					{
						returnValue = -1;
					}

					results.ReturnValue = returnValue;
					return new Result<TResultObject>(results);
				}
				catch (SqlException exc)
				{
					return new Result<TResultObject>(null, exc.Number, exc.Message);
				}
				finally
				{
					connection.Close();
				}
			}
		}

		private void ParseDataTables(SqlCommand dbCommand, IEnumerable<KeyValuePair<string, DynamicDataTable>> dataTables)
		{
			var tableParameters = new Dictionary<string, List<SqlDataRecord>>();

			foreach(var table in dataTables)
			{
				var rowRecords = new List<SqlDataRecord>();

				foreach(var dataRow in table.Value)
				{
					var rowMetaData = new List<SqlMetaData>();
					var rowData = new Dictionary<int, object>();

					foreach (var dataMember in dataRow.PropertyNames)
					{
						var dataItemMeta = dataRow.GetPropertyAttribute(dataMember);
						var dataItemValue = dataRow[dataMember] ?? DBNull.Value;
						SqlMetaData metaData;
						SqlDbType dataType = SqlDbTypeHelper.GetDbType(dataRow.GetPropertyType(dataMember));

						if (dataItemMeta.MaxLength != 0)
							metaData = new SqlMetaData(dataMember, dataType, dataItemMeta.MaxLength);
						else if (dataType == SqlDbType.Decimal)
							metaData = new SqlMetaData(dataMember, dataType, dataItemMeta.DecimalPrecision, dataItemMeta.DecimalScale); // If they aren't set, this will crash
						else
							metaData = new SqlMetaData(dataMember, dataType);

						rowMetaData.Add(metaData);
						rowData.Add(dataItemMeta.Ordinal, dataItemValue);
					}

					var dataRecord = new SqlDataRecord(rowMetaData.ToArray());

					foreach (var item in rowData)
						dataRecord.SetValue(item.Key, item.Value);

					rowRecords.Add(dataRecord);
				}

				var param = dbCommand.Parameters.Add(table.Key, SqlDbType.Structured);
				param.Direction = ParameterDirection.Input;
				param.TypeName = table.Value.DatabaseTypeName;
				param.Value = rowRecords;
			}
		}

		private TResultObject CreateResult<TResultObject>(SqlDataReader reader)
			where TResultObject : DatabaseResult, new()
		{
			var result = new TResultObject();
			int resultSetIndex = 0;

			if (result.Results.Count == 0)
				return new TResultObject();

			Type genericType = result.Results[resultSetIndex].GetType().GetGenericArguments().First();
			if (reader.HasRows)
			{
				var dataObject = ReadResultSet(resultSetIndex, genericType, reader);
				result.Results[resultSetIndex] = dataObject;
			}
			else
				result.Results[resultSetIndex] = (ResultSet)GetDefault(genericType);

			while (reader.NextResult())
			{
				resultSetIndex++;

				genericType = result.Results[resultSetIndex].GetType().GetGenericArguments().First();
				if (reader.HasRows)
				{
					var dataObject = ReadResultSet(resultSetIndex, genericType, reader);
					result.Results[resultSetIndex] = dataObject;
				}
				else
					result.Results[resultSetIndex] = (ResultSet)GetDefault(genericType);
			}

			return result;
		}

		private ResultSet ReadResultSet(int resultSetIndex, Type resultType, IDataReader reader)
		{
			var createResultMethod = typeof(ResultSet).GetMethod("CreateResultSet", BindingFlags.Static | BindingFlags.Public);
			var methodToExecute = createResultMethod.MakeGenericMethod(resultType);
			var resultSet = methodToExecute.Invoke(null, new object[] { resultSetIndex, reader });

			return resultSet as ResultSet;
		}

		private object GetDefault(Type type)
		{
			if (type.GetTypeInfo().IsValueType)
				return Activator.CreateInstance(type);
			else
				return null;
		}
	}
}
