namespace JK.Platform.Database.Migrations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
public class MigrationDependencyAttribute : Attribute
{
    public Type MarkerType { get; }

    public MigrationDependencyAttribute(Type markerType)
    {
        MarkerType = markerType;
    }
}

public abstract class PlatformMigrator
{
}

public abstract class PlatformMigrator<T> : PlatformMigrator, IMigrateWith<T>
{
}

public interface IMigrateWith<T>
{
}
