# TaskFlow – Team Productivity Platform

Full-stack task management and analytics platform.

## Quick Start (No Setup Required)

**No PostgreSQL or Redis needed.** Uses SQLite and in-memory cache.

> 📄 **Детални инструкции:** види [START.md](START.md)

### Option 1: Run everything (два терминали)

**Терминал 1 – Backend:**
```powershell
cd "C:\Users\anaza\OneDrive\Десктоп\TaskFlow\TaskFlow.API"
dotnet run
```

**Терминал 2 – Frontend:**
```powershell
cd "C:\Users\anaza\OneDrive\Десктоп\TaskFlow\taskflow-frontend"
npm install
npm run dev
```

Потоа отвори **http://localhost:5173**

### Option 2: Windows batch file

```bash
run.bat
```

Then open **http://localhost:5173** in your browser.

---

## What You'll See

- **http://localhost:5173** – React app (Login, Dashboard, Projects)
- **http://localhost:5000** – API
- **http://localhost:5000/swagger** – API documentation

### First Steps

1. Open http://localhost:5173
2. Click **Register** tab, create an account
3. Create a project on the Dashboard
4. Click a project to add tasks

---

## Tech Stack

- **Backend:** ASP.NET Core (.NET 8)
- **Frontend:** React + TypeScript (Vite)
- **Database:** SQLite (default) / PostgreSQL
- **Auth:** JWT
- **Cache:** In-memory (default) / Redis
- **Features:** SignalR, Hangfire, Serilog

---

## Project Structure

```
TaskFlow/
├── TaskFlow.API/           # Backend
│   ├── Controllers/
│   ├── Models/
│   ├── Services/
│   └── Data/
├── taskflow-frontend/       # React app
├── run.bat                 # Run both (Windows)
└── run.ps1                 # Run both (PowerShell)
```

---

## Docker (PostgreSQL + Redis)

```bash
docker compose up
```

- Frontend: http://localhost
- API: http://localhost:5000

---

## Configuration

Edit `TaskFlow.API/appsettings.json`:

| Setting | Default | Description |
|---------|---------|-------------|
| ConnectionStrings:DefaultConnection | Data Source=taskflow.db | SQLite (default) or PostgreSQL |
| Redis:ConnectionString | (empty) | Redis URL; empty = in-memory cache |
