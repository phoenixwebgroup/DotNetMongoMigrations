namespace MongoMigrations
{
	using System;

	public class MigrationException : ApplicationException
	{
		public MigrationException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}