using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace EscapeTheUnderground
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool right;
        bool left;
        bool jump;
        int MomentumUnit = 23;
        int Momentum;
        string currentImage;
        //
        double player_health = 100;
        string direction = "right";
        int player_ammo = 20;
        int player_score = 0;
        bool shootable = true;

        //
        double x = 0;
        double y = 0;
        public MainWindow()
        {
            InitializeComponent();
            ChangePlayer("still");

            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += new EventHandler(MovePlayer);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            timer.Start();

            string pathToFiles = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;

            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(pathToFiles + @"\Assets\Background\background1.gif");
            image.EndInit();
            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(Background, image);


            //delete later
            GameControl();

        }
        public bool Intersected()
        {
            Rect playerRect = new Rect();
            playerRect.Location = Player.PointToScreen(new Point(0D, 0D));
            playerRect.Size = new Size(Player.Width, Player.Height);

            Rect colRect = new Rect();
            colRect.Location = colBlock.PointToScreen(new Point(0D, 0D));
            colRect.Size = new Size(colBlock.Width, colBlock.Height);

            bool doesIntersect = playerRect.IntersectsWith(colRect);

            return doesIntersect;
        }
        public void ChangePlayer(string imageName)
        {
            if(imageName != currentImage)
            {
                string pathToFiles = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
                var image = new BitmapImage();

                image.BeginInit();
                image.UriSource = new Uri(pathToFiles + @"\Assets\Character\" + imageName + ".gif");
                image.EndInit();

                // Set image
                WpfAnimatedGif.ImageBehavior.SetAnimatedSource(Player, image);
                currentImage = imageName;
            }
        }
        public void Shoot(string direction)
        {
            Rectangle playerRect = new Rectangle();
            playerRect.Height = 30;
            playerRect.Width = 30;
            playerRect.Fill = new SolidColorBrush(System.Windows.Media.Colors.AliceBlue);
            Canvas.SetLeft(playerRect, Canvas.GetLeft(Player));
            Canvas.SetTop(playerRect, Canvas.GetTop(Player));
          
        }
        public void createEnemy()
        {

        }

        public void GameControl()
        {
            health_bar.Value = player_health;
            if(player_health <= 15)
            {
                health_bar.Foreground = Brushes.DarkRed;
            }
            else if(player_health >= 15 && player_health <= 30 )
            {
                health_bar.Foreground = Brushes.DarkOrange;
            }
            else
            {
                health_bar.Foreground = Brushes.DarkGreen;
            }
            foreach (Image tb in FindVisualChildren<Image>(LayoutRoot))
            {
                string tst = "";
                Debug.WriteLine(tb.Tag);
                if (tb.Tag != null) { tst = tb.Tag.ToString(); }
                if(tst == "projectile")
                {
                    if (Canvas.GetLeft(tb) > 1000)
                    {
                        tb.Visibility = Visibility.Hidden;
                    }
                }
            }
        }
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)

                {

                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

                    if (child != null && child is T)

                    {

                        yield return (T)child;

                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
        public void MovePlayer(object sender, EventArgs e)
        {
            if(Intersected())
            {
                Debug.WriteLine("Intersected");
            }
            else
            {
                Debug.WriteLine("IntersectedNOT");
            }
            if (Keyboard.IsKeyDown(Key.A)) { ChangePlayer("walk_left"); left = true; direction = "left"; }
            if(Keyboard.IsKeyDown(Key.D)) { ChangePlayer("walk_right"); right = true; direction = "right"; }
            if (Keyboard.IsKeyDown(Key.E))
            {
                ChangePlayer("fight_right");
                //changable = false;
            }
            // Shooting 
            if(Keyboard.IsKeyDown(Key.W))
            {
                if(player_ammo > 0 && shootable == true)
                {
                    Shoot(direction);
                    player_ammo -= 1;
                    ammo_label.Content = player_ammo;
                    shootable = false;
                }
            }
            if (Keyboard.IsKeyUp(Key.A)) { left = false; }
            if(Keyboard.IsKeyUp(Key.D)) { right = false; }
            if (Keyboard.IsKeyUp(Key.E) && Keyboard.IsKeyUp(Key.D) && Keyboard.IsKeyUp(Key.A))
            {
                ChangePlayer("still");
                //changable = false;
            }
            if(Keyboard.IsKeyUp(Key.W)) { shootable = true; }
            //changable = true;
            // Kolize

            double PlayerLeft = Canvas.GetLeft(Player);
            double PlayerRight = Canvas.GetRight(Player);
            double PlayerTop = Canvas.GetTop(Player);
            double PlayerBot = Canvas.GetBottom(Player);

            double ColsLeft = Canvas.GetLeft(colBlock);
            double ColsRight = Canvas.GetRight(colBlock);
            double ColsTop = Canvas.GetTop(colBlock);
            double ColsBot = Canvas.GetBottom(colBlock);

            int PlayerWidth = 36;

            //test
            //if(Intersected() && Cavas )

            //test /
            if(PlayerLeft > ColsLeft - PlayerWidth*2.1 && PlayerLeft < ColsLeft + colBlock.Width && y >= Canvas.GetTop(colBlock) - colBlock.Height)
            {
                right = false;
            }
            if (PlayerLeft < ColsLeft  + colBlock.Width+5 && PlayerLeft > ColsLeft + colBlock.Width && y >= Canvas.GetTop(colBlock) - colBlock.Height)
            {
                left = false;
            }
            if(PlayerTop >= colBlock.Height && PlayerLeft >= ColsLeft- PlayerWidth*1.9 && PlayerLeft < ColsLeft + colBlock.Width && PlayerTop + colBlock.Height >= ColsTop -7)
            {
                jump = false;
                y = 393-colBlock.Height;
                Canvas.SetTop(Player, y);
            }

            // Skok
            if (jump != true)
            {
                if(Keyboard.IsKeyDown(Key.Space)) { jump = true; Momentum = MomentumUnit; }
            }
            // Provedení pohybu
            if(right == true)
            {
                x += 5;
                Canvas.SetLeft(Player, x);
            }
            if(left == true)
            {
                x -= 5;
                Canvas.SetLeft(Player, x);
            }
            if(jump == true)
            {
                y -= Momentum;
                Canvas.SetTop(Player, y);
                Momentum -= 1;
            }
            if( y + Player.Height >= 485)
            {
                jump = false;
                y = 385;
                Canvas.SetTop(Player, y);
            }
            else
            {
                y += 5;
                Canvas.SetTop(Player, y);
            }
            Xpos.Content = x;
            Ypos.Content = y;
        }
    }
}
