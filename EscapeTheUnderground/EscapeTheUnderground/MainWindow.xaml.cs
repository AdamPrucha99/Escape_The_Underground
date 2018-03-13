using System;
using System.Collections.Generic;
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

        double x = 0;
        double y = 0;
        public MainWindow()
        {
            InitializeComponent();
            
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += new EventHandler(MovePlayer);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            timer.Start();
        }
        public void MovePlayer(object sender, EventArgs e)
        {

            if (Keyboard.IsKeyDown(Key.A)) { left = true; }
            if(Keyboard.IsKeyDown(Key.D)) { right = true; }
            if(Keyboard.IsKeyUp(Key.A)) { left = false; }
            if(Keyboard.IsKeyUp(Key.D)) { right = false; }
            // Kolize

            double PlayerLeft = Canvas.GetLeft(Player);
            double PlayerRight = Canvas.GetRight(Player);
            double PlayerTop = Canvas.GetTop(Player);
            double PlayerBot = Canvas.GetBottom(Player);

            double ColsLeft = Canvas.GetLeft(colBlock);
            double ColsRight = Canvas.GetRight(colBlock);
            double ColsTop = Canvas.GetTop(colBlock);
            double ColsBot = Canvas.GetBottom(colBlock);

            if(PlayerLeft > ColsLeft - Player.Width*1.1 && PlayerLeft < ColsLeft + colBlock.Width && y >= Canvas.GetTop(colBlock) - colBlock.Height)
            {
                right = false;
            }
            if (PlayerLeft < ColsLeft  + colBlock.Width+5 && PlayerLeft > ColsLeft + colBlock.Width && y >= Canvas.GetTop(colBlock) - colBlock.Height)
            {
                left = false;
            }
            if(PlayerTop >= colBlock.Height && PlayerLeft >= ColsLeft- Player.Width && PlayerLeft < ColsLeft + colBlock.Width && PlayerTop + colBlock.Height >= ColsTop -7)
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
            if( y + Player.Height >= 500)
            {
                jump = false;
                y = 400;
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
