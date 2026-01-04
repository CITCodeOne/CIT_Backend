using System.Security.Cryptography;
using System.Text;

namespace BusinessLayer.Services;

// En simpel hashing-service: den tager et almindeligt password og laver
// en (sikker) hash som kan gemmes i databasen. Når en bruger senere logger ind,
// hasher vi det indtastede password igen med samme salt og sammenligner.
// Forklaring for ikke-teknikere: en hash er en uigenkendelig repræsentation af passwordet,
// det betyder at selv hvis nogen får adgang til databasen, kan de ikke se dine passwords.
public class Hashing
{
    // Størrelsen på salt i bits. 64 bits = 8 bytes.
    // Et salt er en tilfældig værdi der gør at selv samme password ikke får samme hash.
    protected const int saltBitsize = 64;
    protected const byte saltBytesize = saltBitsize / 8;

    // SHA-256 producerer 256-bit hash (32 bytes)
    protected const int hashBitsize = 256;
    protected const int hashBytesize = hashBitsize / 8;

    // En SHA256-instans som bruges til at beregne hashene
    private HashAlgorithm sha256 = SHA256.Create();

    // Krypterings-sikker tilfældighedsgenerator til at lave salt
    protected RandomNumberGenerator rand = RandomNumberGenerator.Create();

    // Hash: tager et plaintext-password og returnerer en tuple (hash, salt)
    // Begge gemmes i databasen ved brugeroprettelse.
    public (string hash, string salt) Hash(string password)
    {
        // Opret en buffer til salt og fyld den med tilfældige bytes
        byte[] salt = new byte[saltBytesize];
        rand.GetBytes(salt);

        // Gem salt som en hex-streng for nem lagring
        string saltString = Convert.ToHexString(salt);

        // Beregn den faktiske hash ved at kombinere salt og password
        string hash = HashSHA256(password, saltString);

        // Returnér begge som strenge
        return (hash, saltString);
    }

    // Verify: kontrollerer om et indtastet password svarer til den gemte hash
    // ved at hashe det indtastede password med samme salt og sammenligne.
    public bool Verify(string loginPassword, string hashedRegisteredPassword, string saltString)
    {
        string hashedLoginPassword = HashSHA256(loginPassword, saltString);
        return hashedRegisteredPassword == hashedLoginPassword;
    }

    // HashSHA256: kombinerer salt og password og beregner SHA-256 hash.
    // Bemærk: hvordan salt og password kombineres (før/efter) skal være konsekvent.
    private string HashSHA256(string password, string saltString)
    {
        byte[] hashInput = Encoding.UTF8.GetBytes(saltString + password);
        byte[] hashOutput = sha256.ComputeHash(hashInput);
        return Convert.ToHexString(hashOutput);
    }
}
