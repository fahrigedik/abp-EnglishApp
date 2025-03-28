using EnglishApplication.Books;
using Xunit;

namespace EnglishApplication.EntityFrameworkCore.Applications.Books;

[Collection(EnglishApplicationTestConsts.CollectionDefinitionName)]
public class EfCoreBookAppService_Tests : BookAppService_Tests<EnglishApplicationEntityFrameworkCoreTestModule>
{

}