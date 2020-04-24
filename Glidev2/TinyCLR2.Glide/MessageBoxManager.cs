////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using GHI.Glide.Display;
using GHI.Glide.UI;

namespace GHI.Glide
{
    /// <summary>
    /// Manages the MessageBox component.
    /// </summary>
    public class MessageBoxManager
    {
        private Window _window;
        private MessageBox _msgBox;
        private Canvas _canvas;
        private ManualResetEvent _resetEvent;
        private ModalResult _result;
        private Thread _touchThread;
        private bool _forceClose;

        /// <summary>
        /// Creates a new MessageBoxManager.
        /// </summary>
        public MessageBoxManager()
        {
            _msgBox = new MessageBox("msgBox", 255, 0, 0, (int)(Glide.LCD.Width * 0.80), 0);

            _canvas = new Canvas();
            _canvas.DrawRectangle(TinyCLR2.Glide.Ext.Colors.Black, 0, 0, 0, Glide.LCD.Width, Glide.LCD.Height, 0, 0, TinyCLR2.Glide.Ext.Colors.DarkGray, 0, 0, TinyCLR2.Glide.Ext.Colors.Black, 0, 0, 125);

            _result = ModalResult.None;
        }

        /// <summary>
        /// Shows a MessageBox on screen.
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns></returns>
        public ModalResult Show(string message)
        {
            return Show(message, String.Empty, ModalButtons.Ok);
        }

        /// <summary>
        /// Shows a MessageBox on screen.
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="title">Title</param>
        /// <returns></returns>
        public ModalResult Show(string message, string title)
        {
            return Show(message, title, ModalButtons.Ok);
        }

