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
        private Image laser;
        private Image enemyAI;
        private BitmapImage src;
        private DispatcherTimer laserTimer;
        double newPos = 0;

        bool right;
        bool left;
        bool jump;
        int MomentumUnit = 23;
        int Momentum;
        string currentImage;
        string filePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
        double player_health = 100;
        string direction = "right";
        string fixedDirection;
        int player_ammo = 50;
        int player_score = 0;
        bool shootable = true;
        bool hittable = true;
        bool clearable = false;
        bool removable = false;
        double x = 0;
        double y = 0;
        public MainWindow()
        {
            InitializeComponent();
            ChangePlayer("still", Player);

            laser = new Image();
            
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

            //load laser
            
            src = new BitmapImage();
            src.BeginInit();
            src.UriSource = new Uri(pathToFiles + @"\Assets\Misc\bullet_friendly.png", UriKind.RelativeOrAbsolute);
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.EndInit();
            // Enemy Timer
            DispatcherTimer Enemytimer = new DispatcherTimer();
            Enemytimer.Tick += new EventHandler(enemyCycle);
            Enemytimer.Interval = new TimeSpan(0,0,1);
            Enemytimer.Start();
        }
        private void pauseGame()
        {
            BitmapImage srcL = new BitmapImage();
            srcL.BeginInit();
            srcL.UriSource = new Uri(filePath + @"\Assets\Misc\paused_screen.png", UriKind.RelativeOrAbsolute);
            srcL.CacheOption = BitmapCacheOption.OnLoad;
            srcL.EndInit();

            Paused.Source = srcL;

            

            Paused.Visibility = Visibility.Visible;
            InfoLabelTitle.Visibility = Visibility.Visible;
            InfoLabelTitle.FontSize = 24;
            InfoLabelTitle.Foreground = Brushes.White;
            ControlButton.Visibility = Visibility.Visible;

        }
        private int difficultyLevel()
        {
            int spawnProbability = 0;
            if(player_score < 5)
            {
                spawnProbability = 10;
            }
            else if(player_score >= 5 && player_score <= 11)
            {
                spawnProbability = 5;
            }
            else if(player_score > 11)
            {
                spawnProbability = 2;
            }
            return spawnProbability;
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
        public void ChangePlayer(string imageName, Image Entity)
        {
            if(imageName != currentImage)
            {
                string pathToFiles = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
                var image = new BitmapImage();

                image.BeginInit();
                image.UriSource = new Uri(pathToFiles + @"\Assets\Character\" + imageName + ".gif");
                image.EndInit();

                // Set image
                WpfAnimatedGif.ImageBehavior.SetAnimatedSource(Entity, image);
                currentImage = imageName;
            }
        }
        public void MovePlayer(object sender, EventArgs e)
        {
            // Controls AI
            GameControl();

            if (Keyboard.IsKeyDown(Key.A)) { ChangePlayer("walk_left", Player); left = true; direction = "left"; }
            if(Keyboard.IsKeyDown(Key.D)) { ChangePlayer("walk_right", Player); right = true; direction = "right"; }
            if (Keyboard.IsKeyDown(Key.E))
            {
                ChangePlayer("fight_right", Player);
                //changable = false;
            }
            if (Keyboard.IsKeyUp(Key.A)) { left = false; }
            if(Keyboard.IsKeyUp(Key.D)) { right = false; }
            if (Keyboard.IsKeyUp(Key.E) && Keyboard.IsKeyUp(Key.D) && Keyboard.IsKeyUp(Key.A))
            {
                ChangePlayer("still", Player);
            }

            double PlayerLeft = Canvas.GetLeft(Player);
            double PlayerRight = Canvas.GetRight(Player);
            double PlayerTop = Canvas.GetTop(Player);
            double PlayerBot = Canvas.GetBottom(Player);

            double ColsLeft = Canvas.GetLeft(colBlock);
            double ColsRight = Canvas.GetRight(colBlock);
            double ColsTop = Canvas.GetTop(colBlock);
            double ColsBot = Canvas.GetBottom(colBlock);

            int PlayerWidth = 36;

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

        private void Player_KeyDown(object sender, KeyEventArgs e)
        {
            if(Keyboard.IsKeyDown(Key.F))
            {
                MakeLaser(laser);
                clearable = true;
            }

        }
        private void MakeLaser(Image laser)
        {
            if(shootable == true)
            {
                laser.Source = src;
                laser.Stretch = Stretch.Uniform;
                laser.Width = 20;
                laser.Height = 5;
                laser.Tag = "projectile";

                Canvas.SetLeft(laser, Canvas.GetLeft(Player) + (Player.Width / 2));
                Canvas.SetTop(laser, Canvas.GetTop(Player) + 75);

                LayoutRoot.Children.Add(laser);

            }
            
        }
        private void undergroundGame_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Key)e.Key == Key.F && shootable == true && player_ammo > 0)
            {
                MakeLaser(laser);
                fireLaser();
                shootable = false;
                player_ammo -= 1;
                ammo_label.Content = "Náboje: " +  player_ammo;
                // Add for later (!important)
                // makeEnemy();
            }
        }
        private void fireLaser()
        {
            laserTimer = new DispatcherTimer();
            fixedDirection = direction;
            laserTimer.Tick += new EventHandler(laserTimer_tick);
            laserTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            laserTimer.Start();
        }
        private void laserTimer_tick(object sender, EventArgs e)
        {
            if(fixedDirection == "left")
            {
                newPos = Canvas.GetLeft(laser) - 10;
                Canvas.SetLeft(laser, newPos);
            } 
            else if(fixedDirection == "right")
            {
                newPos = newPos = Canvas.GetLeft(laser) + 10;
                Canvas.SetLeft(laser, newPos);
            }

            if(newPos > Background.Width || newPos <= 0)
            {
                laserTimer.Stop();
                LayoutRoot.Children.Remove(laser);
                shootable = true;
            }
        }
        private void makeEnemy(int left)
        {
            string pathToFiles = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            BitmapImage enemy = new BitmapImage();
            enemy.BeginInit();
            enemy.UriSource = new Uri(pathToFiles + @"\Assets\Character\still.gif", UriKind.RelativeOrAbsolute);
            enemy.CacheOption = BitmapCacheOption.OnLoad;
            enemy.EndInit();

            enemyAI = new Image();

            enemyAI.Tag = "enemy";
            enemyAI.Width = 135;
            enemyAI.Height = 100;
            enemyAI.Stretch = Stretch.Uniform;
            //Canvas.SetLeft(enemyAI, Canvas.GetLeft(Player) + left + (Player.Width / 2));
            Canvas.SetTop(enemyAI, Canvas.GetTop(Player) + 75);

            Canvas.SetLeft(enemyAI, left);
            enemyAI.Source = enemy;
            LayoutRoot.Children.Add(enemyAI);
        }
        public void GameControl()
        {
            health_bar.Value = player_health;
            if (player_health <= 15 && player_health >= 0)
            {
                health_bar.Foreground = Brushes.DarkRed;
            }
            else if (player_health >= 15 && player_health <= 30)
            {
                health_bar.Foreground = Brushes.DarkOrange;
            }
            else if(player_health <= 0)
            {
                pauseGame();
            }
            else
            {
                health_bar.Foreground = Brushes.DarkGreen;
            }
            foreach (Image tb in FindVisualChildren<Image>(LayoutRoot))
            {
                string tst = "";
                if (tb.Tag != null) { tst = tb.Tag.ToString(); }
                if (tst == "enemy")
                {
                    if(removable) {
                        LayoutRoot.Children.Remove(tb);
                    }
                    else
                    {
                        Canvas.SetTop(tb, 385);
                        if (Canvas.GetLeft(tb) > Canvas.GetLeft(Player) - 60)
                        {
                            Canvas.SetLeft(tb, Canvas.GetLeft(tb) - 2);
                            // Walk enemy to right
                            // Rotate enemy to right
                        }
                       if(Canvas.GetLeft(tb) < Canvas.GetLeft(Player) + 60)
                       {
                            Canvas.SetLeft(tb, Canvas.GetLeft(tb) + 2);
                            // Walk enemy to left
                            // Rotate enemy to left
                       }
                        Rect tbRect = new Rect();
                        tbRect.Location = tb.PointToScreen(new Point(0D, 0D));
                        tbRect.Size = new Size(tb.Width, tb.Height);

                        Rect PlayerRect = new Rect();
                        PlayerRect.Location = Player.PointToScreen(new Point(0D, 0D));
                        PlayerRect.Size = new Size(Player.Width, Player.Height);

                        if(PlayerRect.IntersectsWith(tbRect)) {
                            player_health -= 0.1;
                        }
                    }
                }
                foreach (Image sh in FindVisualChildren<Image>(LayoutRoot))
                {
                    string sh_tst = "";
                    if (sh.Tag != null) { sh_tst = sh.Tag.ToString(); }
                    if (sh_tst == "projectile" && tst == "enemy")
                    {
                        Rect shRect = new Rect();
                        shRect.Location = sh.PointToScreen(new Point(0D, 0D));
                        shRect.Size = new Size(sh.Width, sh.Height);

                        Rect tbRect = new Rect();
                        tbRect.Location = tb.PointToScreen(new Point(0D, 0D));
                        tbRect.Size = new Size(tb.Width, tb.Height);

                        if (shRect.IntersectsWith(tbRect))
                        {
                            LayoutRoot.Children.Remove(tb);
                            LayoutRoot.Children.Remove(sh);
                            player_score += 1;
                            score_label.Content = "Skóre: " + player_score;
                        }
                    }
                }
            }
            // Ensure no further removal is made
            removable = false;
        }
        private void enemyCycle(object sender, EventArgs e)
        {
            Random rnd = new Random();
            int side = rnd.Next(1, 3);
            int spawnProbability = difficultyLevel();
            Debug.WriteLine(spawnProbability + "xxxxxxxxxxxxxx");
            int spawn = rnd.Next(1, spawnProbability);
            int hittableInt = rnd.Next(1, 3);
            if(hittableInt == 1) { hittable = true; } else { hittable = false; }
            if (spawn == 1)
            {
                if(side == 1)
                {
                    makeEnemy(-100);
                    makeEnemy(1300);
                }
                else if(side == 2)
                {
                    makeEnemy(1300);
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
        private void ControlButton_Click(object sender, RoutedEventArgs e)
        {
            player_score = 0;
            score_label.Content = player_score;
            player_health = 100;
            health_bar.Value = player_health;
            ControlButton.Visibility = Visibility.Hidden;
            Paused.Visibility = Visibility.Hidden;
            InfoLabelTitle.Visibility = Visibility.Hidden;
            removable = true;
        }
    }
}
