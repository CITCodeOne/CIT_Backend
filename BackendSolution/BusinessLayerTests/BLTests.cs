using System;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;
using AutoMapper;
using BusinessLayer.Mappings;
using BusinessLayer.Services;

public class BusinessLayerTests
{
    private CITContext DbContext()
    {
        var options = new DbContextOptionsBuilder<CITContext>()
            .UseNpgsql("Host=your_host;Database=your_database;Username=your_user;Password=your_password") // PostgreSQL eksempel
            .Options;

        return new CITContext(options);
    }

    private IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        return config.CreateMapper();
    }

    // Test to verify retrieval of title details through the business layer 'TitleService'
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