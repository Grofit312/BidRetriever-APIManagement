using _440DocumentManagement.Helpers;
using _440DocumentManagement.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Data;

namespace _440DocumentManagement.Services.Concrete
{
	public class BaseService : IBaseService
	{
		private readonly IDbConnection _dbConnection;

		public BaseService(
			IDbConnection dbConnection)
		{
			_dbConnection = dbConnection;
		}
		
		public string CreateRecord<Q>(Q newRecord, string tableName, string primaryKey)
		{
			string newRecordKey = "";
			try
			{
				using (var conn = _dbConnection)
				{
					conn.Open();
					using (var cmd = conn.CreateCommand())
					{
						var modelPropertyNames = typeof(Q).GetProperties().Select(property => property.Name).ToArray();
						var bindPropertyNames = modelPropertyNames.Select(propertyName =>
						{
							return Regex.Replace(propertyName, @"([A-Z])", "_$1").Substring(1).ToLower();
						}).ToArray();

						cmd.CommandText = $"INSERT INTO \"{tableName}\""
							+ $"({string.Join(",", bindPropertyNames)}) VALUES ("
							+ string.Join(",", bindPropertyNames.Select(name => $"@{name}"))
							+ ");";

						for (var index = 0; index < modelPropertyNames.Length; index++)
						{
							try
							{
								var modelPropertyValue = newRecord.GetType().GetProperty(modelPropertyNames[index]).GetValue(newRecord, null);
								if (bindPropertyNames[index] == primaryKey)
								{
									modelPropertyValue = modelPropertyValue ?? Guid.NewGuid().ToString();
									newRecordKey = modelPropertyValue.ToString();
								}
								else if (modelPropertyValue == null)
								{
									if (bindPropertyNames[index] == "create_datetime"
										|| bindPropertyNames[index] == "edit_datetime")
									{
										modelPropertyValue = DateTime.UtcNow;
									}
								}
								cmd.AddWithValue($"@{bindPropertyNames[index]}", modelPropertyValue ?? DBNull.Value);
							}
							catch (NullReferenceException)
							{
								if (bindPropertyNames[index] == primaryKey)
								{
									newRecordKey = Guid.NewGuid().ToString();
									cmd.AddWithValue($"@{bindPropertyNames[index]}", newRecordKey);
								}
								else if (bindPropertyNames[index] == "create_datetime"
									|| bindPropertyNames[index] == "edit_datetime")
								{
									cmd.AddWithValue($"@{bindPropertyNames[index]}", DateTime.UtcNow);
								}
								else
								{
									cmd.AddWithValue($"@{bindPropertyNames[index]}", DBNull.Value);
								}
							}
						}

						cmd.ExecuteNonQuery();

						return newRecordKey;
					}
				}
			}
			catch (Exception ex)
			{
				throw new ApiException(ex.Message);
			}
		}

