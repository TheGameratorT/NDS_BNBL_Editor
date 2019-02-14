using System;
using System.Windows;

namespace NDS_BNBL_Editor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string[] ExeArgs;

        [STAThread]
        static void Main(string[] args)
        {
            ExeArgs = args;

            var application = new App();
            application.InitializeComponent();
            application.Run();
        }
    }
}
