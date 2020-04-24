////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Text;
using System.Xml;
using System.IO;
//using Microsoft.SPOT;
//using Microsoft.SPOT.Presentation.Media;
using GHI.Glide.Display;
using GHI.Glide.UI;
using System.Drawing;
using TinyCLR2.Glide.Properties;

namespace GHI.Glide
{
    /// <summary>
    /// The GlideLoader class parses XML into components.
    /// </summary>
    public static class GlideLoader
    {
        private static Font _font = Resources.GetFont(Resources.FontResources.droid_reg10);
        private static DisplayObject _defaultDisplayObject = new DisplayObject();
        private static Window _window = null;

        /// <summary>
        /// Loads a Window from an XML string.
        /// </summary>
        /// <param name="xmlStr">XML string.</param>
        /// <returns>Window object.</returns>
        public static Window LoadWindow(string xmlStr)
        {
            //StringBuilder sb = new StringBuilder(xmlStr);
            //sb = sb.Replace("\n", String.Empty);
            //sb = sb.Replace("\r", String.Empty);
            //sb = sb.Replace("\t", String.Empty);
            return LoadWindow(UTF8Encoding.UTF8.GetBytes(xmlStr));
        }

        /// <summary>
        /// Loads a Window from XML bytes.
        /// </summary>
        /// <param name="xmlBytes">XML bytes.</param>
        /// <returns>Window object.</returns>
        public static Window LoadWindow(byte[] xmlBytes)
        {
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();

            xmlReaderSettings.IgnoreComments = true;
            xmlReaderSettings.IgnoreProcessingInstructions = true;
            xmlReaderSettings.IgnoreWhitespace = true;
            
             MemoryStream stream = new MemoryStream(xmlBytes);

            XmlReader reader = XmlReader.Create(stream, xmlReaderSettings);


            if(!reader.ReadToDescendant("Glide"))
                throw new ArgumentException("Glide not detected.");

            stream.Seek(0, SeekOrigin.Begin);
            reader = XmlReader.Create(stream, xmlReaderSettings);

            if (!reader.ReadToDescendant("Window"))
                throw new ArgumentException("XML does not contain a Window element.");

            _window = ParseWindow(reader);

            DisplayObject component;
            while (reader.Read() && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "Window"))
            {
                component = GetComponent(reader);

                if (component != null)
                    _window.AddChild(component);
                else
                    throw new Exception(reader.Name + " is not a valid UI component.");
            }

            reader.Close();

            _window.Render();

            return _window;
        }

