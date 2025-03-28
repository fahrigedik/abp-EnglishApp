using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace EnglishApplication.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class EnglishApplicationDbContextFactory : IDesignTimeDbContextFactory<EnglishApplicationDbContext>
{
    public EnglishApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        EnglishApplicationEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<EnglishApplicationDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new EnglishApplicationDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../EnglishApplication.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
