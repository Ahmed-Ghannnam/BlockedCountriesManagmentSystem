# Blocked Countries API

A .NET Core Web API to manage blocked countries and validate IP addresses using third-party geolocation APIs (ipapi.co or IPGeolocation.io). The API stores data in-memory and supports permanent and temporary country blocks, IP lookups, and logs blocked IP attempts.

---

## Features

- Add or remove permanently blocked countries
- Temporarily block countries for a specified duration with automatic expiry
- Lookup country information by IP address (supports caller IP auto-detection)
- Check if caller’s IP is blocked based on country
- Paginated, searchable listing of blocked countries and blocked attempts logs
- Thread-safe in-memory storage with `ConcurrentDictionary` and `List`
- Background service for removing expired temporary blocks
- Clean separation of concerns (services, controllers, models)
- Swagger-based API documentation and testing UI

---

## Tech Stack

- [.NET Core 7/8/9 Web API](https://dotnet.microsoft.com/en-us/)
- `HttpClient` with async/await for external API calls
- `ConcurrentDictionary` for thread-safe in-memory data
- [Swagger / Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) for API docs
- Newtonsoft.Json for JSON parsing (optional)

---

## Getting Started

### Prerequisites

- [.NET SDK 7.0 or later](https://dotnet.microsoft.com/en-us/download)
- API key from [ipapi.co](https://ipapi.co/) or [IPGeolocation.io](https://ipgeolocation.io/)

### Setup

1. Clone the repository:

   ```bash
   git clone https://github.com/yourusername/BlockedCountriesAPI.git
   cd BlockedCountriesAPI


2. Add your API key to appsettings.json:

{
  "GeoApi": {
   "BaseUrl": "https://api.ipgeolocation.io/ipgeo",
    "ApiKey": "a6d28a71143541f89d1b8dd5e35732cc"
  }
}

4. Build and run the API:

dotnet build
dotnet run

5. Open Swagger UI in your browser for interactive API docs and testing:

https://localhost:{port}/swagger

6. API Endpoints Overview

Endpoint	Method	Description
/api/countries/block	POST	Block a country permanently
/api/countries/block/{countryCode}	DELETE	Remove a country from the blocked list
/api/countries/blocked	GET	List all blocked countries (supports paging, filtering)
/api/countries/block-range	POST	Block multiple countries at once
/api/countries/temporal-block	POST	Temporarily block a country for a specific duration
/api/ip/lookup	GET	Lookup IP address info (country, ISP, etc.)
/api/ip/check-block	GET	Check if the caller IP is blocked
/api/logs/blocked-attempts	GET	Get paginated list of blocked IP access attempts


7. Pagination and Filtering

Use page and pageSize query parameters for paginated endpoints.
Filter blocked countries by country code search.
Filter blocked attempts logs by IP address or country code.

8. Contributing
Contributions are welcome! Please fork the repo and submit pull requests.