        /// <summary>
        /// Shows a MessageBox on screen.
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="title">Title</param>
        /// <param name="buttons">MessageBoxButtons constant.</param>
        /// <returns></returns>
        public ModalResult Show(string message, string title, ModalButtons buttons)
        {
            _window = Glide.MainWindow;
            _result = ModalResult.None;
            _resetEvent = new ManualResetEvent(false);

            while (_msgBox.NumChildren > 0)
                _msgBox.RemoveChildAt(0);

            Update(message, title);

            Button abortBtn, okBtn, cancelBtn, retryBtn, ignoreBtn, yesBtn, noBtn;
            int startX;
            int buttonY = _msgBox.Height - 32 - 10;

            switch (buttons)
            {
                case ModalButtons.Ok:
                    okBtn = new Button("okBtn", 255, (_msgBox.Width - 50) / 2, buttonY, 50, 32);
                    okBtn.Text = "Ok";
                    okBtn.Font = FontManager.GetFont(FontManager.FontType.droid_reg11);
                    okBtn.TapEvent += new OnTap(okBtn_TapEvent);
                    _msgBox.AddChild(okBtn);
                    break;

                case ModalButtons.OkCancel:
                    startX = (_msgBox.Width - (50 + 5 + 70)) / 2;

                    okBtn = new Button("okBtn", 255, startX, buttonY, 50, 32);
                    okBtn.Text = "Ok";
                    okBtn.Font = FontManager.GetFont(FontManager.FontType.droid_reg11);
                    okBtn.TapEvent += new OnTap(okBtn_TapEvent);
                    _msgBox.AddChild(okBtn);

                    cancelBtn = new Button("cancelBtn", 255, okBtn.X + okBtn.Width + 5, buttonY, 70, 32);
                    cancelBtn.Text = "Cancel";
                    cancelBtn.Font = FontManager.GetFont(FontManager.FontType.droid_reg11);
                    cancelBtn.TapEvent += new OnTap(cancelBtn_TapEvent);
                    _msgBox.AddChild(cancelBtn);
                    break;

                case ModalButtons.RetryCancel:
                    startX = (_msgBox.Width - (60 + 5 + 70)) / 2;

                    retryBtn = new Button("retryBtn", 255, startX, buttonY, 60, 32);
                    retryBtn.Text = "Retry";
                    retryBtn.Font = FontManager.GetFont(FontManager.FontType.droid_reg11);
                    retryBtn.TapEvent += new OnTap(retryBtn_TapEvent);
                    _msgBox.AddChild(retryBtn);

                    cancelBtn = new Button("cancelBtn", 255, retryBtn.X + retryBtn.Width + 5, buttonY, 70, 32);
                    cancelBtn.Text = "Cancel";
                    cancelBtn.Font = FontManager.GetFont(FontManager.FontType.droid_reg11);
                    cancelBtn.TapEvent += new OnTap(cancelBtn_TapEvent);
                    _msgBox.AddChild(cancelBtn);
                    break;

                case ModalButtons.AbortRetryIgnore:
                    startX = (_msgBox.Width - (60 + 5 + 60 + 5 + 70)) / 2;

                    abortBtn = new Button("abortBtn", 255, startX, buttonY, 60, 32);
                    abortBtn.Text = "Abort";
                    abortBtn.Font = FontManager.GetFont(FontManager.FontType.droid_reg11);
                    abortBtn.TapEvent += new OnTap(abortBtn_TapEvent);
                    _msgBox.AddChild(abortBtn);

                    retryBtn = new Button("retryBtn", 255, abortBtn.X + abortBtn.Width + 5, buttonY, 60, 32);
                    retryBtn.Text = "Retry";
                    retryBtn.Font = FontManager.GetFont(FontManager.FontType.droid_reg11);
                    retryBtn.TapEvent += new OnTap(retryBtn_TapEvent);
                    _msgBox.AddChild(retryBtn);

                    ignoreBtn = new Button("ignoreBtn", 255, retryBtn.X + retryBtn.Width + 5, buttonY, 70, 32);
                    ignoreBtn.Text = "Ignore";
                    ignoreBtn.Font = FontManager.GetFont(FontManager.FontType.droid_reg11);
                    ignoreBtn.TapEvent += new OnTap(ignoreBtn_TapEvent);
                    _msgBox.AddChild(ignoreBtn);
                    break;

                case ModalButtons.YesNo:
                    startX = (_msgBox.Width - (55 + 5 + 60)) / 2;

                    yesBtn = new Button("yesBtn", 255, startX, buttonY, 55, 32);
                    yesBtn.Text = "Yes";
                    yesBtn.Font = FontManager.GetFont(FontManager.FontType.droid_reg11);
                    yesBtn.TapEvent += new OnTap(yesBtn_TapEvent);
                    _msgBox.AddChild(yesBtn);

                    noBtn = new Button("noBtn", 255, yesBtn.X + yesBtn.Width + 5, buttonY, 50, 32);
                    noBtn.Text = "No";
                    noBtn.Font = FontManager.GetFont(FontManager.FontType.droid_reg11);
                    noBtn.TapEvent += new OnTap(noBtn_TapEvent);
                    _msgBox.AddChild(noBtn);
                    break;

                case ModalButtons.YesNoCancel:
                    startX = (_msgBox.Width - (55 + 5 + 50 + 5 + 70)) / 2;

                    yesBtn = new Button("yesBtn", 255, startX, buttonY, 55, 32);
                    yesBtn.Text = "Yes";
                    yesBtn.Font = FontManager.GetFont(FontManager.FontType.droid_reg11);
                    yesBtn.TapEvent += new OnTap(yesBtn_TapEvent);
                    _msgBox.AddChild(yesBtn);

                    noBtn = new Button("noBtn", 255, yesBtn.X + yesBtn.Width + 5, buttonY, 50, 32);
                    noBtn.Text = "No";
                    noBtn.Font = FontManager.GetFont(FontManager.FontType.droid_reg11);
                    noBtn.TapEvent += new OnTap(noBtn_TapEvent);
                    _msgBox.AddChild(noBtn);

                    cancelBtn = new Button("cancelBtn", 255, noBtn.X + noBtn.Width + 5, buttonY, 70, 32);
                    cancelBtn.Text = "Cancel";
                    cancelBtn.Font = FontManager.GetFont(FontManager.FontType.droid_reg11);
                    cancelBtn.TapEvent += new OnTap(cancelBtn_TapEvent);
                    _msgBox.AddChild(cancelBtn);
                    break;
            }

            _forceClose = false;
            _touchThread = new Thread(TouchThread);
            _touchThread.Priority = ThreadPriority.AboveNormal;
            _touchThread.Start();

            Open();

            _resetEvent.WaitOne();

            Close();

            return _result;
        }

        /// <summary>
        /// Hides the on screen MessageBox.
        /// </summary>
        public void Hide()
        {
            if (IsOpen)
                _forceClose = true;
        }

        /// <summary>
        /// Updates the MessageBox message.
        /// </summary>
        /// <param name="message">Message</param>
        public void Update(string message)
        {
            Update(message, _msgBox.Title);
        }

