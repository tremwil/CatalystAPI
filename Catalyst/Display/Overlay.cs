using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Catalyst.Display
{
    class Field
    {
        public int priority;
        public string format;
        public object value;
        public bool isManual;

        public Func<object> updater;

        public static Field Create(int priority, string format, object value)
        {
            Field f = new Field();
            f.priority = priority;
            f.format = format;
            f.isManual = true;
            f.value = value;

            f.updater = f.getterManual;

            return f;
        }

        public static Field Create(int priority, string format, Func<object> getter)
        {
            Field f = new Field();
            f.priority = priority;
            f.format = format;
            f.isManual = false;

            f.updater = getter;

            return f;
        }

        private object getterManual()
        {
            return value;
        }
    }

    /// <summary>
    /// A static class to control the game overlay.
    /// </summary>
    public static class Overlay
    {
        private static OverlayForm overlayWnd = new OverlayForm("MirrorsEdgeCatalyst");
        private static Dictionary<string, Field> overlayMap = new Dictionary<string, Field>();
        private static Dictionary<int, string> priorityMap = new Dictionary<int, string>();
        
        /// <summary>
        /// The color of the text. Currently bordered text is not supported.
        /// </summary>
        public static Color Color
        {
            get { return overlayWnd.TextColor; }
            set { SafeInvoke(f => f.TextColor = value); }
        }

        /// <summary>
        /// The font to display text with.
        /// </summary>
        public static Font Font
        {
            get { return overlayWnd.TextFont; }
            set { SafeInvoke(f => f.TextFont = value); }
        }

        /// <summary>
        /// The refresh rate of the overlay in hertz. default is about 14.3hz (70ms).
        /// </summary>
        public static float RefreshRate
        {
            get { return 1000f / overlayWnd.RefreshRateMS; }
            set { SafeInvoke(f => f.RefreshRateMS = 1000 / (int)value); }
        }

        /// <summary>
        /// The interval, in milliseconds, before the overlay updates. Default is 70ms.
        /// </summary>
        public static int IntervalMS
        {
            get { return overlayWnd.RefreshRateMS; }
            set { SafeInvoke(f => f.RefreshRateMS = value); }
        }

        /// <summary>
        /// The Y spacing, in pixels, between multiple fields.
        /// </summary>
        public static int Spacing
        {
            get { return overlayWnd.TextSpacing; }
            set { SafeInvoke(f => f.TextSpacing = value); }
        }
        /// <summary>
        /// The positive offset from the bottom right corner of the text to the same corner of the screen.
        /// </summary>
        public static Point Offset
        {
            get { return overlayWnd.TextOffset; }
            set { SafeInvoke(f => f.TextOffset = value); }
        }

        private static bool enabled = false;
        /// <summary>
        /// A boolean indicating if the overlay has been enabled.
        /// </summary>
        public static bool Enabled => enabled;

        /// <summary>
        /// A boolean indicating if the overlay is currently shown and working.
        /// </summary>
        public static bool Running => overlayWnd.OverlayEnabled;
        /// <summary>
        /// A boolean to get/set the visibility of the overlay.
        /// </summary>
        public static bool Displaying
        {
            get { return overlayWnd.Displaying; }
            set { SafeInvoke(f => f.Displaying = value); }
        }

        /// <summary>
        /// Add an automatically updated field.
        /// </summary>
        /// <param name="fieldName">The new field.</param>
        /// <param name="getter">A function to get the current value of this field.</param>
        /// <param name="formatString">The formatting string. Leave to "{0}" if none.</param>
        public static void AddAutoField(string fieldName, Func<object> getter, string formatString = "{0}")
        {
            int priority = 0;

            if (overlayMap.ContainsKey(fieldName))
            {
                priority = overlayMap[fieldName].priority;
            }
            else
            {
                var keys = priorityMap.Keys.OrderBy(x => x).ToArray();

                for (int i = 0; i < keys.Length; i++)
                {
                    if (!keys.Contains(keys[i] + 1))
                    {
                        priority = keys[i] + 1;
                        break;
                    }
                }
            }

            priorityMap[priority] = fieldName;

            Field f = Field.Create(priority, formatString, getter);
            overlayMap[fieldName] = f;

            UpdateFields(f, false);
        }

        /// <summary>
        /// Add an automatically updated field with the given priority.
        /// </summary>
        /// <param name="fieldName">The new field.</param>
        /// <param name="priority">The priority of the field.</param>
        /// <param name="getter">A function to get the current value of this field.</param>
        /// <param name="formatString">The formatting string. Leave to "{0}" if none.</param>
        public static void AddAutoField(string fieldName, int priority, Func<object> getter, string formatString = "{0}")
        {
            if (overlayMap.ContainsKey(fieldName))
            {
                priorityMap.Remove(overlayMap[fieldName].priority);
            }

            else if (priorityMap.ContainsKey(priority))
                throw new ArgumentException("Another field has this priority", "priority");

            priorityMap[priority] = fieldName;

            Field f = Field.Create(priority, formatString, getter);
            overlayMap[fieldName] = f;

            UpdateFields(f, false);
        }

        /// <summary>
        /// Add a manually updated field.
        /// </summary>
        /// <param name="fieldName">The new field.</param>
        /// <param name="value">The current value of this manual field.</param>
        /// <param name="formatString">The formatting string. Leave to "{0}" if none.</param>
        public static void AddManualField(string fieldName, object value, string formatString = "{0}")
        {
            int priority = 0;

            if (overlayMap.ContainsKey(fieldName))
            {
                priority = overlayMap[fieldName].priority;
            }
            else
            {
                var keys = priorityMap.Keys.OrderBy(x => x).ToArray();

                for (int i = 0; i < keys.Length; i++)
                {
                    if (!keys.Contains(keys[i] + 1))
                    {
                        priority = keys[i] + 1;
                        break;
                    }
                }
            }

            priorityMap[priority] = fieldName;

            Field f = Field.Create(priority, formatString, value);
            overlayMap[fieldName] = f;

            UpdateFields(f, false);
        }

        /// <summary>
        /// Add a manually updated field with the given priority.
        /// </summary>
        /// <param name="fieldName">The new field.</param>
        /// <param name="priority">The priority of the field.</param>
        /// <param name="value">The current value of this manual field.</param>
        /// <param name="formatString">The formatting string. Leave to "{0}" if none.</param>
        public static void AddManualField(string fieldName, int priority, object value, string formatString = "{0}")
        {
            if (overlayMap.ContainsKey(fieldName))
            {
                priorityMap.Remove(overlayMap[fieldName].priority);
            }

            else if (priorityMap.ContainsKey(priority))
                throw new ArgumentException("Another field has this priority", "priority");

            priorityMap[priority] = fieldName;

            Field f = Field.Create(priority, formatString, value);
            overlayMap[fieldName] = f;

            UpdateFields(f, false);
        }

        /// <summary>
        /// Set the priority of a field to some new value.
        /// </summary>
        /// <param name="fieldName">The field name.</param>
        /// <param name="priority">The new priority.</param>
        public static void SetPriority(string fieldName, int priority)
        {
            if (!overlayMap.ContainsKey(fieldName))
            {
                throw new ArgumentException("No field of the provided name");
            }

            priorityMap.Remove(overlayMap[fieldName].priority);
            priorityMap[priority] = fieldName;

            UpdateFields(overlayMap[fieldName], false);
        }

        /// <summary>
        /// Remove the given field from the overlay.
        /// </summary>
        /// <param name="fieldName">The field to remove.</param>
        public static void RemoveField(string fieldName)
        {
            if (!overlayMap.ContainsKey(fieldName))
            {
                throw new ArgumentException("No field of the provided name");
            }

            var f = overlayMap[fieldName];

            overlayMap.Remove(fieldName);

            UpdateFields(f, true);
            priorityMap.Remove(f.priority);
        }

        /// <summary>
        /// Remove all fields from the overlay.
        /// </summary>
        public static void Clear()
        {
            overlayWnd.Overlays.Clear();
            overlayMap.Clear();
            priorityMap.Clear();
        }

        /// <summary>
        /// Update a field manually.
        /// </summary>
        /// <param name="fieldName">The field to update.</param>
        /// <param name="newValue">The new value.</param>
        public static void UpdateManualField(string fieldName, object newValue)
        {
            if (!overlayMap.ContainsKey(fieldName))
            {
                throw new ArgumentException("No field of the provided name");
            }
            if (!overlayMap[fieldName].isManual)
            {
                throw new ArgumentException("The provided field is not manual");
            }

            overlayMap[fieldName].value = newValue;
        }

        private static void UpdateFields(Field update, bool remove)
        {
            var okeys = priorityMap.Keys.OrderBy(x => x).ToList();
            int index = okeys.IndexOf(update.priority);

            if (remove)
            {
                SafeInvoke(f => f.Overlays.RemoveAt(index));
            }
            else
            {
                int c = SafeInvoke(f => f.Overlays.Count);
                for (int i = 0; i < c; i++)
                {
                    var ofield = SafeInvoke(f => f.Overlays[i]);

                    if (ReferenceEquals(ofield.Getter, update.updater))
                    {
                        SafeInvoke(f => f.Overlays.RemoveAt(i));
                        break;
                    }
                }

                var newfield = new OverlayField(update.format, update.updater);
                SafeInvoke(f => f.Overlays.Insert(index, newfield));
            }
        }

        private delegate void SafeInvokeDelegate(OverlayForm f);
        private static void SafeInvoke(Action<OverlayForm> action)
        {
            if (overlayWnd.InvokeRequired)
                overlayWnd.Invoke(new SafeInvokeDelegate(action), overlayWnd);

            else action(overlayWnd);
        }

        private delegate T SafeInvokeSetDelegate<T>(OverlayForm f);
        private static T SafeInvoke<T>(Func<OverlayForm, T> action)
        {
            if (overlayWnd.InvokeRequired)
                return (T)overlayWnd.Invoke(new SafeInvokeSetDelegate<T>(action), overlayWnd);

            else return action(overlayWnd);
        }

        /// <summary>
        /// Enable the overlay.
        /// </summary>
        /// <param name="show">If true, will call the Show() method after enabling.</param>
        public static void Enable(bool show)
        {
            if (!enabled)
            {
                var t = new Thread(() => Application.Run(overlayWnd));
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
            }

            if (show) Show();
        }

        /// <summary>
        /// Enable the overlay, but do not show it yet.
        /// </summary>
        public static void Enable()
        {
            Enable(false);
        }

        /// <summary>
        /// Completely disable the overlay.
        /// </summary>
        public static void Disable()
        {
            if (enabled)
            {
                overlayWnd.Close();
                enabled = false;
            }
        }

        /// <summary>
        /// Make the overlay visible and attach it. To keep it attached, use the Displaying property.
        /// </summary>
        public static void Show()
        {
            Thread.Sleep(50);
            SafeInvoke(f => f.EnableOverlay());
        }

        /// <summary>
        /// Hide the overlay  and detach it. To keep it attached, use the Displaying property..
        /// </summary>
        public static void Hide()
        {
            SafeInvoke(f => f.DisableOverlay());
        }
    }
}
