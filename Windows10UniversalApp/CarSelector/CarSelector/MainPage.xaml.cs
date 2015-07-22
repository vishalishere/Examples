using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CarSelector
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;
      
        public const string FEATURE_NAME = "Car Selector";
        bool isAlphabetical;
        bool alphabeticalScenariosAdded;

        List<Scenario> scenarios = new List<Scenario>();
        List<Configuration> configs = new List<Configuration>();
        int configIndex;

        public MainPage()
        {
            this.InitializeComponent();
            scenarios.Add(new Scenario() { Title = "Enter Make", ClassType = typeof(Scenario1) });
            scenarios.Add(new Scenario() { Title = "Enter Model", ClassType = typeof(Scenario2) });
            scenarios.Add(new Scenario() { Title = "Enter Year", ClassType = typeof(Scenario3) });
            scenarios.Add(new Scenario() { Title = "Your Selection", ClassType = typeof(Scenario4) });
            // This is a static public property that allows downstream pages to get a handle to the MainPage instance
            // in order to call methods that are in this class.
            Current = this;
            isAlphabetical = false;
            alphabeticalScenariosAdded = false;
            SampleTitle.Text = FEATURE_NAME;
        }

        public void saveLastConfig()
        {
            configs.Add(new Configuration() { LastScenario = ScenarioFrame.CurrentSourcePageType, LastMake = tMake.Text, LastModel = tModel.Text, LastYear = tYear.Text, LastIsAlphabetical = isAlphabetical });
            configIndex = configs.Count-1;
        }
        
        public void implementLastConfig()
        {
            if (configIndex > 0)
            {
                ScenarioFrame.Navigate(configs[configIndex].LastScenario);
            for(int i=0; i<scenarios.Count; i++)
            {
                if(scenarios[i].ClassType.Equals(ScenarioFrame.CurrentSourcePageType))
                {
                        if (i > 3)
                        {
                            ScenarioControl.SelectedIndex = i - 4;
                        }
                        else
                        {
                            ScenarioControl.SelectedIndex = i;
                        }
                }
            }
            tModel.Text = configs[configIndex].LastModel;
            tMake.Text = configs[configIndex].LastMake;
            tYear.Text = configs[configIndex].LastYear;
            scenarios[0].getContent = tMake.Text;
            scenarios[1].getContent = tModel.Text;
            scenarios[2].getContent = tYear.Text;
            if(alphabeticalScenariosAdded)
            {
                scenarios[4].getContent = tMake.Text;
                scenarios[5].getContent = tModel.Text;
                scenarios[6].getContent = tYear.Text;
            }
            if(configs[configIndex].LastIsAlphabetical && alphabetical.IsChecked==false)
            {
                alphabetical.IsChecked = true;
                isAlphabetical = true;
            }
            else if(!configs[configIndex].LastIsAlphabetical && alphabetical.IsChecked==true)
            {
                alphabetical.IsChecked = false;
                isAlphabetical = false;
            }
            
                configIndex -= 1;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Populate the scenario list from the SampleConfiguration.cs file
            ScenarioControl.ItemsSource = scenarios;
            saveLastConfig();
            if (Window.Current.Bounds.Width < 640)
            {
                ScenarioControl.SelectedIndex = -1;
            }
            else
            {
                ScenarioControl.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Called whenever the user changes selection in the scenarios list.  This method will navigate to the respective
        /// sample scenario page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScenarioControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //saveLastConfig();
            if (!isAlphabetical || ScenarioControl.SelectedIndex == 3)
            {
                ScenarioFrame.Navigate(scenarios[ScenarioControl.SelectedIndex].ClassType);
            }
            else
            {
                ScenarioFrame.Navigate(scenarios[ScenarioControl.SelectedIndex + 4].ClassType);
            }
        }

        public void changeToNextScenario()
        {
          //  saveLastConfig();
            for(int i=0; i<scenarios.Count; i++)
            {
                if(scenarios[i].Equals(ScenarioControl.SelectedItem as Scenario))
                {
                    ScenarioFrame.Navigate(scenarios[i + 1].ClassType);
                }
            }
            ScenarioControl.SelectedIndex += 1;
        }

        public void updateMake(string make)
        {
            saveLastConfig();
            tMake.Text = make;
            tModel.Text = "";
            tYear.Text = "";
        }

        public void updateModel(string model)
        {
            saveLastConfig();
            tModel.Text = model;
            tYear.Text = "";
        }

        public void updateYear(string year)
        {
            saveLastConfig();
            tYear.Text = year;
        }

        public List<Scenario> Scenarios
        {
            get { return this.scenarios; }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Splitter.IsPaneOpen = (Splitter.IsPaneOpen == true) ? false : true;
        }

        private void alphabetical_Checked(object sender, RoutedEventArgs e)
        {
            if (!alphabeticalScenariosAdded)
            {
                alphabeticalScenariosAdded = true;
                Scenario alphabeticalOne = new Scenario { Title = "Enter Make", ClassType = typeof(Scenario1_Alphabetical) };
                Scenario alphabeticalTwo = new Scenario { Title = "Enter Model", ClassType = typeof(Scenario2_Alphabetical) };
                Scenario alphabeticalThree = new Scenario { Title = "Enter Year", ClassType = typeof(Scenario3_Alphabetical) };
                scenarios.Add(alphabeticalOne);
                scenarios.Add(alphabeticalTwo);
                scenarios.Add(alphabeticalThree);
                ScenarioControl.ItemsSource = scenarios;
            }
            if (isAlphabetical)
            {
                saveLastConfig();
                isAlphabetical = false;
                if (ScenarioControl.SelectedIndex == 0)
                {
                    ScenarioFrame.Navigate(scenarios[0].ClassType);
                }
                else if (ScenarioControl.SelectedIndex == 1)
                {
                    ScenarioFrame.Navigate(scenarios[1].ClassType);
                }
                else if (ScenarioControl.SelectedIndex == 2)
                {
                    ScenarioFrame.Navigate(scenarios[2].ClassType);
                }
                else
                {
                    ScenarioFrame.Navigate(scenarios[3].ClassType);
                }
            }
            else
            {
                saveLastConfig();
                isAlphabetical = true;
                if (ScenarioControl.SelectedIndex == 0)
                {
                    ScenarioFrame.Navigate(scenarios[4].ClassType);
                }
                else if (ScenarioControl.SelectedIndex == 1)
                {
                    ScenarioFrame.Navigate(scenarios[5].ClassType);
                }
                else if (ScenarioControl.SelectedIndex == 2)
                {
                    ScenarioFrame.Navigate(scenarios[6].ClassType);
                }
                else
                {
                    ScenarioFrame.Navigate(scenarios[3].ClassType);
                }
            }
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            implementLastConfig();
        }
    }

    public enum NotifyType
    {
        StatusMessage,
        ErrorMessage
    };

    public class ScenarioBindingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Scenario s = value as Scenario;
            return (MainPage.Current.Scenarios.IndexOf(s) + 1) + ") " + s.Title;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return true;
        }
    }
}
