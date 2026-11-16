# Arduino Concepts — Q&A Recap

A plain-language summary of key concepts for this project (a C# WPF app that
controls an Arduino over serial). Written so anyone new to the project can get
the mental model quickly.

See also: [`arduino-builtin-functions.md`](arduino-builtin-functions.md) for the
core function reference.

---

## How do you know all the Arduino built-in functions?

The Arduino core API is **small and stable** — the whole "language" is a thin
C/C++ wrapper. The authoritative source is the official reference, not memory:

- https://docs.arduino.cc/language-reference/

Everything in the core fits on one page (digital I/O, analog I/O, timing, math,
serial, advanced I/O, interrupts, and the `setup()`/`loop()` structure).
Anything beyond that (`Servo`, `Wire`, `LiquidCrystal`, …) is a **library**, not
a built-in.

---

## What does the serial `loop()` do?

The `loop()` in the firmware does **one job**: read serial input and assemble it
into complete command lines, then hand each finished line to `handleCommand()`.

```cpp
void loop() {
  while (Serial.available() > 0) {
    char c = (char)Serial.read();
    if (c == '\n' || c == '\r') {
      if (inputLine.length() > 0) {
        handleCommand(inputLine);
        inputLine = "";
      }
    } else {
      inputLine += c;
    }
  }
}
```

- Keeps reading while bytes are waiting in the serial buffer.
- Appends each character to `inputLine`.
- On a newline/carriage return, the line is complete → run `handleCommand()` and
  reset the buffer.
- It is **non-blocking** — it never pauses the board, just accumulates input.

**Important:** commands must be terminated with a newline (`\n` or `\r`), or
`handleCommand()` is never called.

---

## How do you control or read other devices connected to the Arduino?

Devices fall into a few categories, each with a standard approach:

**1. Simple on/off & analog (direct pins)**
- Outputs (LED, relay, buzzer): `digitalWrite()`, `analogWrite()` (PWM).
- Inputs (button, switch, PIR): `digitalRead()`.
- Analog sensors (pot, LDR, TMP36): `analogRead()` → 0–1023.

**2. "Smart" devices on a bus protocol** (use a library, not raw signals)

| Protocol | Wires | Typical devices | Core library |
| --- | --- | --- | --- |
| I²C | 2 (SDA, SCL) | OLED, BME280, RTC, MPU6050 | `Wire.h` |
| SPI | 4 | SD card, TFT display | `SPI.h` |
| 1-Wire / custom | 1 | DS18B20, DHT22 | `OneWire.h`, `DHT.h` |

**3. Another computer / app** — the serial route this project uses: the WPF app
sends commands over USB, and `handleCommand()` acts on them.

---

## Do most devices already have a library?

Yes. For almost any mildly popular device, a library already exists.

- Popular sensors, displays, modules, and vendor parts (Adafruit, SparkFun,
  DFRobot) nearly always ship a library.
- Install via **Arduino IDE → Tools → Manage Libraries** (Library Manager);
  search by part name.
- Example sketches ship with libraries: **File → Examples**.
- Only obscure, brand-new, or self-built parts need manual work — then you use
  the base protocol libraries (`Wire`, `SPI`, `Serial`) plus the datasheet.
- Caution: library quality varies. Prefer actively maintained ones with examples.

---

## Can you write Arduino firmware in only C#?

**No — not for a classic Arduino (Uno, Nano, Mega).** The on-board firmware must
be C/C++.

- Classic Arduinos use an **AVR microcontroller** (ATmega328, ~2 KB RAM). C#
  needs the .NET runtime, which is far too big to fit.
- The correct architecture (what this project uses) keeps the layers split:
  - **PC:** C# (WPF app) sends commands.
  - **Arduino:** C/C++ firmware receives them via `Serial`.

If you specifically want C# **on the device**, use a more powerful board:

| Option | What it is |
| --- | --- |
| .NET nanoFramework | Runs C# on ESP32, STM32, etc. |
| Meadow (Wilderness Labs) | Board designed to run full .NET C#. |
| Raspberry Pi (not Pico) | Full Linux + .NET, but it's a computer, not an MCU. |

---

## The overall model

**Languages**
- External side (PC): **any language** that can speak the COM/serial protocol —
  C#, Python, Node.js, Java, etc.
- On-board (classic Arduino): **C++ only**.

**Protocols**
- PC ↔ Arduino: **UART/serial over USB** (handled by `Serial`).
- Arduino ↔ peripherals: **I²C / SPI / 1-Wire** (handled by `Wire.h`, `SPI.h`, …).

So "any language on the PC side, C++ on the board side" is correct — and the
Arduino↔peripheral side is *also* a protocol; you're simply writing it in C++.
