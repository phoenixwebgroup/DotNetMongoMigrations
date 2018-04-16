namespace MongoMigrations
{
    using System;
    using System.Collections.Generic;
    using MongoDB.Bson;
    using MongoDB.Driver;

    public abstract class CollectionMigration : Migration
    {
        protected string CollectionName;

        public CollectionMigration(MigrationVersion version, string collectionName)
            : base(version)
        {
            CollectionName = collectionName;
        }

        public override void Update()
        {
            var collection = GetCollection();
            var documents = GetDocuments(collection);
            UpdateDocuments(collection, documents);
        }

        public override void Rollback()
        {
            var collection = GetCollection();
            var documents = GetDocuments(collection);
            RollbackDocuments(collection, documents);
        }

        public virtual void UpdateDocuments(IMongoCollection<BsonDocument> collection, IEnumerable<BsonDocument> documents)
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

        public virtual void RollbackDocuments(IMongoCollection<BsonDocument> collection, IEnumerable<BsonDocument> documents)
        {
            foreach (var document in documents)
            {
                try
                {
                    RollbackDocument(collection, document);
                }
                catch (Exception exception)
                {
                    OnErrorRollingBackDocument(document, exception);
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

        protected virtual void OnErrorRollingBackDocument(BsonDocument document, Exception exception)
        {
            var message =
                new
                {
                    Message = "Failed to rollback document",
                    CollectionName,
                    Id = document.TryGetDocumentId(),
                    MigrationVersion = Version,
                    MigrationDescription = Description
                };
            throw new MigrationException(message.ToString(), exception);
        }

        public abstract void UpdateDocument(IMongoCollection<BsonDocument> collection, BsonDocument document);
        public abstract void RollbackDocument(IMongoCollection<BsonDocument> collection, BsonDocument document);

        protected virtual IMongoCollection<BsonDocument> GetCollection()
        {
            return Database.GetCollection<BsonDocument>(CollectionName);
        }

        protected virtual IEnumerable<BsonDocument> GetDocuments(IMongoCollection<BsonDocument> collection)
        {
            return collection.Find(Builders<BsonDocument>.Filter.Empty).ToList();
        }
    }
}