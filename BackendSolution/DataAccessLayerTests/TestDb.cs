using System.Text.Json;
//helper function to read database connection string from dbconfig.json
internal static class TestDb
{
    public static string GetConnectionString()
    {
        var json = File.ReadAllText("dbconfig.json");
        var JsonSerialized = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                  ?? throw new InvalidOperationException("Failed to parse dbconfig.json");

        var host = JsonSerialized["Host"];
        var port = JsonSerialized.ContainsKey("Port") ? JsonSerialized["Port"] : "5432";
        var user = JsonSerialized["User"];
        var password = JsonSerialized["Password"];
        var database = JsonSerialized.ContainsKey("Database") ? JsonSerialized["Database"] : user; // fallback for current file

        return $"Host={host};Port={port};Database={database};Username={user};Password={password}";
    }
}
