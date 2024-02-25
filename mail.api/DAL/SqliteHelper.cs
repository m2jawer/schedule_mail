using Microsoft.Data.Sqlite;
using System.Data;

namespace mail.api.DAL
{
    public static class SqliteHelper
    {
        public static async Task<DataTable> ExecuteWithQuery(string queryCmd, Dictionary<string, object> paras)
        {
            string dbPath = AppDomain.CurrentDomain.BaseDirectory + "Data\\schedule.db";

            using (var connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = queryCmd;

                if (paras != null)
                {
                    foreach (var key in paras.Keys)
                    {
                        command.Parameters.AddWithValue("$" + key, paras[key]);
                    }
                }

                DataTable retDt = new DataTable();

                using (var reader = command.ExecuteReader())
                {
                    var columns = await reader.GetColumnSchemaAsync();

                    foreach (var column in columns)
                    {
                        retDt.Columns.Add(new DataColumn(column.ColumnName, column.DataType!));
                    }

                    while (reader.Read())
                    {
                        var row = retDt.NewRow();

                        for (int i = 0; i < columns.Count; i++)
                        {
                            row[i] = reader[i].ToString();
                        }

                        retDt.Rows.Add(row);
                    }
                }

                return retDt;
            }
        }
    }
}
