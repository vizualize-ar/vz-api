using Dapper;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VZ.Shared.Models;

namespace VZ.Shared.Data
{
    public class SqlBaseRepository<TEntity> : ISqlBaseRepository<TEntity> where TEntity : SqlBaseModel
    {
        protected readonly string _connectionString = null;
        protected readonly string _table = null;

        static SqlBaseRepository()
        {
            // Map snake case Postgres columns to our camel case properties
            DefaultTypeMap.MatchNamesWithUnderscores = true;

            // Map any class properties of type List<string> to json data type in Postgres
            SqlMapper.AddTypeHandler(typeof(List<string>), new JsonObjectTypeHandler());
        }

        public SqlBaseRepository(string table)
        {
            this._connectionString = Config.DB.PostgresConnection;
            this._table = table;

            SqlBuilder.RegisterType(typeof(TEntity));
        }

        public async Task<long> AddAsync(TEntity entity)
        {
            using (var connection = new NpgsqlConnection(this._connectionString))
            {
                connection.Open();
                string sql = $"INSERT INTO {this._table} {SqlBuilder.InsertColumnsSql(typeof(TEntity))} RETURNING id";
                var id = await connection.ExecuteScalarAsync<long>(sql, entity);
                entity.id = id;
                return id;
            }
        }

        public async Task<DeleteDocumentResult> DeleteAsync(long id)
        {
            using (var connection = new NpgsqlConnection(this._connectionString))
            {
                connection.Open();
                await connection.ExecuteAsync($"DELETE FROM {this._table} WHERE id=@id", new { id });
                return DeleteDocumentResult.Success;
            }
        }

        public async Task<TEntity> GetAsync(long id)
        {
            using (var connection = new NpgsqlConnection(this._connectionString))
            {
                connection.Open();
                string query = $"SELECT * FROM {this._table} c WHERE id = @id";
                return (await connection.QueryAsync<TEntity>(query, new { id })).FirstOrDefault();
            }
        }

        public List<TAnything> GetAll<TAnything>(string fields = "c.*")
        {
            using (var connection = new NpgsqlConnection(this._connectionString))
            {
                connection.Open();
                string query = $"SELECT {fields} FROM {this._table} c";
                return connection.Query<TAnything>(query).AsList();
            }
        }

        public async Task<List<TAnything>> GetAllAsync<TAnything>(string fields = "c.*")
        {
            using (var connection = new NpgsqlConnection(this._connectionString))
            {
                connection.Open();
                string query = $"SELECT {fields} FROM {this._table} c";
                return (await connection.QueryAsync<TAnything>(query)).AsList();
            }
        }

        public async Task<TAnything> GetFirstAsync<TAnything>(string fields = "*", string predicate = null, (string, object)[] parameters = null)
        {
            var results = await GetSomeAsync<TAnything>(fields, predicate, parameters, 1);
            return results.FirstOrDefault();
        }

        public async Task<List<TAnything>> GetSomeAsync<TAnything>(string fields = "*", string predicate = null, (string, object)[] parameters = null, int? take = null, int? skip = null)
        {
            using (var connection = new NpgsqlConnection(this._connectionString))
            {
                connection.Open();
                string query = $"SELECT {fields.Snakify()} FROM {this._table} c";
                DynamicParameters sqlParams = null;
                if (predicate != null)
                {
                    query += " WHERE " + predicate.SnakifyPredicate();
                    sqlParams = new DynamicParameters();
                    foreach (var param in parameters)
                    {
                        sqlParams.Add(param.Item1, param.Item2);
                    }
                }
                if (take.HasValue && take.Value > 0)
                {
                    query += $" LIMIT {take.Value}";
                }
                if (skip.HasValue && skip.Value > 0)
                {
                    query += $" OFFSET {skip.Value}";
                }
                return (await connection.QueryAsync<TAnything>(query, sqlParams)).AsList();
            }
        }

        public async Task<List<TAnything>> GetSomeRawAsync<TAnything>(string sql = "select * from c", params ValueTuple<string, object>[] parameters)
        {
            using (var connection = new NpgsqlConnection(this._connectionString))
            {
                connection.Open();
                DynamicParameters sqlParams = null;
                if (parameters != null)
                {
                    sqlParams = new DynamicParameters();
                    foreach (var param in parameters)
                    {
                        sqlParams.Add(param.Item1, param.Item2);
                    }
                }
                return (await connection.QueryAsync<TAnything>(sql, sqlParams)).AsList();
            }
        }

        public virtual async Task<UpdateDocumentResult> UpdateAsync(TEntity entity)
        {
            if (entity.updatedOn == null)
            {
                entity.updatedOn = DateTime.UtcNow;
            }
            using (var connection = new NpgsqlConnection(this._connectionString))
            {
                connection.Open();
                string sql = $"UPDATE {this._table} SET {SqlBuilder.UpdateSetSql(typeof(TEntity))} WHERE id = @id";
                await connection.QueryAsync(sql, entity);
            }

            return UpdateDocumentResult.Success;
        }
    }

