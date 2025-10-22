namespace DataService.Util; // WARN: might need to just be `namespace DataService.Entities;` or `namespace DataService;`. Unsure.

public class Year4
{
    public int Value { get; }

    public Year4(int year)
    {
        if (year < 1000 || year > 9999)
            throw new ArgumentOutOfRangeException(nameof(year), "Year must be 4 digits.");
        Value = year;
    }

    public static Year4 Parse(string s)
    {
        if (int.TryParse(s, out int year))
            return new Year4(year);
        throw new FormatException("Invalid year format");
    }

    public override string ToString() => Value.ToString();
}


// FIX: The datateam should figure out what a C# reprensentation of the year4 from the db is (old comment)

// Vi skal lige kigge sammen på det her - er det unødvendigt at have exception for range på nu hvor vi ikke opretter noget?