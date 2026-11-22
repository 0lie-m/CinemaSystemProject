# Website validation checks

## Dashboard
- Displayed showing information and seat availability from the Booking API.
- Showed an error message when the Booking API was unavailable.
- Loaded successfully after the API was restarted.

## Movies
- Listed existing movies.
- Added Inception with a duration of 148 minutes and standard pricing.
- Rejected a blank title and duration of zero.

## Screens
- Listed Screen 1 with 100 seats.
- Added Screen 2 with 80 seats.
- Rejected a blank name and capacity of zero.

## Showings
- Scheduled Inception in Screen 2.
- Calculated the end time using the movie duration.
- Rejected an overlapping showing on the same screen.

## Ticket booking
- Requested two student tickets for Inception.
- Displayed a quote of €8 per ticket and €16 total.
- Confirmed the booking and displayed the saved tickets.
- Available seats decreased from 80 to 78.

## Booking history and navigation
- Listed the saved Inception and Interstellar bookings.
- Opened ticket details for each booking.
- Navigation links worked across the website.

## Automated regression checks
- 29 tests passed, 0 failed.
- Results saved in website-booking-regression.trx.
- These automated tests cover backend behaviour.
- The website checks above were performed manually.