# TaskFlow – Како да ја стартуваш апликацијата

## Важно: Два терминали

Треба да отвориш **два терминали** – еден за backend, еден за frontend.

---

## Чекор 1: Backend (API)

Отвори **PowerShell** или **Command Prompt** и изврши:

```powershell
cd "C:\Users\anaza\OneDrive\Десктоп\TaskFlow\TaskFlow.API"
dotnet run
```

Почекај додека не видиш:
```
Now listening on: http://localhost:5000
```

**Затвори го** со `Ctrl+C` кога ќе сакаш да го сопреш.

---

## Чекор 2: Frontend (React)

Отвори **нов терминал** (PowerShell или CMD) и изврши:

```powershell
cd "C:\Users\anaza\OneDrive\Десктоп\TaskFlow\taskflow-frontend"
npm install
npm run dev
```

Почекај додека не видиш:
```
  ➜  Local:   http://localhost:5173/
```

---

## Чекор 3: Отвори во прелистувач

Отвори: **http://localhost:5173**

---

## Ако build не успее

Ако видиш грешка „file is locked“ или „process is being used“:

1. Затвори ги сите терминали каде што работи TaskFlow
2. Или изврши:
   ```powershell
   taskkill /F /IM TaskFlow.API.exe
   ```
3. Потоа повторно: `dotnet run`

---

## Резиме на команди

| Што | Команда |
|-----|---------|
| Backend | `cd TaskFlow.API` → `dotnet run` |
| Frontend | `cd taskflow-frontend` → `npm install` → `npm run dev` |
| Сопирање | `Ctrl+C` во терминалот |
