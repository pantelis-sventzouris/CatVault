# CatVault.API

A .NET 8 Web API that fetches cat images from The Cat API, stores them in a SQL Server database (including binary images and associated temperament tags), and exposes CRUD and background-job endpoints via Hangfire.

## Prerequisites

* [.NET 8 SDK](https://dotnet.microsoft.com/download)
* [SQL Server Express](https://www.microsoft.com/en-us/sql-server) (local or remote)
* [Docker](https://www.docker.com/) (optional, for containerized deployment)
* [Git](https://github.com/pantelis-sventzouris/CatVault.git) (to clone the repo)

## Getting Started

1. **Clone the repository**

   ```bash
   git clone https://github.com/pantelis-sventzouris/CatVault.git
   cd CatVault
   ```

2. **Configure the database**

   * Open `appsettings.json` and set your SQL Server connection string:

     ```json
     "ConnectionStrings": {
       "DefaultConnection": "Server=.\\SQLEXPRESS;Database=CatsDb;Integrated Security=true;Encrypt=true;TrustServerCertificate=true;"
     }
     ```

3. **Run EF Core Migrations**

   Ensure the EF Core CLI is installed:

   ```bash
   dotnet tool install --global dotnet-ef
   ```

   Apply the initial migration to create database and tables:

   ```bash
   dotnet ef database update
   ```

   > **Auto-migrate on startup** The app includes code to auto-apply pending migrations on launch. If you prefer manual migration only, remove or comment out the `db.Database.Migrate()` block in `Program.cs`.

4. **Build and run the API**

   ```bash
   dotnet build
   dotnet run
   ```

   The API listens on `http://localhost:5000` (or `https://localhost:5001`).

5. **Swagger & Hangfire Dashboard**

   * Swagger UI: `http://localhost:5000/swagger`
   * Hangfire Dashboard: `http://localhost:5000/hangfire`

## Endpoints Overview

* **POST** `/api/cats/fetch`

  * Enqueue a background job to fetch and store 25 unique cat images.
  * Returns a JSON payload with the Hangfire `jobId`.

* **GET** `/api/cats/{id}`

  * Retrieve a single cat by its database ID, including dimensions, tags, and image data (Base64).

* **GET** `/api/cats?tag={tag}&page={n}&pageSize={m}`

  * Retrieve a paged list of cats, optionally filtered by temperament tag.

* **GET** `/api/jobs/{jobId}`

  * Check the status of a Hangfire background job (states: Enqueued, Processing, Succeeded, Failed, etc.).

* **(Optional)** **GET** `/api/cats/{id}/image`

  * Returns raw binary image (`image/jpeg`) for the cat.

## Unit Tests

A separate xUnit test project validates core logic.

```bash
cd CatVault.Tests
dotnet test
```

### Test Coverage

* **CatFetchServiceTests**: tests the FetchService.
* **CatsControllerTests**: tests the Cats Controller and its methods.
* **JobsControllerTests**: tests the Jobs controller to get the status by jobId.

## Docker Support

A multi-stage Dockerfile is provided for containerizing the API:

1. **Build the image**

   ```bash
   docker build -t catvault-api .
   ```

2. **Run the container**

   ```bash
   docker run -d \
     -e ConnectionStrings__DefaultConnection="Server=YOUR_SERVER;Database=CatVault;User Id=USERNAME;Password=PASSWORD;" \
     -p 80:80 \
     --name catvault-api \
     catvault-api
   ```

   The API will be available at `http://localhost` and the Hangfire dashboard at `http://localhost/hangfire`.

## Logging

Serilog is used for logging and is also configured via `appsettings.json` to write to console and rolling-file (`Logs/log-.txt`).

## Further Improvements
* Add authentication/authorization (JWT, OAuth).
* Enhance unit-tests coverage (error scenarios, large paging, API error injection).

