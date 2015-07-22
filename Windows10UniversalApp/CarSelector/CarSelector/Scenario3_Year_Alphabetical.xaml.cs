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
    public sealed partial class Scenario3_Alphabetical : Page
    {
        MainPage rootPage = MainPage.Current;
        ObservableCollection<Year> YearList;

        public Scenario3_Alphabetical()
        {
            this.InitializeComponent();
        }

        internal List<GroupInfoList<object>> GetGroupsByLetter()
        {
            var groups = new List<GroupInfoList<object>>();

            var query = from item in YearList
                        orderby ((Year)item).YearName
                        group item by ((Year)item).YearName[0] into g
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ChangeListView(rootPage.Scenarios[1].getContent);
        }

        public async void ChangeListView(string model)
        {
            try
            {
                YearList = new ObservableCollection<Year>();
                string root = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
                string path = root + @"\Resources";
                StorageFolder local = await StorageFolder.GetFolderFromPathAsync(path);
                StorageFile file = await local.GetFileAsync( model + "Years.txt");
                var lines = await FileIO.ReadLinesAsync(file);
                foreach (var line in lines)
                {
                    Year item = new Year();
                    item.YearName = line;
                    item.YearImageSource = "../Assets/Acura.jpg";
                    item.YearRate = "150";
                    YearList.Add(item);
                    item.Text = line;
                }
                YearList = new ObservableCollection<Year>(YearList.OrderBy(c => c.YearName));
                YearsCollectionViewSource.Source = GetGroupsByLetter();
            }
            catch (Exception e)
            {
                Debug.WriteLine("error: " + e);
            }
        }


        private void YearGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            rootPage.Scenarios[2].getContent = e.ClickedItem.ToString();
            rootPage.updateYear(rootPage.Scenarios[2].getContent);
            rootPage.changeToNextScenario();    
        }

        private void YearGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}