using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace aspnet_debug.Extension.Views
{
    public class DebugDefinition
    {
        public ProjectDefinition Project { get; set; }
        public string Command { get; set; }
        public string Endpoint { get; set; }
    }
    /// <summary>
    /// Interaction logic for ProjectSelector.xaml
    /// </summary>
    public partial class ProjectSelector : Window, IDisposable
    {
        public DebugDefinition DebugDefinition { get; private set; }

        public ProjectSelectorViewModel ViewModel { get; private set; }

        public ProjectSelector(ProjectSelectorViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            comboBoxProjects.ItemsSource = viewModel.Projects;
            comboBoxProjects.DisplayMemberPath = "Name";
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ValidateInput();
                DebugDefinition = new DebugDefinition();
                DebugDefinition.Project = (ProjectDefinition)comboBoxProjects.SelectionBoxItem;
                DebugDefinition.Command = textBoxLaunchCommand.Text;
                DebugDefinition.Endpoint = textBoxDebugEndpoint.Text;
                DialogResult = true;
                Close();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                DialogResult = false;
            }
        }

        private void ValidateInput()
        {
            if(string.IsNullOrWhiteSpace(textBoxDebugEndpoint.Text))
                throw new Exception("Debug endpoint is empty.");
            if (string.IsNullOrWhiteSpace(textBoxLaunchCommand.Text))
                throw new Exception("Launch command is empty.");
        }

        public void Dispose()
        {

        }
    }
}
