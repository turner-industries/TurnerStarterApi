using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace TurnerStarterApi.Tests.Integration.Helpers
{
    public class DatabaseDeleter
    {
        private readonly string _connectionString;
        private readonly string[] _ignoredTables = { "__EFMigrationsHistory" };
        private static readonly object Locker = new object();
        private static bool _initialized;
        private static string _deleteSql;

        public DatabaseDeleter(string connectionString)
        {
            _connectionString = connectionString;
            BuildDeleteTables();
        }

        public void DeleteAllData()
        {
            if (string.IsNullOrEmpty(_deleteSql))
            {
                return;
            }

            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();
                sqlConnection.Execute(_deleteSql);
            }
        }

        private void BuildDeleteTables()
        {
            if (_initialized)
            {
                return;
            }

            lock (Locker)
            {
                if (_initialized)
                {
                    return;
                }

                using (var sqlConnection = new SqlConnection(_connectionString))
                {
                    sqlConnection.Open();
                    var allTables = GetAllTables(sqlConnection);
                    var allRelationships = GetRelationships(sqlConnection);
                    var tablesToDelete = BuildTableList(allTables, allRelationships);
                    _deleteSql = BuildTableSql(tablesToDelete);

                    _initialized = true;
                }
            }
        }

        private static string BuildTableSql(IEnumerable<string> tablesToDelete)
        {
            return tablesToDelete.Aggregate(string.Empty, (current, tableName) => current + $"delete from [{tableName}];");
        }

        private static string[] BuildTableList(ICollection<string> allTables, ICollection<Relationship> allRelationships)
        {
            var tablesToDelete = new List<string>();

            while (allTables.Any())
            {
                var leafTables = allTables.Except(allRelationships.Select(rel => rel.PrimaryKeyTable)).ToArray();

                tablesToDelete.AddRange(leafTables);

                foreach (var leafTable in leafTables)
                {
                    allTables.Remove(leafTable);
                    var relToRemove = allRelationships.Where(rel => rel.ForeignKeyTable == leafTable).ToArray();
                    foreach (var rel in relToRemove)
                    {
                        allRelationships.Remove(rel);
                    }
                }
            }

            return tablesToDelete.ToArray();
        }

        private IList<Relationship> GetRelationships(SqlConnection sqlConnection)
        {
            return sqlConnection.Query<Relationship>(@"
                select so_pk.name as PrimaryKeyTable, so_fk.name as ForeignKeyTable
		        from sysforeignkeys sfk
		        inner join sysobjects so_pk on sfk.rkeyid = so_pk.id
		        inner join sysobjects so_fk on sfk.fkeyid = so_fk.id
                where so_pk.name <> so_fk.name
		        order by so_pk.name, so_fk.name"
            ).ToList();
        }

        private IList<string> GetAllTables(SqlConnection sqlConnection)
        {
            return sqlConnection.Query<string>("select t.[name] from sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.[name] = 'dbo'")
                .Except(_ignoredTables)
                .ToList();
        }

        private class Relationship
        {
            public string PrimaryKeyTable { get; private set; }
            public string ForeignKeyTable { get; private set; }
        }
    }
}
