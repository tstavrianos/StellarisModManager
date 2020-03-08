using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using Serilog;
using Stellaris.Data;
using Stellaris.Data.Json;
using StellarisModManager.Core;

namespace StellarisModManager
{
    internal sealed partial class MainWindow
    {
        public ModManager Manager { get; }

        public MainWindow()
        {
            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            this.Manager = new ModManager();

            this.InitializeComponent();
            this.MoveToTop.IsEnabled = false;
            this.MoveUp.IsEnabled = false;
            this.MoveDown.IsEnabled = false;
            this.MoveToBottom.IsEnabled = false;
        }
        
        private void Export_Clicked(object sender, RoutedEventArgs e)
        {
            this.Manager.Save();
        }

        private void Alpha_Clicked(object sender, RoutedEventArgs e)
        {
            this.Manager.AlphaSort();
        }
        
        private void Reverse_Clicked(object sender, RoutedEventArgs e)
        {
            this.Manager.ReverseOrder();
        }

        private void MoveToTop_Clicked(object sender, RoutedEventArgs e)
        {
            this.Manager.MoveToTop();
            this.CheckButtons();
        }

        private void MoveUp_Clicked(object sender, RoutedEventArgs e)
        {
            this.Manager.MoveUp();
            this.CheckButtons();
        }

        private void MoveDown_Clicked(object sender, RoutedEventArgs e)
        {
            this.Manager.MoveDown();
            this.CheckButtons();
        }

        private void MoveToBottom_Clicked(object sender, RoutedEventArgs e)
        {
            this.Manager.MoveToBottom();
            this.CheckButtons();
        }

        private void ModList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.CheckButtons();
        }

        private void CheckButtons()
        {
            var first = -1;
            var last = -1;
            for (var i = 0; i < this.Manager.Mods.Count; i++)
            {
                if (!this.Manager.Mods[i].IsSelected) continue;
                if (first == -1) first = i;
                last = i;
            }
            this.MoveToTop.IsEnabled = last > 0;
            this.MoveUp.IsEnabled = last > 0;
            this.MoveDown.IsEnabled = first >= 0 && first < this.Manager.Mods.Count - 1;
            this.MoveToBottom.IsEnabled = first >= 0 && first < this.Manager.Mods.Count - 1;
        }

        private void CheckAll_Clicked(object sender, RoutedEventArgs e)
        {
            this.Manager.CheckAll();
        }

        private void UncheckAll_Clicked(object sender, RoutedEventArgs e)
        {
            this.Manager.UncheckAll();
        }

        private void InvertCheck_Clicked(object sender, RoutedEventArgs e)
        {
            this.Manager.InvertCheck();        
        }
    }
}