    // see https://radekmaziarka.pl/2018/01/22/dapper-json-type-custom-mapper/
    public class JsonObjectTypeHandler : SqlMapper.ITypeHandler
    {
        public void SetValue(IDbDataParameter parameter, object value)
        {
            parameter.Value = (value == null)
                ? (object)DBNull.Value
                : JsonConvert.SerializeObject(value, Formatting.None);
            parameter.DbType = DbType.String;
        }

        public object Parse(Type destinationType, object value)
        {
            if (value is string)
            {
                var json = (string)value;
                if (json == null) return null;
                return JsonConvert.DeserializeObject(json, destinationType);
            }
            else
            {
                return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(value), destinationType);
            }
        }
    }

    public static class SqlBuilder
    {
        private static ConcurrentDictionary<Type, List<ColumnMap>> _maps = new ConcurrentDictionary<Type, List<ColumnMap>>();
        public static void RegisterType(Type type)
        {
            if (_maps.ContainsKey(type) == false)
            {
                var columnMaps = new List<ColumnMap>();
                foreach (var info in type.GetProperties())
                {
                    columnMaps.Add(new ColumnMap(info));
                }
                _maps.GetOrAdd(type, columnMaps);
            }
        }

        public static List<ColumnMap> Get(Type type) => _maps[type];

        /// <summary>
        /// Returns sql for setting all fields, for update statement
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string UpdateSetSql(Type type)
        {
            string sql = "";
            foreach (var columnMap in Get(type))
            {
                sql += $"{columnMap.ToSetSql()}, ";
            }
            sql = sql.TrimEnd(new char[] { ' ', ',' });

            return sql;
        }

        public static string InsertColumnsSql(Type type)
        {
            string sql = "(";
            foreach (var columnMap in Get(type))
            {
                if (columnMap.name == "id") continue;
                sql += $"{columnMap.snakeName}, ";
            }
            sql = sql.TrimEnd(new char[] { ' ', ',' });

            sql += ") VALUES(";
            foreach (var columnMap in Get(type))
            {
                if (columnMap.name == "id") continue;
                sql += $"{columnMap.ToParameter()}, ";
            }
            sql = sql.TrimEnd(new char[] { ' ', ',' });

            return sql + ")";
        }

        /// <summary>
        /// Convert fields in a sql predicate string to snake case. Assumptions made are that everything is in pascal case.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string SnakifyPredicate(this string input)
        {
            string[] parts = input.Split(" ");
            for (var i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                if (part.StartsWith("@")) continue;

                parts[i] = part.Snakify();
            }
            return string.Join(" ", parts);
        }
    }

    /// <summary>
    /// Generates sql that maps a class property to a snake case sql column for updates and inserts. Treats any non-primitive type as a JSON type
    /// </summary>
    public class ColumnMap
    {
        public string name, snakeName;
        bool isJson;

        public ColumnMap(System.Reflection.PropertyInfo propertyInfo)
        {
            this.name = propertyInfo.Name;
            this.snakeName = this.name.ToSnakeCase();
            var t = propertyInfo.PropertyType;
            this.isJson = !(
                t.IsPrimitive || 
                t == typeof(Decimal) || t == typeof(String) || t == typeof(DateTime) || t == typeof(DateTime?) ||
                t.IsEnum || t == typeof(Guid)
            );
            if (this.isJson
                && t.IsGenericType 
                && t.GetGenericTypeDefinition() == typeof(Nullable<>)
                && t.GetGenericArguments().Any(x =>
                    x.IsPrimitive ||
                    x == typeof(Decimal) || x == typeof(String) || x == typeof(DateTime) || x == typeof(DateTime?) ||
                    x.IsEnum || t == typeof(Guid)))
            {
                // it's a nullable primitive
                this.isJson = false;
            }
        }

        public string ToSetSql()
        {
            if (isJson)
            {
                return $"{this.snakeName} = CAST(@{this.name} AS json)";
            }
            return $"{this.snakeName} = @{this.name}";
        }

        public string ToParameter()
        {
            if (isJson)
            {
                return $"CAST(@{this.name} AS json)";
            }
            return $"@{this.name}";
        }
    }

    public static class StringExtensions
    {
        static Regex snakeRegex = new Regex(@"(\B(?<=[0-9a-z])[A-Z])", RegexOptions.Compiled);

        // Convert the string to Pascal case.
        public static string ToCamelCase(this string input)
        {
            System.Globalization.TextInfo info = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo;
            //the_string = info.ToTitleCase(the_string);
            //string[] parts = the_string.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            string[] parts = input.Split('_');
            parts[0] = parts[0].Substring(0, 1).ToLower() + parts[0].Substring(1);
            for (int i = 1; i < parts.Length; i++)
            {
                parts[i] = info.ToTitleCase(parts[i]);
            }
            string result = String.Join(String.Empty, parts);
            return result;
        }

        public static string ToSnakeCase(this string input)
        {
            return string.Concat(input.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
        }

        public static string Snakify(this string value)
        {
            return snakeRegex.Replace(value, (m) => "_" + m.Value.ToLower()).ToLower();
        }
    }
}
