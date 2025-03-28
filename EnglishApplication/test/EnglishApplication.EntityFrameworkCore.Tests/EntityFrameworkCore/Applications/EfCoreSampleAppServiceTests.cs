using EnglishApplication.Samples;
using Xunit;

namespace EnglishApplication.EntityFrameworkCore.Applications;

[Collection(EnglishApplicationTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<EnglishApplicationEntityFrameworkCoreTestModule>
{

}
