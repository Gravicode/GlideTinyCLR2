////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
////using Microsoft.SPOT;
////using Microsoft.SPOT.Touch;
using GHI.Glide.Geom;
using GHI.Glide.UI;
using Colors = System.Drawing.Color;
namespace GHI.Glide.Display
{
    /// <summary>
    /// The CalibrationWindow allows you to easily calibrate your touchscreen.
    /// </summary>
    public class CalibrationWindow : Window
    {
        private bool _autoStart = false;
        private bool _autoSave = false;
        private Button _startBtn;
        private Button _exitBtn;
        private TextBlock _text1;
        private Canvas _canvas;
        private int _currentCalPoint = 0;
        private bool _calibrating = false;
        private Thread _thread;

        /// <summary>
        /// Creates a new CalibrationWindow.
        /// </summary>
        /// <param name="autoStart">Whether or not to automatically begin calibration.</param>
        /// <param name="autoSave">Whether or not to save the calibration settings using Extended Weak Reference.</param>
        public CalibrationWindow(bool autoStart, bool autoSave)
        {
            _autoStart = autoStart;
            _autoSave = autoSave;

            Settings = new CalibrationSettings();

            Name = "calibrationWindow";
            Width = Glide.LCD.Width;
            Height = Glide.LCD.Height;
            BackColor = TinyCLR2.Glide.Ext.Colors.White;
            Graphics = new Graphics(Width, Height);

            int yPos = (Height / 2) - 50;

            _text1 = new TextBlock("text1", 255, (Glide.LCD.Width - 300) / 2, yPos, 300, 50);
            _text1.Font = FontManager.GetFont(FontManager.FontType.droid_reg12);
            _text1.TextAlign = HorizontalAlignment.Center;
            AddChild(_text1);

            yPos += 50 + 5;

            _startBtn = new Button("startBtn", 255, (Width - 200) / 2, yPos, 122, 32);
            _startBtn.Text = "Recalibrate";
            _startBtn.TapEvent += new OnTap(_startBtn_TapEvent);
            _startBtn.Visible = false;
            AddChild(_startBtn);

            _exitBtn = new Button("exitBtn", 255, _startBtn.X + _startBtn.Width + 10, yPos, 68, 32);
            _exitBtn.Text = "Done";
            _exitBtn.TapEvent += new OnTap(_exitBtn_TapEvent);
            _exitBtn.Visible = false;
            AddChild(_exitBtn);

            _canvas = new Canvas();
            AddChild(_canvas);

            if (_autoStart)
            {
                _text1.FontColor = TinyCLR2.Glide.Ext.Colors.Black;
                _text1.Text = "Touch the crosshair location.";
                Start();
            }
            else
            {
                if (GlideTouch.Calibrated)
                {
                    _text1.FontColor = TinyCLR2.Glide.Ext.Colors.Red;
                    _text1.Text = "Touch is already calibrated.";

                    _exitBtn.Visible = true;
                    _startBtn.Visible = true;
                }
                else
                {
                    _text1.FontColor = TinyCLR2.Glide.Ext.Colors.Black;
                    _text1.Text = "Touch the screen to start.";

                    TapEvent += new OnTap(CalibrationWindow_TapEvent);
                }
            }
        }

        /// <summary>
        /// Handles the touch up event.
        /// </summary>
        /// <param name="e">Touch event arguments.</param>
        /// <returns>Touch event arguments.</returns>
        public override TouchEventArgs OnTouchUp(TouchEventArgs e)
        {

                if (_calibrating)
                {
                    Settings.CX[_currentCalPoint] = (short)e.Point.X;
                    Settings.CY[_currentCalPoint] = (short)e.Point.Y;

                    if (_currentCalPoint == Settings.Points.Length - 1)
                    {
                        _calibrating = false;

                        _canvas.Clear();
                        Invalidate();

                        _text1.FontColor = TinyCLR2.Glide.Ext.Colors.Blue;
                        _text1.Visible = true;

                        Thread.Sleep(250);

                        //Touch.ActiveTouchPanel.SetCalibration(Settings.Points.Length, Settings.SX, Settings.SY, Settings.CX, Settings.CY);
                        GlideTouch.Calibrated = true;

                        _thread = new Thread(Countdown);
                        _thread.Priority = ThreadPriority.AboveNormal;
                        _thread.Start();

                            _exitBtn.Visible = true;
                            _startBtn.Visible = true;

                        ModalResult result = ModalResult.Yes;//Glide.MessageBoxManager.Show(String.Empty, "Calibration Complete", ModalButtons.YesNo);

                        try
                        {
                            _thread.Suspend();
                            _thread = null;
                        }
                        catch { }

                        if (result == ModalResult.Yes)
                        {
                            if (_autoSave)
                            {
                                GlideTouch.SaveCalibration(Settings);
                                _text1.Text = "Calibration set and saved.";
                            }
                            else
                                _text1.Text = "Calibration set but not saved.";

                            Thread.Sleep(1000);

                            TriggerCloseEvent(this);
                        }
                        else
                        {
                            if (GlideTouch.CalSettings != null)
                            {
                                //Touch.ActiveTouchPanel.SetCalibration(Settings.Points.Length, Settings.SX, Settings.SY, Settings.CX, Settings.CY);
                                _text1.Text = "Calibration reverted to previous settings.";
                            }
                            else
                                _text1.Text = "Calibration remains set. Cannot revert; previous settings do not exist.";
                        }

                        FillRect(_text1.Rect);
                        _text1.Invalidate();
                    }
                    else
                    {
                        _currentCalPoint++;
                        DrawCrossHair(Settings.Points[_currentCalPoint].X, Settings.Points[_currentCalPoint].Y);
                    }
            }

            return base.OnTouchUp(e);
        }

