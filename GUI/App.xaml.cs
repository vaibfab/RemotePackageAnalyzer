using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow winOne = new MainWindow();
            winOne.Title = winOne.Title+" | Client 1";
            winOne.Top = 15;
            winOne.Left = 20;
            winOne.Show();
        }
    }
}
