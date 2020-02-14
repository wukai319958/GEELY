using System.Data.Entity.Migrations;

namespace DataAccess.Migrations
{
    /// <summary>
    ///  ˝æ›«®“∆≈‰÷√°£
    /// </summary>
    public class Configuration : DbMigrationsConfiguration<GeelyPtlEntities>
    {
        public Configuration()
        {
            this.AutomaticMigrationsEnabled = true;
            this.AutomaticMigrationDataLossAllowed = true;
            this.ContextKey = "DataAccess.GeelyPtlEntities";
        }
    }
}