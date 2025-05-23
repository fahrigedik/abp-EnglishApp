using EnglishApplication.EntityFrameworkCore;
using EnglishApplication.Localization;
using EnglishApplication.MultiTenancy;
using EnglishApplication.Permissions;
using EnglishApplication.Web.HealthChecks;
using EnglishApplication.Web.Menus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Volo.Abp;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared.Toolbars;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity.Web;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Security.Claims;
using Volo.Abp.Studio.Client.AspNetCore;
using Volo.Abp.Swashbuckle;
using Volo.Abp.UI.Navigation;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.Users;
using Volo.Abp.VirtualFileSystem;
using EnglishApplication.Web.Middlewares;

namespace EnglishApplication.Web;

[DependsOn(
    typeof(EnglishApplicationHttpApiModule),
    typeof(EnglishApplicationApplicationModule),
    typeof(EnglishApplicationEntityFrameworkCoreModule),
    typeof(AbpAutofacModule),
    typeof(AbpStudioClientAspNetCoreModule),
    typeof(AbpIdentityWebModule),
    typeof(AbpAspNetCoreMvcUiLeptonXLiteThemeModule),
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpFeatureManagementWebModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpAspNetCoreSerilogModule)
)]
public class EnglishApplicationWebModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        context.Services.PreConfigure<AbpMvcDataAnnotationsLocalizationOptions>(options =>
        {
            options.AddAssemblyResource(
                typeof(EnglishApplicationResource),
                typeof(EnglishApplicationDomainModule).Assembly,
                typeof(EnglishApplicationDomainSharedModule).Assembly,
                typeof(EnglishApplicationApplicationModule).Assembly,
                typeof(EnglishApplicationApplicationContractsModule).Assembly,
                typeof(EnglishApplicationWebModule).Assembly
            );
        });

        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                options.AddAudiences("EnglishApplication");
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        });

        if (!hostingEnvironment.IsDevelopment())
        {
            PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
            {
                options.AddDevelopmentEncryptionAndSigningCertificate = false;
            });

            PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
            {
                serverBuilder.AddProductionEncryptionAndSigningCertificate("openiddict.pfx", configuration["AuthServer:CertificatePassPhrase"]!);
                serverBuilder.SetIssuer(new Uri(configuration["AuthServer:Authority"]!));
            });
        }
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        if (!configuration.GetValue<bool>("App:DisablePII"))
        {
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.LogCompleteSecurityArtifact = true;
        }

        if (!configuration.GetValue<bool>("AuthServer:RequireHttpsMetadata"))
        {
            Configure<OpenIddictServerAspNetCoreOptions>(options =>
            {
                options.DisableTransportSecurityRequirement = true;
            });
            
            Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
            });
        }
        // Session yap�land�rmas� ekleyin
        context.Services.AddDistributedMemoryCache();
        context.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        ConfigureBundles();
        ConfigureUrls(configuration);
        ConfigureHealthChecks(context);
        ConfigureAuthentication(context);
        ConfigureAutoMapper();
        ConfigureVirtualFileSystem(hostingEnvironment);
        ConfigureNavigationServices();
        ConfigureAutoApiControllers();
        ConfigureSwaggerServices(context.Services);

        Configure<PermissionManagementOptions>(options =>
        {
            options.IsDynamicPermissionStoreEnabled = true;
        });
    }


    private void ConfigureHealthChecks(ServiceConfigurationContext context)
    {
        context.Services.AddEnglishApplicationHealthChecks();
    }

    private void ConfigureBundles()
    {
        Configure<AbpBundlingOptions>(options =>
        {
            options.StyleBundles.Configure(
                LeptonXLiteThemeBundles.Styles.Global,
                bundle =>
                {
                    bundle.AddFiles("/global-scripts.js");
                    bundle.AddFiles("/global-styles.css");
                }
            );
        });
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
        });
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        context.Services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
        });
    }

    private void ConfigureAutoMapper()
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<EnglishApplicationWebModule>();
        });
    }

    private void ConfigureVirtualFileSystem(IWebHostEnvironment hostingEnvironment)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<EnglishApplicationWebModule>();

            if (hostingEnvironment.IsDevelopment())
            {
                options.FileSets.ReplaceEmbeddedByPhysical<EnglishApplicationDomainSharedModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}EnglishApplication.Domain.Shared", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<EnglishApplicationDomainModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}EnglishApplication.Domain", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<EnglishApplicationApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}EnglishApplication.Application.Contracts", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<EnglishApplicationApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}EnglishApplication.Application", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<EnglishApplicationHttpApiModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}EnglishApplication.HttpApi", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<EnglishApplicationWebModule>(hostingEnvironment.ContentRootPath);
            }
        });
    }

    private void ConfigureNavigationServices()
    {
        Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new EnglishApplicationMenuContributor());
        });

        Configure<AbpToolbarOptions>(options =>
        {
            options.Contributors.Add(new EnglishApplicationToolbarContributor());
        });
    }

    private void ConfigureAutoApiControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(EnglishApplicationApplicationModule).Assembly);
        });
    }

    private void ConfigureSwaggerServices(IServiceCollection services)
    {
        services.AddAbpSwaggerGen(
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "EnglishApplication API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);
            }
        );
    }


    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        app.UseForwardedHeaders();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();

        if (!env.IsDevelopment())
        {
            app.UseErrorPage();
            app.UseHsts();
        }

        app.UseCorrelationId();
        app.MapAbpStaticAssets();
        app.UseAbpStudioLink();
        app.UseRouting();
        app.UseAbpSecurityHeaders();
        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }

        // Session middleware'ini UseRouting'den sonra ve UseEndpoints'ten �nce ekleyin
        app.UseSession();

        app.UseAuthenticationRedirect();

        app.UseUnitOfWork();
        app.UseDynamicClaims();
        app.UseAuthorization();
        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "EnglishApplication API");
        });
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();


        app.UseConfiguredEndpoints();
    }
}


