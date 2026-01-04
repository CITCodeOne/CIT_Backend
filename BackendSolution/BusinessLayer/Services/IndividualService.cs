using AutoMapper;
using DataAccessLayer.Data;
using BusinessLayer.DTOs;
using BusinessLayer.Parameters;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Services;

public class IndividualService
{
    // `CITContext` er det objekt der repræsenterer forbindelsen til databasen.
    // Det indeholder tabeller som `Individuals`, `Titles` osv. Vi bruger det til at hente data.
    private readonly CITContext _ctx;

    // `IMapper` bruges til at konvertere database-objekter (entiteter) til DTO'er
    // (DTO = et forenklet dataskema, som applikationen returnerer til klienten).
    private readonly IMapper _mapper;

    // Konstruktør: når tjenesten oprettes, får den adgang til database-konteksten og mapperen.
    // Dette mønster kaldes 'dependency injection' og gør det nemmere at teste og genbruge koden.
    public IndividualService(CITContext ctx, IMapper mapper)
    {
        _ctx = ctx; // Gem database-forbindelsen i en privat variabel
        _mapper = mapper; // Gem mapperen så vi kan konvertere til DTO'er senere
    }

    // Søg efter personer (actors/actresses) baseret på parametre.
    // Metoden returnerer en liste af forenklede objekter med navnet og antal stemmer.
    // Forklaring for ikke-tekniske: vi bygger et spørgsmål til databasen (en "query"),
    // filtrerer efter de ønsker brugeren har angivet (fx navn eller fødselsår),
    // sorterer resultaterne, og sender en side (pagination) tilbage.
    public List<IndividualReferenceWithTotalVotesDTO> SearchIndividuals(IndividualSearchParameters parameters)
    {
        // Starter med en færdiglavet visning (`IndividualVotesViews`) i databasen,
        // som allerede indeholder aggregater (fx total stemmer) for hver person.
        var query = _ctx.IndividualVotesViews.AsQueryable();

        // Filtrer på navn hvis brugeren har angivet et navn (delvis match)
        if (!string.IsNullOrWhiteSpace(parameters.Name))
            // EF.Functions.ILike bruges til at lave et 'case-insensitive' søg
            // med jokertegn (%) foran og efter søgeteksten.
            query = query.Where(i => i.Name != null && EF.Functions.ILike(i.Name, $"%{parameters.Name}%"));

        // Filtrer på fødselsår minimum hvis angivet
        if (parameters.MinBirthYear.HasValue)
            query = query.Where(i => i.BirthYear >= parameters.MinBirthYear.Value);

        // Filtrer på fødselsår maksimum hvis angivet
        if (parameters.MaxBirthYear.HasValue)
            query = query.Where(i => i.BirthYear <= parameters.MaxBirthYear.Value);

        // Sortering: bestem hvordan resultaterne skal ordnes
        // Vi tjekker `SortBy` og vælger den rigtige kolonne at sortere efter.
        query = parameters.SortBy?.ToLower() switch
        {
            // Hvis brugeren bad om sortering efter navn
            "name" => parameters.SortDescending
                ? query.OrderByDescending(i => i.Name)
                : query.OrderBy(i => i.Name),
            // Hvis brugeren bad om sortering efter fødselsår
            "birthyear" => parameters.SortDescending
                ? query.OrderByDescending(i => i.BirthYear)
                : query.OrderBy(i => i.BirthYear),
            // Hvis brugeren bad om sortering efter rating
            "rating" => parameters.SortDescending
                ? query.OrderByDescending(i => i.NameRating)
                : query.OrderBy(i => i.NameRating),
            // Standard: sorter efter antal stemmer (mest populære først)
            _ => query.OrderByDescending(i => i.TotalVotes),
        };

        // Paginering betyder at vi kun henter en del (en "side") af resultaterne ad gangen.
        // Det gør at vi ikke henter for meget data når brugeren kun vil se fx side 2.
        var skip = (parameters.Page - 1) * parameters.PageSize;
        var individuals = query
            .Skip(skip) // spring de første `skip` rækker over
            .Take(parameters.PageSize) // tag `PageSize` antal rækker
            .ToList();

        // Konverter database-objekterne til DTO'er som klienten forstår og returner dem
        return _mapper.Map<List<IndividualReferenceWithTotalVotesDTO>>(individuals);
    }


    // Hent fuld information om en person ud fra et entydigt id (`iconst`).
    // Returnerer `null` hvis personen ikke findes.
    public IndividualFullDTO? FullById(string iconst)
    {
        // Finder den første (og kun) person med det matchende id i databasen
        var individual = _ctx.Individuals.FirstOrDefault(i => i.Iconst == iconst);
        // Hvis der ikke findes nogen, returner null, ellers map til DTO og returner
        return individual == null ? null : _mapper.Map<IndividualFullDTO>(individual);
    }

    // Hent en letvægts-reference til en person (bruges typisk i lister)
    public IndividualReferenceDTO? ReferenceByID(string iconst)
    {
        // Inkluder også relateret sideinformation (IndividualPage) så den er tilgængelig
        var individual = _ctx.Individuals
            .Include(i => i.IndividualPage)
            .FirstOrDefault(i => i.Iconst == iconst);
        return individual == null ? null : _mapper.Map<IndividualReferenceDTO>(individual);
    }

