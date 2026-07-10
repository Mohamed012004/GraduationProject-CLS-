# Podcast Platform Frontend

React frontend for the ASP.NET Core API in `../backend`.

## Run locally

Start the backend first:

```bash
cd ../backend/PodcastPlatform
dotnet run
```

Then start the frontend:

```bash
npm install
npm run dev
```

Open `http://localhost:5173`.

By default Vite proxies `/api` to `https://localhost:7098` with certificate verification disabled inside the dev proxy. To point at another backend URL, create `.env` from `.env.example` and set `VITE_API_BASE_URL`.
