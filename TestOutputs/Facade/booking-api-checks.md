# Booking Facade API checks

Date: 19 November 2026

## Successful booking
- Sent POST /api/bookings for showing 1, adult, quantity 2.
- Two tickets were returned with different IDs.
- Each ticket cost €12.
- Showing availability changed to 2 seats sold and 98 available.
- Result: Passed.

## Insufficient seats
- Requested 99 tickets when only 98 seats remained.
- API returned HTTP 409 Conflict.
- Response: "There are not enough seats available."
- Availability remained at 2 seats sold and 98 available.
- Result: Passed.

## Pricing service unavailable
- Stopped the Pricing API and requested one adult ticket.
- Booking API returned HTTP 503 Service Unavailable.
- Availability remained at 2 seats sold and 98 available.
- Restarted the Pricing API after the check.
- Result: Passed.

## Automated tests
- 26 tests passed, 0 failed.
- Facade tests cover successful persistence, insufficient seats,
  and pricing-service failure.
- Red and green test outputs are stored in this folder.