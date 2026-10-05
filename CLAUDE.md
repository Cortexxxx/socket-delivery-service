# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

A UDP-based delivery-robot simulation system. `protocol.md` at the repo root is the
authoritative wire protocol spec (packet header layout, message catalog, enums, field
semantics) — read it before touching anything in `DeliveryService.Protocol`. The protocol
implementation must match that document exactly; if they ever disagree, the spec wins and
the code is the bug.

The solution (`Solution1/DeliveryService.sln`) has five projects:

- **DeliveryService.Protocol** — wire format: packet header parsing/serialization, message
  types, enums. No networking code; pure byte<->object marshaling.
- **DeliveryService.Server** — authoritative server (console app, currently a stub).
- **DeliveryService.Client** — operator/customer-facing client (console app, currently a stub).
- **DeliveryService.Robot** — robot-side agent (console app, currently a stub).
- **DeliveryService.Tests** — xUnit tests for the Protocol project.

## Commands

All commands run from `Solution1/` (or pass `Solution1/DeliveryService.sln` explicitly).

```
dotnet build                                           # build everything
dotnet test                                             # run all tests
dotnet test --filter "FullyQualifiedName~DriveMessage"  # run one test class
dotnet test --filter "DisplayName~Good_DrivePayloadSerialize"  # run one test method
```

Target framework is `net10.0`, `LangVersion` 12, nullable enabled.

## Protocol architecture (DeliveryService.Protocol)

Every UDP datagram is an 8-byte `PacketHeader` (`PacketHeader.cs`) followed by a payload of
0–1192 bytes. `PacketHeader.TryParse`/`TrySerialize` validate the magic bytes (`RD`), enforce
`payload length == packet length - header size`, and read/write all header fields in little
endian via `System.Buffers.Binary.BinaryPrimitives`. Layout offsets and limits live in
`Constants/HeaderConstants.cs` — never hardcode offsets elsewhere.

Each message type (`Messages/*.cs`, e.g. `DriveMessage`, `AckMessage`, `HelloMessage`,
`HeartbeatMessage`, `StopMessage`) implements `IMessage` and follows the same shape:

- A static `TryParse(ReadOnlySpan<byte> payload, out TMessage? message)` that validates
  payload length and field ranges before constructing the message, returning `false` on any
  invalid input (never throws on bad input).
- An instance `TrySerialize(Span<byte> buffer, out int written)` that re-validates the same
  invariants before writing (serialization is not assumed to be called only on valid state).
- A `[Message(MessageType.X, Flags = PacketFlags.Y)]` class attribute (`MessageAttribute.cs`)
  declaring which `MessageType` enum value and ACK requirement the message corresponds to —
  this must stay consistent with the ACK column in `protocol.md`.
- Fixed per-message payload sizes and limits (e.g. `DriveMaxValue`, `AckPayloadSize`) are
  centralized in `Constants/MessagesConstants.cs`, not inlined in the message classes.

`Enums/` mirrors the protocol's wire-level enums one-to-one (`MessageType`, `PacketFlags`,
`Role`, `OrderStatus`, `ErrorCode`) — their numeric values must match `protocol.md` exactly
since they're serialized as raw bytes.

When adding a new message type: add the `MessageType` value if missing, add payload size/limit
constants to `MessagesConstants`, create the message class under `Messages/` following the
`TryParse`/`TrySerialize` pattern above, and add corresponding tests in `DeliveryService.Tests`
(see `DriveMessageTests.cs`/`AckMessageTests.cs` for the expected test shape: good-parse cases
via `[Theory]`/`InlineData`, bad-parse/invalid-range cases, wrong-payload-length case, and a
serialize-roundtrip case).
