using System;

namespace MongoMigrations
{
	using MongoDB.Driver;

	public abstract class Migration
	{
		public Version Version { get; protected set; }
		public string Description { get; protected set; }

		protected Migration(string version)
		{
			Version = new Version(version);
		}

		protected Migration(Version version)
		{
			Version = version;
		}

		public MongoDatabase Database { get; set; }

		public abstract void Update();
	}
}