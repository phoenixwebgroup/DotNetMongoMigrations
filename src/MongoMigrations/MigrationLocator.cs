namespace MongoMigrations
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	public class MigrationLocator
	{
		protected readonly List<Assembly> Assemblies = new List<Assembly>();
		public List<MigrationFilter> MigrationFilters = new List<MigrationFilter>();

		public MigrationLocator()
		{
			MigrationFilters.Add(new ExcludeExperimentalMigrations());
		}

	    public IEnumerable<Migration> AllMigrations { get; private set; }

		public virtual void LookForMigrationsInAssemblyOfType<T>()
		{
			var assembly = typeof (T).Assembly;
			LookForMigrationsInAssembly(assembly);
		}

		public void LookForMigrationsInAssembly(Assembly assembly)
		{
			if (Assemblies.Contains(assembly))
			{
				return;
			}

			Assemblies.Add(assembly);
		}

		public virtual IEnumerable<Migration> GetAllMigrations()
		{
		    if (this.AllMigrations != null && this.AllMigrations.Any())
		    {
		        return this.AllMigrations;
		    }

		    this.AllMigrations = this.Assemblies
		        .SelectMany(this.GetMigrationsFromAssembly)
		        .OrderBy(m => m.Version)
                .ToList();

		    return this.AllMigrations;
		}

	    protected virtual IEnumerable<Migration> GetMigrationsFromAssembly(Assembly assembly)
		{
            Console.WriteLine($"Getting migrations from assembly: {assembly.FullName}");

			try
			{
				return assembly.GetTypes()
					.Where(t => typeof (Migration).IsAssignableFrom(t) && !t.IsAbstract)
					.Select(Activator.CreateInstance)
					.OfType<Migration>()
					.Where(m => !MigrationFilters.Any(f => f.Exclude(m)));
			}
			catch (Exception exception)
			{
				throw new MigrationException("Cannot load migrations from assembly: " + assembly.FullName, exception);
			}
		}

		public virtual MigrationVersion LatestVersion()
		{
		    var allMigrations = this.GetAllMigrations().ToList();

			if (!allMigrations.Any())
			{
				return MigrationVersion.Default();
			}

			return allMigrations
                .Max(m => m.Version);
		}

		public virtual IEnumerable<Migration> GetMigrationsAfter(AppliedMigration currentVersion)
		{
			var migrations = this.GetAllMigrations();

			if (currentVersion != null)
			{
				migrations = migrations.Where(m => m.Version > currentVersion.Version);
			}

			return migrations.OrderBy(m => m.Version);
		}
	}
}