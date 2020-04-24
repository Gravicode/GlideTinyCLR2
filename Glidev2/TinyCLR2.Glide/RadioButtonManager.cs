////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using GHI.Glide.Display;
using GHI.Glide.Geom;
using GHI.Glide.UI;

namespace GHI.Glide
{
    /// <summary>
    /// The RadioButtonManager manages groups of RadioButtons.
    /// </summary>
    public static class RadioButtonManager
    {
        private static Hashtable _groups = new Hashtable();

        // Adds a radio button to it's group.
        internal static void AddButton(RadioButton radioButton)
        {
            string name = radioButton.GroupName;

            if (!_groups.Contains(name))
                _groups.Add(name, new ArrayList());

            radioButton.TapEvent += new OnTap(OnTapEvent);

            ((ArrayList)_groups[name]).Add(radioButton);
        }

        /// <summary>
        /// Value of the selected radio button within a group.
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        /// <returns>Current value selected within the group.</returns>
        public static string GetValue(string groupName)
        {
            if (!_groups.Contains(groupName))
                throw new ArgumentOutOfRangeException("groupName", "No such radio button group exists.");

            // Find the selected button
            ArrayList group = (ArrayList)_groups[groupName];
            RadioButton button;
            for (int i = 0; i < group.Count; i++)
            {
                button = (RadioButton)group[i];
                if (button.Checked)
                    return button.Value;
            }

            return String.Empty;
        }

        /// <summary>
        /// Get the number of buttons in a group.
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        /// <returns>Number of buttons in the group.</returns>
        public static int GetCount(string groupName)
        {
            if (!_groups.Contains(groupName))
                throw new ArgumentOutOfRangeException("groupName", "No such radio button group exists.");

            return ((ArrayList)_groups[groupName]).Count;
        }

        /// <summary>
        /// Turns off the previously selected radio button.
        /// </summary>
        /// <param name="sender">Associated object with the sender.</param>
        private static void OnTapEvent(object sender)
        {
            RadioButton tappedButton = (RadioButton)sender;
            ArrayList group = (ArrayList)_groups[tappedButton.GroupName];

            // Find the button that was previously selected and unselect it
            RadioButton button;
            for (int i = 0; i < group.Count; i++)
            {
                button = (RadioButton)group[i];
                if (button.Name == tappedButton.Name) continue;
                if (button.Checked)
                {
                    button.Checked = false;
                    break;
                }
            }
        }
    }
}
