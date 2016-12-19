namespace MongoMigrations
{
    using System;
    using MongoDB.Bson;
    using MongoDB.Driver;

    public abstract class CollectionMigration : Migration
    {
        protected string CollectionName;
        protected FilterDefinition<BsonDocument> Filter;

        public IMongoCollection<BsonDocument> Collection { get; protected set; }

        public CollectionMigration(MigrationVersion version, string collectionName, FilterDefinition<BsonDocument> filter = null) : base(version)
        {
            CollectionName = collectionName;          
            this.Filter = filter ?? Builders<BsonDocument>.Filter.Empty;
        }

        public override void Update()
        {
            this.Collection = Database.GetCollection<BsonDocument>(CollectionName);
            using (var cursor = this.Collection.Find(this.Filter).ToCursor())
            {
                while(cursor.MoveNext())
                {
                    foreach (var currentDocument in cursor.Current)
                    {
                        try
                        {
                            UpdateDocument(currentDocument);                          
                        }
                        catch (Exception exception)
                        {
                            OnErrorUpdatingDocument(currentDocument, exception);
                        }
                    }
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

        public abstract void UpdateDocument(BsonDocument document);
    }
}