		public List<object> FindRecords<Q, SBA, SAL, SAD, SCOM>(Q request, string tableName, string additionalResponseParams = "", string joinQueries = "", string whereQueries = "")
			where SBA : new()
			where SAL : new()
			where SAD : new()
			where SCOM : new()
		{
			try
			{
				using (var conn = _dbConnection)
				{
					conn.Open();
					using (var cmd = conn.CreateCommand())
					{
						var whereModelPropertyNames = typeof(Q).GetProperties().Select(property => property.Name).ToArray();
						var whereBindPropertyNames = whereModelPropertyNames.Select(propertyName =>
						{
							return Regex.Replace(propertyName, @"([A-Z])", "_$1").Substring(1).ToLower();
						}).ToArray();

						string detailLevel = null;
						var where = "# ";
						for (var index = 0; index < whereModelPropertyNames.Length; index++)
						{
							if (whereModelPropertyNames[index] == "DetailLevel")
							{
								detailLevel = Convert.ToString(request.GetType().GetProperty(whereModelPropertyNames[index]).GetValue(request, null));
								continue;
							}
							var whereModelPropertyValue = request.GetType().GetProperty(whereModelPropertyNames[index]).GetValue(request, null);
							if (whereModelPropertyValue != null)
							{
								if (whereModelPropertyValue.GetType().IsArray)
								{
									where += " (";
									for (var valIndex = 0; valIndex < ((object[])whereModelPropertyValue).Length; valIndex ++)
									{
										where += $"\"{tableName}\".{whereBindPropertyNames[index]}=@{whereBindPropertyNames[index]}_{valIndex} $ ";
										cmd.AddWithValue($"@{whereBindPropertyNames[index]}_{valIndex}", ((object[])whereModelPropertyValue)[valIndex]);
									}
									where = where.Remove(where.Length - 2);
									where += ") * ";
								}
								else
								{
									where += $" \"{tableName}\".{whereBindPropertyNames[index]}=@{whereBindPropertyNames[index]} * ";
									cmd.AddWithValue($"@{whereBindPropertyNames[index]}", whereModelPropertyValue);
								}
							}
						}
						where = where.Remove(where.Length - 2);
						where = where.Replace("#", " WHERE ").Replace("* ", "AND ").Replace("$ ", "OR ");
						if (!string.IsNullOrEmpty(whereQueries))
						{
							if (string.IsNullOrEmpty(where))
							{
								where = " WHERE ";
							}
							where += $"({whereQueries})";
						}

						if (string.IsNullOrEmpty(detailLevel))
						{
							detailLevel = "basic";
						}
						detailLevel = detailLevel.ToLower();
						string[] modelPropertyNames;
						switch (detailLevel)
						{
							case "compact":
								modelPropertyNames = typeof(SCOM).GetProperties().Select(property => property.Name).ToArray();
								break;
							case "all":
								modelPropertyNames = typeof(SAL).GetProperties().Select(property => property.Name).ToArray();
								break;
							case "admin":
								modelPropertyNames = typeof(SAD).GetProperties().Select(property => property.Name).ToArray();
								break;
							case "basic":
							default:
								modelPropertyNames = typeof(SBA).GetProperties().Select(property => property.Name).ToArray();
								break;
						}
						var bindPropertyNames = modelPropertyNames.Select(propertyName =>
						{
							return Regex.Replace(propertyName, @"([A-Z])", "_$1").Substring(1).ToLower();
						}).ToArray();
						var bindQueryPropertyNames = bindPropertyNames.Select(propertyName =>
						{
							return $"\"{tableName}\".{propertyName}";
						});

						cmd.CommandText = $"SELECT {string.Join(",", bindQueryPropertyNames)}{(additionalResponseParams == "" ? "" : "," + additionalResponseParams)} FROM \"{tableName}\" "
							+ joinQueries + " "
							+ where;

						var reader = cmd.ExecuteReader();
						var resultList = new List<object>();

						while (reader.Read())
						{
							object record;
							switch (detailLevel)
							{
								case "compact": record = new SCOM(); break;
								case "all": record = new SAL(); break;
								case "admin": record = new SAD(); break;
								case "basic":
								default: record = new SBA(); break;
							}
							for (var index = 0; index < modelPropertyNames.Length; index++)
							{
								record.GetType().GetProperty(modelPropertyNames[index]).SetValue(record, reader[bindPropertyNames[index]] == DBNull.Value ? null : reader[bindPropertyNames[index]]);
							}
							resultList.Add(record);
						}

						return resultList;
					}
				}
			}
			catch (Exception ex)
			{
				throw new ApiException(ex.Message);
			}
		}

		public S GetRecord<Q, S>(Q request, string tableName) where S : new()
		{
			try
			{
				using (var conn = _dbConnection)
				{
					conn.Open();
					using (var cmd = conn.CreateCommand())
					{
						var modelPropertyNames = typeof(S).GetProperties().Select(property => property.Name).ToArray();
						var bindPropertyNames = modelPropertyNames.Select(propertyName =>
						{
							return Regex.Replace(propertyName, @"([A-Z])", "_$1").Substring(1).ToLower();
						}).ToArray();

						var where = "# ";
						var whereModelPropertyNames = request.GetType().GetProperties().Select(property => property.Name).ToArray();
						var whereBindPropertyNames = whereModelPropertyNames.Select(propertyName =>
						{
							return Regex.Replace(propertyName, @"([A-Z])", "_$1").Substring(1).ToLower();
						}).ToArray();
						for (var index = 0; index < whereModelPropertyNames.Length; index ++)
						{
							where += $"{whereBindPropertyNames[index]}=@{whereBindPropertyNames[index]} * ";
							cmd.AddWithValue(
								$"@{whereBindPropertyNames[index]}",
								request.GetType().GetProperty(whereModelPropertyNames[index]).GetValue(request, null));
						}
						where = where.Remove(where.Length - 2);
						where = where.Replace("# ", " WHERE ").Replace("* ", "AND ");

						cmd.CommandText = $"SELECT {string.Join(",", bindPropertyNames)} FROM \"{tableName}\"" + where;
						var reader = cmd.ExecuteReader();
						if (reader.Read())
						{
							var record = new S();
							for (var index = 0; index < modelPropertyNames.Length; index++)
							{
								record.GetType().GetProperty(modelPropertyNames[index]).SetValue(record, reader[bindPropertyNames[index]] == DBNull.Value ? null : reader[bindPropertyNames[index]]);
							}
							return record;
						}

						return default(S);
					}
				}
			}
			catch (Exception ex)
			{
				throw new ApiException(ex.Message);
			}
		}

