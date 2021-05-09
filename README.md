# ThemeCreator

An AuroraPatch helper patch primarily used by theme developers to ease the theme creation process.

## Usage

This patch doesn't do anything on its own. It's meant to be used as a dependency of a proper theme patch.
It exposes a handful of helper methods to adjust Aurora colors.

### public static void AddGlobalColorSwap(Color current, Color swap)

A method used to apply color swaps globally across Aurora.
Typical use-case would be swapping out the dreaded blue background color throughout the game.

```c#
ThemeCreator.AddGlobalColorSwap(Color.FromArgb(0, 0, 64), Color.Black); // Blue -> Black.
```

### public static void AddColorChangeByType(Type type, ColorChange colorChange)

A method used to apply background/foreground color changes based on form types.
Useful to modify all Form/ListView/Button/etc colors.

```c#
// All buttons now have a black background and white text.
var colorChange = new ColorChange { BackgroundColor = Color.Black, ForegroundColor = Color.White };
ThemeCreator.AddColorChangeByType(typeof(Button), colorChange);
```

### public static void AddColorChangeByName(string name, ColorChange colorChange)
    
Same as above except that it uses the component name instead of a type to apply color changes.

```c#
// The colony toolbar button now has a black background and white text.
var colorChange = new ColorChange { BackgroundColor = Color.Black, ForegroundColor = Color.White };
ThemeCreator.AddColorChangeByName("cmdToolbarColony", colorChange);
```

### public static void AddColorChangeByNameRegex(string name, ColorChange colorChange)

Applies a color change if the control name matches the regex provided.

```c#
// All toolbar buttons now have a black background and white text.
var colorChange = new ColorChange { BackgroundColor = Color.Black, ForegroundColor = Color.White };
ThemeCreator.AddColorChangeByNameRegex(new Regex("cmdToolbar"), colorChange);
```

## Example

Coming soon.

## Notes

The theme gets applied during the Form.OnShown event.

This means there will be a slight delay between the time a user opens the form initially, and when the color changes actually get applied (a couple seconds on my machine).

## TODO

- Add methods to tweak fonts
- Add methods to tweak non-control colors, such as map components (like planets, orbits, etc).