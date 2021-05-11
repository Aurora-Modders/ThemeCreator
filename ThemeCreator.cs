using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using HarmonyLib;

namespace ThemeCreator
{
    /// <summary>
    /// Class representing a color change on a Control object.
    /// </summary>
    public class ColorChange
    {
        public Color? BackgroundColor { get; set; }
        public Color? ForegroundColor { get; set; }
    }

    /// <summary>
    /// ThemeCreator patch used to facilitate theme creation.
    /// </summary>
    public class ThemeCreator : AuroraPatch.Patch
    {
        public override string Description => "A helper patch to create theme patches";

        private static Dictionary<Func<Control, bool>, ColorChange> ColorPredicates = new Dictionary<Func<Control, bool>, ColorChange>();
        private static Dictionary<Func<Control, bool>, Font> FontPredicates = new Dictionary<Func<Control, bool>, Font>();
        private static Font GlobalFont = null;

        /// <summary>
        /// Initialize our ThemeCreator by patching all Form constructors so that we can inject our colors changes.
        /// </summary>
        /// <param name="harmony"></param>
        protected override void Loaded(Harmony harmony)
        {
            LogInfo("Loading ThemeCreator...");
            HarmonyMethod postfixMethod = new HarmonyMethod(GetType().GetMethod("FormConstructorPostfix", AccessTools.all));
            foreach (var form in AuroraAssembly.GetTypes().Where(t => typeof(Form).IsAssignableFrom(t)))
            {
                try
                {
                    foreach (var ctor in form.GetConstructors())
                    {
                        harmony.Patch(ctor, postfix: postfixMethod);
                    }
                }
                catch (Exception e)
                {
                    LogError($"ThemeCreator failed to patch Form constructor {form.Name}, exception: {e}");
                }
            }
        }

        /// <summary>
        /// Our harmony form constructor postfix method which simply adds our custom load callback.
        /// </summary>
        /// <param name="__instance"></param>
        private static void FormConstructorPostfix(Form __instance)
        {
            __instance.HandleCreated += CustomHandleCreated;
        }

        /// <summary>
        /// Our custom handle created callback is a pass-through method that simply casts our generic callback
        /// object to a Control and sends it off to the IterateControls method for enumeration/modification.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CustomHandleCreated(Object sender, EventArgs e)
        {
            if (GlobalFont != null) ((Control)sender).Font = GlobalFont;
            IterateControls((Control)sender);
        }

        /// <summary>
        /// Recusively loops through all controls and applies our requested changes.
        /// </summary>
        /// <param name="control"></param>
        private static void IterateControls(Control control)
        {
            ApplyChanges(control);
            foreach (Control ctrl in control.Controls)
            {
                IterateControls(ctrl);
            }
        }

        /// <summary>
        /// The meat of our patch. Does all the heavy lifting to apply our color changes.
        /// </summary>
        /// <param name="control"></param>
        private static void ApplyChanges(Control control)
        {
            // -- Colors -- //
            foreach (KeyValuePair<Func<Control, bool>, ColorChange> predicate in ColorPredicates)
            {
                if (predicate.Key.Invoke(control))
                {
                    if (predicate.Value.BackgroundColor.HasValue) control.BackColor = predicate.Value.BackgroundColor.Value;
                    if (predicate.Value.ForegroundColor.HasValue) control.ForeColor = predicate.Value.ForegroundColor.Value;
                }
            }
            // -- Fonts -- //
            foreach (KeyValuePair<Func<Control, bool>, Font> predicate in FontPredicates)
            {
                if (predicate.Key.Invoke(control))
                {
                    control.Font = predicate.Value;
                }
            }
        }

        /// <summary>
        /// Convert every instance of a color into another color.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="swap"></param>
        public static void AddColorChange(Color current, Color swap)
        {
            ColorPredicates.Add(control => control.BackColor == current, new ColorChange { BackgroundColor = swap });
            ColorPredicates.Add(control => control.ForeColor == current, new ColorChange { ForegroundColor = swap });
        }

        /// <summary>
        /// Will apply the color changes specified to the type specified.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="colorChange"></param>
        public static void AddColorChange(Type type, ColorChange colorChange)
        {
            ColorPredicates.Add(control => control.GetType() == type, colorChange);
        }

        /// <summary>
        /// Will apply the color changes specified to the control name specified.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="colorChange"></param>
        public static void AddColorChange(string name, ColorChange colorChange)
        {
            ColorPredicates.Add(control => control.Name == name, colorChange);
        }

        /// <summary>
        /// Will apply the color changes specified to the control name matching the regex specified.
        /// Should be used as a last resort - not very performant.
        /// </summary>
        /// <param name="regex"></param>
        /// <param name="colorChange"></param>
        public static void AddColorChange(Regex regex, ColorChange colorChange)
        {
            ColorPredicates.Add(control => regex.IsMatch(control.Name), colorChange);
        }

        /// <summary>
        /// Apply the color change specified if the predicate returns true.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="colorChange"></param>
        public static void AddColorChange(Func<Control, bool> predicate, ColorChange colorChange)
        {
            ColorPredicates.Add(predicate, colorChange);
        }

        /// <summary>
        /// Sets a default global font.
        /// </summary>
        /// <param name="font"></param>
        public static void AddFontChange(Font font)
        {
            GlobalFont = font;
        }

        /// <summary>
        /// Will apply the font specified to the type specified.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="font"></param>
        public static void AddFontChange(Type type, Font font)
        {
            FontPredicates.Add(control => control.GetType() == type, font);
        }

        /// <summary>
        /// Will apply the font specified to the control name specified.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="font"></param>
        public static void AddFontChange(string name, Font font)
        {
            FontPredicates.Add(control => control.Name == name, font);
        }

        /// <summary>
        /// Will apply the font changes specified to the control name matching the regex specified.
        /// Should be used as a last resort - not very performant.
        /// </summary>
        /// <param name="regex"></param>
        /// <param name="font"></param>
        public static void AddFontChange(Regex regex, Font font)
        {
            FontPredicates.Add(control => regex.IsMatch(control.Name), font);
        }

        /// <summary>
        /// Apply the font specified if the predicate returns true.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="font"></param>
        public static void AddFontChange(Func<Control, bool> predicate, Font font)
        {
            FontPredicates.Add(predicate, font);
        }
    }
}
