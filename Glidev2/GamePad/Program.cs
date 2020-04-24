using GamePad.Properties;
using GHI.Glide;
using GHI.Glide.Display;
using GHIElectronics.TinyCLR.Pins;
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;

namespace GamePad
{
    public class BoardPins
    {
        public enum PinTypes { DigitalWrite, DigitalRead, AnalogRead, AnalogWrite, None };
        public PinTypes PinType { set; get; }
        public object ThisPin { set; get; }
    }

    #region Forms
    public class Screen
    {
        public enum ScreenTypes { Splash = 0, MainMenu, Game, Gallery, Register, MyRoom, Inbox };
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
    public class MainMenuForm : Screen
    {
        GHI.Glide.UI.Button BtnInbox { set; get; }

        public MainMenuForm(ref GHI.Glide.Display.Window window) : base(ref window)
        {

        }
        public void ChangeInboxCounter(int MessageCount)
        {
            if (MessageCount <= 0)
                BtnInbox.Text = "Message";
            else
                BtnInbox.Text = "Message (" + MessageCount + ")";
            BtnInbox.Invalidate();
        }
        public override void Init(params string[] Param)
        {

            MainWindow = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.MainMenu));
            ArrayList control = new ArrayList();
            for (int i = 1; i < 5; i++)
            {
                var img = (GHI.Glide.UI.Image)MainWindow.GetChildByName("Img" + i);
                control.Add(img);
                //Bitmap pic = null;//GT.Picture
                switch (i)
                {
                    case 1:
                        //pic = Resources.GetBitmap(Resources.BitmapResources.game); //new GT.Picture(Resources.GetBytes(Resources.BinaryResources.game), GT.Picture.PictureEncoding.JPEG);
                        img.Bitmap = Resources.GetBitmap(Resources.BitmapResources.game);//pic;//.MakeBitmap();
                        break;
                    case 2:
                        //pic = new GT.Picture(Resources.GetBytes(Resources.BinaryResources.gallery), GT.Picture.PictureEncoding.JPEG);
                        img.Bitmap = Resources.GetBitmap(Resources.BitmapResources.gallery);//pic.MakeBitmap();
                        break;
                    case 3:
                        //pic = new GT.Picture(Resources.GetBytes(Resources.BinaryResources.myroom), GT.Picture.PictureEncoding.JPEG);
                        img.Bitmap = Resources.GetBitmap(Resources.BitmapResources.myroom);//pic.MakeBitmap();
                        break;
                    case 4:
                        //pic = new GT.Picture(Resources.GetBytes(Resources.BinaryResources.message), GT.Picture.PictureEncoding.JPEG);
                        img.Bitmap = Resources.GetBitmap(Resources.BitmapResources.message);//pic.MakeBitmap();
                        break;
                }
                var Btn = (GHI.Glide.UI.Button)MainWindow.GetChildByName("Btn" + i);
                if (i == 4)
                {
                    BtnInbox = Btn;
                }
                control.Add(Btn);
                Btn.PressEvent += (sender) =>
                {
                    var btn = sender as GHI.Glide.UI.Button;
                    switch (btn.Name)
                    {
                        case "Btn1":
                            CallFormRequestEvent(ScreenTypes.Game);
                            break;
                        case "Btn2":
                            CallFormRequestEvent(ScreenTypes.Gallery);
                            break;
                        case "Btn3":
                            CallFormRequestEvent(ScreenTypes.MyRoom);
                            break;
                        case "Btn4":
                            CallFormRequestEvent(ScreenTypes.Inbox);
                            break;
                    }
                };
            }

