# ThemeCreator

An AuroraPatch helper patch primarily used by theme developers to ease the theme creation process.

## Usage

This patch doesn't do anything on its own. It's meant to be used as a dependency of a proper theme patch.
It exposes a handful of helper methods to adjust Aurora colors.

### public static void AddGlobalColorSwap(Color current, Color swap)

A method used to apply color swaps globally across Aurora.
Typical use-case would be swapping out the dreaded blue background color throughout the game.

```c#
ThemeCreator.ThemeCreator.AddGlobalColorSwap(Color.FromArgb(0, 0, 64), Color.Black); // Blue -> Black.
```

### public static void AddColorChangeByType(Type type, ColorChange colorChange)

A method used to apply background/foreground color changes based on form types.
Useful to modify all Form/ListView/Button/etc colors.

```c#
// All buttons now have a black background and white text.
var colorChange = new ThemeCreator.ColorChange { BackgroundColor = Color.Black, ForegroundColor = Color.White };
ThemeCreator.ThemeCreator.AddColorChangeByType(typeof(Button), colorChange);
```

### public static void AddColorChangeByName(string name, ColorChange colorChange)
    
Same as above except that it uses the component name instead of a type to apply color changes.

```c#
// The colony toolbar button now has a black background and white text.
var colorChange = new ThemeCreator.ColorChange { BackgroundColor = Color.Black, ForegroundColor = Color.White };
ThemeCreator.ThemeCreator.AddColorChangeByName("cmdToolbarColony", colorChange);
```

### public static void AddColorChangeByNameRegex(string name, ColorChange colorChange)

Applies a color change if the control name matches the regex provided.

```c#
// All toolbar buttons now have a black background and white text.
var colorChange = new Themecreator.ColorChange { BackgroundColor = Color.Black, ForegroundColor = Color.White };
ThemeCreator.ThemeCreator.AddColorChangeByNameRegex(new Regex("cmdToolbar"), colorChange);
```

### public static void SetGlobalFont(Font font)

A method used to override the global font across Aurora.

```c#
ThemeCreator.ThemeCreator.SetGlobalFont(new Font(FontFamily.GenericSansSerif, 12f));
```

### public static void AddFontChangeByType(Type type, Font font)

A method used to apply font changes based on form types.
Useful to modify all Form/ListView/Button/etc fonts.

```c#
// All ListView controls now have a tiny monospace font.
ThemeCreator.ThemeCreator.AddFontChangeByType(typeof(ListView), new Font(FontFamily.GenericMonospace, 7f));
```

### public static void AddFontChangeByName(string name, Font font)

Same as above except that it uses the component name instead of a type to apply font changes.

```c#
// The increment 30 days button button now has a large font size.
ThemeCreator.ThemeCreator.AddFontChangeByName("cmdIncrement30D", new Font(FontFamily.GenericSans, 20f));
```

### public static void AddFontChangeByNameRegex(string name, Font font)

Applies a font change if the control name matches the regex provided.

```c#
// All toolbar buttons now have a tiny font size.
ThemeCreator.ThemeCreator.AddFontChangeByNameRegex(new Regex("cmdIncrement"), new Font(FontFamily.GenericMonospace, 7f));
```

## Full Theme Patch Example

Working example of a particularly bad theme.

```c#
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using HarmonyLib;

namespace ExampleTheme
{
    public class ExampleTheme : AuroraPatch.Patch
    {
        public override string Description => "Example theme using ThemeCreator patch.";
        public override IEnumerable<string> Dependencies => new[] { "ThemeCreator" };

        protected override void Loaded(Harmony harmony)
        {
            // Larger global font size.
            ThemeCreator.ThemeCreator.SetGlobalFont(new Font(FontFamily.GenericSansSerif, 12f));

            // Black background.
            ThemeCreator.ThemeCreator.AddGlobalColorSwap(Color.FromArgb(0, 0, 64), Color.Black);

            // ListView controls have a light gray background with black text.
            ThemeCreator.ThemeCreator.AddColorChangeByType(typeof(ListView), new ThemeCreator.ColorChange { BackgroundColor = Color.LightGray, ForegroundColor = Color.Black });

            // Same with TreeView controls.
            ThemeCreator.ThemeCreator.AddColorChangeByType(typeof(TreeView), new ThemeCreator.ColorChange { BackgroundColor = Color.LightGray, ForegroundColor = Color.Black });

            // Toolbar buttons have a dark green background color.
            ThemeCreator.ThemeCreator.AddColorChangeByNameRegex(new Regex("cmdToolbar"), new ThemeCreator.ColorChange { BackgroundColor = Color.DarkGreen, ForegroundColor = null });
        }
    }
}
```

## TODO

- Add methods to tweak non-control colors, such as map components (like planets, orbits, etc).