        /// <summary>
        /// Parses a UI component from the XML.
        /// </summary>
        /// <param name="reader">XML reader object.</param>
        /// <returns>DisplayObject</returns>
        private static DisplayObject GetComponent(XmlReader reader)
        {
            switch (reader.Name)
            {
                case "Button":
                    return LoadButton(reader);

                case "CheckBox":
                    return LoadCheckBox(reader);

                case "Dropdown":
                    return LoadDropdown(reader);

                case "DataGrid":
                    return LoadDataGrid(reader);

                case "Image":
                    return LoadImage(reader);

                case "PasswordBox":
                    return LoadPasswordBox(reader);

                case "ProgressBar":
                    return LoadProgressBar(reader);

                case "RadioButton":
                    return LoadRadioButton(reader);

                case "Slider":
                    return LoadSlider(reader);

                case "TextBlock":
                    return LoadTextBlock(reader);

                case "TextBox":
                    return LoadTextBox(reader);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Parses the Window XML into a UI component.
        /// </summary>
        /// <param name="reader">XML reader object.</param>
        /// <returns>Window</returns>
        private static Window ParseWindow(XmlReader reader)
        {
            string name = reader.GetAttribute("Name");
            int width = Convert.ToInt32(reader.GetAttribute("Width"));
            int height = Convert.ToInt32(reader.GetAttribute("Height"));

            if (Glide.FitToScreen && (width < Glide.LCD.Width || height < Glide.LCD.Height))
            {
                width = Glide.LCD.Width;
                height = Glide.LCD.Height;
            }

            Color backColor = GlideUtils.Convert.ToColor(reader.GetAttribute("BackColor"));

            Window window = new Window(name, width, height);
            window.BackColor = backColor;

            return window;
        }

        /// <summary>
        /// Parses the Button XML into a UI component.
        /// </summary>
        /// <param name="reader">XML reader object.</param>
        /// <returns>Button object.</returns>
        private static Button LoadButton(XmlReader reader)
        {
            string name = reader.GetAttribute("Name");
            int x = Convert.ToInt32(reader.GetAttribute("X"));
            int y = Convert.ToInt32(reader.GetAttribute("Y"));
            int width = Convert.ToInt32(reader.GetAttribute("Width"));
            int height = Convert.ToInt32(reader.GetAttribute("Height"));
            ushort alpha = Convert.ToUInt16(reader.GetAttribute("Alpha"));

            string text = reader.GetAttribute("Text");
            int i = text.IndexOf("\\n");
            while (i > -1)
            {
                text = text.Substring(0, i) + "\n" + text.Substring(i + 2, text.Length - (i + 2));
                i = text.IndexOf("\\n");
            }

            Font font = GlideUtils.Convert.ToFont(reader.GetAttribute("Font"));
            Color fontColor = GlideUtils.Convert.ToColor(reader.GetAttribute("FontColor"));
            Color disabledFontColor = GlideUtils.Convert.ToColor(reader.GetAttribute("DisabledFontColor"));
            Color tintColor = GlideUtils.Convert.ToColor(reader.GetAttribute("TintColor"));
            ushort tintAmount = Convert.ToUInt16(reader.GetAttribute("TintAmount"));

            Button button = new Button(name, alpha, x, y, width, height);
            button.Text = text;
            button.Font = font;
            button.FontColor = fontColor;
            button.DisabledFontColor = disabledFontColor;
            button.TintColor = tintColor;
            button.TintAmount = tintAmount;

            return button;
        }

        /*
        /// <summary>
        /// Parses the Button XML into a UI component.
        /// </summary>
        /// <param name="reader">XML reader object.</param>
        /// <returns>Button object.</returns>
        private static Button LoadButton(XmlReader reader)
        {
            //GlideUtils.Timer.Start();

            reader.MoveToAttribute(0);
            string name = reader.Value;
            reader.MoveToNextAttribute();
            int x = Convert.ToInt32(reader.Value);
            reader.MoveToNextAttribute();
            int y = Convert.ToInt32(reader.Value);
            reader.MoveToNextAttribute();
            int width = Convert.ToInt32(reader.Value);
            reader.MoveToNextAttribute();
            int height = Convert.ToInt32(reader.Value);
            reader.MoveToNextAttribute();
            ushort alpha = Convert.ToUInt16(reader.Value);

            Button button = new Button(name, alpha, x, y, width, height);

            if (reader.MoveToNextAttribute())
                button.Text = reader.Value;

            if (reader.MoveToNextAttribute())
                button.Font = GlideUtils.Convert.ToFont(reader.Value);

            if (reader.MoveToNextAttribute())
                button.FontColor = GlideUtils.Convert.ToColor(reader.Value);

            if (reader.MoveToNextAttribute())
                button.DisabledFontColor = GlideUtils.Convert.ToColor(reader.Value);

            //GlideUtils.Timer.Stop(button.Name + " loaded");

            return button;
        }
        */

        /// <summary>
        /// Parses the CheckBox XML into a UI component.
        /// </summary>
        /// <param name="reader">XML reader object.</param>
        /// <returns>CheckBox object.</returns>
        private static CheckBox LoadCheckBox(XmlReader reader)
        {
            string name = reader.GetAttribute("Name");
            ushort alpha = (reader.GetAttribute("Alpha") != null) ? Convert.ToUInt16(reader.GetAttribute("Alpha")) : _defaultDisplayObject.Alpha;
            int x = Convert.ToInt32(reader.GetAttribute("X"));
            int y = Convert.ToInt32(reader.GetAttribute("Y"));
            bool visible = (reader.GetAttribute("Visible") != null) ? (reader.GetAttribute("Visible") == bool.TrueString) : _defaultDisplayObject.Visible;
            bool enabled = (reader.GetAttribute("Enabled") != null) ? (reader.GetAttribute("Enabled") == bool.TrueString) : _defaultDisplayObject.Enabled;

            bool _checked = (reader.GetAttribute("Checked") == bool.TrueString) ? true : false;

            CheckBox checkBox = new CheckBox(name, alpha, x, y);
            checkBox.Visible = visible;
            checkBox.Enabled = enabled;
            checkBox.Checked = _checked;

            return checkBox;
        }

        /// <summary>
        /// Parses the Dropdown XML into a UI component.
        /// </summary>
        /// <param name="reader">XML reader object.</param>
        /// <returns>Dropdown object.</returns>
        private static Dropdown LoadDropdown(XmlReader reader)
        {
            string name = reader.GetAttribute("Name");
            ushort alpha = (reader.GetAttribute("Alpha") != null) ? Convert.ToUInt16(reader.GetAttribute("Alpha")) : _defaultDisplayObject.Alpha;
            int x = Convert.ToInt32(reader.GetAttribute("X"));
            int y = Convert.ToInt32(reader.GetAttribute("Y"));
            int width = Convert.ToInt32(reader.GetAttribute("Width"));
            int height = Convert.ToInt32(reader.GetAttribute("Height"));
            bool visible = (reader.GetAttribute("Visible") != null) ? (reader.GetAttribute("Visible") == bool.TrueString) : _defaultDisplayObject.Visible;
            bool enabled = (reader.GetAttribute("Enabled") != null) ? (reader.GetAttribute("Enabled") == bool.TrueString) : _defaultDisplayObject.Enabled;

            string text = reader.GetAttribute("Text");
            Font font = GlideUtils.Convert.ToFont(reader.GetAttribute("Font"));
            Color fontColor = GlideUtils.Convert.ToColor(reader.GetAttribute("FontColor"));

            Dropdown dropdown = new Dropdown(name, alpha, x, y, width, height);
            dropdown.Visible = visible;
            dropdown.Enabled = enabled;
            dropdown.Text = text;
            dropdown.Font = font;
            dropdown.FontColor = fontColor;

            if (!reader.IsEmptyElement)
            {
                dropdown.Options = new ArrayList();
                while (reader.Read() && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "Dropdown"))
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "Option")
                    {
                        // Apparently if you readstring before getattribute you'll lose position and it cannot find the attribute.
                        string value = reader.GetAttribute("Value");
                        object[] item = new object[2] { reader.ReadString(), value };
                        dropdown.Options.Add(item);
                    }
                }
            }

