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
        private static List<Action<Graphics, Pen>> GraphicsDrawEllipseActions = new List<Action<Graphics, Pen>>();
        private static List<Action<Graphics, Brush>> GraphicsFillEllipseActions = new List<Action<Graphics, Brush>>();
        private static List<Action<Graphics, string, Font, Brush>> GraphicsDrawStringActions = new List<Action<Graphics, string, Font, Brush>>();
        private static List<Action<Graphics, Pen>> GraphicsDrawLineActions = new List<Action<Graphics, Pen>>();
        private static List<Action<SolidBrush>> SolidBrushActions = new List<Action<SolidBrush>>();
        private static List<Action<TextureBrush>> TextureBrushActions = new List<Action<TextureBrush>>();
        private static List<Action<Pen>> PenActions = new List<Action<Pen>>();

        /// <summary>
        /// Initialize our ThemeCreator by patching all Form constructors so that we can inject our colors changes.
        /// </summary>
        /// <param name="harmony"></param>
        protected override void Loaded(Harmony harmony)
        {
            LogInfo("Loading ThemeCreator...");
            HarmonyMethod formConstructorPostfixMethod = new HarmonyMethod(GetType().GetMethod("FormConstructorPostfix", AccessTools.all));
            foreach (var form in AuroraAssembly.GetTypes().Where(t => typeof(Form).IsAssignableFrom(t)))
            {
                try
                {
                    foreach (var ctor in form.GetConstructors())
                    {
                        harmony.Patch(ctor, postfix: formConstructorPostfixMethod);
                    }
                }
                catch (Exception e)
                {
                    LogError($"ThemeCreator failed to patch Form constructor {form.Name}, exception: {e}");
                }
            }
            HarmonyMethod graphicsDrawEllipsePrefixMethod = new HarmonyMethod(GetType().GetMethod("GraphicsDrawEllipsePrefix", AccessTools.all));
            HarmonyMethod graphicsFillEllipsePrefixMethod = new HarmonyMethod(GetType().GetMethod("GraphicsFillEllipsePrefix", AccessTools.all));
            HarmonyMethod graphicsDrawStringPrefixMethod = new HarmonyMethod(GetType().GetMethod("GraphicsDrawStringPrefix", AccessTools.all));
            HarmonyMethod graphicsDrawLinePrefixMethod = new HarmonyMethod(GetType().GetMethod("GraphicsDrawLinePrefix", AccessTools.all));
            foreach (var graphics in typeof(Graphics).Assembly.GetTypes().Where(t => typeof(Graphics).IsAssignableFrom(t)))
            {
                try
                {
                    foreach (var method in graphics.GetMethods(AccessTools.all))
                    {
                        if (method.Name == "DrawEllipse") harmony.Patch(method, prefix: graphicsDrawEllipsePrefixMethod);
                        else if (method.Name == "FillEllipse") harmony.Patch(method, prefix: graphicsFillEllipsePrefixMethod);
                        else if (method.Name == "DrawString") harmony.Patch(method, prefix: graphicsDrawStringPrefixMethod);
                        else if (method.Name == "DrawLine") harmony.Patch(method, prefix: graphicsDrawLinePrefixMethod);
                    }
                }
                catch (Exception e)
                {
                    LogError($"ThemeCreator failed to patch graphics draw/fill ellipse methods for {graphics.Name}, exception: {e}");
                }
            }
            HarmonyMethod brushPostfixMethod = new HarmonyMethod(GetType().GetMethod("BrushConstructorPostfix", AccessTools.all));
            foreach (var brush in typeof(Brush).Assembly.GetTypes().Where(t => typeof(Brush).IsAssignableFrom(t)))
            {
                try
                {
                    foreach (var ctor in brush.GetConstructors())
                    {
                        harmony.Patch(ctor, postfix: brushPostfixMethod);
                    }
                }
                catch (Exception e)
                {
                    LogError($"ThemeCreator failed to patch brush constructor {brush}, exception: {e}");
                }
            }
            HarmonyMethod penPostfixMethod = new HarmonyMethod(GetType().GetMethod("PenConstructorPostfix", AccessTools.all));
            foreach (var pen in typeof(Pen).Assembly.GetTypes().Where(t => typeof(Pen).IsAssignableFrom(t)))
            {
                try
                {
                    foreach (var ctor in pen.GetConstructors())
                    {
                        harmony.Patch(ctor, postfix: penPostfixMethod);
                    }
                }
                catch (Exception e)
                {
                    LogError($"ThemeCreator failed to patch Pen constructor {pen.Name}, exception: {e}");
                }
            }
        }

        /// <summary>
        /// Our harmony form constructor postfix method which simply adds our custom handle created callback.
        /// </summary>
        /// <param name="__instance"></param>
        private static void FormConstructorPostfix(Form __instance)
        {
            __instance.HandleCreated += CustomHandleCreated;
        }

        /// <summary>
        /// Our harmony Graphics DrawEllipse prefix that invokes the necessary actions.
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="pen"></param>
        private static void GraphicsDrawEllipsePrefix(Graphics __instance, Pen pen)
        {
            foreach (Action<Graphics, Pen> action in GraphicsDrawEllipseActions)
            {
                action.Invoke(__instance, pen);
            }
        }

        /// <summary>
        /// Our harmony Graphics FillEllipse prefix that invokes the necessary actions.
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="brush"></param>
        private static void GraphicsFillEllipsePrefix(Graphics __instance, Brush brush)
        {
            foreach (Action<Graphics, Brush> action in GraphicsFillEllipseActions)
            {
                action.Invoke(__instance, brush);
            }
        }

        /// <summary>
        /// Our harmony Graphics DrawString prefix that invokes the necessary actions.
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="s"></param>
        /// <param name="font"></param>
        /// <param name="brush"></param>
        private static void GraphicsDrawStringPrefix(Graphics __instance, string s, Font font, Brush brush)
        {
            foreach (Action<Graphics, string, Font, Brush> action in GraphicsDrawStringActions)
            {
                action.Invoke(__instance, s, font, brush);
            }
        }

        /// <summary>
        /// Our harmony Graphics DrawLine prefix that invokes the necessary actions.
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="pen"></param>
        private static void GraphicsDrawLinePrefix(Graphics __instance, Pen pen)
        {
            foreach (Action<Graphics, Pen> action in GraphicsDrawLineActions)
            {
                action.Invoke(__instance, pen);
            }
        }

        /// <summary>
        /// Our harmony brush constructor postfix method which invokes our solid brush actions.
        /// </summary>
        /// <param name="__instance"></param>
        private static void BrushConstructorPostfix(Brush __instance)
        {
            if (__instance.GetType() == typeof(SolidBrush))
            {
                foreach (Action<SolidBrush> action in SolidBrushActions)
                {
                    action.Invoke(__instance as SolidBrush);
                }
            }
            else if (__instance.GetType() == typeof(TextureBrush))
            {
                foreach (Action<TextureBrush> action in TextureBrushActions)
                {
                    action.Invoke(__instance as TextureBrush);
                }
            }
        }

        /// <summary>
        /// Our harmony pen constructor postfix method which invokes our solid brush actions.
        /// </summary>
        /// <param name="__instance"></param>
        private static void PenConstructorPostfix(Pen __instance)
        {
            foreach (Action<Pen> action in PenActions)
            {
                action.Invoke(__instance);
            }
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

        /// <summary>
        /// Invoke an action with given Graphics and Pen right before Graphics.DrawEllipse is called.
        /// </summary>
        /// <param name="action"></param>
        public static void DrawEllipsePrefixAction(Action<Graphics, Pen> action)
        {
            GraphicsDrawEllipseActions.Add(action);
        }

        /// <summary>
        /// Invoke an action with given Graphics and Brush right before Graphics.FillEllipse is called.
        /// </summary>
        /// <param name="action"></param>
        public static void FillEllipsePrefixAction(Action<Graphics, Brush> action)
        {
            GraphicsFillEllipseActions.Add(action);
        }

        /// <summary>
        /// Helper method to set orbit colors.
        /// </summary>
        /// <param name="color"></param>
        public static void SetOrbitColor(Color color)
        {
            GraphicsDrawEllipseActions.Add((graphics, pen) =>
            {
                pen.Color = color;
            });
        }

        /// <summary>
        /// Helper method to set planet colors on the TacticalMap.
        /// </summary>
        /// <param name="color"></param>
        public static void SetPlanetColor(Color color)
        {
            GraphicsFillEllipseActions.Add((graphics, brush) =>
            {
                Type brushType = brush.GetType();
                if (brushType == typeof(SolidBrush))
                {
                    SolidBrush solidBrush = brush as SolidBrush;
                    if (solidBrush.Color == Color.Blue)
                    {
                        solidBrush.Color = color;
                    }
                }
            });
        }

        /// <summary>
        /// Helper method to set the star system colors on the GalacticMap.
        /// </summary>
        /// <param name="color"></param>
        public static void SetStarSystemColor(Color color)
        {
            FillEllipsePrefixAction((graphics, brush) =>
            {
                Type brushType = brush.GetType();
                if (brushType == typeof(SolidBrush))
                {
                    SolidBrush solidBrush = brush as SolidBrush;
                    if (solidBrush.Color == Color.Lime)
                    {
                        solidBrush.Color = color;
                    }
                }
            });
        }

        /// <summary>
        /// Helper method to set the star color on the TacticalMap.
        /// </summary>
        /// <param name="color"></param>
        public static void SetStarColor(Color color)
        {
            FillEllipsePrefixAction((graphics, brush) =>
            {
                Type brushType = brush.GetType();
                if (brushType == typeof(SolidBrush))
                {
                    SolidBrush solidBrush = brush as SolidBrush;
                    if (solidBrush.Color == Color.FromArgb(255, 245, 236))
                    {
                        solidBrush.Color = color;
                    }
                }
            });
        }

        /// <summary>
        /// Helper method to set the lagrange point colors on the TacticalMap.
        /// </summary>
        /// <param name="color"></param>
        public static void SetLagrangePointColor(Color color)
        {
            FillEllipsePrefixAction((graphics, brush) =>
            {
                Type brushType = brush.GetType();
                if (brushType == typeof(SolidBrush))
                {
                    SolidBrush solidBrush = brush as SolidBrush;
                    if (solidBrush.Color == Color.Orange)
                    {
                        solidBrush.Color = Color.Red;
                    }
                }
            });
        }

        /// <summary>
        /// Invoke an action with given Graphics, string, Font, and Brush right before Graphics.DrawString is called.
        /// <param name="action"></param>
        /// </summary>
        public static void DrawStringPrefixAction(Action<Graphics, string, Font, Brush> action)
        {
            GraphicsDrawStringActions.Add(action);
        }

        /// <summary>
        /// Invoke an action with given Graphics and Pen right before Graphics.DrawLine is called.
        /// </summary>
        /// <param name="action"></param>
        public static void DrawLinePrefixAction(Action<Graphics, Pen> action)
        {
            GraphicsDrawLineActions.Add(action);
        }

        /// <summary>
        /// Helper method to set the color of the distance ruler line on the TacticalMap.
        /// </summary>
        /// <param name="color"></param>
        public static void SetDistanceRulerLineColor(Color color)
        {
            DrawLinePrefixAction((graphics, pen) =>
            {
                if (pen.Color == Color.LimeGreen)
                {
                    pen.Color = color;
                }
            });
        }

        /// <summary>
        /// Helper method to set the color of the comet tails on the TacticalMap.
        /// </summary>
        /// <param name="color"></param>
        public static void SetCometTailColor(Color color)
        {
            DrawLinePrefixAction((graphics, pen) =>
            {
                if (pen.Color == Color.Cyan)
                {
                    pen.Color = color;
                }
            });
        }

        /// <summary>
        /// Helper method to set the TacticalMap and GalaxyMap text color.
        /// </summary>
        /// <param name="color"></param>
        public static void SetMapTextColor(Color color)
        {
            DrawStringPrefixAction((graphics, s, font, brush) =>
            {
                if (brush.GetType() == typeof(SolidBrush))
                {
                    ((SolidBrush)brush).Color = color;
                }
            });
        }

        /// <summary>
        /// Will swap a solid brush color upon creation.
        /// </summary>
        /// <param name="action"></param>
        public static void ApplySolidBrush(Color current, Color swap)
        {
            SolidBrushActions.Add(solidBrush =>
            {
                if (solidBrush.Color == current)
                {
                    solidBrush.Color = swap;
                }
            });
        }

        /// <summary>
        /// Allows for performing action upon solid brush creation.
        /// </summary>
        /// <param name="action"></param>
        public static void ApplySolidBrush(Action<SolidBrush> action)
        {
            SolidBrushActions.Add(action);
        }

        /// <summary>
        /// Allows for performing action upon texture brush creation.
        /// </summary>
        /// <param name="action"></param>
        public static void ApplyTextureBrush(Action<TextureBrush> action)
        {
            TextureBrushActions.Add(action);
        }

        /// <summary>
        /// Allows for performing action upon pen creation.
        /// </summary>
        /// <param name="action"></param>
        public static void ApplyPen(Action<Pen> action)
        {
            PenActions.Add(action);
        }
    }
}
