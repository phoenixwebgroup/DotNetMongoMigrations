using System.Diagnostics;

namespace MongoMigrations
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using MongoDB.Bson.Serialization;
	using MongoDB.Driver;

	public class MigrationRunner
	{
		static MigrationRunner()
		{
			Init();
		}

		public static void Init()
		{
			BsonSerializer.RegisterSerializer(typeof(MigrationVersion), new MigrationVersionSerializer());
		}

		public MigrationRunner(string connectionString, string databaseName)
		{
            var client = new MongoClient(connectionString);
			Database = client.GetServer().GetDatabase(databaseName);
			DatabaseStatus = new DatabaseMigrationStatus(this);
			MigrationLocator = new MigrationLocator();
		}

        public MigrationRunner(MongoDatabase database)
        {
            Database = database;
        }

		public MongoDatabase Database { get; set; }
		public MigrationLocator MigrationLocator { get; set; }
		public DatabaseMigrationStatus DatabaseStatus { get; set; }

		public virtual void UpdateToLatest()
		{
            Trace.TraceInformation("Updating {0} to latest...", Database.Name);
			UpdateTo(MigrationLocator.LatestVersion());
		}

		protected virtual void ApplyMigrations(IEnumerable<Migration> migrations)
		{
			migrations.ToList()
				.ForEach(ApplyMigration);
		}

		protected virtual void ApplyMigration(Migration migration)
		{
            Trace.TraceInformation("Applying migration \"{0}\" for version {1} to database \"{2}\".", migration.Description, migration.Version, Database.Name);

			var appliedMigration = DatabaseStatus.StartMigration(migration);
			migration.Database = Database;
			try
			{
				migration.Update();
			}
			catch (Exception exception)
			{
				OnMigrationException(migration, exception);
			}
			DatabaseStatus.CompleteMigration(appliedMigration);
		}

		protected virtual void OnMigrationException(Migration migration, Exception exception)
		{
            string message = String.Format("Failed applying migration \"{0}\" for version {1} to database \"{2}\": {3}", migration.Description, migration.Version, Database.Name, exception.Message);
            Trace.TraceError(message);
            throw new MigrationException(message, exception);
		}

		public virtual void UpdateTo(MigrationVersion updateToVersion)
		{
			var currentVersion = DatabaseStatus.GetLastAppliedMigration();
            Trace.TraceInformation("Updating migration \"{0}\" for version {1} to database \"{2}\".", currentVersion, updateToVersion, Database.Name);

			var migrations = MigrationLocator.GetMigrationsAfter(currentVersion)
				.Where(m => m.Version <= updateToVersion);

			ApplyMigrations(migrations);
		}
	}
}