using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xunit;
using DataService.Data;
using DataService.Entities;
using System.Text.Json;
using System.IO;

/* This test class contains unit tests for the Data Access Layer (DAL) related to Individuals. 
    The purpose is to test that our data access layer behaves as expected when interacting with Individual entities. */

namespace WebService.Tests.DataAccessLayer
{
    public class DAL_IndividualTestsTest
    {
        private CITContext GetDatabaseContext()
        {
            var config = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText("dbconfig.json"));

            if (config == null || !config.ContainsKey("Host") || !config.ContainsKey("Database") ||
                !config.ContainsKey("User") || !config.ContainsKey("Password"))
            {
                throw new InvalidOperationException("Database configuration is missing or incomplete.");
            }

            var connectionString = $"Host={config["Host"]};Database={config["Database"]};Username={config["User"]};Password={config["Password"]};";

            var options = new DbContextOptionsBuilder<CITContext>()
                .UseNpgsql(connectionString)
                .Options;

            return new CITContext(options);
        }

        // Test to verify that an Individual can be retrieved correctly from the database
        [Fact]
        public void GetIndividual_ShouldReturnIndividual()
        {
            // Arrange
            using var context = GetDatabaseContext();

            // Act
            var individual = context.Individuals
                .FirstOrDefault(i => i.Iconst == "nm16078515forkert"); // Gets Individual

            // Assert
            Assert.NotNull(individual); // Ensures that Individual exists
            if (individual == null)
            {
                throw new InvalidOperationException("Individual was not found in the database.");
            }
            Assert.Equal("James Scott", individual.Name); // Validates the name
        }
    }
}