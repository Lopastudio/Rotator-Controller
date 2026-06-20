# Rotator Interface

A Windows Forms application for controlling an antenna rotator over serial port.

I have received a DIY rotator from a friend (OM1ARH) and I reverse-engineered the Arduino Nano's code to find the commands. They are pretty simple and described below.
One big limitation is that my controller code (this app) assumes that the rotator is pointing north on start of the app.

**The background image is from [RemoteQTH](https://remoteqth.com/xplanet/)'s Antenna Rotator project**

## What it does

Click anywhere on the compass image to rotate the antenna to that azimuth.
The red needle shows the current position, the blue dashed line shows the target.
The rotator is assumed to be at 0 degrees when the program starts.

## Serial commands

| Command | Effect |
|---------|--------|
| `a` | -1° |
| `A` | +1° |
| `b` | -5° |
| `B` | +5° |
| `c` | -10° |
| `C` | +10° |
| `d` | -360° |
| `D` | +360° |

The driver picks the largest step that does not overshoot the target, then waits
for the rotor to physically complete the move before sending the next command.

## Configuration

Open `SerialDriver.cs` and change these fields at the top:

```csharp
public int cas1  = 500;   // milliseconds to complete a ±1° move
public int cas5  = 800;   // milliseconds to complete a ±5° move
public int cas10 = 1200;  // milliseconds to complete a ±10° move
```

Change the port in `Otvor()`:

```csharp
port = new SerialPort("COM17", 9600, ...);
```
