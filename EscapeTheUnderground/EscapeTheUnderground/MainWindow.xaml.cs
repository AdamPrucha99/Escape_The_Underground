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
        private Image ammoBox;
        private BitmapImage src;
        private DispatcherTimer laserTimer;
        private Player mainPlayer;
        private Enemy normalEnemy;
        double newPos = 0;

        int HPlevel;
        bool right;
        bool left;
        bool jump;
        int MomentumUnit = 23;
        int Momentum;
        string currentImage;
        string filePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
        string direction = "right";
        string fixedDirection;
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

            // Nastavení úvodní obrazovky
            BitmapImage srcL = new BitmapImage();
            srcL.BeginInit();
            srcL.UriSource = new Uri(filePath + @"\Assets\Misc\paused_screen.png", UriKind.RelativeOrAbsolute);
            srcL.CacheOption = BitmapCacheOption.OnLoad;
            srcL.EndInit();

            Paused.Source = srcL;
            Paused.Visibility = Visibility.Visible;

            // Nastavení defaultního skinu hráče
            ChangePlayer("still", Player);

            // Objekt hráče a nepřátele
            mainPlayer = new Player();
            mainPlayer.setBaseHealth();
            mainPlayer.setBaseAmmo();
            normalEnemy = new Enemy();
            normalEnemy.Speed = 2;

            // Příprava projektilu
            laser = new Image();
            
        }
        public void startGame()
        {
            // Funkce pro odstartování hry
            // Timer pro pohyb
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += new EventHandler(MovePlayer);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            timer.Start();

            // Nastavení defaultního pozadí
            setBackround("Background1");

            // Naštení laseru z Assets

            src = new BitmapImage();
            src.BeginInit();
            src.UriSource = new Uri(filePath + @"\Assets\Misc\bullet_friendly.png", UriKind.RelativeOrAbsolute);
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.EndInit();

            // Timer pro logiku nepřátel
            DispatcherTimer Enemytimer = new DispatcherTimer();
            Enemytimer.Tick += new EventHandler(enemyCycle);
            Enemytimer.Interval = new TimeSpan(0, 0, 1);
            Enemytimer.Start();

            // Skrytí nepotřebných prvků
            Paused.Visibility = Visibility.Hidden;
            startButton.Visibility = Visibility.Hidden;
            InfoLabelTitle2.Visibility = Visibility.Hidden;
            InfoLabelTitle3.Visibility = Visibility.Hidden;
        }
        public void setBackround(string name)
        {
            // Funkce pro změnu pozadí
            currentImage = "";
            if(name !=  currentImage)
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(filePath + @"\Assets\Background\" + name + ".gif");
                image.EndInit();
                WpfAnimatedGif.ImageBehavior.SetAnimatedSource(Background, image);
                currentImage = name;
            }
        }
        private void pauseGame()
        {
            // Funkce pro pauzu hry / Konec / Zabití
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
            InfoLabelTitle.Content = "Zemřej jsi! Tvé skóre: " + player_score;
            ControlButton.Visibility = Visibility.Visible;

        }
        private int difficultyLevel()
        {
            // Nastavení dynamické obtížnosti
            int spawnProbability = 0;
            if(player_score < 5)
            {
                spawnProbability = 10;
                HPlevel = 1;
                level_label.Content = "Level: " + 1;
                setBackround("Background1");
            }
            else if(player_score >= 5 && player_score <= 11)
            {
                spawnProbability = 8;
                health_bar.Maximum = 150;
                setBackround("Background2");
                if(HPlevel == 1)
                {
                    mainPlayer.Health = 150;
                    level_label.Content = "Level: " + 2;
                }
                HPlevel = 2;
            }
            else if(player_score > 11)
            {
                spawnProbability = 6;
                health_bar.Maximum = 200;
                setBackround("Background3");
                if (HPlevel == 2)
                {
                    mainPlayer.Health = 200;
                    level_label.Content = "Level: " + 3;
                }
                HPlevel = 3;
            }
            else if(player_score > 20)
            {
                spawnProbability = 5;
                if (HPlevel == 3)
                {
                    mainPlayer.Health = 250;
                    level_label.Content = "Level: " + 4;
                }
                HPlevel = 4;
            }
            else if(player_score > 30)
            {
                spawnProbability = 4;
                level_label.Content = "Level: " + 5;
            }
            return spawnProbability;
        }
        public bool Intersected()
        {
            // Kontrola protnutí dvou objektů
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
            // Změna skinu hráče A, D, F
            if(imageName != currentImage)
            {
                var image = new BitmapImage();

                image.BeginInit();
                image.UriSource = new Uri(filePath + @"\Assets\Character\" + imageName + ".gif");
                image.EndInit();

                // Set image
                WpfAnimatedGif.ImageBehavior.SetAnimatedSource(Entity, image);
                currentImage = imageName;
            }
        }
        public void MovePlayer(object sender, EventArgs e)
        {
            // Logika pro pohyb hráče
            // Controls AI
            GameControl();

            if (Keyboard.IsKeyDown(Key.A)) { ChangePlayer("walk_left", Player); left = true; direction = "left"; }
            if(Keyboard.IsKeyDown(Key.D)) { ChangePlayer("walk_right", Player); right = true; direction = "right"; }
            if (Keyboard.IsKeyDown(Key.F))
            {
                if(direction == "right")
                {
                    ChangePlayer("shot_right", Player);
                }
                else
                {
                    ChangePlayer("shot_left", Player);
                }
            }
            if (Keyboard.IsKeyUp(Key.A)) { left = false; }
            if(Keyboard.IsKeyUp(Key.D)) { right = false; }
            if (Keyboard.IsKeyUp(Key.F) && Keyboard.IsKeyUp(Key.D) && Keyboard.IsKeyUp(Key.A))
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

            // Nastavení kolizí s překážkou
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
        }

        private void Player_KeyDown(object sender, KeyEventArgs e)
        {
            // Funkce pro střelbu (event keydown)
            if(Keyboard.IsKeyDown(Key.F))
            {
                MakeLaser(laser);
                clearable = true;
                ChangePlayer("shot_right", Player);
            }

        }
        private void MakeLaser(Image laser)
        {
            // Vytvoření projektilu
            if(shootable == true)
            {
                laser.Source = src;
                laser.Stretch = Stretch.Uniform;
                laser.Width = 20;
                laser.Height = 5;
                laser.Tag = "projectile";

                // Nastavení umístění projektilu
                Canvas.SetLeft(laser, Canvas.GetLeft(Player) + (Player.Width / 2) + 20);
                Canvas.SetTop(laser, Canvas.GetTop(Player) + 70);

                // Přidání projektilu do canvasu
                LayoutRoot.Children.Add(laser);

            }
            
        }
        private void undergroundGame_KeyDown(object sender, KeyEventArgs e)
        {
            // Střelba projektilu
            if ((Key)e.Key == Key.F && shootable == true && mainPlayer.Ammo > 0)
            {
                MakeLaser(laser);
                fireLaser();
                shootable = false;
                mainPlayer.Ammo -= 1;
                ammo_label.Content = "Náboje: " + mainPlayer.Ammo;
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
            // Nastavení směru vypáleného projektilu
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

            // Odebrání projektilu pokud dojde k opuštění canvasu
            if(newPos > Background.Width || newPos <= 0)
            {
                laserTimer.Stop();
                LayoutRoot.Children.Remove(laser);
                shootable = true;
            }
        }
        private void makeEnemy(int left)
        {
            // Vytvoření nepřátele
            BitmapImage enemy = new BitmapImage();
            enemy.BeginInit();
            enemy.UriSource = new Uri(filePath + @"\Assets\Character\still_enemy.gif", UriKind.RelativeOrAbsolute);
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
        private void makeAmmoBox(double left)
        {
            // Vytvoření nábojové schránky
            BitmapImage aBox = new BitmapImage();
            aBox.BeginInit();
            aBox.UriSource = new Uri(filePath + @"\Assets\Misc\ammo.gif", UriKind.RelativeOrAbsolute);
            aBox.CacheOption = BitmapCacheOption.OnLoad;
            aBox.EndInit();

            ammoBox = new Image();

            ammoBox.Tag = "ammobox";
            ammoBox.Width = 60;
            ammoBox.Height = 30;
            ammoBox.Stretch = Stretch.Uniform;
            ammoBox.Source = aBox;

            LayoutRoot.Children.Add(ammoBox);

            Canvas.SetTop(ammoBox, Canvas.GetTop(Player) + 75);
            Canvas.SetLeft(ammoBox, left);

        }
        public void GameControl()
        {
            // "Engine hry"

            // Chování počítadla životů
            health_bar.Value = mainPlayer.Health;
            HP.Content = "HP: (" + Math.Round(mainPlayer.Health, 0) + ")";
            if (mainPlayer.Health <= 15 && mainPlayer.Health >= 0)
            {
                health_bar.Foreground = Brushes.DarkRed;
            }
            else if (mainPlayer.Health >= 15 && mainPlayer.Health <= 30)
            {
                health_bar.Foreground = Brushes.DarkOrange;
            }
            else if(mainPlayer.Health <= 0)
            {
                pauseGame();
            }
            else
            {
                health_bar.Foreground = Brushes.DarkGreen;
            }
            // Loopování Image v canvasu
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
                    {   // AI nepřátel (pohyb a hledání pozice hráče)
                        Canvas.SetTop(tb, 385);
                        if (Canvas.GetLeft(tb) > Canvas.GetLeft(Player) - 60)
                        {
                            // INPUT SPEED
                            Canvas.SetLeft(tb, Canvas.GetLeft(tb) - normalEnemy.Speed);
                        }
                       if(Canvas.GetLeft(tb) < Canvas.GetLeft(Player) + 60)
                       {
                            Canvas.SetLeft(tb, Canvas.GetLeft(tb) + normalEnemy.Speed);
                        }
                        Rect tbRect = new Rect();
                        tbRect.Location = tb.PointToScreen(new Point(0D, 0D));
                        tbRect.Size = new Size(tb.Width, tb.Height);

                        Rect PlayerRect = new Rect();
                        PlayerRect.Location = Player.PointToScreen(new Point(0D, 0D));
                        PlayerRect.Size = new Size(Player.Width, Player.Height);

                        if(PlayerRect.IntersectsWith(tbRect)) {

                            // Damage hráči pokud dojde k protnutí
                            mainPlayer.Damage(0.1);
                        }
                    }
                }
                foreach (Image sh in FindVisualChildren<Image>(LayoutRoot))
                {
                    // Střetnutí hráče a projektilu
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
                            if(mainPlayer.Ammo < 5)
                            {
                                makeAmmoBox(Canvas.GetLeft(tb));
                            }
                            LayoutRoot.Children.Remove(tb);
                            LayoutRoot.Children.Remove(sh);

                            player_score += 1;
                            score_label.Content = "Skóre: " + player_score;
                        }
                    }
                    if(sh_tst == "ammobox" && tst == "Player")
                    {
                        // Fuknce pro sběr nábojové schránky 
                        Rect shRect = new Rect();
                        shRect.Location = sh.PointToScreen(new Point(0D, 0D));
                        shRect.Size = new Size(sh.Width, sh.Height);

                        Rect tbRect = new Rect();
                        tbRect.Location = tb.PointToScreen(new Point(0D, 0D));
                        tbRect.Size = new Size(tb.Width, tb.Height);

                        if(shRect.IntersectsWith(tbRect))
                        {
                            mainPlayer.Ammo += 5;
                            ammo_label.Content = "Náboje: " + mainPlayer.Ammo;
                            LayoutRoot.Children.Remove(sh);
                        }
                    }
                }
            }
            // Ensure no further removal is made
            removable = false;
        }
        private void enemyCycle(object sender, EventArgs e)
        {
            //Funkce pro spawnování nepřátel
            Random rnd = new Random();
            Random rndE = new Random();
            int eCount = rndE.Next(1, 3);
            int side = rnd.Next(1, 3);
            int spawnProbability = difficultyLevel();
            int spawn = rnd.Next(1, spawnProbability);
            int hittableInt = rnd.Next(1, 3);
            if(hittableInt == 1) { hittable = true; } else { hittable = false; }
            if (spawn == 1)
            {
                if(side == 1)
                {
                    //makeEnemy(-100);
                    //makeEnemy(1300);
                    for(int x = 0; x <= eCount; x++)
                    {
                        makeEnemy(-100*x);
                    }
                }
                else if(side == 2)
                {
                    for (int x = 0; x <= eCount; x++)
                    {
                        makeEnemy(1300 + x*100);
                    }
                }
            }
        }
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            // Logika pro funkci na loopování UI elements
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
            // End game
            player_score = 0;
            score_label.Content = player_score;
            mainPlayer.Health = 100;
            health_bar.Value = mainPlayer.Health;
            ControlButton.Visibility = Visibility.Hidden;
            Paused.Visibility = Visibility.Hidden;
            InfoLabelTitle.Visibility = Visibility.Hidden;
            removable = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Button pro odstartování hry
            startGame();
        }
    }
}
