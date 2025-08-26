using Dapper;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using static Dapper.SqlMapper;

namespace CryptoWeb.Shared.Helper.SQLHelper
{
    public static class SQLQueryHelper
    {
        public static async Task<IEnumerable<T>> SelectAllAsync<T>(IDbConnection connection) where T : class
        {
            string query = $"SELECT * FROM {GetTableName<T>()}";
            return await connection.QueryAsync<T>(query);
        }

        public static async Task<IEnumerable<T>> SelectByAsync<T>(IDbConnection connection, Dictionary<string, object> conditions) where T : class
        {
            var tableName = GetTableName<T>();
            var queryBuilder = new StringBuilder($"SELECT * FROM {tableName}");

            if (conditions != null && conditions.Count > 0)
            {
                queryBuilder.Append(" WHERE ");
                queryBuilder.Append(string.Join(" AND ", conditions.Keys.Select(k => $"{k} = @{k}")));
            }

            return await connection.QueryAsync<T>(queryBuilder.ToString(), conditions);
        }

        public static async Task<IEnumerable<T>> SelectByAsync2<T>(
    IDbConnection connection,
    Dictionary<string, object> conditions,
    Dictionary<string, object>? conditions2 = null) where T : class
        {
            var tableName = GetTableName<T>();
            var queryBuilder = new StringBuilder($"SELECT * FROM {tableName}");
            var whereClauses = new List<string>();
            var parameters = new DynamicParameters();

            if (conditions != null)
            {
                foreach (var kvp in conditions)
                {
                    whereClauses.Add($"{kvp.Key} = @{kvp.Key}");
                    parameters.Add(kvp.Key, kvp.Value);
                }
            }
            if (conditions2 != null)
            {
                foreach (var kvp in conditions2)
                {
                    string paramName = $"like_{kvp.Key}";
                    whereClauses.Add($"{kvp.Key} LIKE @{paramName}");
                    parameters.Add(paramName, $"%{kvp.Value}%");
                }
            }

            if (whereClauses.Any())
                queryBuilder.Append(" WHERE " + string.Join(" AND ", whereClauses));

            return await connection.QueryAsync<T>(queryBuilder.ToString(), parameters);
        }

        public static async Task<T> SelectOneByAsync<T>(IDbConnection connection, Dictionary<string, object> conditions = null!) where T : class
        {
            var tableName = GetTableName<T>();
            var queryBuilder = new StringBuilder($"SELECT * FROM {tableName}");

            if (conditions != null && conditions.Count > 0)
            {
                queryBuilder.Append(" WHERE ");
                queryBuilder.Append(string.Join(" AND ", conditions.Keys.Select(k => $"{k} = @{k}")));
            }

            return await connection.QueryFirstOrDefaultAsync<T>(queryBuilder.ToString(), conditions);
        }

        public static async Task<IEnumerable<T>> SelectByLikeAsync<T>(
                            IDbConnection connection,
                            string keyword,
                            params string[] columns
                        ) where T : class
        {
            if (columns == null || columns.Length == 0)
                throw new ArgumentException("At least one column must be specified", nameof(columns));

            var tableName = GetTableName<T>();
            var queryBuilder = new StringBuilder($"SELECT * FROM {tableName} WHERE ");

            var conditions = columns.Select((col, i) => $"{col} LIKE @keyword{i}").ToList();
            queryBuilder.Append(string.Join(" OR ", conditions));

            var parameters = new DynamicParameters();
            for (int i = 0; i < columns.Length; i++)
            {
                parameters.Add($"@keyword{i}", $"%{keyword}%");
            }

            return await connection.QueryAsync<T>(queryBuilder.ToString(), parameters);
        }


