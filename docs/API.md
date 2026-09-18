# API documentation

Base URL when running locally: `http://localhost:5255` (`dotnet run`) or `http://localhost:8080` (Docker).

When the API runs in the `Development` environment Swagger UI is available at `/swagger`, and you can try every endpoint from there (use the **Authorize** button to paste a token). There's also a static copy of the OpenAPI document in [`openapi.json`](openapi.json), handy for generating the Angular client. It's a snapshot, so if the endpoints change you can refresh it with the API running in `Development`:

```bash
curl http://localhost:5255/swagger/v1/swagger.json -o docs/openapi.json
```

All bodies are JSON. Dates can be sent as `2030-10-01` or as a full ISO date time.

## Authentication and roles

Everything except `register` and `login` needs a JWT in the `Authorization: Bearer <token>` header. Tokens last 60 minutes (`Jwt:ExpirationMinutes` in `appsettings.json`).

There are two roles:

- **Customer**: can check availability and book a car. Nothing else.
- **Employee**: can do everything, including booking on behalf of any customer.

When a Customer books, the API ignores the `customerId` in the request and uses the customer profile linked to their own account, so nobody can book in someone else's name.

### `POST /api/auth/register`

Creates an account. If the role is `Customer` a customer profile is created too, so `fullName`, `address` and `email` are required in that case. For `Employee` they're ignored.

```json
{
  "username": "juan",
  "password": "Secret123",
  "role": "Customer",
  "fullName": "Juan Perez",
  "address": "Calle 1",
  "email": "juan@example.com"
}
```

Response `201`:

```json
{ "id": 1, "username": "juan", "role": "Customer" }
```

Username needs at least 3 characters and the password at least 6.

### `POST /api/auth/login`

```json
{ "username": "juan", "password": "Secret123" }
```

