namespace MongoMigrations
{
	using System;
	using System.Linq;
	using MongoDB.Bson;
	using MongoDB.Bson.Serialization;

	public static class BsonDocumentExtensions
	{
		/// <summary>
		/// 	Rename all instances of a name in a bson document to the new name.
		/// </summary>
		public static void ChangeName(this BsonDocument bsonDocument, string originalName, string newName)
		{
			var elements = bsonDocument.Elements
				.Where(e => e.Name == originalName)
				.ToList();
			foreach (var element in elements)
			{
				bsonDocument.RemoveElement(element);
				bsonDocument.Add(new BsonElement(newName, element.Value));
			}
		}

		public static object TryGetDocumentId(this BsonDocument bsonDocument)
		{
			try
			{
				object id;
				Type idNominalType;
				IIdGenerator idGenerator;
				return bsonDocument.GetDocumentId(out id, out idNominalType, out idGenerator);
			}
			catch (Exception)
			{
				return "Cannot find id";
			}
		}
	}
}