        public static string InsertQuery<T>() where T : class
        {
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var columns = new List<string>();
            var parameters = new List<string>();

            foreach (PropertyInfo property in properties)
            {

                if (property.GetMethod?.IsVirtual == true && property.PropertyType != typeof(string))
                    continue;

                var dbGenerate = property.GetCustomAttribute<DatabaseGeneratedAttribute>();
                var isKey = property.GetCustomAttribute<KeyAttribute>() != null;

                //if (!isKey || (dbGenerate != null && dbGenerate.DatabaseGeneratedOption == DatabaseGeneratedOption.None))
                //{
                //    columns.Add(property.Name);
                //    parameters.Add($"@{property.Name}");
                //}

                //if (!(isKey && dbGenerate?.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity))
                //{
                //    columns.Add(property.Name);
                //    parameters.Add($"@{property.Name}");
                //}

                //if (!(isKey && dbGenerate?.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity))
                //{
                //    object? value = property.GetValue(entity);
                //    if (value != null)
                //    {
                //        columns.Add(property.Name);
                //        parameters.Add($"@{property.Name}");
                //        values.Add($"@{property.Name}", value);
                //    }
                //}
            }

            string tableName = type.Name;
            //if (!tableName.EndsWith("s"))
            //    tableName += "s";
            //if (!tableName.EndsWith("y"))
            //    tableName += "s";
            tableName = GetTableName<T>();
            return $"INSERT INTO {tableName} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", parameters)})";
        }

        public static async Task<int> InsertAsync2<T>(IDbConnection connection, T entity) where T : class
        {
            var query = InsertQuery<T>();
            return await connection.ExecuteAsync(query, entity);
        }

        public static async Task<int> InsertAsync2<T>(IDbConnection connection, IEnumerable<T> entities) where T : class
        {
            var query = InsertQuery<T>();
            return await connection.ExecuteAsync(query, entities);
        }

        public static async Task<int> InsertAsync<T>(IDbConnection connection, T entity) where T : class
        {
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var columns = new List<string>();
            var parameters = new List<string>();
            var values = new DynamicParameters();

            foreach (PropertyInfo property in properties)
            {
                if (property.GetMethod?.IsVirtual == true && property.PropertyType != typeof(string))
                    continue;

                var dbGenerate = property.GetCustomAttribute<DatabaseGeneratedAttribute>();
                var isKey = property.GetCustomAttribute<KeyAttribute>() != null;

                if (!isKey || (dbGenerate != null && dbGenerate.DatabaseGeneratedOption == DatabaseGeneratedOption.None))
                {
                    object? value = property.GetValue(entity);
                    if (value != null)
                    {
                        columns.Add(property.Name);
                        parameters.Add($"@{property.Name}");
                        values.Add($"@{property.Name}", value);
                    }
                }

                // ✅ Skip auto-generated key
                //if (!(isKey && dbGenerate?.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity))
                //{
                //    object? value = property.GetValue(entity);
                //    if (value != null)
                //    {
                //        columns.Add(property.Name);
                //        parameters.Add($"@{property.Name}");
                //        values.Add($"@{property.Name}", value);
                //    }
                //}
            }

            string query = $"INSERT INTO {GetTableName<T>()} ({string.Join(" , ", columns)}) VALUES ({string.Join(" , ", parameters)})";

            return await connection.ExecuteAsync(query, values);
        }

        public static async Task<int> UpdateAsync2<T>(
    IDbConnection connection,
    T entity,
    Expression<Func<T, object>>[] keySelectors,
    params Expression<Func<T, object>>[] propertySelectors
) where T : class
        {
            Type type = typeof(T);
            var parameters = new DynamicParameters();
            var setParts = new List<string>();
            var whereParts = new List<string>();

            // Handle keys (composite or single)
            foreach (var keySelector in keySelectors)
            {
                string keyPropName = GetMemberName(keySelector);
                object? keyValue = type.GetProperty(keyPropName)?.GetValue(entity);
                whereParts.Add($"{keyPropName} = @{keyPropName}");
                parameters.Add($"@{keyPropName}", keyValue);
            }

            // Handle updated properties
            foreach (var selector in propertySelectors)
            {
                string propName = GetMemberName(selector);
                var value = type.GetProperty(propName)?.GetValue(entity);
                setParts.Add($"{propName} = @{propName}");
                parameters.Add($"@{propName}", value);
            }

            string query = $"UPDATE {GetTableName<T>()} SET {string.Join(", ", setParts)} WHERE {string.Join(" AND ", whereParts)}";
            return await connection.ExecuteAsync(query, parameters);
        }



        public static async Task<int> UpdateAsync<T>(
            IDbConnection connection,
            T entity,
            Expression<Func<T, object>> keySelector,
            params Expression<Func<T, object>>[] propertySelectors
        ) where T : class
        {
            Type type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var setParts = new List<string>();
            var parameters = new DynamicParameters();

            string keyPropertyName = GetMemberName(keySelector);
            object? keyValue = type.GetProperty(keyPropertyName)?.GetValue(entity);
            parameters.Add($"@{keyPropertyName}", keyValue);

            foreach (var selector in propertySelectors)
            {
                string propName = GetMemberName(selector);
                var value = type.GetProperty(propName)?.GetValue(entity);
                setParts.Add($"{propName} = @{propName}");
                parameters.Add($"@{propName}", value);
            }

            string query = $"UPDATE {GetTableName<T>()} SET {string.Join(", ", setParts)} WHERE {keyPropertyName} = @{keyPropertyName}";
            return await connection.ExecuteAsync(query, parameters);
        }

        public static async Task<int> DeleteAsync<T>(IDbConnection connection, Expression<Func<T, object>> keySelector, object keyValue) where T : class
        {
            string keyProperty = GetMemberName(keySelector);
            string query = $"DELETE FROM {GetTableName<T>()} WHERE {keyProperty} = @{keyProperty}";

            var parameters = new DynamicParameters();
            parameters.Add($"@{keyProperty}", keyValue);

            return await connection.ExecuteAsync(query, parameters);
        }


        // add inside SQLQueryHelper
        public static async Task<int> InsertAndGetIdAsync<T>(IDbConnection connection, T entity) where T : class
        {
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var columns = new List<string>();
            var parameters = new List<string>();
            var values = new DynamicParameters();

            foreach (PropertyInfo property in properties)
            {
                if (property.GetMethod?.IsVirtual == true && property.PropertyType != typeof(string))
                    continue;

                var dbGenerate = property.GetCustomAttribute<DatabaseGeneratedAttribute>();
                var isKey = property.GetCustomAttribute<KeyAttribute>() != null;

                if (!isKey || (dbGenerate != null && dbGenerate.DatabaseGeneratedOption == DatabaseGeneratedOption.None))
                {
                    object? value = property.GetValue(entity);
                    if (value != null)
                    {
                        columns.Add(property.Name);
                        parameters.Add($"@{property.Name}");
                        values.Add($"@{property.Name}", value);
                    }
                }
            }

            string query = $"INSERT INTO {GetTableName<T>()} ({string.Join(" , ", columns)}) VALUES ({string.Join(" , ", parameters)}); " +
                $"SELECT CAST(SCOPE_IDENTITY() AS INT);";

            // ExecuteScalarAsync returns the generated ID
            return await connection.ExecuteScalarAsync<int>(query, values);
        }





        // Helpers
        private static string GetTableName<T>() where T : class
        {
            string name = typeof(T).Name;
            if (name.EndsWith("y", StringComparison.OrdinalIgnoreCase))
            {
                return $"{name.Substring(0, name.Length - 1)}ies";
            }
            else if (!name.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            {
                return $"{name}s";
            }

            return name;
        }

        private static string GetMemberName<T>(Expression<Func<T, object>> expr)
        {
            return expr.Body switch
            {
                MemberExpression m => m.Member.Name,
                UnaryExpression u when u.Operand is MemberExpression m => m.Member.Name,
                _ => throw new InvalidOperationException("Invalid expression")
            };
        }
    }
}
