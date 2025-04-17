using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace EnglishApplication;

public class EnglishApplicationDataSeederContributor
    : IDataSeedContributor, ITransientDependency
{



    public async Task SeedAsync(DataSeedContext context)
    {
       
    }
}