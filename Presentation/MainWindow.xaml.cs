using Plugin.PluginHost;
using SharedContracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Presentation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PluginManager plugiManager;
        private List<object> plugins; 

        public MainWindow()
        {
            InitializeComponent();
            plugiManager = new PluginManager();
        }

        private void GetPlugins_OnClick(object sender, RoutedEventArgs e)
        {
            plugins = plugiManager.CreatePlugins(typeof(IPlugin2), null, null);

            List<string> names = new List<string>();  

            foreach (object plugin in plugins)
            {
                names.Add(plugin.ToString()); 
            }

            PluginsListBox.ItemsSource = names;
        }
    }
}