public class RedirectUnauthenticatedUserFilter : IAsyncAuthorizationFilter
{
    private readonly ICurrentUser _currentUser;
    private readonly string[] _allowedPaths = new[]
    {
        "/",
        "/Home",
        "/Account/Login",
        "/Account/Register",
        "/Account/ForgotPassword",
        "/Account/ResetPassword",
        "/Account/Manage",
        "/Account/Logout",
        "/Account/ConfirmEmail",
        "/Account/ExternalLogin",
        "/swagger",
        "/api"
    };

    public RedirectUnauthenticatedUserFilter(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // IAllowAnonymous attribute ta��yanlar� kontrol et
        if (context.HttpContext.GetEndpoint()?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
        {
            await Task.CompletedTask;
            return;
        }

        // Kullan�c� giri� yapm�� m� kontrol et
        if (_currentUser.IsAuthenticated)
        {
            await Task.CompletedTask;
            return;
        }

        // Giri� yapmam�� kullan�c�lar i�in yolu kontrol et
        var path = context.HttpContext.Request.Path.Value;

        // Statik dosyalar i�in izin ver
        if (path.StartsWith("/libs") ||
            path.StartsWith("/images") ||
            path.StartsWith("/styles") ||
            path.StartsWith("/favicon") ||
            path.Contains("."))
        {
            await Task.CompletedTask;
            return;
        }

        // �zin verilen yollar i�in kontrol et
        foreach (var allowedPath in _allowedPaths)
        {
            if (path.StartsWith(allowedPath, StringComparison.OrdinalIgnoreCase))
            {
                await Task.CompletedTask;
                return;
            }
        }

        // Hi�bir ko�ula uymuyorsa, Home sayfas�na y�nlendir
        context.Result = new RedirectResult("/Home");
        await Task.CompletedTask;
    }
}