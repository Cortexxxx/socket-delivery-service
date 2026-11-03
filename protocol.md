# Protocol specification

## General

- Transport: **UDP**
- Byte order: **little endian**
- Types: `u8`, `u16` — unsigned, `i8` — signed, `f32` — float
- Coordinates: 1 map cell = 1 m, origin in the top-left corner
- Max packet size: **1200 bytes**

## Header (8 bytes)

| Offset | Size | Field            | Description                                         |
|-------:|-----:|------------------|-----------------------------------------------------|
| 0      | 2    | `MAGIC`          | `0x52 0x44` (ASCII `RD`)                            |
| 2      | 2    | `SEQ`            | `u16`, own counter per sender, wraps `65535 → 0`    |
| 4      | 1    | `FLAGS`          | bit 0 — `NEED_ACK`: receiver must reply with an ACK |
| 5      | 1    | `MESSAGE_TYPE`   | See [Messages](#messages)                           |
| 6      | 2    | `PAYLOAD_LENGTH` | `u16`, 0–1192                                       |

## Messages

| Code   | Name           | Direction                   | ACK | Payload                                                      |
|--------|----------------|-----------------------------|-----|--------------------------------------------------------------|
| `0x01` | `HELLO`        | client → server             | yes | `ROLE u8`                                                    |
| `0x02` | `HEARTBEAT`    | client → server             | no  | —                                                            |
| `0x03` | `ACK`          | any → any                   | no  | `SEQ u16`                                                    |
| `0x04` | `ERROR`        | server → client             | no  | `CODE u8`                                                    |
| `0x10` | `MAP`          | server → client             | yes | `HEIGHT u8, WIDTH u8, BITMASK, STORAGE_X u8, STORAGE_Y u8`   |
| `0x20` | `DRIVE`        | operator → server → robot   | no  | `SPEED i8, ROTATE i8`                                        |
| `0x30` | `TELEMETRY`    | robot → server → operator, customer | no  | `X f32, Y f32, ANGLE f32`                                    |
| `0x40` | `ORDER_CREATE` | customer → server           | yes | `X u8, Y u8`                                                 |
| `0x41` | `ORDER_STATUS` | server → customer, operator | yes | `STATUS u8, ORDER_ID u16, ENDPOINT_X u8, ENDPOINT_Y u8`      |

Fields follow each other in the listed order, without padding.

- **ACK** — `SEQ` is the seq of the acknowledged packet.
- **MAP** — `BITMASK` is ⌈W × H / 8⌉ bytes, one bit per cell, rows top to bottom, left to right, least significant bit first; `1` = building. `W × H ≤ 9504`.
- **DRIVE**, **TELEMETRY** — clients never talk to each other: the server relays them. The server forwards `DRIVE` only from the operator and `TELEMETRY` only from the robot, and sends telemetry to the customer only while their order is `PICKING_UP` or `IN_TRANSIT`.
- **DRIVE** — both values −100..100, % of max speed / turn rate; negative `ROTATE` = left.
- **TELEMETRY** — position in meters, heading in degrees.
- **HEARTBEAT** — carries nothing; it only keeps the session alive (see [Sessions](#sessions)).
- `u8` coordinates (`X`, `Y`, `STORAGE_*`, `ENDPOINT_*`) are cell indices.

## Sessions

1. A client sends `HELLO` with its role. The server answers with `MAP`, or with `ERROR ROLE_TAKEN` if the operator or robot slot is occupied.
2. Any message before a successful `HELLO` is answered with `ERROR NOT_REGISTERED`.
3. Every packet received from a registered client refreshes its session. A client that is silent for 5 s is dropped, so clients with nothing else to send emit `HEARTBEAT` (every 0.2 s in the reference client).
4. Only the operator may send `DRIVE`, only customers may send `ORDER_CREATE`; otherwise the server replies `ERROR FORBIDDEN`.
5. The robot stops on its own if no `DRIVE` arrives for 300 ms.

## Reliability

- A packet with `NEED_ACK` is resent every 200 ms, up to 5 times, until an `ACK` with its `SEQ` arrives.
- The receiver answers every `NEED_ACK` packet with an `ACK`, even a duplicate, but handles it only once. Duplicates are recognized by sender address and `SEQ`.
- `DRIVE` and `TELEMETRY` have no ACK. A packet with a `SEQ` older than the latest one from the same sender is dropped (comparison with `u16` wrap-around), so late packets never override fresh ones.

## Enums

**Roles:** `0x01` OPERATOR · `0x02` CUSTOMER · `0x03` ROBOT

**Order statuses**

| Value  | Name         | Meaning                            |
|--------|--------------|------------------------------------|
| `0x01` | `PENDING`    | Waiting in the queue               |
| `0x02` | `PICKING_UP` | Robot is heading to the warehouse  |
| `0x03` | `IN_TRANSIT` | Robot is heading to the customer   |
| `0x04` | `DELIVERED`  | Order delivered                    |

**Error codes**

| Value  | Name              | Meaning                                            |
|--------|-------------------|----------------------------------------------------|
| `0x01` | `UNKNOWN_MESSAGE` | Unsupported message type                           |
| `0x02` | `BAD_PAYLOAD`     | Invalid payload length or values                   |
| `0x03` | `NOT_REGISTERED`  | Message sent before HELLO                          |
| `0x04` | `ROLE_TAKEN`      | Operator or robot is already connected             |
| `0x05` | `FORBIDDEN`       | Role is not allowed to send this message           |
| `0x06` | `INVALID_POINT`   | Delivery point is outside the map or in a building |
| `0x07` | `ROBOT_OFFLINE`   | No robot is connected                              |
