using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EnglishApp.Data;

public class EnglishAppDbContextFactory : IDesignTimeDbContextFactory<EnglishAppDbContext>
{
    public EnglishAppDbContext CreateDbContext(string[] args)
    {
        EnglishAppEfCoreEntityExtensionMappings.Configure();
        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<EnglishAppDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));

        return new EnglishAppDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}