		public int RemoveRecords<Q>(Q request, string tableName)
		{
			try
			{
				using (var conn = _dbConnection)
				{
					conn.Open();
					using (var cmd = conn.CreateCommand())
					{
						var where = "# ";
						var whereModelPropertyNames = request.GetType().GetProperties().Select(property => property.Name).ToArray();
						var whereBindPropertyNames = whereModelPropertyNames.Select(propertyName =>
						{
							return Regex.Replace(propertyName, @"([A-Z])", "_$1").Substring(1).ToLower();
						}).ToArray();
						for (var index = 0; index < whereModelPropertyNames.Length; index++)
						{
							where += $"{whereBindPropertyNames[index]}=@{whereBindPropertyNames[index]} * ";
							cmd.AddWithValue(
								$"@{whereBindPropertyNames[index]}",
								request.GetType().GetProperty(whereModelPropertyNames[index]).GetValue(request, null));
						}
						where = where.Remove(where.Length - 2);
						where = where.Replace("# ", " WHERE ").Replace("* ", "AND ");

						cmd.CommandText = $"DELETE FROM \"{tableName}\"" + where;
						var removedRowCount = cmd.ExecuteNonQuery();
						return removedRowCount;
					}
				}
			}
			catch (Exception ex)
			{
				throw new ApiException(ex.Message);
			}
		}

		public int UpdateRecords<Q>(Q request, string tableName)
		{
			try
			{
				using (var conn = _dbConnection)
				{
					conn.Open();
					using (var cmd = conn.CreateCommand())
					{
						var requestPropertyNames = request.GetType().GetProperties().Select(property => property.Name).ToArray();
						var whereModelPropertyNames = requestPropertyNames.Where(propertyName => propertyName.StartsWith("Search")).ToArray();
						var whereBindPropertyNames = whereModelPropertyNames.Select(propertyName =>
						{
							return Regex.Replace(propertyName.Substring(6), @"([A-Z])", "_$1").Substring(1).ToLower();
						}).ToArray();

						var where = "# ";
						for (var index = 0; index < whereModelPropertyNames.Length; index++)
						{
							var whereModelPropertyValue = request.GetType().GetProperty(whereModelPropertyNames[index]).GetValue(request, null);
							if (whereModelPropertyValue == null)
							{
								continue;
							}
							where += $"{whereBindPropertyNames[index]}=@search_{whereBindPropertyNames[index]} * ";
							cmd.AddWithValue($"@search_{whereBindPropertyNames[index]}", whereModelPropertyValue);
						}
						where = where.Remove(where.Length - 2);
						where = where.Replace("# ", " WHERE ").Replace("* ", "AND ");

						var modelPropertyNames = requestPropertyNames.Where(propertyName => !propertyName.StartsWith("Search")).ToArray();
						var bindPropertyNames = modelPropertyNames.Select(propertyName =>
						{
							return Regex.Replace(propertyName, @"([A-Z])", "_$1").Substring(1).ToLower();
						}).ToArray();
						var query = $"UPDATE \"{tableName}\" SET edit_datetime=@edit_datetime, ";
						cmd.AddWithValue("edit_datetime", DateTime.UtcNow);
						for (var index = 0; index < modelPropertyNames.Length; index++)
						{
							var modelPropertyValue = request.GetType().GetProperty(modelPropertyNames[index]).GetValue(request, null);
							var modelPropertyType = request.GetType().GetProperty(modelPropertyNames[index]).PropertyType;
							if (modelPropertyType == typeof(DateTime?)
								&& (DateTime?)modelPropertyValue == ApiExtension.UNDEFINED_DATETIME)
							{
								continue;
							}
							if (modelPropertyType == typeof(string)
								&& (string)modelPropertyValue == ApiExtension.UNDEFINED_STRING)
							{
								continue;
							}
							if (modelPropertyType == typeof(int?)
								&& (int?)modelPropertyValue == ApiExtension.UNDEFINED_INT)
							{
								continue;
							}
							if (modelPropertyType == typeof(short?)
								&& (short?)modelPropertyValue == ApiExtension.UNDEFINED_SHORT)
							{
								continue;
							}
							if (modelPropertyType != typeof(DateTime?)
								&& modelPropertyType != typeof(string)
								&& modelPropertyType != typeof(int?)
								&& modelPropertyValue == null)
							{
								continue;
							}
							query += $"{bindPropertyNames[index]}=@{bindPropertyNames[index]}, ";
							cmd.AddWithValue($"@{bindPropertyNames[index]}", modelPropertyValue ?? DBNull.Value);
						}
						query = query.Remove(query.Length - 2);
						query += where;

						cmd.CommandText = query;
						var updatedRowCount = cmd.ExecuteNonQuery();
						return updatedRowCount;
					}
				}
			}
			catch (Exception ex)
			{
				throw new ApiException(ex.Message);
			}
		}
	}
}
