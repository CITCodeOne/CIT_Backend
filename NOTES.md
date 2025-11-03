# Notes

The following are notes on what we did when, taken with the express purpose of
remembering important details later when we write the report.

## Architecture

Our initial approach was to consider what entities the front end would need.
This at some point along the way gave us a class diagram of sorts. From here,
we considered how the IDataService interface would look, and subsequently
started working on the DataService for each entity.

## Moving to 3 layer architecture

So, after a lot of work, we have arrived at a point where a 3 layer
architecture makes more sense. This requires a large refactoring. These notes
are to help us remember what we did.

### refactoring to 3 layers as 3 projects

So, step 1 is creating the 3 new layers as their own csproj files.
Specifically, this is a cople Dotnet commands from the terminal.

First, we remove the existing projects from the solution:

```bash
dotnet sln remove WebService/WebService.csproj
dotnet sln remove DataService/DataService.csproj
```

Then we create the new projects:

```bash
dotnet new webapi -n WebServiceLayer
dotnet new classlib -n BusinessLayer
dotnet new classlib -n DataAccessLayer
```

Finally, we add the new projects to the solution:

```bash
dotnet sln add WebServiceLayer/WebServiceLayer.csproj
dotnet sln add BusinessLayer/BusinessLayer.csproj
dotnet sln add DataAccessLayer/DataAccessLayer.csproj
```

### moving code to the new projects

This is mostly a matter of moving files around. However, a main part of this is
keeping track of dependencies and namespaces.

## Operational notes

- VPN prerequisite for DB access
	- Our PostgreSQL database is only reachable on the university network. Before running any DB-backed endpoints locally, verify the VPN is connected. If endpoints suddenly return 500 errors, first ask: "Am I on VPN?".
	- Quick checks:
		- Host resolves: `dig +short cit.ruc.dk` (should return an IP when VPN is on)
		- Port open: `nc -vz cit.ruc.dk 5432`

- Smoke test routine (local)
	- Start WebService in Development on http://localhost:5001
	- Minimal checks:
		- GET `/openapi/v1.json` → 200 (no DB dependency)
		- GET `/api/title/tt0052520` → 200 (existing title)
		- GET `/api/title/tt9999999` → 404 (missing title)
		- GET `/api/contributor/tt0052520/basic` → 200
	- Purpose: quick “does it boot and basic routes work?” before deeper testing.
