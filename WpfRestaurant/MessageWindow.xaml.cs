using System;
using MahApps.Metro.Controls;

namespace WpfRestaurant
{
    /// <summary>
    ///     MessageWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MessageWindow : MetroWindow
    {
        private readonly MainWindow _mainWindow;

        public MessageWindow(string message, MainWindow mainWindow, string title = "通知")
        {
            _mainWindow = mainWindow;
            InitializeComponent();
            TextBlock.Text = message;
            Title = title;
            //var dispatcherTimer = new DispatcherTimer();
            //dispatcherTimer.Tick += CloseWindow;
            //dispatcherTimer.Interval = new TimeSpan(0, 0, 5);
            //dispatcherTimer.Start();
        }

        private void CloseWindow(object sender, EventArgs e)
        {
            Close();
        }

        private void MessageWindow_OnClosed(object sender, EventArgs e)
        {
            _mainWindow.MessageWindows.Remove(this);
        }
    }
}