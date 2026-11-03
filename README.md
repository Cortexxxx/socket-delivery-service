# Socket Delivery Service

A study project: a delivery robot on a grid map, controlled over a custom binary protocol on top of UDP. No HTTP, no frameworks for networking — just sockets, a hand-written packet format and our own reliability layer.

## How it works

Four programs talk through a central server:

- **Server** — holds the map, keeps client sessions, queues orders and tracks their status.
- **Robot** — a simulated robot. Moves according to drive commands, sends its position (telemetry).
- **Operator client** — drives the robot manually (`W`/`A`/`S`/`D`) and sees it on the map.
- **Customer client** — picks a point on the map to create an order and follows its status.

An order goes `PENDING → PICKING_UP → IN_TRANSIT → DELIVERED`: the robot drives from the storage to the customer's point, and the server advances the status when the robot gets within 0.5 m of the target. Delivery itself is driven by the operator, the server does not do any routing.

The map is a text file ([`DeliveryService.Server/map.txt`](DeliveryService.Server/map.txt)): `#` is a building, `*` is the storage, `.` is a free cell. The server sends it to clients as a bitmask.

## What is interesting here

- **Own wire protocol** — 8-byte header, little endian, 1200-byte packet limit. Full spec in [`protocol.md`](protocol.md).
- **Reliability over UDP** — packets with the `NEED_ACK` flag are retransmitted until acknowledged; duplicates are filtered by sequence number. Incoming packet loss (5% by default) is simulated so this is actually exercised.
- **Dead-man switch** — the robot stops if drive commands stop arriving.
- **Attribute-based messages** — a message is a class with `[Message]` and a static `TryParse`, a handler is a method with `[MessageHandler]`. Both are discovered by reflection at startup, so adding a message needs no manual registration.

## Run

Requires the .NET 10 SDK. Start the server first (it listens on UDP port 9000):

```
dotnet run --project DeliveryService.Server
dotnet run --project DeliveryService.Robot
dotnet run --project DeliveryService.Client.GUI       # or DeliveryService.Client.Console
```

The server loads `map.txt` from its working directory, so run it from `DeliveryService.Server/` if it can't find the map.

The robot can connect to another machine: `dotnet run --project DeliveryService.Robot -- <host> <port>`.

Only one operator and one robot can be connected at a time; any number of customers can.

## Tests

```
dotnet test
```

The tests cover message encoding/parsing, the packet codec, the header and the message registry.

## Project layout

| Project                         | Purpose                                              |
|---------------------------------|------------------------------------------------------|
| `DeliveryService.Protocol`      | Shared library: messages, codec, reliability, map    |
| `DeliveryService.Server`        | Server, sessions, orders                             |
| `DeliveryService.Robot`         | Robot simulation                                     |
| `DeliveryService.Client.Core`   | Client logic shared by both UIs                      |
| `DeliveryService.Client.Console`| Console UI                                           |
| `DeliveryService.Client.GUI`    | Avalonia desktop UI                                  |
| `DeliveryService.Tests`         | xUnit tests                                          |
