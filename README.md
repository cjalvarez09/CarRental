# CarRental

## Known limitations

- **Employee self-registration**: `POST /api/auth/register` lets anyone register as either `Customer` or `Employee`. In a real system, Employee accounts shouldn't be self-service — they'd be created/invited by an admin instead. This was simplified for the scope of this challenge.
- **Customers registered by an employee have no login**: `POST /api/customers` / `PUT /api/customers/{id}` (Employee-only, e.g. for a walk-in customer at the counter) create or update a `Customer` profile with no `User` account linked to it — unlike self-registration via `/api/auth/register`, which creates both atomically. In a real system, registering a customer this way should generate a temporary password and email the customer an activation link so they can claim a login for that profile later. That email/activation flow wasn't built for this challenge; the simplification was to leave those customers without login access instead of half-implementing it.
