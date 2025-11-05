using BusinessLayer.Services;

public class BLTests
{
    // Hashing Test
    // WARN: I dont know what this is supposed to do exactly. But this is an example of a unit test for the Hashing service.
    [Fact]
    public void HashPassword_ShouldReturnHashedPassword()
    {
        // Arrange

        // create an instance of the Hashing service
        Hashing bl = new Hashing();
        string password = "TestPassword123";

        // Act
        var hashedPassword = bl.Hash(password);

        // Assert
        Assert.NotNull(hashedPassword);
    }
}
