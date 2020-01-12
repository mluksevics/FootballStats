using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Serialization;
using StatsUI.StatsXML;
using StatsUI.Utilities;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace StatsUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LogTab.IsSelected = true;
            StatsMain.Main.CountDBdata();

            listBox.ItemsSource = Logger.Names;

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            LogTab.IsSelected = true;

            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();
            if(result == CommonFileDialogResult.Ok)
            {
                String folderPath = dialog.FileName;
                //String folderPath = "C:/Users/User/Google Drive/!Studies/Year1/Programming/PD2/XMLFirstRound/";
                StatsMain.Main.WriteToDB(folderPath);
                Content.DataContext = null;

                //scroll to bottom of the listbox
                listBox.SelectedIndex = listBox.Items.Count - 1;
                listBox.ScrollIntoView(listBox.SelectedItem);
            }
            else
            {
                Logger.Log("Lietotājs atcēlis XML direktorija izvēlēšanos");
            }

        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            LogTab.IsSelected = true;
            StatsMain.Main.DeleteAllDataDB();
            Content.DataContext = null;

            //scroll to bottom of the listbox
            listBox.SelectedIndex = listBox.Items.Count - 1;
            listBox.ScrollIntoView(listBox.SelectedItem);

        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void ViewTopTeams(object sender, MouseButtonEventArgs e)
        {
            StatsTab.Visibility = Visibility.Visible;
            StatsTab.IsSelected = true;
            Content.DataContext = StatsIO.OutputTopTeams.TopTeams();
        }

        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            LogTab.IsSelected = true;
            StatsMain.Main.CountDBdata();

            //scroll to bottom of the listbox
            listBox.SelectedIndex = listBox.Items.Count - 1;
            listBox.ScrollIntoView(listBox.SelectedItem);

        }

        private void ViewTopPlayers(object sender, MouseButtonEventArgs e)
        {
            StatsTab.Visibility = Visibility.Visible;
            StatsTab.IsSelected = true;
            Content.DataContext = StatsIO.OutputTopPlayers.TopPlayers();
        }

        private void ViewTopReferees(object sender, MouseButtonEventArgs e)
        {
            StatsTab.Visibility = Visibility.Visible;
            StatsTab.IsSelected = true;
            Content.DataContext = StatsIO.OutputTopRefs.TopReferees();

        }
    }
}
