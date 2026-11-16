/*
 * Lab1 - minimal learning sketch
 *
 * Talks to the C# WPF app over the USB serial port at 9600 baud.
 * Reads newline-terminated text commands and replies with one line each.
 *
 * Commands (sent by the app):
 *   PING     -> replies: PONG                 (connection test)
 *   LED ON   -> turns the onboard LED on;  replies: OK LED=ON
 *   LED OFF  -> turns the onboard LED off; replies: OK LED=OFF
 *
 * Anything else -> replies: ERR <the command>
 */

const unsigned long BAUD_RATE = 9600;   // must match the C# app

String inputLine;   // collects characters until we see a newline

void setup() 
{
  Serial.begin(BAUD_RATE);
  pinMode(LED_BUILTIN, OUTPUT);
  digitalWrite(LED_BUILTIN, LOW);
  Serial.println("BOOT ready");   // the app sees this as soon as it connects
}

void loop() 
{
  // Build up a line one character at a time; act on it at the newline.
  while (Serial.available() > 0) 
  {
    char c = (char)Serial.read();
    if (c == '\n' || c == '\r') 
    {
      if (inputLine.length() > 0) 
      {
        handleCommand(inputLine);
        inputLine = "";
      }
    } else {
      inputLine += c;
    }
  }
}

void handleCommand(String cmd) {
  cmd.trim();   // drop stray spaces

  if (cmd.equalsIgnoreCase("PING")) 
  {
    Serial.println("PONG");
  } 
  else if (cmd.equalsIgnoreCase("LED ON")) 
  {
    digitalWrite(LED_BUILTIN, HIGH);
    Serial.println("OK LED=ON");
  }
  else if (cmd.equalsIgnoreCase("LED OFF")) 
  {
    digitalWrite(LED_BUILTIN, LOW);
    Serial.println("OK LED=OFF");
  }
  else 
  {
    Serial.print("ERR ");
    Serial.println(cmd);
  }
}
