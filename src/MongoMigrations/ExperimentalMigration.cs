namespace MongoMigrations
{
	using System;

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class ExperimentalAttribute : Attribute
	{
	}
}