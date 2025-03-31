using EnglishApplication.UserSettings;
using EnglishApplication.WordDetails;
using EnglishApplication.Words;
using EnglishApplication.WordSamples;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.Identity;

namespace EnglishApplication.EntityFrameworkCore;

public static class EnglishApplicationDbContextModelCreatingExtensions
{
    public static void ConfigureEnglishApp(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        /* Configure your own tables/entities inside here */

        builder.Entity<Word>(b =>
        {
            b.ConfigureByConvention();
            b.HasOne<IdentityUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<WordDetail>(b =>
        {
            b.ConfigureByConvention();
            b.HasOne<Word>()
                .WithOne()
                .HasForeignKey<WordDetail>(wd => wd.WordId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<WordSample>(b =>
        {
            b.ConfigureByConvention();
            b.HasOne<Word>().WithMany().HasForeignKey(x => x.WordId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<UserSetting>(b =>
        {
            b.ConfigureByConvention();
            b.HasOne<IdentityUser>().WithOne().HasForeignKey<UserSetting>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });






    }
}