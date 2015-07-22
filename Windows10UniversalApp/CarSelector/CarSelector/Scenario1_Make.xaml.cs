using System;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.IO;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace CarSelector
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Scenario1 : Page
    {
        MainPage rootPage = MainPage.Current;

        public Scenario1()
        {
            this.InitializeComponent();
            addItemsToList();
        }


        public async void addItemsToList()
        {
            try
            {
                string root = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
                string path = root + @"\Resources";
                StorageFolder local = await StorageFolder.GetFolderFromPathAsync(path);
                StorageFile file = await local.GetFileAsync("CarMakes.txt");
                var lines = await FileIO.ReadLinesAsync(file);
                foreach (var line in lines)
                {
                    Make item = new Make();
                    item.MakeName = line;
                    item.MakeImageSource = "../Assets/Acura.jpg";
                    item.MakeRate = "150";
                    item.Text = line;
                    listView.Items.Add(item);
                }
               
            }
            catch (Exception e)
            {
                Debug.WriteLine("error: " + e);
            }

        }
        
        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            rootPage.Scenarios[0].getContent = listView.SelectedItem.ToString();
            rootPage.updateMake(rootPage.Scenarios[0].getContent);
            rootPage.changeToNextScenario();
        }

        private void alphabet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
        }

        private void MakeGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            
            rootPage.Scenarios[0].getContent = e.ClickedItem.ToString();
            rootPage.changeToNextScenario();
            rootPage.updateMake(rootPage.Scenarios[0].getContent);
        }

        private void MakeGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
        }
    }
}