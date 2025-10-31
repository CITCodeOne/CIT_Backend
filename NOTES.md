# Notes

The following are notes on what we did when, taken with the express purpose of remembering important details later when we write the report.

## Architecture

Our initial approach was to consider what entities the front end would need. This at some point along the way gave us a class diagram of sorts. From here, we considered how the IDataService interface would look, and subsequently started working on the DataService for each entity.

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
