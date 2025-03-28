using AutoMapper;
using EnglishApplication.Books;

namespace EnglishApplication;

public class EnglishApplicationApplicationAutoMapperProfile : Profile
{
    public EnglishApplicationApplicationAutoMapperProfile()
    {
        CreateMap<Book, BookDto>();
        CreateMap<CreateUpdateBookDto, Book>();
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */
    }
}
