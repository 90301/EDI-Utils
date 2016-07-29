using System;
using System.Collections.Generic;
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

namespace ExampleApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            console1.Text = "The button was clicked: ";

            List<Person> allPeople = new List<Person>();
            for (int i=0;i<10;i++)
            {
                Person p = new Person();
                p.name = " PERSON X" + i;
                p.age = i;
                allPeople.Add(p);
            }
            
            foreach (Person p in allPeople)
            {
                console1.Text += Environment.NewLine;
                console1.Text += p.outputInfo() + Environment.NewLine;
                console1.Text += p.ToString() + Environment.NewLine;
            }

        }
    }
}
