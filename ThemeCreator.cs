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

        // Global Color Swaps - swap any original Aurora color with another from any control.
        private static Dictionary<Color, Color> GlobalColorSwaps = new Dictionary<Color, Color>();
        // Color changes by Type - Set any Type's background and foreground colors.
        private static Dictionary<Type, ColorChange> TypeColorChanges = new Dictionary<Type, ColorChange>();
        // Named control color changes.
        private static Dictionary<string, ColorChange> NameColorChanges = new Dictionary<string, ColorChange>();
        // Color changes by a regex match of named controls - last resort, not very performant.
        private static Dictionary<Regex, ColorChange> RegexNameColorChanges = new Dictionary<Regex, ColorChange>();

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
            // Global color swaps.
            if (GlobalColorSwaps.ContainsKey(control.BackColor)) control.BackColor = GlobalColorSwaps[control.BackColor];
            if (GlobalColorSwaps.ContainsKey(control.ForeColor)) control.ForeColor = GlobalColorSwaps[control.ForeColor];
            // Type color changes.
            Type type = control.GetType();
            if (TypeColorChanges.ContainsKey(type))
            {
                var colorChange = TypeColorChanges[type];
                if (colorChange.BackgroundColor.HasValue) control.BackColor = colorChange.BackgroundColor.Value;
                if (colorChange.ForegroundColor.HasValue) control.ForeColor = colorChange.ForegroundColor.Value;
            }
            // Named color changes.
            if (NameColorChanges.ContainsKey(control.Name))
            {
                var colorChange = NameColorChanges[control.Name];
                if (colorChange.BackgroundColor.HasValue) control.BackColor = colorChange.BackgroundColor.Value;
                if (colorChange.ForegroundColor.HasValue) control.ForeColor = colorChange.ForegroundColor.Value;
            }
            // Regex named colors changes.
            foreach (KeyValuePair<Regex, ColorChange> regexColor in RegexNameColorChanges)
            {
                if (regexColor.Key.IsMatch(control.Name))
                {
                    if (regexColor.Value.BackgroundColor.HasValue) control.BackColor = regexColor.Value.BackgroundColor.Value;
                    if (regexColor.Value.ForegroundColor.HasValue) control.ForeColor = regexColor.Value.ForegroundColor.Value;
                }
            }
        }

        /// <summary>
        /// Convert every instance of an original Aurora color into another color.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="swap"></param>
        public static void AddGlobalColorSwap(Color current, Color swap)
        {
            GlobalColorSwaps[current] = swap;
        }

        /// <summary>
        /// Will apply the color changes specified to the type specified.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="colorChange"></param>
        public static void AddColorChangeByType(Type type, ColorChange colorChange)
        {
            TypeColorChanges[type] = colorChange;
        }

        /// <summary>
        /// Will apply the color changes specified to the control name specified.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="colorChange"></param>
        public static void AddColorChangeByName(string name, ColorChange colorChange)
        {
            NameColorChanges[name] = colorChange;
        }

        /// <summary>
        /// Will apply the color changes specified to the control name matching the regex specified.
        /// Should be used as a last resort - not very performant.
        /// </summary>
        /// <param name="regex"></param>
        /// <param name="colorChange"></param>
        public static void AddColorChangeByNameRegex(Regex regex, ColorChange colorChange)
        {
            RegexNameColorChanges[regex] = colorChange;
        }
    }
}
