using System;
using BusinessLayer.Services;
using Xunit;

public class BLTests
{
    // Hashing Test - ensure hashed password is different from the plain password
    [Fact]
    public void Hash_WithValidPassword_ReturnsHashedPassword()
    {
        var hashing = new Hashing();
        string password = "TestPassword123";

        var (hash, salt) = hashing.Hash(password);

        Assert.NotNull(hash);
        Assert.NotNull(salt);
        Assert.NotEqual(password, hash);
    }

    // Verify Test - ensure correct password verifies successfully

    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        var hashing = new Hashing();
        string password = "MyS3cret!";
        var (hash, salt) = hashing.Hash(password);

        var ok = hashing.Verify(password, hash, salt);

        Assert.True(ok);
    }

    // Verify Test - ensure wrong password fails verification
    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        var hashing = new Hashing();
        string password = "RightPass";
        var (hash, salt) = hashing.Hash(password);

        var ok = hashing.Verify("WrongPass", hash, salt);

        Assert.False(ok);
    }
    // Hashing Test - ensure hashing the same password twice produces different salts and likely different hashes
    [Fact]
    public void Hash_SamePasswordTwice_ProducesDifferentSaltAndHash()
    {
        var hashing = new Hashing();
        string password = "RepeatablePass";

        var (hash1, salt1) = hashing.Hash(password);
        var (hash2, salt2) = hashing.Hash(password);

        Assert.NotEqual(salt1, salt2);
        Assert.NotEqual(hash1, hash2);
    }
}
