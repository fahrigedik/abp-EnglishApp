using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace EnglishApplication.WordDetails;

public interface IWordDetailRepository : IRepository<WordDetail, Guid>
{
    public Task<WordDetail> GetWordDetailByWordId(Guid wordId);


    public Task<List<WordDetail>> GetWordDetailsByWordIds(List<Guid> WordIds);

    /// <summary>
    /// Bugün çalışılması gereken (zamanı gelmiş ve öğrenilmemiş) kelimelerin detaylarını getirir
    /// </summary>
    /// <param name="wordIds">Filtreleme yapılacak kelime ID'leri</param>
    /// <param name="date">Baz alınacak tarih (varsayılan olarak bugün)</param>
    /// <returns>Çalışılması gereken kelime detayları</returns>
    Task<List<WordDetail>> GetDueWordDetailsAsync(List<Guid> wordIds, DateTime? date = null);
}