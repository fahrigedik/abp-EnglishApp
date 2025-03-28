using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace EnglishApplication.Pages;

[Collection(EnglishApplicationTestConsts.CollectionDefinitionName)]
public class Index_Tests : EnglishApplicationWebTestBase
{
    [Fact]
    public async Task Welcome_Page()
    {
        var response = await GetResponseAsStringAsync("/");
        response.ShouldNotBeNull();
    }
}
