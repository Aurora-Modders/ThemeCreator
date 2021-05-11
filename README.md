# ThemeCreator

An AuroraPatch helper patch primarily used by theme developers to ease the theme creation process.

## Usage

This patch doesn't do anything on its own. It's meant to be used as a dependency of a proper theme patch.
It exposes a handful of helper methods to adjust Aurora colors.

### Color

#### public static void AddColorChange(Color current, Color swap)

A method used to apply color swaps globally across Aurora.

#### public static void AddColorChange(Type type, ColorChange colorChange)

A method used to apply background/foreground color changes based on form types.
Useful to modify all Form/ListView/Button/etc colors.

#### public static void AddColorChange(string name, ColorChange colorChange)
    
Same as above except that it uses the component name instead of a type to apply color changes.

#### public static void AddColorChange(Regex regex, ColorChange colorChange)

Applies a color change if the control name matches the regex provided.

#### public static void AddColorChange(Func<Control, bool> predicate, ColorChange colorChange)

Will apply the color change to the Control provided if the predicate returns true.

#### public static void AddFontChange(Font font)

A method used to override the global font across Aurora.

#### public static void AddFontChange(Type type, Font font)

A method used to apply font changes based on form types.
Useful to modify all Form/ListView/Button/etc fonts.

#### public static void AddFontChange(string name, Font font)

Same as above except that it uses the component name instead of a type to apply font changes.

#### public static void AddFontChange(Regex name, Font font)

Applies a font change if the control name matches the regex provided.

#### public static void AddFontChange(Func<Control, bool> predicate, Font font)

Will apply the font to the Control provided if the predicate returns true.

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
            ThemeCreator.ThemeCreator.AddFontChange(new Font(FontFamily.GenericSansSerif, 12f));

            // Black background.
            ThemeCreator.ThemeCreator.AddColorChange(Color.FromArgb(0, 0, 64), Color.Black);

            // ListView controls have a light gray background with black text.
            ThemeCreator.ThemeCreator.AddColorChange(typeof(ListView), new ThemeCreator.ColorChange { BackgroundColor = Color.LightGray, ForegroundColor = Color.Black });

            // Same with TreeView controls.
            ThemeCreator.ThemeCreator.AddColorChange(typeof(TreeView), new ThemeCreator.ColorChange { BackgroundColor = Color.LightGray, ForegroundColor = Color.Black });

            // Toolbar buttons have a dark green background color.
            ThemeCreator.ThemeCreator.AddColorChange(new Regex("cmdToolbar"), new ThemeCreator.ColorChange { BackgroundColor = Color.DarkGreen, ForegroundColor = null });

            // Increment buttons have a dark blue background color.
            ThemeCreator.ThemeCreator.AddColorChange(control => control.Name.Contains("cmdToolbar"), new ThemeCreator.ColorChange { BackgroundColor = Color.DarkBlue, ForegroundColor = null });
        }
    }
}
```

## TODO

- Add methods to tweak non-control colors, such as map components (like planets, orbits, etc).
