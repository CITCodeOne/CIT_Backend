using System;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class DALTests
{
    private CITContext DbContext()
    {
        var options = new DbContextOptionsBuilder<CITContext>()
            .UseNpgsql("Host=your_host;Database=your_database;Username=your_user;Password=your_password") // PostgreSQL eksempel
            .Options;

        return new CITContext(options);
    }

    [Fact]
    public async Task RetrieveTitle_WithValidTconst_ReturnsCorrectTitle()
    {
        // Arrange
        using var context = DbContext();

        // Act
        var retrievedTitle = await context.Titles
            .Where(t => t.Tconst == "tt0264464")
            .Select(t => new
            {
                t.Tconst,
                t.TitleName,
                t.AvgRating
            })
            .FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(retrievedTitle);
        Assert.Equal("tt0264464", retrievedTitle.Tconst);
        Assert.Equal("Catch Me If You Can", retrievedTitle.TitleName);
        Assert.Equal(8.1, retrievedTitle.AvgRating);
    }
}