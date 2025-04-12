using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnglishApplication.WordDetails;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace EnglishApplication.EntityFrameworkCore.Repository.WordDetails;

public class WordDetailRepository : EfCoreRepository<EnglishApplicationDbContext, WordDetail, Guid>, IWordDetailRepository
{
    public WordDetailRepository(IDbContextProvider<EnglishApplicationDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public Task<WordDetail> GetWordDetailByWordId(Guid wordId)
    {
        var wordDetail = DbSet.Where(x => x.WordId == wordId).FirstOrDefaultAsync();
        return wordDetail;
    }

    public Task<List<WordDetail>> GetWordDetailsByWordIds(List<Guid> wordIds)
    {
        return DbSet.Where(wd => wordIds.Contains(wd.WordId)).ToListAsync();
    }

    /// <summary>
    /// Bugün çalışılması gereken (zamanı gelmiş ve öğrenilmemiş) kelimelerin detaylarını getirir
    /// </summary>
    /// <param name="wordIds">Filtreleme yapılacak kelime ID'leri</param>
    /// <param name="date">Baz alınacak tarih (varsayılan olarak bugün)</param>
    /// <returns>Çalışılması gereken kelime detayları</returns>
    public async Task<List<WordDetail>> GetDueWordDetailsAsync(List<Guid> wordIds, DateTime? date = null)
    {
        var today = date?.Date ?? DateTime.Now.Date;

        // Aşağıdaki durumları içeren kelime detaylarını getir:
        // 1. Zamanı gelmiş kelimeler (NextDate <= today)
        // 2. NextDate değeri null olan yeni kelimeler
        return await DbSet
            .Where(wd => wordIds.Contains(wd.WordId) &&
                         (
                             // NextDate değeri null olan yeni kelimeler
                             !wd.NextDate.HasValue ||

                             // NextDate değeri olan ve zamanı gelmiş kelimeler
                             (wd.NextDate.HasValue &&
                              ((wd.NextDate.Value.Date == today) || // Aynı gün eklenenleri dahil et
                               (wd.NextDate.Value.Date < today)))   // Geçmiş tarihleri de dahil et
                         ) &&
                         wd.IsLearn != true)
            .ToListAsync();

    }
}