Response `200`:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiresAtUtc": "2030-10-01T15:00:00Z",
  "user": { "id": 1, "username": "juan", "role": "Customer" }
}
```

## Endpoints

| Method | Path | Who | Success | Other possible errors |
|---|---|---|---|---|
| POST | `/api/auth/register` | anyone | 201 | 400, 409 |
| POST | `/api/auth/login` | anyone | 200 | 400, 401 |
| GET | `/api/cars/availability` | Customer, Employee | 200 | 401 |
| GET | `/api/cars` | Employee | 200 | 401, 403 |
| GET | `/api/cars/{id}` | Employee | 200 | 401, 403, 404 |
| POST | `/api/cars` | Employee | 201 | 400, 401, 403 |
| PUT | `/api/cars/{id}` | Employee | 200 | 400, 401, 403, 404 |
| DELETE | `/api/cars/{id}` | Employee | 204 | 401, 403, 404, 409 |
| GET | `/api/customers` | Employee | 200 | 401, 403 |
| GET | `/api/customers/{id}` | Employee | 200 | 401, 403, 404 |
| POST | `/api/customers` | Employee | 201 | 400, 401, 403 |
| PUT | `/api/customers/{id}` | Employee | 200 | 400, 401, 403, 404 |
| DELETE | `/api/customers/{id}` | Employee | 204 | 401, 403, 404, 409 |
| POST | `/api/rentals` | Customer, Employee | 201 | 400, 401, 404, 409 |
| GET | `/api/rentals/{id}` | Employee | 200 | 401, 403, 404 |
| PUT | `/api/rentals/{id}` | Employee | 200 | 400, 401, 403, 404, 409 |
| POST | `/api/rentals/{id}/cancel` | Employee | 204 | 401, 403, 404, 409 |

### Cars

A car is `{ "id": 1, "type": "Sedan", "model": "Corolla" }`. Create and update take `type` and `model`, both required.

**Availability** returns the cars of a given type that are free for the whole date range. `model` is optional.

```
GET /api/cars/availability?type=Sedan&model=Corolla&startDate=2030-10-01&endDate=2030-10-05
```

```json
[ { "id": 1, "type": "Sedan", "model": "Corolla" } ]
```

A car can't be deleted while it has rentals (active or cancelled), you get a `409`. Otherwise it's soft deleted: it disappears from every endpoint but the row stays in the database.

### Customers

A customer is `{ "id": 1, "fullName": "Juan Perez", "address": "Calle 1", "email": "juan@example.com" }`. Create and update take those three fields (`email` must be a valid address).

`POST /api/customers` creates only the customer profile, without any login. Customers who register themselves through `/api/auth/register` get their profile automatically and don't need this endpoint.

Same delete rule as cars: `409` if the customer has rentals, soft delete otherwise.

### Rentals

**Book a car**

```json
POST /api/rentals
{
  "customerId": 1,
  "carId": 3,
  "startDate": "2030-10-01",
  "endDate": "2030-10-05"
}
```

`customerId` has to be a positive number for everyone, but for a Customer it's ignored (see above). Response `201`:

```json
{
  "id": 7,
  "customer": { "id": 1, "fullName": "Juan Perez", "address": "Calle 1", "email": "juan@example.com" },
  "car": { "id": 3, "type": "Sedan", "model": "Corolla" },
  "startDate": "2030-10-01T00:00:00",
  "endDate": "2030-10-05T00:00:00",
  "status": "Active"
}
```

Rules:

- The start date can't be in the past and the end date has to be after the start date (`400` otherwise).
- The car can't have another active rental that overlaps those dates (`409`). Back to back rentals are fine, where one ends the same day the next one starts. Cancelled rentals don't block anything.
- Unknown customer or car gives `404`.

**Change dates**: `PUT /api/rentals/{id}` with `{ "startDate": "...", "endDate": "..." }`. Same date rules, and the overlap check ignores the rental being changed. A cancelled rental can't be modified (`409`).

**Cancel**: `POST /api/rentals/{id}/cancel`, no body. Sets the status to `Cancelled` and frees the car for those dates. Cancelling twice gives `409`.

`status` is either `Active` or `Cancelled`.

## Errors

Every error is an [RFC 7807](https://www.rfc-editor.org/rfc/rfc7807) problem details JSON (`application/problem+json`).

| Status | When |
|---|---|
| 400 | Validation failed. The `errors` object lists the messages per field. |
| 401 | No token, invalid or expired token, or wrong credentials on login. |
| 403 | Valid token but the role isn't allowed to use that endpoint. |
| 404 | The resource doesn't exist (or was soft deleted). |
| 409 | A business rule was broken: username already taken, car not available, deleting something that has rentals, cancelling a cancelled rental. |
| 500 | Something unexpected. The message is generic on purpose, details only go to the logs. |

Validation error:

```json
{
  "title": "Validation failed",
  "status": 400,
  "detail": "Validation failed: ...",
  "errors": {
    "Email": ["'Email' is not a valid email address."]
  }
}
```

Business rule error:

```json
{
  "title": "Business rule violation",
  "status": 409,
  "detail": "Car 3 is not available between 2030-10-02 and 2030-10-03."
}
```

The 401 for a missing or invalid token and the 403 for a wrong role are produced by the authentication middleware, so they only carry the standard fields (`type`, `title`, `status`, `traceId`) and no `detail`.

## CORS

Browsers can call the API from the origins listed in `Cors:AllowedOrigins` in `appsettings.json`, which by default is just `http://localhost:4200` (the Angular dev server). Any header and method is allowed, and the `Authorization` header works, so the bearer token can be sent normally. Error responses carry the CORS headers too, so the front end can read the problem details instead of getting an opaque network error.

To allow another origin, add it to that list or set it with an environment variable, for example `Cors__AllowedOrigins__1=https://my-frontend.example.com`.

## Caching

Read endpoints (lists, get by id, availability) are cached in memory for 5 minutes. Any write that could change what they return clears the related cache entries, so clients shouldn't notice it. It's a per instance cache, see the notes in the README.
