using System.Security.Cryptography;
using System.Text;

namespace BusinessLayer;

/// <summary>
/// Password hashing service using SHA256 with salt.
/// Source: https://github.com/bulskov/CIT_2025_Authentication
/// </summary>
public class Hashing
{
    protected const int saltBitsize = 64;
    protected const byte saltBytesize = saltBitsize / 8;
    protected const int hashBitsize = 256;
    protected const int hashBytesize = hashBitsize / 8;

    private HashAlgorithm sha256 = SHA256.Create();
    protected RandomNumberGenerator rand = RandomNumberGenerator.Create();

    // hash(string password)
    // called from Authenticator.register()
    // where salt and hashed password have not been generated,
    // so both are returned for storing in the password database

    public (string hash, string salt) Hash(string password)
    {
        byte[] salt = new byte[saltBytesize];
        rand.GetBytes(salt);
        string saltString = Convert.ToHexString(salt);
        string hash = HashSHA256(password, saltString);
        return (hash, saltString);
    }

    // verify(string login_password, string hashed_registered_password, string saltstring)
    // is called from Authenticator.login()

    public bool Verify(string loginPassword, string hashedRegisteredPassword, string saltString)
    {
        string hashedLoginPassword = HashSHA256(loginPassword, saltString);
        if (hashedRegisteredPassword == hashedLoginPassword) return true;
        else return false;
    }

    // hashSHA256 is the "workhorse" --- the actual hashing

    private string HashSHA256(string password, string saltString)
    {
        byte[] hashInput = Encoding.UTF8.GetBytes(saltString + password); // perhaps encode only the password part?
        byte[] hashOutput = sha256.ComputeHash(hashInput);
        return Convert.ToHexString(hashOutput);
    }
}
