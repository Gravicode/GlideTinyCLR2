using GHI.Glide;
using GHI.Glide.Display;
using GHIElectronics.TinyCLR.Devices.Storage;
using GHIElectronics.TinyCLR.IO;
using GHIElectronics.TinyCLR.Pins;
using ShellOS.Tool;
using Skewworks.Labs;
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using TinyCLR2.ShellOS.Properties;

namespace TinyCLR2.ShellOS
{
    #region Shell
    public class GvShell
    {
        #region Handler
        public delegate void IncomingPrintEventHandler(Bitmap result);
        public event IncomingPrintEventHandler PrintEvent;
        private void CallPrintEvent(Bitmap result)
        {
            // Event will be null if there are no subscribers
            if (PrintEvent != null)
            {
                PrintEvent(result);
            }
        }

        public delegate void IncomingClearScreenEventHandler();
        public event IncomingClearScreenEventHandler ClearScreenEvent;
        private void CallClearScreenEvent()
        {
            // Event will be null if there are no subscribers
            if (ClearScreenEvent != null)
            {
                ClearScreenEvent();
            }
        }
        #endregion Handler

        #region controller
        public DisplayDriver43 displayTE35 { set; get; }
        public IDriveProvider sdCard { set; get; }
        public GHIElectronics.TinyCLR.Devices.UsbHost.UsbHostController usbHost { set; get; }
        #endregion
        static SBASIC basic;
        public static string CurrentPath { set; get; }
        public static int CurrentLine { set; get; }
        public string TypedCommand { set; get; }
        public System.Drawing.Graphics Screen { set; get; }
        public int ScreenWidth { set; get; } = 320;
        const int FontHeight = 20;
        public SolidBrush ForeGround { set; get; } = new SolidBrush(Color.White);
        public Color BackGround { set; get; } = Color.Black;
        public ArrayList DataLines { set; get; }
        public int MaxLine { set; get; }
        public int ScreenHeight { set; get; } = 240;
        public Font CurrentFont { set; get; }
        private void ClearScreen()
        {

            Screen.Clear();
            var pen = new Pen(new SolidBrush(Color.Black));
            Screen.DrawRectangle(pen, 0, 0, ScreenWidth, ScreenHeight);
        }
        public void ExecuteScript(string Cmd)
        {
            string[] ParsedMsg = Cmd.Split(' ');
            PrintLine(">" + TypedCommand);
            switch (ParsedMsg[0].ToUpper())
            {
                case "CLS":
                    ClearScreen();
                    for (int i = 0; i < MaxLine; i++) DataLines[i] = string.Empty;
                    CurrentLine = 0;
                    break;
                case "DIR":
                    //if (sdCard.IsCardInserted && sdCard.IsCardMounted) //need a way to check if sd is inserted
                    {
                        ShowFiles();
                    }
                    break;
                case "CD..":
                    {
                        DirectoryInfo dir = new DirectoryInfo(CurrentPath);
                        if (CurrentPath.ToUpper() != "\\SD\\")
                        {
                            CurrentPath = Strings.NormalizeDirectory(dir.Parent.FullName);

                        }
                        PrintLine("Current:" + CurrentPath);
                    }
                    break;
                case "CD":
                    {
                        DirectoryInfo dir = new DirectoryInfo(CurrentPath + ParsedMsg[1]);
                        if (dir.Exists)
                        {
                            CurrentPath = Strings.NormalizeDirectory(dir.FullName);
                            PrintLine("Current:" + CurrentPath);
                        }
                    }
                    break;
                case "PRINT":
                    if (ParsedMsg.Length >= 2)
                    {
                        PrintFile(ParsedMsg[1]);
                    }
                    break;
                default:
                    bool Executed = false;
                    if (ParsedMsg.Length == 1)
                    {
                        //execute file
                        Executed = ExecuteFile(ParsedMsg[0]);
                    }
                    if (!Executed)
                        PrintLine("Unknown command.");
                    break;
            }
            PrintLine(">", false);
            Screen.Flush();
            //CallPrintEvent(Screen);
        }