            Glide.MainWindow = MainWindow;
            //MainWindow.Invalidate();
        }
    }
    public class SplashForm : Screen
    {
        public SplashForm(ref GHI.Glide.Display.Window window) : base(ref window)
        {

        }
        public override void Init(params string[] Param)
        {
            var frm = Resources.GetString( Resources.StringResources.SplashScreen);//.GetString(Resources.StringResources.SplashScreen);
            MainWindow = GlideLoader.LoadWindow(frm);
            var img = (GHI.Glide.UI.Image)MainWindow.GetChildByName("ImgLogo");

            //GT.Picture pic = new GT.Picture(Resources.GetBytes(Resources.BinaryResources.logo), GT.Picture.PictureEncoding.JPEG);
            img.Bitmap = Resources.GetBitmap(Resources.BitmapResources.logo);//pic.MakeBitmap();

            Glide.MainWindow = MainWindow;
            //MainWindow.Invalidate();
            Thread.Sleep(2000);
            CallFormRequestEvent(ScreenTypes.MainMenu);

        }
    }
    public class GameForm : Screen
    {
        GHI.Glide.UI.Image ImgFull { set; get; }
        DisplayDriver43 displayTE35;
        //Gadgeteer.Modules.GHIElectronics.DisplayTE35 displayTE35;
        bool GameIsOver = false;
        PlayerChips Turn = PlayerChips.O;
        PlayerChips Winner = PlayerChips.Blank;
        public enum PlayerChips { X, O, Blank }
        Hashtable Box { set; get; }
        Hashtable Control { set; get; }
        //imgFull
        public GameForm(ref GHI.Glide.Display.Window window, ref DisplayDriver43 displayTE35) : base(ref window)
        {
            this.displayTE35 = displayTE35;
        }
        void Choose(int Pos)
        {
            if (GameIsOver) return;
            var box = (PlayerChips)Box[Pos];
            if (box == PlayerChips.Blank)
            {
                var img = (GHI.Glide.UI.Image)Control[Pos];
                Box[Pos] = Turn;
                if (Turn == PlayerChips.X)
                {
                    //var tmp = new GT.Picture(Resources.GetBytes(Resources.BinaryResources.x), GT.Picture.PictureEncoding.JPEG);
                    img.Bitmap = Resources.GetBitmap(Resources.BitmapResources.x);//tmp.MakeBitmap();
                    Turn = PlayerChips.O;
                }
                else
                {
                    //var tmp = new GT.Picture(Resources.GetBytes(Resources.BinaryResources.o), GT.Picture.PictureEncoding.JPEG);
                    img.Bitmap = Resources.GetBitmap(Resources.BitmapResources.o);// tmp.MakeBitmap();
                    Turn = PlayerChips.X;
                }
                img.Invalidate();
                //[change]
                MainWindow.Invalidate();
                if (CheckWin())
                {
                    GameIsOver = true;
                    //load game over
                    Bitmap bmp = null;
                    if (Winner == PlayerChips.X)
                    {
                        //var tmp = new GT.Picture(Resources.GetBytes(Resources.BinaryResources.WIN), GT.Picture.PictureEncoding.JPEG);
                        bmp = Resources.GetBitmap(Resources.BitmapResources.WIN);//tmp.MakeBitmap();
                    }
                    else if (Winner == PlayerChips.O)
                    {
                        //var tmp = new GT.Picture(Resources.GetBytes(Resources.BinaryResources.LOSE), GT.Picture.PictureEncoding.JPEG);
                        bmp = Resources.GetBitmap(Resources.BitmapResources.LOSE);//tmp.MakeBitmap();
                    }
                    else
                    {
                        //var tmp = new GT.Picture(Resources.GetBytes(Resources.BinaryResources.draw), GT.Picture.PictureEncoding.JPEG);
                        bmp = Resources.GetBitmap(Resources.BitmapResources.draw);//tmp.MakeBitmap();
                    }

                    ImgFull.Visible = true;
                    ImgFull.Bitmap = bmp;
                    ImgFull.Invalidate();
                    //[change]
                    MainWindow.Invalidate();
                    Thread.Sleep(3000);
                    CallFormRequestEvent(ScreenTypes.MainMenu);
                }
                else if (Turn == PlayerChips.O)
                {
                    Thread.Sleep(500);
                    ComMove();
                }
            }
        }

        bool EvaluatePos(PlayerChips player)
        {
            //ambil yg tinggal menang
            int BlankCounter = 0;
            int PlayerCounter = 0;
            int BlankPos = 0;
            //check horizontal
            for (int i = 1; i <= 7; i += 3)
            {
                BlankCounter = 0;
                PlayerCounter = 0;
                BlankPos = 0;
                for (int x = i; x <= i + 2; x++)
                {
                    if ((PlayerChips)Box[x] == player) PlayerCounter++;
                    if ((PlayerChips)Box[x] == PlayerChips.Blank)
                    {
                        BlankCounter++;
                        BlankPos = x;
                    }
                }
                if (BlankCounter == 1 && PlayerCounter == 2)
                {
                    Choose(BlankPos);
                    return true;
                }
            }
            //check vertikal
            for (int i = 1; i <= 3; i++)
            {
                BlankCounter = 0;
                PlayerCounter = 0;
                BlankPos = 0;

                for (int y = i; y <= i + 6; y += 3)
                {
                    if ((PlayerChips)Box[y] == player) PlayerCounter++;
                    if ((PlayerChips)Box[y] == PlayerChips.Blank)
                    {
                        BlankCounter++;
                        BlankPos = y;
                    }
                }
                if (BlankCounter == 1 && PlayerCounter == 2)
                {
                    Choose(BlankPos);
                    return true;
                }
            }
            //check diagonal

            {
                BlankCounter = 0;
                PlayerCounter = 0;
                BlankPos = 0;

                for (int y = 1; y <= 9; y += 4)
                {
                    if ((PlayerChips)Box[y] == player) PlayerCounter++;
                    if ((PlayerChips)Box[y] == PlayerChips.Blank)
                    {
                        BlankCounter++;
                        BlankPos = y;
                    }
                }
                if (BlankCounter == 1 && PlayerCounter == 2)
                {
                    Choose(BlankPos);
                    return true;
                }

            }
            {
                BlankCounter = 0;
                PlayerCounter = 0;
                BlankPos = 0;
                var tmp = (PlayerChips)Box[3];
                if (tmp != PlayerChips.Blank)
                {
                    for (int y = 3; y <= 7; y += 2)
                    {
                        if ((PlayerChips)Box[y] == player) PlayerCounter++;
                        if ((PlayerChips)Box[y] == PlayerChips.Blank)
                        {
                            BlankCounter++;
                            BlankPos = y;
                        }
                    }
                    if (BlankCounter == 1 && PlayerCounter == 2)
                    {
                        Choose(BlankPos);
                        return true;
                    }
                }
            }
            return false;
        }
        void ComMove()
        {
            //cek yang langsung menang
            if (EvaluatePos(PlayerChips.O)) return;
            //halangin mush yang mau menang
            if (EvaluatePos(PlayerChips.X)) return;

            //ambil tengah
            if ((PlayerChips)Box[5] == PlayerChips.Blank)
            {
                Choose(5);
                return;
            }
            //ambil sudut
            for (int i = 1; i <= 3; i += 2)
            {
                if ((PlayerChips)Box[i] == PlayerChips.Blank)
                {
                    Choose(i);
                    return;
                }
            }
            for (int i = 7; i <= 9; i += 2)
            {
                if ((PlayerChips)Box[i] == PlayerChips.Blank)
                {
                    Choose(i);
                    return;
                }
            }
            //acak
            for (int i = 1; i <= 9; i++)
            {
                if ((PlayerChips)Box[i] == PlayerChips.Blank)
                {
                    Choose(i);
                    return;
                }
            }
        }

        bool CheckWin()
        {
            int counter = 0;
            //check horizontal
            for (int i = 1; i <= 7; i += 3)
            {
                counter = 0;
                var tmp = (PlayerChips)Box[i];
                if (tmp == PlayerChips.Blank) break;
                for (int x = i; x <= i + 2; x++)
                {
                    if (tmp != (PlayerChips)Box[x]) break;
                    counter++;
                }
                if (counter >= 3)
                {
                    Winner = tmp;
                    return true;
                }
            }
            //check vertikal
            for (int i = 1; i <= 3; i++)
            {
                counter = 0;
                var tmp = (PlayerChips)Box[i];
                if (tmp == PlayerChips.Blank) break;
                for (int y = i; y <= i + 6; y += 3)
                {
                    if (tmp != (PlayerChips)Box[y]) break;
                    counter++;
                }
                if (counter >= 3)
                {
                    Winner = tmp;
                    return true;
                }
            }
            //check diagonal

            {
                counter = 0;
                var tmp = (PlayerChips)Box[1];
                if (tmp != PlayerChips.Blank)
                {
                    for (int y = 1; y <= 9; y += 4)
                    {
                        if (tmp != (PlayerChips)Box[y]) break;
                        counter++;
                    }
                    if (counter >= 3)
                    {
                        Winner = tmp;
                        return true;
                    }
                }
            }
            {
                counter = 0;
                var tmp = (PlayerChips)Box[3];
                if (tmp != PlayerChips.Blank)
                {
                    for (int y = 3; y <= 7; y += 2)
                    {
                        if (tmp != (PlayerChips)Box[y]) break;
                        counter++;
                    }
                    if (counter >= 3)
                    {
                        Winner = tmp;
                        return true;
                    }
                }
            }
            //check all
            counter = 0;
            for (int i = 1; i <= 9; i++)
            {
                if ((PlayerChips)Box[i] != PlayerChips.Blank)
                {
                    counter++;
                }
            }
            if (counter >= 9)
            {
                Winner = PlayerChips.Blank;
                return true;
            }
            return false;
        }
        public override void Init(params string[] Param)
        {
            GameIsOver = false;
            Turn = PlayerChips.X;
            MainWindow = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.Game2));
            Control = new Hashtable();
            //GT.Picture pic = null;
            Bitmap pic;
            Box = new Hashtable();
            ImgFull = (GHI.Glide.UI.Image)MainWindow.GetChildByName("imgFull");
            ImgFull.Visible = false;
            for (int i = 1; i <= 9; i++)
            {
                var imgTemp = (GHI.Glide.UI.Image)MainWindow.GetChildByName("box" + i);
                //pic = new GT.Picture(Resources.GetBytes(Resources.BinaryResources.blank), GT.Picture.PictureEncoding.JPEG);
                imgTemp.Bitmap = Resources.GetBitmap(Resources.BitmapResources.blank);//pic.MakeBitmap();
                Control.Add(i, imgTemp);
                Box.Add(i, PlayerChips.Blank);
                imgTemp.TapEvent += (x) =>
                {
                    if (Turn == PlayerChips.X)
                    {
                        var img = x as GHI.Glide.UI.Image;
                        var PinSel = int.Parse(img.Name.Substring(3));
                        Choose(PinSel);
                    }
                };
                if (i <= 2)
                {
                    var linehor = (GHI.Glide.UI.Image)MainWindow.GetChildByName("line" + i);
                    //pic = new GT.Picture(Resources.GetBytes(Resources.BinaryResources.linehor), GT.Picture.PictureEncoding.JPEG);
                    linehor.Bitmap = Resources.GetBitmap(Resources.BitmapResources.linehor);//pic.MakeBitmap();
                }
                else if (i <= 4)
                {
                    var linever = (GHI.Glide.UI.Image)MainWindow.GetChildByName("line" + i);
                    //pic = new GT.Picture(Resources.GetBytes(Resources.BinaryResources.linever), GT.Picture.PictureEncoding.JPEG);
                    linever.Bitmap = Resources.GetBitmap(Resources.BitmapResources.linever);//pic.MakeBitmap();
                }

            }

            Glide.MainWindow = MainWindow;

            //MainWindow.Invalidate();
        }
    }
   
    #endregion
    
    class Program
    {
       
        static void Main()
        {
            try
            {
                var game = new MyGame();
                game.StartProgram();
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            Thread.Sleep(-1);
        }

      
    }

    public class MyGame
    {
        private static int NewMessageCounter;
        private static GHI.Glide.Display.Window MainWindow;
        private static Screen.ScreenTypes ActiveWindow { set; get; }
        static Hashtable Screens { set; get; }
        public void StartProgram()
        {
            Debug.WriteLine("Program Started");
            var lcd = new DisplayDriver43(SC20260.GpioPin.PA15);

            Glide.SetupGlide(480, 272, 96, 0, lcd.display);
            Screens = new Hashtable();
            //populate all form
            var F1 = new SplashForm(ref MainWindow);
            F1.FormRequestEvent += General_FormRequestEvent;
            Screens.Add(Screen.ScreenTypes.Splash, F1);

            var F2 = new MainMenuForm(ref MainWindow);
            F2.FormRequestEvent += General_FormRequestEvent;
            Screens.Add(Screen.ScreenTypes.MainMenu, F2);
            
            var F6 = new GameForm(ref MainWindow, ref lcd);
            F6.FormRequestEvent += General_FormRequestEvent;
            Screens.Add(Screen.ScreenTypes.Game, F6);
            //init glide touch
            Glide.FitToScreen = true;
            GlideTouch.Initialize();
            //enable touch screen
            lcd.CapacitiveScreenReleased += Lcd_CapacitiveScreenReleased;
            lcd.CapacitiveScreenPressed += Lcd_CapacitiveScreenPressed;
            lcd.CapacitiveScreenMove += Lcd_CapacitiveScreenMove;
            //load splash
            LoadForm(Screen.ScreenTypes.Splash);
        }
        void LoadForm(Screen.ScreenTypes form, params string[] Param)
        {
            
            switch (form)
            {
                case Screen.ScreenTypes.Splash:
                case Screen.ScreenTypes.MainMenu:
                case Screen.ScreenTypes.Game:
                    ActiveWindow = form;

                    (Screens[form] as Screen).Init(Param);
                    break;
                case Screen.ScreenTypes.MyRoom:
                case Screen.ScreenTypes.Inbox:
                case Screen.ScreenTypes.Gallery:
                case Screen.ScreenTypes.Register:
                    return;
                    //break;
                default:
                    return;
                    //throw new Exception("Belum diterapkan");
            }
            
            if (form == Screen.ScreenTypes.Inbox) NewMessageCounter = 0;
            if (form == Screen.ScreenTypes.MainMenu)
            {
                (Screens[Screen.ScreenTypes.MainMenu] as MainMenuForm).ChangeInboxCounter(NewMessageCounter);
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
}
