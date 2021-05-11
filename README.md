﻿# ThemeCreator

An AuroraPatch helper patch primarily used by theme developers to ease the theme creation process.

## Usage

This patch doesn't do anything on its own. It's meant to be used as a dependency of a proper theme patch.
It exposes a handful of helper methods to adjust Aurora colors:

```c#
AddColorChange(Color current, Color swap)
AddColorChange(Type type, ColorChange colorChange)
AddColorChange(string name, ColorChange colorChange)
AddColorChange(Regex regex, ColorChange colorChange)
AddColorChange(Func<Control, bool> predicate, ColorChange colorChange)
AddFontChange(Font font)
AddFontChange(Type type, Font font)
AddFontChange(string name, Font font)
AddFontChange(Regex regex, Font font)
AddFontChange(Func<Control, bool> predicate, Font font)
DrawEllipsePrefixAction(Action<Graphics, Pen> action)
FillEllipsePrefixAction(Action<Graphics, Brush> action)
SetOrbitColor(Color color)
SetPlanetColor(Color color)
SetStarSystemColor(Color color)
SetStarColor(Color color)
SetLagrangePointColor(Color color)
DrawStringPrefixAction(Action<Graphics, string, Font, Brush> action)
DrawLinePrefixAction(Action<Graphics, Pen> action)
SetDistanceRulerLineColor(Color color)
SetCometTailColor(Color color)
SetMapTextColor(Color color)
ApplySolidBrush(Color current, Color swap)
ApplySolidBrush(Action<SolidBrush> action)
ApplyTextureBrush(Action<TextureBrush> action)
ApplyPen(Action<Pen> action)
```

## Example

See the [T2DTheme](https://github.com/Aurora-Modders/T2DTheme) for example usage.
