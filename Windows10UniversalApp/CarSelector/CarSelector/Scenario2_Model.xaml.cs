using System;
using System.Text;
using System.Collections.Generic;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Linq;

namespace CarSelector
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Scenario2 : Page
    {
        MainPage rootPage = MainPage.Current;

        public Scenario2()
        {
            this.InitializeComponent();
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ChangeListView(rootPage.Scenarios[0].getContent);
        }

        public async void ChangeListView(string make)
        {
            listView.Items.Clear();
            try
            {
               
                string root = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
                string path = root + @"\Resources";
                StorageFolder local = await StorageFolder.GetFolderFromPathAsync(path);
                StorageFile file = await local.GetFileAsync(make + "Models.txt");
                var lines = await FileIO.ReadLinesAsync(file);
                foreach (var line in lines)
                {
                    Model item = new Model();
                    item.ModelName = line;
                    item.ModelImageSource = "../Assets/Acura.jpg";
                    item.ModelRate = "150";
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
            rootPage.Scenarios[1].getContent = listView.SelectedItem.ToString();
            rootPage.updateModel(rootPage.Scenarios[1].getContent);
            rootPage.changeToNextScenario();
        }

        private void ModelGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
           
        }

        private void ModelGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