            return dropdown;
        }

        /// <summary>
        /// Parses the DataGrid XML into a UI component.
        /// </summary>
        /// <param name="reader">XML reader object.</param>
        /// <returns>Datagrid object.</returns>
        private static DataGrid LoadDataGrid(XmlReader reader)
        {
            string name = reader.GetAttribute("Name");
            ushort alpha = (reader.GetAttribute("Alpha") != null) ? Convert.ToUInt16(reader.GetAttribute("Alpha")) : _defaultDisplayObject.Alpha;
            int x = Convert.ToInt32(reader.GetAttribute("X"));
            int y = Convert.ToInt32(reader.GetAttribute("Y"));
            int width = Convert.ToInt32(reader.GetAttribute("Width"));
            bool visible = (reader.GetAttribute("Visible") != null) ? (reader.GetAttribute("Visible") == bool.TrueString) : _defaultDisplayObject.Visible;
            bool enabled = (reader.GetAttribute("Enabled") != null) ? (reader.GetAttribute("Enabled") == bool.TrueString) : _defaultDisplayObject.Enabled;

            Font font = GlideUtils.Convert.ToFont(reader.GetAttribute("Font"));
            int rowHeight = Convert.ToInt32(reader.GetAttribute("RowHeight"));
            int rowCount = Convert.ToInt32(reader.GetAttribute("RowCount"));

            bool draggable = (reader.GetAttribute("Draggable") == bool.TrueString) ? true : false;
            bool tappableCells = (reader.GetAttribute("TappableCells") == bool.TrueString) ? true : false;
            bool sortableHeaders = (reader.GetAttribute("SortableHeaders") == bool.TrueString) ? true : false;
            bool showHeaders = (reader.GetAttribute("ShowHeaders") == bool.TrueString) ? true : false;
            bool showScrollbar = (reader.GetAttribute("ShowScrollbar") == bool.TrueString) ? true : false;
            int scrollbarWidth = Convert.ToInt32(reader.GetAttribute("ScrollbarWidth"));

            Color headersBackColor = GlideUtils.Convert.ToColor(reader.GetAttribute("HeadersBackColor"));
            Color headersFontColor = GlideUtils.Convert.ToColor(reader.GetAttribute("HeadersFontColor"));
            Color itemsBackColor = GlideUtils.Convert.ToColor(reader.GetAttribute("ItemsBackColor"));
            Color itemsAltBackColor = GlideUtils.Convert.ToColor(reader.GetAttribute("ItemsAltBackColor"));
            Color itemsFontColor = GlideUtils.Convert.ToColor(reader.GetAttribute("ItemsFontColor"));
            Color selectedItemBackColor = GlideUtils.Convert.ToColor(reader.GetAttribute("SelectedItemBackColor"));
            Color selectedItemFontColor = GlideUtils.Convert.ToColor(reader.GetAttribute("SelectedItemFontColor"));
            Color gridColor = GlideUtils.Convert.ToColor(reader.GetAttribute("GridColor"));
            Color scrollbarBackColor = GlideUtils.Convert.ToColor(reader.GetAttribute("ScrollbarBackColor"));
            Color scrollbarScrubberColor = GlideUtils.Convert.ToColor(reader.GetAttribute("ScrollbarScrubberColor"));

            DataGrid dataGrid = new DataGrid(name, alpha, x, y, width, rowHeight, rowCount);
            dataGrid.Visible = visible;
            dataGrid.Enabled = enabled;
            dataGrid.Font = font;

            dataGrid.Draggable = draggable;
            dataGrid.TappableCells = tappableCells;
            dataGrid.SortableHeaders = sortableHeaders;
            dataGrid.ShowHeaders = showHeaders;
            dataGrid.ShowScrollbar = showScrollbar;
            dataGrid.ScrollbarWidth = scrollbarWidth;

            dataGrid.HeadersBackColor = headersBackColor;
            dataGrid.HeadersFontColor = headersFontColor;
            dataGrid.ItemsBackColor = itemsBackColor;
            dataGrid.ItemsAltBackColor = itemsAltBackColor;
            dataGrid.ItemsFontColor = itemsFontColor;
            dataGrid.SelectedItemBackColor = selectedItemBackColor;
            dataGrid.SelectedItemFontColor = selectedItemFontColor;
            dataGrid.GridColor = gridColor;
            dataGrid.ShowScrollbar = showScrollbar;
            dataGrid.ScrollbarWidth = scrollbarWidth;

            return dataGrid;
        }

        /// <summary>
        /// Parses the Image XML into a UI component.
        /// </summary>
        /// <param name="reader">XML reader object.</param>
        /// <returns>Image object.</returns>
        private static UI.Image LoadImage(XmlReader reader)
        {
            string name = reader.GetAttribute("Name");
            ushort alpha = (reader.GetAttribute("Alpha") != null) ? Convert.ToUInt16(reader.GetAttribute("Alpha")) : _defaultDisplayObject.Alpha;
            int x = Convert.ToInt32(reader.GetAttribute("X"));
            int y = Convert.ToInt32(reader.GetAttribute("Y"));
            int width = Convert.ToInt32(reader.GetAttribute("Width"));
            int height = Convert.ToInt32(reader.GetAttribute("Height"));
            bool visible = (reader.GetAttribute("Visible") != null) ? (reader.GetAttribute("Visible") == bool.TrueString) : _defaultDisplayObject.Visible;
            bool enabled = (reader.GetAttribute("Enabled") != null) ? (reader.GetAttribute("Enabled") == bool.TrueString) : _defaultDisplayObject.Enabled;

            UI.Image image = new UI.Image(name, alpha, x, y, width, height);
            image.Visible = visible;
            image.Enabled = enabled;

            return image;
        }

        /// <summary>
        /// Parses the PasswordBox XML into a UI component.
        /// </summary>
        /// <param name="reader">XML reader object.</param>
        /// <returns>PasswordBox object.</returns>
        private static PasswordBox LoadPasswordBox(XmlReader reader)
        {
            string name = reader.GetAttribute("Name");
            ushort alpha = (reader.GetAttribute("Alpha") != null) ? Convert.ToUInt16(reader.GetAttribute("Alpha")) : _defaultDisplayObject.Alpha;
            int x = Convert.ToInt32(reader.GetAttribute("X"));
            int y = Convert.ToInt32(reader.GetAttribute("Y"));
            int width = Convert.ToInt32(reader.GetAttribute("Width"));
            int height = Convert.ToInt32(reader.GetAttribute("Height"));
            bool visible = (reader.GetAttribute("Visible") != null) ? (reader.GetAttribute("Visible") == bool.TrueString) : _defaultDisplayObject.Visible;
            bool enabled = (reader.GetAttribute("Enabled") != null) ? (reader.GetAttribute("Enabled") == bool.TrueString) : _defaultDisplayObject.Enabled;

            string text = reader.GetAttribute("Text");
            uint textAlign = GlideUtils.Convert.ToAlignment(reader.GetAttribute("TextAlign"));
            Font font = GlideUtils.Convert.ToFont(reader.GetAttribute("Font"));
            Color fontColor = GlideUtils.Convert.ToColor(reader.GetAttribute("FontColor"));

            PasswordBox passwordBox = new PasswordBox(name, alpha, x, y, width, height);
            passwordBox.Visible = visible;
            passwordBox.Enabled = enabled;
            passwordBox.Text = text;
            passwordBox.TextAlign = FromAlignment(textAlign);
            passwordBox.Font = font;
            passwordBox.FontColor = fontColor;

            return passwordBox;
        }
        static StringFormat FromAlignment(uint Alignment)
        {
            var format = new StringFormat();
            switch (Alignment)
            {
                case TinyCLR2.Glide.BitmapHelper.DT_AlignmentCenter:
                    format.Alignment = StringAlignment.Center; 
                    break;
                case TinyCLR2.Glide.BitmapHelper.DT_AlignmentLeft:
                    format.Alignment = StringAlignment.Near;
                    break;
                case TinyCLR2.Glide.BitmapHelper.DT_AlignmentRight:
                    format.Alignment = StringAlignment.Far;
                    break;
                default: throw new ArgumentException();
            }
            return format;
        }
        /// <summary>
        /// Parses the ProgressBar XML into a UI component.
        /// </summary>
        /// <param name="reader">XML reader object.</param>
        /// <returns>ProgressBar object.</returns>
        private static ProgressBar LoadProgressBar(XmlReader reader)
        {
            string name = reader.GetAttribute("Name");
            ushort alpha = (reader.GetAttribute("Alpha") != null) ? Convert.ToUInt16(reader.GetAttribute("Alpha")) : _defaultDisplayObject.Alpha;
            int x = Convert.ToInt32(reader.GetAttribute("X"));
            int y = Convert.ToInt32(reader.GetAttribute("Y"));
            int width = Convert.ToInt32(reader.GetAttribute("Width"));
            int height = Convert.ToInt32(reader.GetAttribute("Height"));
            bool visible = (reader.GetAttribute("Visible") != null) ? (reader.GetAttribute("Visible") == bool.TrueString) : _defaultDisplayObject.Visible;
            bool enabled = (reader.GetAttribute("Enabled") != null) ? (reader.GetAttribute("Enabled") == bool.TrueString) : _defaultDisplayObject.Enabled;

            Direction direction;
            switch (reader.GetAttribute("Direction"))
            {
                case "Up":
                    direction = Direction.Up;
                    break;
                case "Left":
                    direction = Direction.Left;
                    break;
                case "Down":
                    direction = Direction.Down;
                    break;
                case "Right":
                default:
                    direction = Direction.Right;
                    break;
            }
            int maxValue = Convert.ToInt32(reader.GetAttribute("MaxValue"));
            int value = Convert.ToInt32(reader.GetAttribute("Value"));

            ProgressBar progressBar = new ProgressBar(name, alpha, x, y, width, height);
            progressBar.Visible = visible;
            progressBar.Enabled = enabled;
            progressBar.Direction = direction;
            progressBar.MaxValue = maxValue;
            progressBar.Value = value;

            return progressBar;
        }

        /// <summary>
        /// Parses the RadioButton XML into a UI component.
        /// </summary>
        /// <param name="reader">XML reader object.</param>
        /// <returns>RadioButton object.</returns>
        private static RadioButton LoadRadioButton(XmlReader reader)
        {
            string name = reader.GetAttribute("Name");
            int x = Convert.ToInt32(reader.GetAttribute("X"));
            int y = Convert.ToInt32(reader.GetAttribute("Y"));
            int width = Convert.ToInt32(reader.GetAttribute("Width"));
            int height = Convert.ToInt32(reader.GetAttribute("Height"));
            ushort alpha = Convert.ToUInt16(reader.GetAttribute("Alpha"));

            string value = reader.GetAttribute("Value");
            bool _checked = (reader.GetAttribute("Checked") == bool.TrueString) ? true : false;
            string groupName = reader.GetAttribute("GroupName");
            bool showBackground = (reader.GetAttribute("ShowBackground") == bool.TrueString) ? true : false;
            Color color = GlideUtils.Convert.ToColor(reader.GetAttribute("Color"));
            Color outlineColor = GlideUtils.Convert.ToColor(reader.GetAttribute("OutlineColor"));
            Color selectedColor = GlideUtils.Convert.ToColor(reader.GetAttribute("SelectedColor"));
            Color selectedOutlineColor = GlideUtils.Convert.ToColor(reader.GetAttribute("SelectedOutlineColor"));

            RadioButton radioButton = new RadioButton(name, alpha, x, y, width, height);
            radioButton.Value = value;
            radioButton.Checked = _checked;
            radioButton.GroupName = groupName;
            radioButton.ShowBackground = showBackground;
            radioButton.Color = color;
            radioButton.OutlineColor = outlineColor;
            radioButton.SelectedColor = selectedColor;
            radioButton.SelectedOutlineColor = selectedOutlineColor;

            RadioButtonManager.AddButton(radioButton);

            return radioButton;
        }

        /// <summary>
        /// Parses the Slider XML into a UI component.
        /// </summary>
        /// <param name="reader">XML reader object.</param>
        /// <returns>Slider object.</returns>
        private static Slider LoadSlider(XmlReader reader)
        {
            string name = reader.GetAttribute("Name");
            ushort alpha = (reader.GetAttribute("Alpha") != null) ? Convert.ToUInt16(reader.GetAttribute("Alpha")) : _defaultDisplayObject.Alpha;
            int x = Convert.ToInt32(reader.GetAttribute("X"));
            int y = Convert.ToInt32(reader.GetAttribute("Y"));
            int width = Convert.ToInt32(reader.GetAttribute("Width"));
            int height = Convert.ToInt32(reader.GetAttribute("Height"));
            bool visible = (reader.GetAttribute("Visible") != null) ? (reader.GetAttribute("Visible") == bool.TrueString) : _defaultDisplayObject.Visible;
            bool enabled = (reader.GetAttribute("Enabled") != null) ? (reader.GetAttribute("Enabled") == bool.TrueString) : _defaultDisplayObject.Enabled;

            string direction = reader.GetAttribute("Direction");
            int snapInterval = Convert.ToInt32(reader.GetAttribute("SnapInterval"));
            int tickInterval = Convert.ToInt32(reader.GetAttribute("TickInterval"));
            Color tickColor = GlideUtils.Convert.ToColor(reader.GetAttribute("TickColor"));
            int knobSize = Convert.ToInt32(reader.GetAttribute("KnobSize"));
            double minimum = Convert.ToDouble(reader.GetAttribute("Minimum"));
            double maximum = Convert.ToDouble(reader.GetAttribute("Maximum"));
            double value = Convert.ToDouble(reader.GetAttribute("Value"));

            Slider slider = new Slider(name, alpha, x, y, width, height);
            slider.Visible = visible;
            slider.Enabled = enabled;
            slider.Direction = direction;
            slider.SnapInterval = snapInterval;
            slider.TickInterval = tickInterval;
            slider.TickColor = tickColor;
            slider.KnobSize = knobSize;
            slider.Minimum = minimum;
            slider.Maximum = maximum;
            slider.Value = value;

            return slider;
        }

        /// <summary>
        /// Parses the TextBlock XML into a UI component.
        /// </summary>
        /// <param name="reader">XML reader object.</param>
        /// <returns>TextBlock object.</returns>
        private static TextBlock LoadTextBlock(XmlReader reader)
        {
            string name = reader.GetAttribute("Name");
            ushort alpha = (reader.GetAttribute("Alpha") != null) ? Convert.ToUInt16(reader.GetAttribute("Alpha")) : _defaultDisplayObject.Alpha;
            int x = Convert.ToInt32(reader.GetAttribute("X"));
            int y = Convert.ToInt32(reader.GetAttribute("Y"));
            int width = Convert.ToInt32(reader.GetAttribute("Width"));
            int height = Convert.ToInt32(reader.GetAttribute("Height"));
            bool visible = (reader.GetAttribute("Visible") != null) ? (reader.GetAttribute("Visible") == bool.TrueString) : _defaultDisplayObject.Visible;
            bool enabled = (reader.GetAttribute("Enabled") != null) ? (reader.GetAttribute("Enabled") == bool.TrueString) : _defaultDisplayObject.Enabled;

            string text = reader.GetAttribute("Text");
            HorizontalAlignment textAlign = GlideUtils.Convert.ToHorizontalAlign(reader.GetAttribute("TextAlign"));
            VerticalAlignment textVerticalAlign = GlideUtils.Convert.ToVerticalAlign(reader.GetAttribute("TextVerticalAlign"));
            Font font = GlideUtils.Convert.ToFont(reader.GetAttribute("Font"));
            Color fontColor = GlideUtils.Convert.ToColor(reader.GetAttribute("FontColor"));
            Color backColor = GlideUtils.Convert.ToColor(reader.GetAttribute("BackColor"));
            bool showBackColor = (reader.GetAttribute("ShowBackColor") == bool.TrueString) ? true : false;

            TextBlock textBlock = new TextBlock(name, alpha, x, y, width, height);
            textBlock.Visible = visible;
            textBlock.Enabled = enabled;
            textBlock.Text = text;
            textBlock.TextAlign = textAlign;
            textBlock.TextVerticalAlign = textVerticalAlign;
            textBlock.Font = font;
            textBlock.FontColor = fontColor;
            textBlock.BackColor = backColor;
            textBlock.ShowBackColor = showBackColor;

            return textBlock;
        }

        /// <summary>
        /// Parses the TextBox XML into a UI component.
        /// </summary>
        /// <param name="reader">XML reader object.</param>
        /// <returns>TextBox object.</returns>
        private static TextBox LoadTextBox(XmlReader reader)
        {
            string name = reader.GetAttribute("Name");
            ushort alpha = (reader.GetAttribute("Alpha") != null) ? Convert.ToUInt16(reader.GetAttribute("Alpha")) : _defaultDisplayObject.Alpha;
            int x = Convert.ToInt32(reader.GetAttribute("X"));
            int y = Convert.ToInt32(reader.GetAttribute("Y"));
            int width = Convert.ToInt32(reader.GetAttribute("Width"));
            int height = Convert.ToInt32(reader.GetAttribute("Height"));
            bool visible = (reader.GetAttribute("Visible") != null) ? (reader.GetAttribute("Visible") == bool.TrueString) : _defaultDisplayObject.Visible;
            bool enabled = (reader.GetAttribute("Enabled") != null) ? (reader.GetAttribute("Enabled") == bool.TrueString) : _defaultDisplayObject.Enabled;

            string text = reader.GetAttribute("Text");
            uint textAlign = GlideUtils.Convert.ToAlignment(reader.GetAttribute("TextAlign"));
            Font font = GlideUtils.Convert.ToFont(reader.GetAttribute("Font"));
            Color fontColor = GlideUtils.Convert.ToColor(reader.GetAttribute("FontColor"));

            TextBox textBox = new TextBox(name, alpha, x, y, width, height);
            textBox.Visible = visible;
            textBox.Enabled = enabled;
            textBox.Text = text;
            textBox.TextAlign = FromAlignment(textAlign);
            textBox.Font = font;
            textBox.FontColor = fontColor;

            return textBox;
        }
    }
}