        /// <summary>
        /// Updates the MessageBox message and title.
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="title">Title</param>
        public void Update(string message, string title)
        {
            _msgBox.Message = message;
            _msgBox.Title = title;

            // Determine the number of lines.
            int width = _msgBox.Width - 20;
            Geom.Size size = FontManager.GetSize(_msgBox.MessageFont, message);
            int numLines = (int)System.Math.Ceiling((double)size.Width / width);

            // Set the Height based on the message size.
            int realHeight = _msgBox.TitlebarHeight + 10 + (numLines * _msgBox.MessageFont.Height) + 15 + 32;
            if (realHeight != _msgBox.Height)
                _msgBox.Height = realHeight;

            _msgBox.X = (Glide.LCD.Width - _msgBox.Width) / 2;
            _msgBox.Y = (Glide.LCD.Height - _msgBox.Height) / 2;

            int buttonY = _msgBox.Height - 32 - 10;
            for (int i = 0; i < _msgBox.NumChildren; i++)
            {
                if (_msgBox[i] is Button)
                    _msgBox[i].Y = buttonY;
            }

            if (IsOpen)
                _msgBox.Invalidate();
        }

        private void Open()
        {
            for (int i = 0; i < _window.NumChildren; i++)
                _window[i].Interactive = false;

            _window.AddChild(_canvas);
            _window.AddChild(_msgBox);
            _window.Invalidate();

            GlideTouch.IgnoreAllEvents = true;

            IsOpen = true;
        }

        private void Close()
        {
            IsOpen = false;

            for (int i = 0; i < _window.NumChildren; i++)
                _window[i].Interactive = true;

            _window.RemoveChild(_canvas);
            _window.RemoveChild(_msgBox);
            _window.Invalidate();
            _window = null;

            GlideTouch.IgnoreAllEvents = false;
        }

        private void TouchThread()
        {
            // These are used for the touch up event
            int lastX = 0;
            int lastY = 0;

            // These store the current X and Y
            int x = 0;
            int y = 0;

            // Keeps track of whether the panel was touched or not
            bool isTouched = false;

            // Create touch inputs that are used as arguments
            GHI.Glide.UI.TouchInput[] touches = new GHI.Glide.UI.TouchInput[] { new GHI.Glide.UI.TouchInput() };

            // Begin touch panel polling
            while (_result == ModalResult.None && !_forceClose)
            {
                GlideTouch.GetLastTouchPoint(ref x, ref y);

                if (x >= 0 && x <= Glide.LCD.Width && y >= 0 && y <= Glide.LCD.Height)
                {
                    if (isTouched == false)
                    {
                        // Touch down
                        touches[0].X = x;
                        touches[0].Y = y;
                        GlideTouch.RaiseTouchDownEvent(this, new TouchEventArgs(touches));

                        lastX = x;
                        lastY = y;
                        isTouched = true;
                    }
                    else
                    {
                        // Filter finger movements to avoid spamming
                        if (System.Math.Abs(x - lastX) > 2 || System.Math.Abs(y - lastY) > 2)
                        {
                            // Touch move
                            touches[0].X = x;
                            touches[0].Y = y;
                            GlideTouch.RaiseTouchMoveEvent(this, new TouchEventArgs(touches));

                            lastX = x;
                            lastY = y;
                        }
                    }
                }
                else
                {
                    if (isTouched == true)
                    {
                        // Touch up
                        touches[0].X = lastX;
                        touches[0].Y = lastY;
                        GlideTouch.RaiseTouchUpEvent(this, new TouchEventArgs(touches));

                        isTouched = false;
                    }
                }

                Thread.Sleep(50);
            }

            _resetEvent.Set();

            // Allow other threads to run so we dont get double touch events
            // once the message box closes.
            Thread.Sleep(0);
        }

        private void abortBtn_TapEvent(object sender)
        {
            _result = ModalResult.Abort;
        }

        private void okBtn_TapEvent(object sender)
        {
            _result = ModalResult.Ok;
        }

        private void cancelBtn_TapEvent(object sender)
        {
            _result = ModalResult.Cancel;
        }

        private void retryBtn_TapEvent(object sender)
        {
            _result = ModalResult.Retry;
        }

        private void ignoreBtn_TapEvent(object sender)
        {
            _result = ModalResult.Ignore;
        }

        private void yesBtn_TapEvent(object sender)
        {
            _result = ModalResult.Yes;
        }

        private void noBtn_TapEvent(object sender)
        {
            _result = ModalResult.No;
        }

        /// <summary>
        /// Indicates whether or not the message box is open.
        /// </summary>
        public bool IsOpen = false;
    }
}
