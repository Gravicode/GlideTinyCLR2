////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
//using Microsoft.SPOT;
using GHI.Glide.Display;
using GHI.Glide.Geom;
using System.Drawing;
namespace GHI.Glide.UI
{
    /// <summary>
    /// The Keyboard component is an on-screen keyboard that allows user input.
    /// </summary>
    /// <remarks>The Keyboard component should be used in conjunction with a TextBox or PasswordBox. Please see those components for further details.</remarks>
    public class Keyboard : DisplayObject
    {
        private int _keyHeight;
        private int _keySpacing;
        private string[][][] _keyContent;
        private int[][][] _keyWidth;
        private string[] _keyValues;
        private Geom.Rectangle[] _keyCoords;
        private bool[] _keyActive;
        private int _bitmapIndex = 0;
        private int _bitmapX = 0;
        private View _view = View.Letters;
        private int _downIndex = -1;
        private bool _tapHold = false;
        private TouchEventArgs _touchEventArgs;
        private bool _keepAlive = false;

        /// <summary>
        /// Creates a new Keyboard.
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="numViews">Number of views.</param>
        /// <param name="keyHeight">Key height.</param>
        /// <param name="keySpacing">Key spacing.</param>
        public Keyboard(int width, int height, int numViews, int keyHeight, int keySpacing)
        {
            Name = "keyboard";
            Alpha = 255;
            X = 0;
            Y = Glide.LCD.Height - height;
            Width = width;
            Height = height;

            _keyContent = new string[numViews][][];
            _keyWidth = new int[numViews][][];
            _keyHeight = keyHeight;
            _keySpacing = keySpacing;
            _bitmapX = (Glide.LCD.Width - width) / 2;

            Uppercase = true;

            DefaultKeyContent();
            DefaultKeyWidth();
            CalculateKeys();
        }

        /// <summary>
        /// A key entered the up state.
        /// </summary>
        public event OnTapKey TapKeyEvent;

        /// <summary>
        /// Triggers a key up event.
        /// </summary>
        /// <param name="sender">Object associated with the event.</param>
        /// <param name="args">Key up event arguments.</param>
        public void TriggerTapKeyEvent(object sender, TapKeyEventArgs args)
        {
            if (TapKeyEvent != null)
                TapKeyEvent(sender, args);
        }

        /// <summary>
        /// Renders the Keyboard onto it's parent container's graphics.
        /// </summary>
        public override void Render()
        {
            int x = Parent.X + X;
            int y = Parent.Y + Y;

            Parent.Graphics.DrawRectangle(TinyCLR2.Glide.Ext.Colors.Black, 0, x, y, Glide.LCD.Width, Height, 0, 0, TinyCLR2.Glide.Ext.Colors.DarkGray, 0, 0, TinyCLR2.Glide.Ext.Colors.Black, 0, 0, 255);
            Parent.Graphics.DrawImage(_bitmapX, y, BitmapUp[_bitmapIndex], 0, 0, Width, Height);

            for (int i = 0; i < _keyActive.Length; i++)
            {
                if (!_keyActive[i])
                    Parent.Graphics.DrawRectangle(_keyCoords[i], TinyCLR2.Glide.Ext.Colors.DarkGray, 200);
            }
        }

        /// <summary>
        /// Renders and flushes the specified key within the current view.
        /// </summary>
        /// <param name="index"></param>
        public void DrawKeyDown(int index)
        {
            if (index > -1 && index < _keyValues.Length)
            {
                Geom.Rectangle keyCoord = _keyCoords[index];
                Parent.Graphics.DrawRectangle(TinyCLR2.Glide.Ext.Colors.Black, 0, keyCoord.X, keyCoord.Y, keyCoord.Width, keyCoord.Height, 0, 0, TinyCLR2.Glide.Ext.Colors.Black, 0, 0, TinyCLR2.Glide.Ext.Colors.Black, 0, 0, 0xff);
                Glide.Flush(keyCoord);
            }
            else
            {
                string currentView = String.Empty;
                switch (_view)
                {
                    case View.Letters:
                        currentView = "Letters";
                        break;
                    case View.Numbers:
                        currentView = "Numbers";
                        break;
                    case View.Symbols:
                        currentView = "Symbols";
                        break;
                }
                throw new ArgumentOutOfRangeException("index", "Index does not exist within the " + currentView + " view.");
            }
        }

