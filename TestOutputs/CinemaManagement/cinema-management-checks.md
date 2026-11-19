# Cinema management checks

Date: 19 November 2026
Booking API: http://localhost:5209

## Database creation
InitialCinemaSchema migration applied successfully.
Movies, Screens and Showings tables were created.

## Movie and screen storage
Retrieved movie 1: Interstellar, 169 minutes, premium.
Retrieved screen 1: Screen 1, capacity 100.
Both records were retrieved after the reported API restart.

## Schedule a showing
Created showing 1 for movie 1 in screen 1.
Start: 20 November 2026 at 18:00.
Calculated end: 20 November 2026 at 20:49.
Base price: EUR 10.
Result: Passed.

## Reject overlapping showing
Attempted another showing in screen 1 at 19:00 on the same date.
Actual response: HTTP 409 Conflict.
Message: "This screen already has a showing during that time."
A subsequent GET returned only showing 1.
Result: Passed.

## Existing pattern tests
All 23 existing automated tests passed.
Results saved in cinema-management-regression.trx.
These are regression tests for the existing patterns.
The management checks above were manual HTTP checks.

## Dependency check
NuGet initially reported vulnerability GHSA-2m69-gcr7-jv3q
in SQLitePCLRaw.lib.e_sqlite3 2.1.11.
Updated SQLitePCLRaw.bundle_e_sqlite3 to 3.0.5.
The subsequent vulnerable-package check, including transitive
dependencies, reported no known vulnerable packages from its sources.