# SignalR Message Routing Visualizer

Educational demo that shows **who receives each SignalR message** when you use different `Clients.*` targeting APIs. This is **not** a chat app — it is a routing visualizer for learning.

## Stack

| Layer | Tech |
|-------|------|
| Backend | ASP.NET Core 8, SignalR `NotificationHub`, in-memory connection manager |
| Frontend | Next.js 14, React, TypeScript, Tailwind CSS, `@microsoft/signalr` |
| Presentation | `presentation/Understanding_SignalR_Message_Routing.pptx` |

## Demo users & groups

| User | Groups |
|------|--------|
| Ahmad | Developers Team, DevOps Team |
| Maen | Quality Team |
| Mostafa | Developers Team |

## Run locally

### 1. Backend (http://localhost:5002)

```bash
cd backend
dotnet run --launch-profile http
```

### 2. Frontend (http://localhost:3001)

```bash
cd frontend
npm install
npm run dev
```

Open http://localhost:3001 — pick a demo user, click **Connect**, then try each routing method. Open a second browser as another user to compare `Clients.Client` vs `Clients.User`.

> Port **3000** is often reserved by Windows (Hyper-V). This app uses **3001** instead.

## Hub methods

| UI button | Hub method | SignalR API |
|-----------|------------|-------------|
| Clients.All | `SendToAll` | `Clients.All` |
| Clients.Caller | `SendToCaller` | `Clients.Caller` |
| Clients.Others | `SendToOthers` | `Clients.Others` |
| Clients.Client | `SendToConnection` | `Clients.Client(id)` |
| Clients.Clients | `SendToConnections` | `Clients.Clients(ids)` |
| Clients.User | `SendToUser` | `Clients.User(userId)` |
| Clients.Users | `SendToUsers` | `Clients.Users(ids)` |
| Clients.Group | `SendToGroup` | `Clients.Group(name)` |
| Clients.Groups | `SendToGroups` | `Clients.Groups(names)` |
| Clients.GroupExcept | `SendToGroupExcept` | `Clients.GroupExcept(name, ids)` |

## REST API

- `GET /api/users`
- `GET /api/groups`
- `GET /api/connections`
- Hub: `/hubs/notifications?userId=ahmad&browser=Chrome`

## Key learning points

1. **User ≠ Connection** — one user can have many ConnectionIds (tabs/browsers).
2. **`Clients.Client`** targets one socket; **`Clients.User`** targets all sockets for that user.
3. **Groups** are server-side labels joined via `Groups.AddToGroupAsync` (done automatically on connect in this demo).
4. Network path: **HTTP negotiate → WebSocket → Hub protocol → Clients.* routing**.

See the in-app page **How SignalR Works** and the PowerPoint deck for diagrams and protocol details.
