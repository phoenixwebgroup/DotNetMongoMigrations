namespace MongoMigrations
{
	using System;
	using System.Collections.Generic;
	using MongoDB.Bson;
	using MongoDB.Driver;

	public abstract class CollectionMigration : Migration
	{
		protected string CollectionName;

		public CollectionMigration(MigrationVersion version, string collectionName) : base(version)
		{
			CollectionName = collectionName;
		}

		public virtual IMongoQuery Filter()
		{
			return null;
		}

		public override void Update()
		{
			var collection = GetCollection();
			var documents = GetDocuments(collection);
			UpdateDocuments(collection, documents);
		}

		public virtual void UpdateDocuments(MongoCollection<BsonDocument> collection, IEnumerable<BsonDocument> documents)
		{
			foreach (var document in documents)
			{
				try
				{
					UpdateDocument(collection, document);
				}
				catch (Exception exception)
				{
					OnErrorUpdatingDocument(document, exception);
				}
			}
		}

		protected virtual void OnErrorUpdatingDocument(BsonDocument document, Exception exception)
		{
			var message =
				new
					{
						Message = "Failed to update document",
						CollectionName,
						Id = document.TryGetDocumentId(),
						MigrationVersion = Version,
						MigrationDescription = Description
					};
			throw new MigrationException(message.ToString(), exception);
		}

		public abstract void UpdateDocument(MongoCollection<BsonDocument> collection, BsonDocument document);

		protected virtual MongoCollection<BsonDocument> GetCollection()
		{
			return Database.GetCollection(CollectionName);
		}

		protected virtual IEnumerable<BsonDocument> GetDocuments(MongoCollection<BsonDocument> collection)
		{
			var query = Filter();
			return query != null
			       	? collection.Find(query)
			       	: collection.FindAll();
		}
	}
}