        /// <summary>
        /// Renders this display object and flushes it to the screen.
        /// </summary>
        public override void Invalidate()
        {
            // We must do this because the keyboard's width and height only match the art dimensions.
            // The actual size runs the whole screen length.
            Render();
            Glide.Flush(Parent.X + X, Parent.Y + Y, Glide.LCD.Width, Height);
        }

        /// <summary>
        /// Handles the touch down event.
        /// </summary>
        /// <param name="e">Touch event arguments.</param>
        /// <returns>Touch event arguments.</returns>
        public override TouchEventArgs OnTouchDown(TouchEventArgs e)
        {
            Geom.Rectangle keyCoord;
            for (int i = 0; i < _keyCoords.Length; i++)
            {
                keyCoord = _keyCoords[i];

                if (keyCoord.Contains(e.Point) && _keyActive[i] && (_keyValues[i] != null && _keyValues[i] != ""))
                {
                    _downIndex = i;

                    DrawKeyDown(i);

                    if (_keyValues[i] == ActionKey.Backspace)
                        _touchEventArgs = e;

                    e.StopPropagation();
                    break;
                }
            }
            return e;
        }

        /// <summary>
        /// Handles the touch up event.
        /// </summary>
        /// <param name="e">Touch event arguments.</param>
        /// <returns>Touch event arguments.</returns>
        public override TouchEventArgs OnTouchUp(TouchEventArgs e)
        {
            // A key must be down
            if (_downIndex == -1)
                return e;

            _touchEventArgs = null;

            // If we just finished holding backspace down
            // avoid doing more logic than needed.
            if (_tapHold)
            {
                _tapHold = false;
                _downIndex = -1;
                Invalidate();
                e.StopPropagation();
                return e;
            }

            Geom.Rectangle keyCoord;
            for (int i = 0; i < _keyCoords.Length; i++)
            {
                keyCoord = _keyCoords[i];

                if (keyCoord.Contains(e.Point) && _downIndex == i)
                {
                    switch (_keyValues[i])
                    {
                        case ActionKey.ToggleCase:
                            Uppercase = !Uppercase;
                            if (Uppercase)
                                _bitmapIndex = 0;
                            else
                                _bitmapIndex = 1;

                            _downIndex = -1;
                            Invalidate();
                            e.StopPropagation();
                            return e;

                        case ActionKey.ToSymbols:
                            _view = View.Symbols;
                            CalculateKeys();
                            _bitmapIndex = 3;

                            _downIndex = -1;
                            Invalidate();
                            e.StopPropagation();
                            return e;

                        case ActionKey.ToNumbers:
                            _view = View.Numbers;
                            CalculateKeys();
                            _bitmapIndex = 2;

                            _downIndex = -1;
                            Invalidate();
                            e.StopPropagation();
                            return e;

                        case ActionKey.ToLetters:
                            _view = View.Letters;
                            CalculateKeys();
                            Uppercase = true;
                            _bitmapIndex = 0;

                            _downIndex = -1;
                            Invalidate();
                            e.StopPropagation();
                            return e;

                        default:
                            if (_keyValues[i] != ActionKey.Return)
                            {
                                _downIndex = -1;
                                Invalidate();
                            }

                            // Triggering a TapKeyEvent will reset the keyboard and make the current index (i) out of range.
                            // Switching to numbers makes keyValues 34 length
                            // Tapping done is index 33 causing the keyboard to close and reset.
                            // When reset, we switch back to letters and keyValues becomes 33 length
                            // Now our index (33) is out of range.
                            TriggerTapKeyEvent(this, new TapKeyEventArgs((Uppercase) ? _keyValues[i].ToUpper() : _keyValues[i], i));

                            e.StopPropagation();
                            return e;
                    }
                }
            }

            // Released outside of keyboard
            _downIndex = -1;
            Invalidate();

            return e;
        }

