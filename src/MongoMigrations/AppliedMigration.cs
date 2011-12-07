namespace MongoMigrations
{
	using System;
	using MongoDB.Bson.Serialization.Attributes;

	public class AppliedMigration
	{
		public const string ManuallyMarked = "Manually marked";

		public AppliedMigration()
		{
		}

		public AppliedMigration(Migration migration)
		{
			Version = migration.Version;
			StartedOn = DateTime.Now;
			Description = migration.Description;
		}

		[BsonId]
		public MigrationVersion Version { get; set; }
		public string Description { get; set; }
		public DateTime StartedOn { get; set; }
		public DateTime? CompletedOn { get; set; }

		public override string ToString()
		{
			return Version.ToString() + " started on " + StartedOn + " completed on " + CompletedOn;
		}

		public static AppliedMigration MarkerOnly(MigrationVersion version)
		{
			return new AppliedMigration
			       	{
			       		Version = version,
			       		Description = ManuallyMarked,
			       		StartedOn = DateTime.Now,
			       		CompletedOn = DateTime.Now
			       	};
		}
	}
}