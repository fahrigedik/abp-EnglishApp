using AutoMapper;
using EnglishApp.Entities.Books;
using EnglishApp.Services.Dtos.Books;

namespace EnglishApp.ObjectMapping;

public class EnglishAppAutoMapperProfile : Profile
{
    public EnglishAppAutoMapperProfile()
    {
        CreateMap<Book, BookDto>();
        CreateMap<CreateUpdateBookDto, Book>();
        CreateMap<BookDto, CreateUpdateBookDto>();
        /* Create your AutoMapper object mappings here */
    }
}
