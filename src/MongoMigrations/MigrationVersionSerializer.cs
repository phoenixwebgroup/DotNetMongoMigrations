namespace MongoMigrations
{
	using MongoDB.Bson.Serialization;
	using MongoDB.Bson.Serialization.Serializers;

    public class MigrationVersionSerializer : SerializerBase<MigrationVersion>
	{
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, MigrationVersion value)
        {
            var versionString = string.Format("{0}.{1}.{2}", value.Major, value.Minor, value.Revision);
            context.Writer.WriteString(versionString);
		}

        public override MigrationVersion Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
			var versionString = context.Reader.ReadString();
			return new MigrationVersion(versionString);
		}
	}
}