        /// <summary>
        /// Disposes all disposable objects in this object.
        /// </summary>
        public override void Dispose()
        {
            int i;

            for (i = 0; i < BitmapUp.Length; i++)
                BitmapUp[i].Dispose();
        }

        /// <summary>
        /// Sets a view's key content.
        /// </summary>
        /// <param name="view">KeyboardView</param>
        /// <param name="keyContent">Array containing the key content.</param>
        public void SetViewKeyContent(View view, string[][] keyContent)
        {
            int i = (int)view;
            _keyContent[i] = new string[keyContent.Length][];
            for (int j = 0; j < keyContent.Length; j++)
                _keyContent[i][j] = keyContent[j];
        }

        /// <summary>
        /// Sets a view's key width.
        /// </summary>
        /// <param name="view">KeyboardView</param>
        /// <param name="keyWidth">Array containing the key widths.</param>
        public void SetViewKeyWidth(View view, int[][] keyWidth)
        {
            int i = (int)view;
            _keyWidth[i] = new int[keyWidth.Length][];
            for (int j = 0; j < keyWidth.Length; j++)
                _keyWidth[i][j] = keyWidth[j];
        }

        /// <summary>
        /// Starts the keyboard.
        /// </summary>
        public void Start()
        {
            if (!_keepAlive)
            {
                _keepAlive = true;
                Thread thread = new Thread(IsTapHold);
                thread.Priority = ThreadPriority.Normal;
                thread.Start();
            }
        }

        /// <summary>
        /// Stops the keyboard.
        /// </summary>
        public void Stop()
        {
            _view = View.Letters;
            Uppercase = true;
            _downIndex = -1;
            CalculateKeys();
            _bitmapIndex = 0;
            _keepAlive = false;
            Render();
        }

        /// <summary>
        /// Set the keyboard view.
        /// </summary>
        public void SetView(View view)
        {
            _view = view;
            Uppercase = true;

            switch (view)
            {
                case View.Letters:
                    _bitmapIndex = 0;
                    break;
                case View.Numbers:
                    _bitmapIndex = 2;
                    break;
                case View.Symbols:
                    _bitmapIndex = 3;
                    break;
            }

            _downIndex = -1;
            CalculateKeys();
        }

        /// <summary>
        /// Calculates the space each key occupies using the X and Y offset.
        /// </summary>
        /// <remarks>The keys must be centered and spaced exactly the same.</remarks>
        public void CalculateKeys()
        {
            int size = GetContentLength();
            _keyValues = new string[size];
            _keyCoords = new Geom.Rectangle[size];
            _keyActive = new bool[size];

            int x, y, index = 0;

            // Loop through each row of keys
            for (int i = 0; i < _keyContent[(int)_view].Length; i++)
            {
                // Starting X (centered) and Y
                x = X + (((Glide.LCD.Width - GetKeysWidth(i)) - (_keyContent[(int)_view][i].Length - 1) * _keySpacing) / 2);
                y = Y + (_keySpacing + (i * _keyHeight) + (i * _keySpacing));

                for (int j = 0; j < _keyContent[(int)_view][i].Length; j++)
                {
                    _keyValues[index] = _keyContent[(int)_view][i][j];
                    _keyCoords[index] = new Geom.Rectangle(x, y, _keyWidth[(int)_view][i][j], _keyHeight);
                    _keyActive[index] = true;

                    for (int k = 0; k < Restricted.Length; k++)
                    {
                        if (_keyValues[index].ToLower() == Restricted[k].ToLower())
                        {
                            _keyActive[index] = false;
                            break;
                        }
                    }

                    index++;
                    x += _keyWidth[(int)_view][i][j] + _keySpacing;
                }
            }
        }

