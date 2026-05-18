using System.Windows.Forms;
using System;

namespace SnakeGame
{
    // Programın başlangıç noktası — sadece formu açar
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SnakeGameForm());
        }
    }
}
