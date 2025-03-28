using EnglishApplication.Samples;
using Xunit;

namespace EnglishApplication.EntityFrameworkCore.Domains;

[Collection(EnglishApplicationTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<EnglishApplicationEntityFrameworkCoreTestModule>
{

}
