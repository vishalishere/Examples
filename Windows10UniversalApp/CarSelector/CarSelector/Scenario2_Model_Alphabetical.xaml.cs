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
    public sealed partial class Scenario2_Alphabetical : Page
    {
        MainPage rootPage = MainPage.Current;
        ObservableCollection<Model> ModelList;

        public Scenario2_Alphabetical()
        {
            this.InitializeComponent();
        }

        internal List<GroupInfoList<object>> GetGroupsByLetter()
        {
            var groups = new List<GroupInfoList<object>>();

            var query = from item in ModelList
                        orderby ((Model)item).ModelName
                        group item by ((Model)item).ModelName[0] into g
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
            ChangeListView(rootPage.Scenarios[0].getContent);
        }

        public async void ChangeListView(string make)
        {
            try
            {
                ModelList = new ObservableCollection<Model>();
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
                    ModelList.Add(item);
                    item.Text = line;
                }
                ModelList = new ObservableCollection<Model>(ModelList.OrderBy(c => c.ModelName));
                ModelsCollectionViewSource.Source = GetGroupsByLetter();
            }
            catch (Exception e)
            {
                Debug.WriteLine("error: " + e);
            }
        }


        private void ModelGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            rootPage.Scenarios[1].getContent = e.ClickedItem.ToString();
            rootPage.updateModel(rootPage.Scenarios[1].getContent);
            rootPage.changeToNextScenario();
        }

        private void ModelGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
