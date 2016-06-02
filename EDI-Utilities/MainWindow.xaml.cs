using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace EDI_Utilities
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

        private void richTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textSelect(object sender, RoutedEventArgs e)
        {
            /*
            Console.Out.WriteLine("Text Select: " + sender + " Routed Event args: " + e);
            //Find the selected text
            RichTextBox rtb = (RichTextBox) sender;

            String selected = rtb.Selection.Text;
            Console.Out.WriteLine("Text selected: " + selected);
            //find selection in other textbox
            RichTextBox otherBox = null;
            if (richTextBox.Equals(rtb))
            {
                otherBox = richTextBox_Copy;
            } else
            {
                otherBox = richTextBox;
            }
            //switch to textbox?
            if (!String.IsNullOrEmpty(otherBox.Document.))
            {
                otherBox.Selection.Start = 0;
                otherBox.SelectionLength = otherBox.Text.Length;
            }
            */
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            String workingText = sourceTextbox.Text;

            Regex rgx = new Regex("[ ]{2,}");
            String rtrned = "";
            String delmiter = delimiterTextbox.Text;
            rtrned = rgx.Replace(workingText, delmiter);
            delimitedTextbox.Text = rtrned;
        }
    }
}
