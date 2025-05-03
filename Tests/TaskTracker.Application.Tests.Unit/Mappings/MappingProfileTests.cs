using AutoMapper;
using TaskTracker.Application.Mappings;

namespace TaskTracker.Application.Tests.Unit.Mappings;

public class MappingProfileTests
{
    [Fact]
    public void AutoMapper_Configuration_IsValid()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<TaskProfile>();
        });

        config.AssertConfigurationIsValid();
    }
}