        bool PrintFile(string Filename)
        {
            bool Result = false;
            FileInfo info = new FileInfo(CurrentPath + Filename);
            if (info.Exists)
            {
                var data = File.ReadAllBytes(info.FullName);
                //var data = sdCard.StorageDevice.ReadFile(info.FullName);
                var strdata = new string(Encoding.UTF8.GetChars(data));
                try
                {

                    foreach (var str in Regex.Split("\r\n", strdata,
                        RegexOptions.IgnoreCase))
                    {
                        PrintLine(str);
                        Screen.Flush();
                        //CallPrintEvent(Screen);
                    }
                    Result = true;
                }
                catch { }
            }
            return Result;
        }
        bool ExecuteFile(string Filename)
        {
            bool Result = false;
            FileInfo info = new FileInfo(CurrentPath + Filename);
            if (info.Exists)
            {
                switch (info.Extension.ToLower())
                {
                    case ".bas":
                        {
                            var data = File.ReadAllBytes(info.FullName);
                            //var data = sdCard.StorageDevice.ReadFile(info.FullName);
                            var codes = new string(Encoding.UTF8.GetChars(data));
                            try
                            {
                                basic.Run(codes);
                                Result = true;
                            }
                            catch { }
                        }
                        break;
                    case ".pe":
                        try
                        {
                            //DeviceController ctl = new DeviceController(displayTE35, sdCard, usbHost, usbClientEDP);
                            Launcher.ExecApp(info.FullName);
                            Result = true;
                        }
                        catch { }
                        break;
                    case ".jpg":
                        {
                            var file = new FileStream(info.FullName, FileMode.Open);
                            Image bitmap = Bitmap.FromStream(file);
                            //var storage = sdCard.StorageDevice;
                            //Bitmap bitmap = storage.LoadBitmap(info.FullName, Bitmap.BitmapImageType.Jpeg);
                            CallPrintEvent((Bitmap)bitmap);
                            Result = true;
                            Thread.Sleep(2000);
                        }
                        break;
                    case ".bmp":
                        {
                            var file = new FileStream(info.FullName, FileMode.Open);
                            Image bitmap = Bitmap.FromStream(file);
                            //var storage = sdCard.StorageDevice;
                            //Bitmap bitmap = storage.LoadBitmap(info.FullName, Bitmap.BitmapImageType.Bmp);
                            CallPrintEvent((Bitmap)bitmap);
                            Result = true;
                            Thread.Sleep(2000);
                        }
                        break;
                    case ".gif":
                        {
                            var file = new FileStream(info.FullName, FileMode.Open);
                            Image bitmap = Bitmap.FromStream(file);
                            //var storage = sdCard.StorageDevice;
                            //Bitmap bitmap = storage.LoadBitmap(info.FullName, Bitmap.BitmapImageType.Gif);
                            CallPrintEvent((Bitmap)bitmap);
                            Result = true;
                            Thread.Sleep(2000);

                        }
                        break;
                }
            }
            return Result;
        }
        void ShowFiles()
        {
            //if (VolumeInfo.GetVolumes()[0].IsFormatted) // assume it is exists
            //{
                if (Directory.Exists(CurrentPath))
                {
                    DirectoryInfo dir = new DirectoryInfo(CurrentPath);

                    var files = dir.GetFiles();
                    var folders = dir.GetDirectories();

                    PrintLine("Files available on " + CurrentPath + ":");
                    if (files.Length > 0)
                    {
                        for (int i = 0; i < files.Length; i++)
                            PrintLine(files[i].Name + " - " + Strings.FormatDiskSize(files[i].Length));
                    }
                    else
                    {
                        PrintLine("Files not found.");
                    }

                    PrintLine("Folders available on " + CurrentPath + ":");
                    if (folders.Length > 0)
                    {
                        for (int i = 0; i < folders.Length; i++)
                            PrintLine(folders[i].Name);
                    }
                    else
                    {
                        PrintLine("folders not found.");
                    }
                }
                /*
            }
            else
            {
                PrintLine("Storage is not formatted. " +
                    "Format on PC with FAT32/FAT16 first!");
            }*/
        }
        public void PrintLine(string Output, bool AddNewLine = true)
        {
            if (CurrentLine >= MaxLine - 1)
            {
                ClearScreen();
                for (int i = 0; i < MaxLine - 1; i++)
                {
                    DataLines[i] = DataLines[i + 1];
                    Screen.DrawString(DataLines[i].ToString(), CurrentFont, ForeGround, 5, FontHeight * i);
                }
                DataLines[CurrentLine] = Output;
                Screen.DrawString(Output, CurrentFont, ForeGround, 5, FontHeight * CurrentLine);
            }
            else
            {
                DataLines[CurrentLine] = Output;
                Screen.DrawString(Output, CurrentFont, ForeGround, 5, FontHeight * CurrentLine);
                if (AddNewLine)
                    CurrentLine++;
            }

        }
        public void PrintLine(int LineNumber, string Output)
        {
            ClearScreen();
            for (int i = 0; i <= CurrentLine - 1; i++)
            {
                Screen.DrawString(DataLines[i].ToString(), CurrentFont, ForeGround, 5, FontHeight * i);
            }
            Screen.DrawString(">" + Output, CurrentFont, ForeGround, 5, FontHeight * CurrentLine);
        }
        public void TypeInCommand(char KeyDown)
        {
            switch ((int)KeyDown)
            {
                //enter
                case 10:
                    ExecuteScript(TypedCommand.Trim());
                    TypedCommand = string.Empty;
                    break;
                //backspace
                case 8:
                    if (TypedCommand.Length > 0)
                    {
                        TypedCommand = TypedCommand.Substring(0, TypedCommand.Length - 1);
                        PrintLine(CurrentLine, TypedCommand);
                        Screen.Flush();
                        //CallPrintEvent(Screen);
                    }
                    break;
                default:
                    TypedCommand = TypedCommand + KeyDown.ToString();
                    PrintLine(CurrentLine, TypedCommand);
                    Screen.Flush();
                    //CallPrintEvent(Screen);
                    break;
            }

        }
        public GvShell(ref DisplayDriver43 displayTE35, ref IDriveProvider sdCard, ref GHIElectronics.TinyCLR.Devices.UsbHost.UsbHostController usbHost)
        {
            this.displayTE35 = displayTE35;
            this.usbHost = usbHost;
            //this.usbClientEDP = usbClientEdp;
            this.sdCard = sdCard;
            Screen = displayTE35.Screen; //new Bitmap(ScreenWidth, ScreenHeight);
            ClearScreen();
            MaxLine = ScreenHeight / 20;
            CurrentLine = 0;
            CurrentFont = Resources.GetFont(Resources.FontResources.NinaB);
            CurrentPath = "\\SD\\";
            DataLines = new ArrayList();
            for (int i = 0; i < MaxLine; i++) DataLines.Add(string.Empty);
            TypedCommand = string.Empty; if (basic == null)
                if (basic == null)
                {
                    basic = new SBASIC();
                    basic.Print += Basic_Print;
                    basic.ClearScreen += Basic_ClearScreen;
                }

        }

