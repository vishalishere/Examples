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
    public sealed partial class Scenario1_Alphabetical : Page
    {
        MainPage rootPage = MainPage.Current;
        ObservableCollection<Make> MakeList;

        public Scenario1_Alphabetical()
        {
            this.InitializeComponent();
            addItemsToList();
        }

        internal List<GroupInfoList<object>> GetGroupsByLetter()
        {
            var groups = new List<GroupInfoList<object>>();

            var query = from item in MakeList
                        orderby ((Make)item).MakeName
                        group item by ((Make)item).MakeName[0] into g
                        select new { GroupName = g.Key, Items = g };
            foreach (var g in query)
            {
                var info = new GroupInfoList<object>();
                info.Key = g.GroupName;
                foreach (var item in g.Items)
                {
                    info.Add(item);
                }

                groups.Add(info);
            }

            return groups;
        }

        public async void addItemsToList()
        {
            try
            {
                MakeList = new ObservableCollection<Make>();
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
                    MakeList.Add(item);
                    item.Text = line;
                }
                MakeList = new ObservableCollection<Make>(MakeList.OrderBy(c => c.MakeName));
                MakesCollectionViewSource.Source = GetGroupsByLetter();
            }
            catch (Exception e)
            {
                Debug.WriteLine("error: " + e);
            }

        }

        private void MakeGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            rootPage.Scenarios[0].getContent = e.ClickedItem.ToString();
            rootPage.updateMake(rootPage.Scenarios[0].getContent);
            rootPage.changeToNextScenario(); 
        }
    }
}