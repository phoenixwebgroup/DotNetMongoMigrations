Why mongo migrations?
--
We no longer need create schema migrations, as this is a schemaless database, when we add collections (tables) or properties on our entities (columns), we don't need to run creation scripts.

However, we need migrations when:

1. Rename collections
1. Rename keys
1. Manipulate data types
 * Moving data between types
 * Setting default values for new properties
1. Index creation
1. Removing collections / data

So the idea is to have a simple migration script that executes commands against Mongo.  It should track the applied migrations and quickly be able to apply new migrations.

Why DotNetMongoMigrations?
--

1. This project is meant to allow for migrations to be created in C# or other .Net languages and kept with the code base.  
1. Access .Net APIs for manipulating documents
1. Leverage existing NUnit test projects or other test projects to test migrations
1. Provide an automatable foundation to track and apply migrations.

Migration recommendations
--

1. Keep them as simple as possible
1. Do not couple migrations to your domain types, they will be brittle to change, and the point of a migration is to update the data representation when your model changes.
1. Stick to the mongo BsonDocument interface or use javascript based mongo commands for migrations, much like with SQL, the mongo javascript API is less likely to change which might break migrations
1. Add an application startup check that the database is at the correct version
1. Write tests of your migrations, TDD them from existing data scenarios to new forms
1. Automate the deployment of migrations with application deployment (backup the database and then apply migrations, on a failure restore the database)

Migration 
--

This is a simple migration that executes a mongo javascript command to rename the Customer key from Name to FullName.

```csharp
	public class Migration1 : Migration
	{
		public Migration1 : base("1.0.0")
		{
		}

		public override void Update()
		{
			Database.Eval(new BsonJavaScript("db.Customers.update({}, { $rename : { 'Name' : 'FullName' } });"));
		}
	}
```

Collection Migration
--

These are migrations performed on every document in a given collection.  Supply the version number and collection name, then simply implement the update per document method UpdateDocument to manipulate each document.

```csharp
	public class Migration1 : CollectionMigration
	{
		public Migration1()
			: base("1.0.1", "Customer")
		{
			Description = "Drop social security information from customers";
		}

		protected override void UpdateDocument(MongoCollection<BsonDocument> collection, BsonDocument document)
		{
			document.Remove("SocialSecurityNumber");
			collection.Save(document);
		}
	}
```

Experimental Migrations
-- 

Sometimes we want to work on a migration but it's not complete yet, these can be attributed with the ExperimentalAttribute and the base migrations runner will exclude them.

```csharp
	[Experimental]
	public class Migration1 : Migration
	{
```

Running Migrations
--

The project provides a MigrationRunner which contains:

* DatabaseMigrationStatus
 * Contains methods to monitor the version of the database.
 * Applied migrations are stored in a collection named "DatabaseVersion", this can be configured via the VersionCollectionName instance property.  
* MigrationLocator
 * Scans the provided assemblies for migrations
 * Filters on experimental by default.  
 * Filters can be added/removed via the MigrationLocator.MigrationFilters list.

Sample App Startup Version Check
--

Simply plug the following code into your application startup, whether that be a web or service app, to terminate the application if the database is not at the expected version.  This assumes you have mongo server location and database name in a settings file and that Migration1 is in the assembly containing migrations to be scanned for.

```csharp
	public class CheckDbVersionOnStartup : IRunOnApplicationStart
	{
		public void Start()
		{
			var runner = new MigrationRunner(Settings.Default.MongoServerLocation, Settings.Default.MongoDatabaseName);
			runner.MigrationLocator.LookForMigrationsInAssemblyOfType<Migration1>();
			runner.DatabaseStatus.ThrowIfNotLatestVersion();
		}
	}
```

Using migrations with a deployment process
--

Testing migrations
--

