using Xunit;

namespace EnglishApplication.EntityFrameworkCore;

[CollectionDefinition(EnglishApplicationTestConsts.CollectionDefinitionName)]
public class EnglishApplicationEntityFrameworkCoreCollection : ICollectionFixture<EnglishApplicationEntityFrameworkCoreFixture>
{

}
