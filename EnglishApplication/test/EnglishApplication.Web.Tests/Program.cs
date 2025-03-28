using Microsoft.AspNetCore.Builder;
using EnglishApplication;
using Volo.Abp.AspNetCore.TestBase;

var builder = WebApplication.CreateBuilder();
builder.Environment.ContentRootPath = GetWebProjectContentRootPathHelper.Get("EnglishApplication.Web.csproj"); 
await builder.RunAbpModuleAsync<EnglishApplicationWebTestModule>(applicationName: "EnglishApplication.Web");

public partial class Program
{
}
