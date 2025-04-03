using AutoMapper;
using EnglishApplication.Books;
using EnglishApplication.UserSettings;
using EnglishApplication.WordDetails;
using EnglishApplication.Words;
using EnglishApplication.WordSamples;

namespace EnglishApplication;

public class EnglishApplicationApplicationAutoMapperProfile : Profile
{
    public EnglishApplicationApplicationAutoMapperProfile()
    {
        CreateMap<Book, BookDto>();
        CreateMap<CreateUpdateBookDto, Book>();


        CreateMap<Word, WordDto>();
        CreateMap<CreateUpdateWordDto, Word>();
        CreateMap<WordDto, CreateUpdateWordDto>();

        CreateMap<WordDetail, WordDetailDto>();
        CreateMap<CreateUpdateWordDetailDto, WordDetail>();

        CreateMap<WordSample, WordSampleDto>();
        CreateMap<CreateUpdateWordSampleDto, WordSample>();

        CreateMap<UserSetting, UserSettingDto>();
        CreateMap<CreateUpdateUserSettingDto, UserSetting>();


        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */
    }
}
