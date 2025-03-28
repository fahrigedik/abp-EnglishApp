using System.Threading.Tasks;

namespace EnglishApplication.Data;

public interface IEnglishApplicationDbSchemaMigrator
{
    Task MigrateAsync();
}
