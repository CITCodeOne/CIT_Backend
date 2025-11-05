using System;
using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using BusinessLayer.Mappings;
using BusinessLayer.Services;

public class BusinessLayerTests
{
    private CITContext DbContext()
    {
        var conn = TestDb.GetConnectionString();
        var options = new DbContextOptionsBuilder<CITContext>()
            .UseNpgsql(conn)
            .Options;
        return new CITContext(options);
    }

    private IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>(); // Adding mapping profile
        });

        return config.CreateMapper();
    }

    // Test to verify retrieval of title through the business layer 'TitleService'
    [Fact]
    public void GetTitleById_WithValidTconst_ReturnsExpectedTitle()
    {
        // Arrange
        var dbContext = DbContext();
        var mapper = CreateMapper();
        var titleService = new TitleService(dbContext, mapper);

        // Act
        var retrievedTitle = titleService.GetTitleById("tt0088634");

        // Assert
        Assert.NotNull(retrievedTitle);
        Assert.Equal("The Twilight Zone", retrievedTitle.Name);
    }
}