        /// <summary>
        /// Sets the default key content.
        /// </summary>
        public void DefaultKeyContent()
        {
            string[][] keyContent = new string[4][];

            // Letters
            keyContent[0] = new string[10] { "q", "w", "e", "r", "t", "y", "u", "i", "o", "p" };
            keyContent[1] = new string[9] { "a", "s", "d", "f", "g", "h", "j", "k", "l" };
            keyContent[2] = new string[9] { ActionKey.ToggleCase, "z", "x", "c", "v", "b", "n", "m", ActionKey.Backspace };
            keyContent[3] = new string[5] { ActionKey.ToNumbers, ",", ActionKey.Space, ".", ActionKey.Return };
            SetViewKeyContent(View.Letters, keyContent);

            // Numbers
            keyContent[0] = new string[10] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
            keyContent[1] = new string[10] { "@", "#", "$", "%", "&", "*", "-", "+", "(", ")" };
            keyContent[2] = new string[9] { ActionKey.ToSymbols, "!", "\"", "'", ":", ";", "/", "?", ActionKey.Backspace };
            keyContent[3] = new string[5] { ActionKey.ToLetters, ",", ActionKey.Space, ".", ActionKey.Return };
            SetViewKeyContent(Keyboard.View.Numbers, keyContent);

            // Symbols
            keyContent[0] = new string[10] { "~", "`", "|", "•", "√", "π", "÷", "×", "{", "}" };
            keyContent[1] = new string[10] { ActionKey.Tab, "£", "¢", "€", "º", "^", "_", "=", "[", "]" };
            keyContent[2] = new string[9] { ActionKey.ToNumbers, "™", "®", "©", "¶", "\\", "<", ">", ActionKey.Backspace };
            keyContent[3] = new string[5] { ActionKey.ToLetters, ",", ActionKey.Space, ".", ActionKey.Return };
            SetViewKeyContent(Keyboard.View.Symbols, keyContent);
        }

        /// <summary>
        /// Sets the default key width.
        /// </summary>
        public void DefaultKeyWidth()
        {
            int[][] keyWidth = new int[4][];

            // Letters
            keyWidth[0] = new int[10] { 32, 32, 32, 32, 32, 32, 32, 32, 32, 32 };
            keyWidth[1] = new int[9] { 32, 32, 32, 32, 32, 32, 32, 32, 32 };
            keyWidth[2] = new int[9] { 48, 32, 32, 32, 32, 32, 32, 32, 48 };
            keyWidth[3] = new int[5] { 48, 32, 160, 32, 48 };
            SetViewKeyWidth(View.Letters, keyWidth);

            // Numbers
            keyWidth[0] = new int[10] { 32, 32, 32, 32, 32, 32, 32, 32, 32, 32 };
            keyWidth[1] = new int[10] { 32, 32, 32, 32, 32, 32, 32, 32, 32, 32 };
            keyWidth[2] = new int[9] { 48, 32, 32, 32, 32, 32, 32, 32, 48 };
            keyWidth[3] = new int[5] { 48, 32, 160, 32, 48 };
            SetViewKeyWidth(View.Numbers, keyWidth);

            // Symbols
            keyWidth[0] = new int[10] { 32, 32, 32, 32, 32, 32, 32, 32, 32, 32 };
            keyWidth[1] = new int[10] { 32, 32, 32, 32, 32, 32, 32, 32, 32, 32 };
            keyWidth[2] = new int[9] { 48, 32, 32, 32, 32, 32, 32, 32, 48 };
            keyWidth[3] = new int[5] { 48, 32, 160, 32, 48 };
            SetViewKeyWidth(View.Symbols, keyWidth);
        }

