param($server, $database, $backupLocation, $migrationsAssemblyPath)

# Backup location is base\server\timestamp
$backupLocation = join-path (join-path $backupLocation $server)  $(get-date -f yyyy_MM_dd_HH_mm_ss)

# Backup current database
mongodump -h $server -d $database -o $backupLocation

# Load assembly with migrations and MongoMigrations framework assembly
$migrationsAssembly = [System.Reflection.Assembly]::LoadFrom($migrationsAssemblyPath)
$migrationFrameworkAssemblyPath = join-path ([IO.Path]::GetDirectoryName($migrationsAssemblyPath)) 'MongoMigrations.dll'
[System.Reflection.Assembly]::LoadFrom($migrationFrameworkAssemblyPath)

# Create migration runner and load migrations
$runner = new-object MongoMigrations.MigrationRunner(('mongodb://' + $server), $database)
$runner.MigrationLocator.LookForMigrationsInAssembly($migrationsAssembly)

Try
{ 
    $runner.UpdateToLatest()
}
Catch [MongoMigrations.MigrationException]{
    # Attempt restore on failure
    echo "Migrations failed: "
    Write-Host $_.Exception.ToString()
    echo "Attempting restore from " + $backupLocation
    $restoreLocation = join-path $backupLocation $database
    mongorestore -h $server -d $database -drop $restoreLocation
    throw
}
