# Protocol specification

## General

- Transport: **UDP**
- Byte order: **little endian**
- Types: `u8`, `u16` — unsigned, `i8` — signed, `f32` — IEEE 754 float
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
| `0x20` | `DRIVE`        | operator → robot            | no  | `SPEED i8, ROTATE i8`                                        |
| `0x21` | `STOP`         | operator → robot            | yes | —                                                            |
| `0x30` | `TELEMETRY`    | robot → operator, customer  | no  | `X f32, Y f32, ANGLE f32`                                    |
| `0x40` | `ORDER_CREATE` | customer → server           | yes | `X u8, Y u8`                                                 |
| `0x41` | `ORDER_STATUS` | server → customer, operator | yes | `STATUS u8, ORDER_ID u16, ENDPOINT_X u8, ENDPOINT_Y u8`      |

Fields follow each other in the listed order, without padding.

- **ACK** — `SEQ` is the seq of the acknowledged packet.
- **MAP** — `BITMASK` is ⌈W × H / 8⌉ bytes, one bit per cell, rows top to bottom, left to right, least significant bit first; `1` = building. `W × H ≤ 9504`.
- **DRIVE** — both values −100..100, % of max speed / turn rate; negative `ROTATE` = left.
- **TELEMETRY** — position in meters, heading in degrees.
- `u8` coordinates (`X`, `Y`, `STORAGE_*`, `ENDPOINT_*`) are cell indices.

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