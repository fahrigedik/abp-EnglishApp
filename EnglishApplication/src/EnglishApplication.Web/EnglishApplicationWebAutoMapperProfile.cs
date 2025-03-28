using AutoMapper;
using EnglishApplication.Books;

namespace EnglishApplication.Web;

public class EnglishApplicationWebAutoMapperProfile : Profile
{
    public EnglishApplicationWebAutoMapperProfile()
    {
        CreateMap<BookDto, CreateUpdateBookDto>();
        
        //Define your object mappings here, for the Web project
    }
}
