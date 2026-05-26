using JK.Platform.Database.Migrations;
using JK.Platform.LongRunningTasks.Database;

namespace JK.Offer.Database;

[MigrationDependency(typeof(LongRunningTasksDatabaseMarker))]
public class OfferDatabaseMarker : PlatformMigrator
{
}
