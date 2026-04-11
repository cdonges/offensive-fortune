# Copilot Instructions for offensive-fortune

## Project Overview

**offensive-fortune** is an ASP.NET Core 10.0 web application that displays random offensive fortunes from Unix fortune data files. It serves fortunes via a web interface and uses a specialized binary format parser for fortune databases.

### Key Architecture

- **Tech Stack**: C# 10+, ASP.NET Core (Razor Pages), .NET 10.0
- **Port**: 6724 (default)
- **Data Source**: Binary fortune database files (`.dat` + content files) in `/usr/share/games/fortunes/off/`
- **Runtime**: Docker (multi-stage build) or direct `dotnet` execution

## Core Components

### Fortune Service (`IFortuneService`, `FortuneService.cs`)

Dependency-injected service for fortune retrieval with two implementation methods:

1. **`GetRandom2(string folderName)`** - Legacy/fallback method
   - Simple text-based parsing (splits on `%` delimiter)
   - Applies ROT13 transform to all strings

2. **`GetRandom(string folderName)`** - Primary method (production-ready)
   - Parses Unix fortune binary format using custom `Header` struct and `FortuneFlags`
   - Uses weighted-random file selection based on file size (larger files = more likely)
   - Handles ROT13 decoding when `STR_ROTATED` flag is set
   - Binary format: 24-byte header (5 uint32s + char) + pointer offsets + string data
   - String delimiter from header determines where fortune ends

**Thread Safety**: Both methods use instance-level `SemaphoreSlim` for thread-safe access to cached `fortunes` array (lazy-loaded on first request).

### Service Registration

In `Program.cs`:
```csharp
builder.Services.AddSingleton<IFortuneService, FortuneService>();
```

Injected in Razor pages via `@inject IFortuneService FortuneService`

### Data Models

- **`Header`**: Binary structure mapping Unix fortune `.dat` file headers (big-endian)
- **`FortuneFlags`**: Enum for header flags (RANDOM, ORDERED, ROTATED)
- **`IEnumerableExtensions.RandomElementByWeight<T>()`**: Custom weighted random selection (from StackOverflow CC BY-SA)

### Web Interface (`Pages/Index.cshtml`)

- Injects `IFortuneService` via Razor's `@inject` directive
- Calls `GetRandom2()` to retrieve fortunes (consider switching to `GetRandom()` for better binary format support)
- Replaces `\n` with `<br>` for HTML rendering
- Dark theme styling (black background, white monospace text)
- Raw HTML output via `@Html.Raw()` (offensive content requires careful escaping considerations)

### Request Logging (`RequestLoggingMiddleware`, `ApplicationDbContext`, `RequestLog`)

All HTTP requests are logged to SQLite database (`requests.db`) with:
- **IP Address**: Client's remote IP address
- **Timestamp**: UTC time of request (stored as ISO 8601 string)
- **HTTP Method**: GET, POST, etc.
- **Path**: Request path (e.g., `/`, `/about`)

**Database Schema**: `RequestLogs` table with Id, IpAddress, Timestamp, Path, Method

**Middleware**: Runs early in the pipeline (after routing) to capture all requests before handler execution

**Database Context**: `ApplicationDbContext` uses Entity Framework Core with SQLite; database is auto-created on first run via `EnsureCreated()`

## Build & Run

### Local Development
```bash
dotnet build
dotnet run
# Accessible at https://localhost:5001 (HTTPS redirect) or HTTP as configured
# SQLite database (data/requests.db) created automatically on first run
```

### Docker
```bash
# Build image
docker build -t offensive-fortunes .

# Run with docker-compose
docker-compose up -d
# Accessible at http://localhost:6724
```

**Volume Requirement**: Mount fortune data at `/usr/share/games/fortunes/off/` (contains `.dat` + content files)

**Database Note**: SQLite database (`data/requests.db`) persists in container's working directory; volume `offensive-fortune-db` mounts to `/App/data` for persistence across container restarts

## Project Conventions

### Threading & Concurrency
- Use `SemaphoreSlim` for request-safe state synchronization within service instances
- Fortune cache (`fortunes` array) is instance-level and protected by semaphore per service instance
- Service is registered as `Singleton` in DI container (shared across all requests)
- `Random.Shared` for thread-safe randomization

### Numeric Handling
- Fortune binary format uses **big-endian** uint32 for all header values (requires `BinaryPrimitives.ReadUInt32BigEndian()`)
- File offsets are 32-bit unsigned integers

### Text Encoding
- Fortune files use ASCII encoding (as per Unix fortune tradition)
- ROT13 applied only to letters (a-z, A-Z); digits/symbols unchanged
- Content is HTML-escaped implicitly via Razor templates, except `@Html.Raw()` calls

## Important Implementation Notes

1. **`GetRandom2()` vs `GetRandom()`**: Index.cshtml uses `GetRandom2()` (simpler) but codebase supports optimized `GetRandom()` which properly respects binary format—consider alignment on which method to use

2. **File Size Weighting**: Larger fortune databases are selected more often (weighted random by file size in bytes)—design choice for better fortune distribution

3. **Header Padding**: After reading 5 uint32s, 1 char, the code reads 3 padding bytes to align with Unix format (24-byte header total)

4. **Nullable/ImplicitUsings**: Project enables `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>` (C# modern style)

## Common Tasks

**Add a new feature**: Update `Program.cs` (middleware), add handlers in `Pages/`, update services in `builder.Services`

**Change fortune data source**: Modify folder path in `Index.cshtml` call to `GetRandom()` or `GetRandom2()`

**Debugging fortune parsing**: Use `GetRandom()` to inspect binary header reads; validate `Header` struct matches Unix format spec

**Container deployment**: Ensure fortune files are mounted as volume, port 6724 is exposed, and `ASPNETCORE_HTTP_PORTS=6724` is set
