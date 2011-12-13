namespace MongoMigrations
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using MongoDB.Bson.Serialization;
	using MongoDB.Driver;
	using log4net;

	public class MigrationRunner
	{
		private readonly string _MongoServerLocation;
		public static ILog Log = LogManager.GetLogger("MongoMigrations");

		static MigrationRunner()
		{
			Init();
		}

		public static void Init()
		{
			BsonSerializer.RegisterSerializer(typeof (MigrationVersion), new MigrationVersionSerializer());
		}

		public MigrationRunner(string mongoServerLocation, string databaseName)
		{
			_MongoServerLocation = mongoServerLocation;
			Database = MongoServer.Create(mongoServerLocation).GetDatabase(databaseName);
			DatabaseStatus = new DatabaseMigrationStatus(this);
			MigrationLocator = new MigrationLocator();
		}

		public MongoDatabase Database { get; set; }
		public MigrationLocator MigrationLocator { get; set; }
		public DatabaseMigrationStatus DatabaseStatus { get; set; }

		public virtual void UpdateToLatest()
		{
			Log.Info("Updating " + _MongoServerLocation + " to latest");
			UpdateTo(MigrationLocator.LatestVersion());
		}

		protected virtual void ApplyMigrations(IEnumerable<Migration> migrations)
		{
			migrations.ToList()
				.ForEach(ApplyMigration);
		}

		protected virtual void ApplyMigration(Migration migration)
		{
			Log.Info(new {Message = "Applying migration", migration.Version, migration.Description, DatabaseName = Database.Name});

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
			var message = new {Message = "Migration failed to be applied: ", migration.Version, Name = migration.GetType(), migration.Description, DatabaseName = Database.Name};
			Log.Error(message, exception);
			throw new MigrationException(message.ToString(), exception);
		}

		public virtual void UpdateTo(MigrationVersion updateToVersion)
		{
			var currentVersion = DatabaseStatus.GetLastAppliedMigration();
			Log.Info(new {Message = "Updating", currentVersion, updateToVersion, DatabaseName = Database.Name});

			var migrations = MigrationLocator.GetMigrationsAfter(currentVersion)
				.Where(m => m.Version <= updateToVersion);

			ApplyMigrations(migrations);
		}
	}
}