using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CarSelector
{
 

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Scenario4 : Page
    {
        [DllImport(@"C:\Users\skitchen\Documents\TFS\WinAlign\Mainline\Src\SharedOutput\Dll\Win32\Release\HibDeviceRM.dll", EntryPoint = "getHibDriver")]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern string getHibDriver();

        MainPage rootPage = MainPage.Current;

        public Scenario4()
        {
            this.InitializeComponent();
            string bob = getHibDriver();
            string sally = "hello";
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string selection = rootPage.Scenarios[3].getContent;
            ChangeMakeText(selection);
        }
        public void ChangeMakeText(string make)
        {
            rootPage.Scenarios[3].getContent = rootPage.Scenarios[2].getContent + " " + rootPage.Scenarios[0].getContent + " " + rootPage.Scenarios[1].getContent;
            MakeSelected.Text = rootPage.Scenarios[3].getContent;
        }

        private void ModelSelected_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}