        public void PrintWelcome()
        {
            PrintLine("Welcome to Gadgeteer Shell (C) Gravicode");
            PrintLine(">", false);
            Screen.Flush();
            //CallPrintEvent(Screen);


        }

        private void Basic_ClearScreen(SBASIC sender)
        {
            ClearScreen();
        }

        private void Basic_Print(SBASIC sender, string value)
        {
            PrintLine(value);
            Screen.Flush();
            //CallPrintEvent(Screen);
        }
    }
    #endregion

    #region Forms
    public class Screen
    {
        public enum ScreenTypes { Splash = 0, Prompt };
        public delegate void GoToFormEventHandler(ScreenTypes form, params string[] Param);
        public event GoToFormEventHandler FormRequestEvent;
        protected void CallFormRequestEvent(ScreenTypes form, params string[] Param)
        {
            // Event will be null if there are no subscribers
            if (FormRequestEvent != null)
            {
                FormRequestEvent(form, Param);
            }
        }
        protected GHI.Glide.Display.Window MainWindow { set; get; }
        public virtual void Init(params string[] Param)
        {
            //do nothing
        }

        public Screen(ref GHI.Glide.Display.Window window)
        {
            MainWindow = window;
        }
    }
    public class PromptForm : Screen
    {
        int LineCounter
        {
            set; get;
        }

        const int LineSpacing = 20;
        private DisplayDriver43 displayTE35;
        IDriveProvider sdCard;
        GHIElectronics.TinyCLR.Devices.UsbHost.UsbHostController usbHost;
        //USBClientEDP usbClientEDP;
        GHI.Glide.UI.Image imgCode { set; get; }
        ArrayList LinesOfCode;
        GvShell s;
        public PromptForm(ref GHI.Glide.Display.Window window, ref DisplayDriver43 displayTE35, ref IDriveProvider sdCard, ref GHIElectronics.TinyCLR.Devices.UsbHost.UsbHostController usbHost) : base(ref window)
        {
            //this.usbClientEDP = usbClientEDP;
            this.usbHost = usbHost;
            this.sdCard = sdCard;
            this.displayTE35 = displayTE35;
        }
        public override void Init(params string[] Param)
        {
            LinesOfCode = new ArrayList();
            LineCounter = 0;
            MainWindow = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.PromptForm));

