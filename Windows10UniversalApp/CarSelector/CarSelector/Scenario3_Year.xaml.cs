
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Linq;

namespace CarSelector
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Scenario3 : Page
    {
        MainPage rootPage = MainPage.Current;
    

        public Scenario3()
        {
            this.InitializeComponent();
        }

       

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ChangeListView(rootPage.Scenarios[1].getContent);
        }

        public async void ChangeListView(string model)
        {
            listView.Items.Clear();
            Year item;
            try
            {
                string root = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
                string path = root + @"\Resources";
                StorageFolder local = await StorageFolder.GetFolderFromPathAsync(path);
                StorageFile file = await local.GetFileAsync(model + "Years.txt");
                var lines = await FileIO.ReadLinesAsync(file);
                foreach (var line in lines)
                {
                    item = new Year();
                    item.YearName = line;
                    item.YearImageSource = "../Assets/Acura.jpg";
                    item.YearRate = "150";
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
            rootPage.Scenarios[2].getContent = listView.SelectedItem.ToString();
            rootPage.updateYear(rootPage.Scenarios[2].getContent);
            rootPage.changeToNextScenario();      
        }

        private void YearGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
        }

        private void YearGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
