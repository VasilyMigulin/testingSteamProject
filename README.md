# Co-op Physics Prototype — Netcode for GameObjects + Steam

2–4 players host or join, wait in a network lobby, load into a level together and interact with shared physics crates.

**Stack:** Unity 6 (3.25f1), Netcode for GameObjects 2.14, Unity Transport 2.7, VContainer, Steamworks.NET 2025.165.0, Input System
**Steam Transport:**  [SteamNetworkingSocketsTransport](https://github.com/Unity-Technologies/multiplayer-community-contributions/tree/main/Transports/com.community.netcode.transport.steamnetworkingsockets) (MIT), vendored into `Assets/_Project/ThirdParty` with a fix (see *Steam*).

Pure time spent: **40-45 hours**.

## 1. Running 

- **BUILD** uses **Steam** by default. If Steam is not running, the menu says so and the game falls back to **direct IP** (Unity transport). 
- **EDITOR** (open `Bootstrap`) uses **direct IP** by default: Multiplayer Play Mode instances share one Steam account Defaults are set on `RootLifetimeScope`. 

| | Steam | Direct IP |
|---|---|---|
| Host | Session name, max players 2–4, Friends only / Private
→ **Host** | **Host** (port 7777) |
| Join | Overlay invite, **Join Game** in the friends list, or
lobby ID → **Join** | `ip` or `ip:port` → **Join** |

In the lobby the host presses **Start**. Late join into running game is supported. 

**Command line:** `-transport steam|ip`, `-name <name>`,`-port <port>`. Several instances on one PC: `Game.exe -transport ip -name Bob`, Join  `127.0.0.1`. 

**Controls:** WASD / MOUSE / Space - move, look, jump, **F** spawn crate, **E** grab / drop, **LMB** throw, **X** delete held crate, **C** change color, **Tab** player list, **Esc** pause (kick, leave)

**Steam test:** 2 PCs, 2 Steam accounts, Steam running.  `steam_appid.txt` (`480`) must be next to the exe - the build step copies it, the archive includes it. Host - **Invite** - friend accepts. No IP or port forwarding: traffic goes through Steam Datagram Relay. 

**Latency test:**  `Network Simulator` on the NetworkManager, preset `Lag150` (~150 ms RTT, 3 % loss).

--- 

## 2. Architecture 

Each module is an assembly definition, so dependencies are enforced by the compiler.

- **Shared** - ports: `IConnectionService`, `ITransportAdapter`, `IPlayerIdentityProvider`, `IPlayerRegistry`, `ISessionInvites`, `IAchievementService` etc. 
- **Net** - session on NGO: connection, approval, player list, scene loading, kick, ping. 
- **Gameplay** - avatar, input, items and host-side item rules. 
- **UI** - MVP presenters; depends on Shared only (no NGO, no transport)
- **Transport.Utp / Platform.Local / Platform.Steam** - adapters.
- **Bootstrap** - VContainer scopes; picks platform and transport once at startup. 

Game code knows only interfaces in shared, so **gameplay an UI never reference a transport or Steam** Steam was added without changing `ConnectionService`, the lobby, loading or items. 

**Add a transport:** implement `ITransportAdapter` (+ `ISessionInvites`, optionally `IPlayerIdentityProvider`), register it in an installer like `SteamInstaller`, select it in  `RootLifetimeScope`. 
**Add an item type:** a prefab with `NetworkObject` (Dont destroy with owner), `NetworkTransform` (Owner), `NetworkRigidbody` (Auto Update Kinematic State off) and `GrabbableItem`; extra behaviour as additional components. Several types would need a prefab list in `ItemSettings` and a type index in the spawn request. 

---

## 3. Network design 

### Authority 
 
| Entity | Authority | Sync |
|---|---|---|
| Session (players, loading, ping), approval, kick, scenes | Host | `NetworkList`, NGO approval, 
`NetworkSceneManager` |
| Character | Owner | `NetworkTransform` (Owner), interpolated |
| Color | Host, on owner's request | `NetworkVariable<byte>` palette index |
 Free / flying item | Host | physics only on the host |
| Held item | Holder (ownership transferred) | holder moves it to its hand |
| Grab, throw, drop, delete, spawn | Host decides | owner-only RPC → host validation |

### Character - owner-authoritative 
Co-op without PvP: instant response under 150ms matters more than cheat protection, and NGO has no built-in prediction (server authrity with reconciliation would cost days). Remote players are interpolated. Cost: movement is not validated (see *known issues*).

### Items - physics only on the host 
- **One simulation** - a thrown crate flies the same everyone and does not snap on landing. Clients keep items kinematic.
- **Grab:** client sends request and changes nothing local, Host checks: exists, free, hands empty, distance (with latency tolerance) - sets `holderId`, transfers ownership. The holder moves it; `NetworkTransform` replicates.
- **Throw / drop:** holder sends position, rotation, velocity. Host checks it holds *this* item, takes ownership back, starts from the holder position (the host copy lags; starting from it would make the crate jump back), clamps the speed.
- **Race:** requests are processed in oreder; the second fails *is free*. No optimistic grab - nothing to roll back, no desync. 
- **Holder disconnects:** `DontDestroyWithOwner` keeps the crate; the host drops it.
- **Spawn:** position computed by the host, owned by the host, item count limited. 
- RPC are on the player own avatar with `InvokePermission = Owner`: a client cant act for another one.

### Traffic
- Tick rate 30, `NetworkTransform` sends only on change: half precision, thresholds 0.01 m / 1 degree.
- Character: position + **yaw only** (camera pitch never sent). Items: position + rotation, no scale; resting crates send nothing.
- Holder id, color (`byte`), player list - on change only. Ping: once per second, only if changed by 5 ms or more.
- RPC only on player actions, never per frame. Crosshair, hightlight and hints are local.

### Session lifecycle 
- **Approval:** payload `{PlayerId, name, version}` - version - limit - duplicate. The limit counts approved players, so two requests cant take the last slot. The client sees the reason: session full, other version, already in session, timeout, kicked, host left, not found. 
- **PlayerId:** Steam - SteamID; IP - GUID per running instance (mutex slots, so instances on one PC differ). 
- **Single exit path:** button, kick, host leaving, network loss - all end in  `OnClientStopped` - `Finish(reason)` - transport cleanup - menu wuth the reason. Leaving while waiting for Steam cancels the pending operation. 
- **Loading:** host replicates who is still loading; the screen hides for everyone on `LoadEventCompleted`. A client dropping mid-load is removed from the wait by NGO. 
- **Late join:** client syncs the scenes itself (nobody waits), then gets an avatar; crates, holders, colors, and players come from replicated state.
- **Client leaves** (incl Alt+f4) avatar despawned, held crate dropped. **Host leaves:** everyone returns to menu with notice. 
- **Timeouts:** connect 10s, disconnect 10s (by default 30s looked like freeze game), scene load 30s. 

### Steam 
- **P2P via Steam datagram relay:** clients connect to **SteamID of the lobby owner**; no ip, no port forwarding. 
- **Lobby** holds only metadata (name, max players, friends only, private, a game key to filter AppID 480). Join by invite, friends list or lobby id; for the game an invite is just a `Join` started by the platform.
- **Lobby = session:** host mirrors player count and joinable flag. Rejected, kicked or disconnected clients leave the lobby through the same cleanup. The host closes the lobby before leaving (Steam would otherwise hand it to another member).
- **Identity:** steam names, `PlayerId` = SteamID. **Rich presence:** *In menu / In lobby 2/4 / In game 2/4*. 
- **Stats:** the **host confirms** a throw, then **the throwers client** reports it - Steam stats belong to the local user.  `StoreStats` runs on achievement unlock, session end and quit, not per event (rate-limited). Names are placeholders. 
- **Transport fix:** original `GetCurrentRtt` always returned 0 and expected SteamIDs instead of NGO ids. It now uses `GetConnectionRealTimeStatus` - needed for the ping column and NGO time sync. 

---

## 4. Known issues
| Issue | How to fix |
|---|---|
| The thrower sees the crate pause ~1 RTT before it
flies | Local predicted copy blended into host snapshots |
| Movement is not validated | Host-side speed check; 
server authority with prediction for PvP |
| A held crate has no collider (passes through walls) |
Collider on a layer ignoring only the holder |
| Held crate may jitter slightly relative to a remote 
avatar | Parent it to the avatar while held |
| Disconnect is detected after 10 s | Trade-off vs false
disconnects on lag spikes |
| A kicked player can rejoin | Ban list in approval |
| Rich presence uses `status`: the friends list line needs
localization tokens, impossible on AppID 480 | Tokens + `steam_display` on an own AppID |
| Stat and achievement names are placeholders | Define them for an own AppID |
| No host migration (not required) | Hand state and lobby ownership to a successor |