            imgCode = (GHI.Glide.UI.Image)MainWindow.GetChildByName("imgCode");

            GHI.Glide.Glide.MainWindow = MainWindow;
            s = new GvShell(ref displayTE35,ref sdCard, ref usbHost);
            s.PrintEvent += S_Print;
            s.ClearScreenEvent += S_ClearScreen;
            usbHost.OnConnectionChangedEvent +=
            UsbHostController_OnConnectionChangedEvent;
            /*
            usbHost.ConnectedKeyboard.KeyDown += (GHI.Usb.Host.Keyboard sender, GHI.Usb.Host.Keyboard.KeyboardEventArgs args) =>
            {
               
            };*/
            s.PrintWelcome();
            Thread.Sleep(500);

            //execute the code
            //s.ExecuteScript(Param[0]);
            //MainWindow.Invalidate();
        }

        #region usbhost
        private void UsbHostController_OnConnectionChangedEvent
       (GHIElectronics.TinyCLR.Devices.UsbHost.UsbHostController sender,
       GHIElectronics.TinyCLR.Devices.UsbHost.DeviceConnectionEventArgs e)
        {

            System.Diagnostics.Debug.WriteLine("e.Id = " + e.Id + " \n");
            System.Diagnostics.Debug.WriteLine("e.InterfaceIndex = " + e.InterfaceIndex + " \n");
            System.Diagnostics.Debug.WriteLine("e.PortNumber = " + e.PortNumber);
            System.Diagnostics.Debug.WriteLine("e.Type = " + ((object)(e.Type)).
                ToString() + " \n");

            System.Diagnostics.Debug.WriteLine("e.VendorId = " + e.VendorId + " \n");
            System.Diagnostics.Debug.WriteLine("e.ProductId = " + e.ProductId + " \n");

            switch (e.DeviceStatus)
            {
                case GHIElectronics.TinyCLR.Devices.UsbHost.DeviceConnectionStatus.Connected:
                    switch (e.Type)
                    {
                        case GHIElectronics.TinyCLR.Devices.UsbHost.BaseDevice.
                            DeviceType.Keyboard:

                            var keyboard = new GHIElectronics.TinyCLR.Devices.UsbHost.
                                Keyboard(e.Id, e.InterfaceIndex);

                            keyboard.KeyUp += Keyboard_KeyUp;
                            keyboard.KeyDown += Keyboard_KeyDown;
                            break;

                        case GHIElectronics.TinyCLR.Devices.UsbHost.BaseDevice.DeviceType.Mouse:
                            var mouse = new GHIElectronics.TinyCLR.Devices.UsbHost.
                                Mouse(e.Id, e.InterfaceIndex);

                            mouse.ButtonChanged += Mouse_ButtonChanged;
                            mouse.CursorMoved += Mouse_CursorMoved;
                            break;

                        case GHIElectronics.TinyCLR.Devices.UsbHost.BaseDevice.
                            DeviceType.MassStorage:

                            var strogareController = GHIElectronics.TinyCLR.Devices.Storage.
                                StorageController.FromName(GHIElectronics.TinyCLR.Pins.
                                SC20260.StorageController.UsbHostMassStorage);

                            var driver = GHIElectronics.TinyCLR.IO.FileSystem.
                                Mount(strogareController.Hdc);

                            var driveInfo = new System.IO.DriveInfo(driver.Name);

                            System.Diagnostics.Debug.WriteLine
                                ("Free: " + driveInfo.TotalFreeSpace);

                            System.Diagnostics.Debug.WriteLine
                                ("TotalSize: " + driveInfo.TotalSize);

                            System.Diagnostics.Debug.WriteLine
                                ("VolumeLabel:" + driveInfo.VolumeLabel);

                            System.Diagnostics.Debug.WriteLine
                                ("RootDirectory: " + driveInfo.RootDirectory);

                            System.Diagnostics.Debug.WriteLine
                                ("DriveFormat: " + driveInfo.DriveFormat);

                            break;

                        default:
                            var rawDevice = new GHIElectronics.TinyCLR.Devices.UsbHost.
                                RawDevice(e.Id, e.InterfaceIndex, e.Type);

                            var devDesc = rawDevice.GetDeviceDescriptor();
                            var cfgDesc = rawDevice.GetConfigurationDescriptor(0);
                            var endpointData = new byte[7];

                            endpointData[0] = 7;        //Length in bytes of this descriptor.
                            endpointData[1] = 5;        //Descriptor type (endpoint).
                            endpointData[2] = 0x81;     //Input endpoint address.
                            endpointData[3] = 3;        //Transfer type is interrupt endpoint.
                            endpointData[4] = 8;        //Max packet size LSB.
                            endpointData[5] = 0;        //Max packet size MSB.
                            endpointData[6] = 10;       //Polling interval.

                            var endpoint = new GHIElectronics.TinyCLR.Devices.UsbHost.
                                Descriptors.Endpoint(endpointData, 0);

                            var pipe = rawDevice.OpenPipe(endpoint);
                            pipe.TransferTimeout = 10;

                            var data = new byte[8];
                            var read = pipe.Transfer(data);

                            if (read > 0)
                            {
                                System.Diagnostics.Debug.WriteLine("Raw Device has new data "
                                    + data[0] + ", " + data[1] + ", " + data[2] + ", " + data[3]);
                            }

                            else if (read == 0)
                            {
                                System.Diagnostics.Debug.WriteLine("No new data");
                            }

                            System.Threading.Thread.Sleep(500);
                            break;
                    }
                    break;

                case GHIElectronics.TinyCLR.Devices.UsbHost.DeviceConnectionStatus.Disconnected:
                    System.Diagnostics.Debug.WriteLine("Device Disconnected");
                    //Unmount filesystem if it was mounted.
                    break;

                case GHIElectronics.TinyCLR.Devices.UsbHost.DeviceConnectionStatus.Bad:
                    System.Diagnostics.Debug.WriteLine("Bad Device");
                    break;
            }
        }

        private  void Keyboard_KeyDown(GHIElectronics.TinyCLR.Devices.UsbHost.Keyboard
            sender, GHIElectronics.TinyCLR.Devices.UsbHost.Keyboard.KeyboardEventArgs args)
        {

            System.Diagnostics.Debug.WriteLine("Key pressed: " + ((object)args.Which).ToString());
            System.Diagnostics.Debug.WriteLine("Key pressed ASCII: " +
                ((object)args.ASCII).ToString());
        }

        private  void Keyboard_KeyUp(GHIElectronics.TinyCLR.Devices.UsbHost.Keyboard
            sender, GHIElectronics.TinyCLR.Devices.UsbHost.Keyboard.KeyboardEventArgs args)
        {
            Debug.WriteLine(((int)args.ASCII).ToString());
            s.TypeInCommand(args.ASCII);
            System.Diagnostics.Debug.WriteLine
                ("Key released: " + ((object)args.Which).ToString());

            System.Diagnostics.Debug.WriteLine
                ("Key released ASCII: " + ((object)args.ASCII).ToString());
        }

        private  void Mouse_CursorMoved(GHIElectronics.TinyCLR.Devices.UsbHost.Mouse
            sender, GHIElectronics.TinyCLR.Devices.UsbHost.Mouse.CursorMovedEventArgs e)
        {

            System.Diagnostics.Debug.WriteLine("Mouse moved to: " + e.NewPosition.X +
                 ", " + e.NewPosition.Y);
        }

        private  void Mouse_ButtonChanged(GHIElectronics.TinyCLR.Devices.UsbHost.Mouse
            sender, GHIElectronics.TinyCLR.Devices.UsbHost.Mouse.ButtonChangedEventArgs args)
        {

            System.Diagnostics.Debug.WriteLine
                ("Mouse button changed: " + ((object)args.Which).ToString());
        }
        #endregion

        private void S_ClearScreen()
        {

        }

        private void S_Print(Bitmap value)
        {

            imgCode.Bitmap = value;
            imgCode.Invalidate();
            MainWindow.Invalidate();
        }
    }
    public class SplashForm : Screen
    {
        public SplashForm(ref GHI.Glide.Display.Window window) : base(ref window)
        {

        }
        public override void Init(params string[] Param)
        {

            MainWindow = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.SplashScreen));
            var img = (GHI.Glide.UI.Image)MainWindow.GetChildByName("ImgLogo");

            var pic = Resources.GetBitmap(Resources.BitmapResources.logo); //new GT.Picture(Resources.GetBytes(Resources.BinaryResources.logo), GT.Picture.PictureEncoding.JPEG);
            img.Bitmap = pic;//.MakeBitmap();

            GHI.Glide.Glide.MainWindow = MainWindow;
            //MainWindow.Invalidate();
            Thread.Sleep(2000);
            CallFormRequestEvent(ScreenTypes.Prompt);

        }
    }

    #endregion
    public class MainApp
    {
        private static GHI.Glide.Display.Window MainWindow;
        private static Screen.ScreenTypes ActiveWindow { set; get; }
        Hashtable Screens { set; get; }
        // This method is run when the mainboard is powered up or reset.   
        public void ProgramStarted()
        {
            //usb host
            var usbHostController = GHIElectronics.TinyCLR.Devices.UsbHost.
            UsbHostController.GetDefault();

            usbHostController.Enable();
            //sdcard
            var sd = StorageController.FromName(SC20260.StorageController.SdCard);
            var drive = FileSystem.Mount(sd.Hdc);


            var lcd = new DisplayDriver43(SC20260.GpioPin.PA15);

            GHI.Glide.Glide.SetupGlide(480, 272, 96, 0, lcd.display);

            // Use Debug.WriteLine to show messages in Visual Studio's "Output" window during debugging.
            Debug.WriteLine("Program Started");
            Screens = new Hashtable();
            //populate all form
            var F1 = new SplashForm(ref MainWindow);
            F1.FormRequestEvent += General_FormRequestEvent;
            Screens.Add(Screen.ScreenTypes.Splash, F1);

            var F2 = new PromptForm(ref MainWindow, ref lcd, ref drive, ref usbHostController);
            F2.FormRequestEvent += General_FormRequestEvent;
            Screens.Add(Screen.ScreenTypes.Prompt, F2);

            GHI.Glide.Glide.FitToScreen = true;
            GlideTouch.Initialize();

            //load splash
            LoadForm(Screen.ScreenTypes.Splash);
            //GHI.Glide.Glide.MainWindow = window;

            lcd.CapacitiveScreenReleased += Lcd_CapacitiveScreenReleased;
            lcd.CapacitiveScreenPressed += Lcd_CapacitiveScreenPressed;
            lcd.CapacitiveScreenMove += Lcd_CapacitiveScreenMove;

            //string dir = Strings.NormalizeDirectory("\\sd");
            //string siz = Strings.FormatDiskSize(1128);
        }
        void LoadForm(Screen.ScreenTypes form, params string[] Param)
        {
            ActiveWindow = form;
            switch (form)
            {
                case Screen.ScreenTypes.Splash:
                case Screen.ScreenTypes.Prompt:
                    (Screens[form] as Screen).Init(Param);
                    break;
                default:
                    return;
                    //throw new Exception("Belum diterapkan");
            }

        }
        void General_FormRequestEvent(Screen.ScreenTypes form, params string[] Param)
        {
            LoadForm(form, Param);
        }



        #region Lcd Capacitive Touch Events
        /// <summary>
        /// Function called when released event raises
        /// </summary>
        /// <param name="sender">sender of event</param>
        /// <param name="e">EventArgs of event</param>
        private static void Lcd_CapacitiveScreenReleased(object sender, DisplayDriver43.TouchEventArgs e)
        {
            Debug.WriteLine("you release the lcd at X:" + e.X + " ,Y:" + e.Y);
            GlideTouch.RaiseTouchUpEvent(e.X, e.Y);
        }

        /// <summary>
        /// Function called when pressed event raises
        /// </summary>
        /// <param name="sender">sender of event</param>
        /// <param name="e">EventArgs of event</param>
        private static void Lcd_CapacitiveScreenPressed(object sender, DisplayDriver43.TouchEventArgs e)
        {
            Debug.WriteLine("you press the lcd at X:" + e.X + " ,Y:" + e.Y);
            GlideTouch.RaiseTouchDownEvent(e.X, e.Y);
        }

        private static void Lcd_CapacitiveScreenMove(object sender, DisplayDriver43.TouchEventArgs e)
        {
            Debug.WriteLine("you move finger on the lcd at X:" + e.X + " ,Y:" + e.Y);
            GlideTouch.RaiseTouchMoveEvent(sender, new TouchEventArgs(new GHI.Glide.Geom.Point(e.X, e.Y)));
        }
        #endregion

    }
    class Program
    {
        static void Main()
        {
            try
            {
                var app = new MainApp();
                app.ProgramStarted();

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            Thread.Sleep(-1);
        }



    }
}