        private void Countdown()
        {
            //To cure the issue in glide of touch-up events and multi-layered button presses
            //We wont ask the user if settings are ok.
            return;

            // Wait for the message box to be open...
            //while (!Glide.MessageBoxManager.IsOpen)
            //    Thread.Sleep(10);

            //// Begin the countdown
            //int count = 0;
            //int waitTime = 15;

            //while (count <= waitTime)
            //{
            //    Glide.MessageBoxManager.Update("Keep these settings?\nTime remaining: " + (waitTime - count) + " second(s)");
            //    Thread.Sleep(1000);
            //    count++;
            //}

            //Glide.MessageBoxManager.Hide();
        }

        private void DrawCrossHair(int x, int y)
        {
            _canvas.Clear();
            _canvas.DrawLine(TinyCLR2.Glide.Ext.Colors.Red, 1, x - 10, y, x - 2, y);
            _canvas.DrawLine(TinyCLR2.Glide.Ext.Colors.Red, 1, x - 10, y, x - 2, y);
            _canvas.DrawLine(TinyCLR2.Glide.Ext.Colors.Red, 1, x + 10, y, x + 2, y);
            _canvas.DrawLine(TinyCLR2.Glide.Ext.Colors.Red, 1, x, y - 10, x, y - 2);
            _canvas.DrawLine(TinyCLR2.Glide.Ext.Colors.Red, 1, x, y + 10, x, y + 2);
            Invalidate();
        }

        private void Start()
        {
            _text1.Visible = false;

            // Ask the touch system how many points are needed to calibrate.
            int calibrationPointCount = 0;
            //Touch.ActiveTouchPanel.GetCalibrationPointCount(ref calibrationPointCount);

            // Create the calibration point array.
            Settings.Points = new Point[calibrationPointCount];
            Settings.SX = new short[calibrationPointCount];
            Settings.SY = new short[calibrationPointCount];
            Settings.CX = new short[calibrationPointCount];
            Settings.CY = new short[calibrationPointCount];

            // Get the points for calibration.
            int i = 0, x = 0, y = 0;
            for (i = 0; i < calibrationPointCount; i++)
            {
                //Touch.ActiveTouchPanel.GetCalibrationPoint(i, ref x, ref y);
                Settings.Points[i].X = x;
                Settings.Points[i].Y = y;
                Settings.SX[i] = (short)x;
                Settings.SY[i] = (short)y;
            }

            // Start the calibration process.
            _currentCalPoint = 0;
            //Touch.ActiveTouchPanel.StartCalibration();
            _calibrating = true;

            DrawCrossHair(Settings.Points[_currentCalPoint].X, Settings.Points[_currentCalPoint].Y);
            Invalidate();
        }

        private void CalibrationWindow_TapEvent(object sender)
        {
            // Remove the window event so we don't keep starting
            // when we touch the first cross hair
            TapEvent -= new OnTap(CalibrationWindow_TapEvent);

            Start();
        }

        private void _startBtn_TapEvent(object sender)
        {
            _text1.Visible = false;
            _exitBtn.Visible = false;
            _startBtn.Visible = false;

            Start();
        }

        private void _exitBtn_TapEvent(object sender)
        {
            TriggerCloseEvent(this);

            if (GlideTouch.Calibrated)
            {
                _text1.FontColor = TinyCLR2.Glide.Ext.Colors.Red;
                _text1.Text = "Touch is already calibrated.";
            }
        }

        /// <summary>
        /// Calibration settings.
        /// </summary>
        public CalibrationSettings Settings { get; set; }
    }
}