        private int GetContentLength()
        {
            int length = 0;
            for (int i = 0; i < _keyContent[(int)_view].Length; i++)
            {
                for (int j = 0; j < _keyContent[(int)_view][i].Length; j++)
                    length++;
            }
            return length;
        }

        // Get the width for a row of keys.
        private int GetKeysWidth(int index)
        {
            int width = 0;
            for (int i = 0; i < _keyWidth[(int)_view][index].Length; i++)
                width += _keyWidth[(int)_view][index][i];
            return width;
        }

        private void IsTapHold()
        {
            while (_keepAlive)
            {
                // Wait for button down
                while (_touchEventArgs == null)
                    Thread.Sleep(50);

                // Wait for tap hold
                Thread.Sleep(500);

                if (_downIndex == -1)
                    continue;

                _tapHold = true;

                int time = 500;
                int interval = 500;

                while (true)
                {
                    if (_downIndex == -1)
                        break;

                    if (time % interval == 0)
                        TriggerTapKeyEvent(this, new TapKeyEventArgs(ActionKey.Backspace, _downIndex));

                    if (time == 600)
                        interval = 400;
                    else if (time == 700)
                        interval = 300;
                    else if (time == 800)
                        interval = 200;
                    else if (time == 900)
                        interval = 100;
                    else if (time == 1000)
                        interval = 50;

                    time += 50;
                    Thread.Sleep(50);
                }
            }
        }

        /// <summary>
        /// Current view.
        /// </summary>
        public View CurrentView
        {
            get { return _view; }
            set
            {
                _view = value;
                if (_view == View.Letters)
                {
                    _bitmapIndex = (Uppercase) ? 0 : 1;
                }
                else
                    _bitmapIndex = 2;
            }
        }

        /// <summary>
        /// Bitmaps for each view containing keys in the up position.
        /// </summary>
        public Bitmap[] BitmapUp { get; set; }

        /// <summary>
        /// Indicates whether or not the letters are uppercase or lowercase.
        /// </summary>
        public bool Uppercase { get; set; }

        /// <summary>
        /// Restricted characters; the corresponding keys will be disabled.
        /// </summary>
        /// <example>
        /// // Here for example, we restrict invalid file name characters and the Symbols view.
        /// <code>Glide.Keyboard.Restricted = new string[9] { "\\", "/", ":", "*", "?", "\"", "&lt;", "&gt;", Keyboard.ActionKey.ToSymbols };</code>
        /// </example>
        public string[] Restricted = new string[0];

        /// <summary>
        /// Keyboard views.
        /// </summary>
        public enum View
        {
            /// <summary>
            /// Letters
            /// </summary>
            Letters,

            /// <summary>
            /// Numbers
            /// </summary>
            Numbers,

            /// <summary>
            /// Symbols
            /// </summary>
            Symbols
        }

        /// <summary>
        /// Keys that perform an action.
        /// </summary>
        public struct ActionKey
        {
            /// <summary>
            /// Toggle between uppercase and lowercase.
            /// </summary>
            public const string ToggleCase = "togglecase";

            /// <summary>
            /// Backspace
            /// </summary>
            public const string Backspace = "backspace";

            /// <summary>
            /// Switches to the Numbers keyboard view.
            /// </summary>
            public const string ToNumbers = "tonumbers";

            /// <summary>
            /// Switches to the Letters keyboard view.
            /// </summary>
            public const string ToLetters = "toletters";

            /// <summary>
            /// Switches to the Symbols keyboard view.
            /// </summary>
            public const string ToSymbols = "tosymbols";

            /// <summary>
            /// Space
            /// </summary>
            public const string Space = "space";

            /// <summary>
            /// Return
            /// </summary>
            public const string Return = "return";

            /// <summary>
            /// Tab
            /// </summary>
            public const string Tab = "tab";
        }
    }
}
