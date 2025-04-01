using AutoMapper;
using EnglishApplication.Books;
using EnglishApplication.WordDetails;
using EnglishApplication.Words;

namespace EnglishApplication;

public class EnglishApplicationApplicationAutoMapperProfile : Profile
{
    public EnglishApplicationApplicationAutoMapperProfile()
    {
        CreateMap<Book, BookDto>();
        CreateMap<CreateUpdateBookDto, Book>();


        CreateMap<Word, WordDto>();
        CreateMap<CreateUpdateWordDto, Word>();

        CreateMap<WordDetail, WordDetailDto>();
        CreateMap<CreateUpdateWordDetailDto, WordDetail>();

        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */
    }
}
