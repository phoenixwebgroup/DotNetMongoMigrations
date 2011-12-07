namespace MongoMigrations
{
	using System;
	using System.Linq;
	using MongoDB.Driver;

	public class DatabaseMigrationStatus
	{
		private readonly MigrationRunner _Runner;

		public string VersionCollectionName = "DatabaseVersion";

		public DatabaseMigrationStatus(MigrationRunner runner)
		{
			_Runner = runner;
		}

		public virtual MongoCollection<AppliedMigration> GetMigrationsApplied()
		{
			return _Runner.Database.GetCollection<AppliedMigration>(VersionCollectionName);
		}

		public virtual bool IsNotLatestVersion()
		{
			return _Runner.MigrationLocator.LatestVersion()
			       > GetVersion();
		}

		public virtual MigrationVersion GetVersion()
		{
			var lastAppliedMigration = GetLastAppliedMigration();
			return lastAppliedMigration == null
			       	? MigrationVersion.Default()
			       	: lastAppliedMigration.Version;
		}

		public virtual AppliedMigration GetLastAppliedMigration()
		{
			return GetMigrationsApplied()
				.FindAll()
				.ToList() // in memory but this will never get big enough to matter
				.OrderByDescending(v => v.Version)
				.FirstOrDefault();
		}

		public virtual AppliedMigration StartMigration(Migration migration)
		{
			var appliedMigration = new AppliedMigration(migration);
			GetMigrationsApplied().Insert(appliedMigration);
			return appliedMigration;
		}

		public virtual void CompleteMigration(AppliedMigration appliedMigration)
		{
			appliedMigration.CompletedOn = DateTime.Now;
			GetMigrationsApplied().Save(appliedMigration);
		}

		public virtual void MarkUpToVersion(MigrationVersion version)
		{
			_Runner.MigrationLocator.GetAllMigrations()
				.Where(m => m.Version <= version)
				.ToList()
				.ForEach(m => MarkVersion(m.Version));
		}

		public virtual void MarkVersion(MigrationVersion version)
		{
			var appliedMigration = AppliedMigration.MarkerOnly(version);
			GetMigrationsApplied().Insert(appliedMigration);
		}
	}
}