    // Hent en side med personer (simpel paginering).
    // Bemærk: kommentaren i koden nævner "keyset paging" som en mulig forbedring,
    // men her dokumenterer vi kun hvad koden gør nu.
    public List<IndividualReferenceDTO> ReferenceByPage(int page = 1, int pageSize = 20)
    {
        var individuals = _ctx.Individuals
            // Sortér efter en score (NameRating) så de mest relevante vises først
            .OrderByDescending(t => t.NameRating)
            // Inkluder relateret side-uddata
            .Include(i => i.IndividualPage)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return _mapper.Map<List<IndividualReferenceDTO>>(individuals);
    }

    // Hent de mest populære personer baseret på rating, ekskluderer dem uden rating eller fødselsår
    public List<IndividualReferenceDTO> GetMostPopularIndividuals(int page = 1, int pageSize = 20)
    {
        var individuals = _ctx.Individuals
            .Where(i => i.NameRating != null && i.BirthYear != null)
            .OrderByDescending(i => i.NameRating)
            .ThenBy(i => i.Iconst) // sekundær sortering for at sikre deterministisk rækkefølge
            .Include(i => i.IndividualPage)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return _mapper.Map<List<IndividualReferenceDTO>>(individuals);
    }

    // Hent de mest populære personer sorteret efter antal stemmer.
    // Denne funktion kører en SQL-forespørgsel direkte mod databasen for at samle stemmer
    // og begrænse resultatet til et antal rækker for performance.
    public List<IndividualReferenceWithTotalVotesDTO> GetMostPopularIndividualsByVotes(int page = 1, int pageSize = 20)
    {
        var offset = (page - 1) * pageSize;

        // Den nedenstående SQL-tekst laver to trin: først beregnes total stemmer per person,
        // derefter joines det med person- og side-tabellerne for at hente detaljer.
        var individuals = _ctx.Database
            .SqlQueryRaw<IndividualReferenceWithTotalVotesDTO>(
                @"WITH popular_actors AS (
                    SELECT 
                        c.iconst,
                        SUM(t.numvotes) AS total_votes
                    FROM mdb.contributor c
                    INNER JOIN mdb.title t USING (tconst)
                    WHERE c.contribution IN ('actor', 'actress')
                      AND t.media_type = 'movie'
                      AND t.numvotes > 0
                    GROUP BY c.iconst
                    ORDER BY total_votes DESC
                    LIMIT 1000
                )
                SELECT 
                    pa.iconst AS ""Id"",
                    i.name AS ""Name"",
                    p.pconst AS ""PageId"",
                    pa.total_votes AS ""TotalVotes"",
                    i.name_rating AS ""NameRating"",
                    i.birth_year AS ""BirthYear"",
                    i.death_year AS ""DeathYear""
                FROM popular_actors pa
                INNER JOIN mdb.individual i USING (iconst)
                LEFT JOIN mdb.page p USING (iconst)
                ORDER BY pa.total_votes DESC
                LIMIT {0} OFFSET {1}",
                pageSize, offset)
            .AsNoTracking() // Vi skal ikke ændre disse objekter, derfor slåes "tracking" fra
            .ToList();

        // Denne metode returnerer allerede DTO-typen direkte fra SQL-forespørgslen
        return individuals;
    }

    // Hent titler (film/serier) hvor en given person har været bidragyder
    public List<TitlePreviewDTO> TitlesByIndividual(string iconst)
    {
        var titles = _ctx.Titles
            // Kig kun på titler hvor personen er listet som contributor
            .Where(t => t.Contributors.Any(c => c.Iconst == iconst))
            .Include(t => t.TitlePage)
            // Sortér efter antal stemmer for at vise mest populære titler først
            .OrderByDescending(t => t.Numvotes)
            .ToList();
        return _mapper.Map<List<TitlePreviewDTO>>(titles);
    }

    // Hent populære skuespillere relateret til et givent id (kan være person eller titel)
    public List<IndividualFullDTO> GetPopularActors(string qConst)
    {
        var individuals = _ctx.Individuals
            // Kald en database-funktion/procedure der returnerer populære skuespillere
            .FromSqlRaw("SELECT * FROM mdb.popular_actor({0})", qConst)
            .ToList();
        return _mapper.Map<List<IndividualFullDTO>>(individuals);
    }

    // Find co-actors (personer som har arbejdet sammen med en given skuespiller)
    public List<CoActorDTO> FindCoActors(string actorName)
    {
        // Log til konsollen så udviklere kan se inputtet
        Console.WriteLine($"Finding co-actors for: {actorName}");
        // Hvis navnet er URL-kodet (fx mellemrum som %20), så dekoder vi det
        actorName = System.Net.WebUtility.UrlDecode(actorName);
        Console.WriteLine($"Encoded actor name: {actorName}");

        // Hvis der ikke er noget navn at søge på, returner en tom liste
        if (string.IsNullOrWhiteSpace(actorName))
            return new List<CoActorDTO>();

        // Kør en databasefunktion der finder co-actors. Vi bruger parameterisering
        // ({0}) for at sende navnet ind i forespørgslen uden at ændre SQL-strengen direkte.
        var results = _ctx.Database
            .SqlQueryRaw<CoActorDTO>("SELECT iconst AS Id, primaryname AS Name, co_count AS CollaborationCount FROM mdb.find_co_actors({0})", actorName)
            .ToList();

        return results;
    }

    // Søg efter personer ved hjælp af en database-funktion der samler relevante oplysninger
    public List<IndividualSearchResultDTO> SearchIndividualsByFunction(string name)
    {
        var results = _ctx.Database
            .SqlQueryRaw<IndividualSearchResultDTO>("SELECT iconst AS Id, name AS Name, contribution AS Contribution, title_name AS TitleName, COALESCE(detail, '') AS Detail, COALESCE(genre, '') AS Genre FROM mdb.find_name({0})", name)
            .ToList();
        return results;
    }
}
