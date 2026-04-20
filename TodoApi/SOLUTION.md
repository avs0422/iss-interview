# SOLUTION

## Problems Identified

The original implementation worked but had several production-readiness issues:

- The controller instantiated `TodoService` directly instead of using dependency injection.
- Business logic and data access concerns were mixed together.
- SQL statements were built with string interpolation, creating SQL injection and correctness risks.
- API routes were action-based (`createTodo`, `getTodo`) and used POST even for reads and deletes.
- Error handling returned raw exception messages as bad requests.
- Tests were tightly coupled to the real SQLite database and depended on shared state and hardcoded IDs.
- All test cases were not covered.

## Architectural Decisions

I refactored the solution into a lightweight layered architecture:

- Controllers: HTTP routing and response mapping.
- Services: application behavior and coordination.
- Repositories: SQLite persistence logic.
- Contracts: request validation and API contract isolation.
- Startup configuration: dependency injection and database initialization.

I chose this design because it improves separation of concerns and testability without introducing unnecessary complexity for a small CRUD API.

## Trade-offs

I kept the solution as a single API project plus a test project rather than splitting it into many assemblies. This keeps the exercise practical while still applying clean boundaries inside the project.

## How to Run

### Prerequisites
- .NET 8 SDK

### Build
```bash
dotnet build
```

### Run
```bash
dotnet run --project TodoApi
```

### Test
```bash
dotnet test
```

## API Documentation

### Create TODO
```http
POST /api/todos
Content-Type: application/json
```

```json
{
  "title": "Buy milk",
  "description": "2 liters",
  "isCompleted": false
}
```

Response: `201 Created`

### Get all TODOs
```http
GET /api/todos
```

Response: `200 OK`

### Get TODO by id
```http
GET /api/todos/1
```

Response: `200 OK` or `404 Not Found`

### Update TODO
```http
PUT /api/todos/1
Content-Type: application/json
```

```json
{
  "title": "Buy milk and bread",
  "description": "From local store",
  "isCompleted": true
}
```

Response: `200 OK` or `404 Not Found`

### Delete TODO
```http
DELETE /api/todos/1
```

Response: `204 No Content` or `404 Not Found`

## Future Improvements

- Add integration tests.
- Add pagination and filtering for large todo lists.
- Add structured logging and centralized exception middleware with `ProblemDetails`.
- Add authentication and user ownership if this becomes a multi-user API.