# Pricing API manual checks

Date: 19 November 2026
Endpoint: POST http://localhost:5104/api/pricing/quote

## Premium student tickets at high demand

Input:
- Ticket type: student
- Base price: 10
- Quantity: 2
- Premium movie: true
- Occupancy: 0.8

Expected: unit price 10.56, quantity 2, total price 21.12.
Actual: unit price 10.56, quantity 2, total price 21.12.
Result: Passed.

## Zero quantity

Input:
- Ticket type: adult
- Base price: 10
- Quantity: 0
- Premium movie: false
- Occupancy: 0

Expected: HTTP 400 with "Quantity must be greater than zero."
Actual: HTTP 400 Bad Request.
Response: {"error":"Quantity must be greater than zero."}
Result: Passed.

These are manual HTTP checks, separate from the automated unit tests.