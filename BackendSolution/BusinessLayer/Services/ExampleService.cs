using BusinessLayer.Mappings;

namespace BusinessLayer.Services;

public class ExampleService
{
    // INFO: unsure if this is the way this needs to look. But for now, this is how it is.
    private readonly MappingProfile _Mapper;

    public ExampleService(MappingProfile mappingProfile)
    {
        _Mapper = mappingProfile;
    }

    // WARN: this just to test namespace things
    // Service methods would go here
}
