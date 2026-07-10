# PodcastPlatform

PodcastPlatform is a .NET 10 ASP.NET Core Web API for managing a podcast streaming platform. It supports user authentication, podcast and episode management, playlists, subscriptions, and podcast ratings.

## Features

- JWT-based authentication with ASP.NET Core Identity
- Podcast CRUD operations
- Episode CRUD operations
- Playlist creation and episode management
- Subscribe / unsubscribe to podcasts
- Podcast ratings and rating statistics
- Cloudinary-backed media storage
- SQL Server database persistence with Entity Framework Core
- API documentation via Scalar / OpenAPI in development

## Tech Stack

- **.NET:** 10.0
- **API:** ASP.NET Core Web API
- **Database:** SQL Server
- **ORM:** Entity Framework Core
- **Auth:** ASP.NET Core Identity + JWT Bearer tokens
- **Media:** Cloudinary
- **Docs:** Scalar / Swagger-compatible OpenAPI

## Prerequisites

Before running the project locally, make sure you have:

- .NET 10 SDK
- SQL Server running locally or a reachable SQL Server instance
- Cloudinary credentials if you plan to upload media

## Configuration

The main configuration is stored in `PodcastPlatform/appsettings.json`.

Required settings:

- `ConnectionStrings:DefaultConnection`
- `Jwt:Key`
- `Jwt:Issuer`
- `Jwt:Audience`
- `Jwt:ExpirationDays`
- `Cloudinary:CloudName`
- `Cloudinary:ApiKey`
- `Cloudinary:ApiSecret`

> Tip: do not commit real secrets to source control. Replace any development-only values with environment-specific configuration or user secrets.

## Running Locally

From the repository root:

```bash
cd PodcastPlatform

dotnet restore

dotnet ef database update

dotnet run
```

The app is configured to listen on:

- `https://localhost:7098`
- `http://localhost:5024`

In Development, the API documentation is available at:

- `https://localhost:7098/scalar/v1`
- `http://localhost:5024/scalar/v1`

The root path also redirects to Scalar in Development.

## Main API Endpoints

### Auth

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/auth/profile`
- `POST /api/auth/logout`

### Podcasts

- `GET /api/podcasts`
- `GET /api/podcasts/{id}`
- `GET /api/podcasts/owner/{ownerId}`
- `POST /api/podcasts`
- `PATCH /api/podcasts/{id}`
- `DELETE /api/podcasts/{id}`
- `GET /api/podcasts/{id}/subscribers`

### Episodes

- `GET /api/episodes`
- `GET /api/episodes/{id}`
- `GET /api/episodes/podcast/{podcastId}`
- `GET /api/episodes/playlist/{playlistId}`
- `POST /api/episodes`
- `PATCH /api/episodes/{id}`
- `DELETE /api/episodes/{id}`

### Playlists

- `GET /api/playlists`
- `GET /api/playlists/{id}`
- `GET /api/playlists/user/{userId}`
- `POST /api/playlists`
- `PUT /api/playlists/{id}`
- `DELETE /api/playlists/{id}`
- `POST /api/playlists/{playlistId}/episodes/{episodeId}`
- `DELETE /api/playlists/{playlistId}/episodes/{episodeId}`

### Subscriptions

- `GET /api/subscriptions/my-subscriptions`
- `GET /api/subscriptions/podcast/{podcastId}/subscribers`
- `GET /api/subscriptions/is-subscribed/{podcastId}`
- `POST /api/subscriptions/subscribe/{podcastId}`
- `DELETE /api/subscriptions/unsubscribe/{podcastId}`

### Ratings

- `GET /api/ratings/podcast/{podcastId}`
- `GET /api/ratings/podcast/{podcastId}/stats`
- `GET /api/ratings/my-rating/{podcastId}`
- `POST /api/ratings`
- `PATCH /api/ratings/{podcastId}`
- `DELETE /api/ratings/{id}`

## Project Structure

- `Controllers/` - API endpoints
- `Data/` - EF Core `AppDbContext` and migrations
- `DTOs/` - request and response models
- `Models/Entities/` - domain entities
- `Repositories/` - data access layer
- `Services/` - business logic and integrations
- `Properties/launchSettings.json` - local launch profiles

## Notes

- Authentication uses JWT bearer tokens.
- Some endpoints require authorization via the `Authorization: Bearer <token>` header.
- New users get default playlists created automatically during registration.
- CORS is currently configured to allow all origins in development.

## License

No license has been specified yet.

