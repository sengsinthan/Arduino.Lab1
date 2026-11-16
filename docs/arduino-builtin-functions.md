# Arduino Built-in Functions Reference

A quick reference to the Arduino core API (the built-in language functions).
Everything here is part of the Arduino core — libraries like `Servo`, `Wire`,
or `LiquidCrystal` are separate.

Official reference: https://docs.arduino.cc/language-reference/

## Digital I/O

| Function | Signature | Purpose |
| --- | --- | --- |
| `pinMode()` | `pinMode(pin, mode)` | Set a pin as `INPUT`, `OUTPUT`, or `INPUT_PULLUP`. |
| `digitalWrite()` | `digitalWrite(pin, value)` | Drive a pin `HIGH` or `LOW`. |
| `digitalRead()` | `digitalRead(pin)` | Read a pin's state; returns `HIGH` or `LOW`. |

```cpp
pinMode(13, OUTPUT);
digitalWrite(13, HIGH);
```

## Analog I/O

| Function | Signature | Purpose |
| --- | --- | --- |
| `analogRead()` | `analogRead(pin)` | Read an analog pin; returns 0–1023 (10-bit). |
| `analogWrite()` | `analogWrite(pin, value)` | Output a PWM signal, value 0–255. |
| `analogReference()` | `analogReference(type)` | Set the reference voltage for `analogRead()`. |

```cpp
int v = analogRead(A0);
analogWrite(9, v / 4);  // scale 0-1023 to 0-255
```

## Timing

| Function | Signature | Purpose |
| --- | --- | --- |
| `delay()` | `delay(ms)` | Pause for a number of milliseconds (blocking). |
| `delayMicroseconds()` | `delayMicroseconds(us)` | Pause for microseconds (blocking). |
| `millis()` | `millis()` | Milliseconds since the sketch started; wraps after ~50 days. |
| `micros()` | `micros()` | Microseconds since start; wraps after ~70 minutes. |

Use `millis()` for non-blocking timing instead of `delay()`:

```cpp
if (millis() - last >= 1000) { last = millis(); /* run every 1s */ }
```

## Math

| Function | Signature | Purpose |
| --- | --- | --- |
| `min()` | `min(a, b)` | Smaller of two values. |
| `max()` | `max(a, b)` | Larger of two values. |
| `abs()` | `abs(x)` | Absolute value. |
| `constrain()` | `constrain(x, low, high)` | Clamp `x` to the range `[low, high]`. |
| `map()` | `map(x, inLow, inHigh, outLow, outHigh)` | Re-scale a value from one range to another. |
| `pow()` | `pow(base, exponent)` | Raise `base` to a power (float). |
| `sqrt()` | `sqrt(x)` | Square root. |

```cpp
int angle = map(analogRead(A0), 0, 1023, 0, 180);
```

## Serial

| Function | Signature | Purpose |
| --- | --- | --- |
| `Serial.begin()` | `Serial.begin(baud)` | Start serial at a baud rate (e.g. 9600). |
| `Serial.print()` | `Serial.print(data)` | Send data without a newline. |
| `Serial.println()` | `Serial.println(data)` | Send data followed by a newline. |
| `Serial.read()` | `Serial.read()` | Read one incoming byte; returns -1 if none. |
| `Serial.available()` | `Serial.available()` | Number of bytes waiting to be read. |
| `Serial.write()` | `Serial.write(data)` | Send raw bytes. |

```cpp
Serial.begin(9600);
if (Serial.available()) { char c = Serial.read(); }
```

## Advanced I/O

| Function | Signature | Purpose |
| --- | --- | --- |
| `tone()` | `tone(pin, freq[, duration])` | Generate a square wave (e.g. for a buzzer). |
| `noTone()` | `noTone(pin)` | Stop a tone started with `tone()`. |
| `pulseIn()` | `pulseIn(pin, value)` | Measure the length of a pulse in microseconds. |
| `shiftOut()` | `shiftOut(dataPin, clockPin, order, value)` | Shift a byte out one bit at a time. |
| `shiftIn()` | `shiftIn(dataPin, clockPin, order)` | Shift a byte in one bit at a time. |

## Interrupts

| Function | Signature | Purpose |
| --- | --- | --- |
| `attachInterrupt()` | `attachInterrupt(digitalPinToInterrupt(pin), isr, mode)` | Run an ISR on a pin event (`RISING`, `FALLING`, `CHANGE`). |
| `detachInterrupt()` | `detachInterrupt(digitalPinToInterrupt(pin))` | Stop an interrupt. |
| `interrupts()` | `interrupts()` | Re-enable interrupts. |
| `noInterrupts()` | `noInterrupts()` | Disable interrupts (for timing-critical code). |

## Sketch structure

Every sketch has these two functions:

| Function | Purpose |
| --- | --- |
| `setup()` | Runs once at power-on/reset. Configure pins, start serial. |
| `loop()` | Runs repeatedly after `setup()`, forever. |

```cpp
void setup() {
  pinMode(13, OUTPUT);
}

void loop() {
  digitalWrite(13, HIGH);
  delay(500);
  digitalWrite(13, LOW);
  delay(500);
}
```
