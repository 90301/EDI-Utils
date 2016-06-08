using System;
using System.Collections;
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
            delimiter = delimiterTextbox.Text;
            rtrned = rgx.Replace(workingText, delimiter);
            
            delimitedTextbox.Text = rtrned;

            parseSourceLines();
            parseFields();

        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void findTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Search?
        }

        private void findNext(object sender, RoutedEventArgs e)
        {
            //perform a basic search for the string
            String findThis = findTextbox.Text;

            string output = findString(findThis);

            fieldFinderResultsTextbox.Text = output;

            /*
            String workingText = sourceTextbox.Text;
            int index = workingText.IndexOf(findThis);
            //Alt Method, split every line


            Console.WriteLine("INDEX: " + index);
            String results = "Index: " + index + Environment.NewLine;
            fieldFinderResultsTextbox.Text += results;

            //need the number of times the field is present
            //find the field name
            //find the line number
            //find the distance from the line.
            */

        }

        public List<String> sourceLines = new List<String>();
        public List<String> delimitedLines = new List<String>();
        public void parseSourceLines()
        {
            String workingText = sourceTextbox.Text;
            sourceLines.Clear();
            delimitedLines.Clear();

            String[] lines = workingText.Split(new string[] { Environment.NewLine, "\r\n", "\n" }, StringSplitOptions.None);

            foreach (String line in lines)
            {
                sourceLines.Add(line);
                delimitedLines.Add(toDelimited(line));
                // this adds all the next lines
            }

        }

        public String toDelimited(String sourceText)
        {
            Regex rgx = new Regex("[ ]{2,}");
            String delimitedText = "";
            String delimiter = delimiterTextbox.Text;
            delimitedText = rgx.Replace(sourceText, delimiter);
            return delimitedText;
        }
        String delimiter;
        Dictionary<String, int> fieldCounts = new Dictionary<string, int>();
        /// <summary>
        /// parses all the fields (first item in delimited text)
        /// then gets a count for how many times each field is present
        /// </summary>
        /// <remarks>
        /// should be run AFTER delimited lines are parsed
        /// </remarks>
        public void parseFields()
        {
            fieldCounts.Clear();
            bool debugFunction = true;
            delimiter = delimiterTextbox.Text;
            foreach (String dLine in delimitedLines)
            {
                String fieldName = parseFieldName(dLine);
                if (fieldName == null)
                {
                    Console.WriteLine("Wrong formatting on line: " + dLine);
                }
                else
                {
                    if (debugFunction)
                    {
                        Console.WriteLine(fieldName);
                    }
                    if (fieldCounts.ContainsKey(fieldName))
                    {
                        int prevCount = fieldCounts[fieldName];
                        fieldCounts[fieldName] = prevCount + 1;
                    }
                    else
                    {
                        fieldCounts[fieldName] = 1;
                    }
                }
                
            }

        }

        public String parseFieldName(String line)
        {
            var dFields = line.Split(new String[] { delimiter }, StringSplitOptions.None);
            if (dFields.Length >= 1)
            {
                String fieldName = dFields[0];
                return fieldName;
            }

            return null;
        }
        /// <summary>
        /// Returns how many lines BEFORE the current line
        /// share the same field name
        /// </summary>
        /// <param name="lineNumber"></param>
        /// <returns></returns>
        public int determineFieldNameCount(int lineNumber)
        {
            int count = 1;
            String fieldName = parseFieldName(delimitedLines[lineNumber]);
            //stops before the line number
            for (int i=0;i<lineNumber;i++)
            {
                String lineField = parseFieldName(delimitedLines[i]);
                if (lineField.Equals(fieldName))
                {
                    count++;
                }

            }

            return count;
        }

        String finderString = "";
        int finderLineOn = 0;
        int finderSkips = 0;//skip this many hits before accepting the result
        int finderResultNumber = 1;
        public const string LINE = "-------------------------";

        public String findString(String str)
        {
            bool debugFunction = false;
            String output = "";
            if (!str.Equals(finderString))
            {
                if (debugFunction)
                    output += " NEW SEARCH STARTED" + Environment.NewLine;
                //reset find variables
                resetFinder();
                finderString = str;
                
            } else
            {
                if (debugFunction)
                {
                    output += " Search continue: " + Environment.NewLine;
                    output += " finderLineOn: " + finderLineOn + Environment.NewLine;
                    output += " finderSkips: " + finderSkips + Environment.NewLine;
                }
            }
            Regex regex = new Regex(str);

            for (int i=finderLineOn;i<delimitedLines.Count;i++)
            {
                String delimitedLine = delimitedLines[i];
                String sourceLine = sourceLines[i];


                int toSkip = finderSkips;
                foreach (Match match in regex.Matches(sourceLine)) {
                    if (toSkip <=0)
                    {
                        finderSkips++;
                        finderLineOn = i;
                        String fieldName = parseFieldName(delimitedLine);
                        output += "Found : " + finderString + " [" + finderResultNumber + "]" + Environment.NewLine;
                        output += "FieldName: " + fieldName + Environment.NewLine;
                        output += "Occurance: " + determineFieldNameCount(i) + Environment.NewLine;
                        output += "Field Length: " + fieldName.Length + Environment.NewLine;
                        //determine position in line
                        output += "Position: " + (match.Index+1) + Environment.NewLine;

                        output += LINE + Environment.NewLine;
                        output += "Found on Line: " + finderLineOn + Environment.NewLine;
                        //SET CLIPBOARD
                        if (sffAutoClipboardCheckBox.IsChecked.Value)
                        {
                            Clipboard.SetText(fieldName);
                        }


                        return output;
                    } else
                    {
                        toSkip--;
                    }
                    
                }

                finderSkips=0;
            }
            resetFinder();
            output = "No further matches found.";
            return output;
        }

        public void resetFinder()
        {
            finderString = "";
            finderLineOn = 0;
            finderSkips = 0;
            finderResultNumber = 1;
        }

        private void processExpected(object sender, RoutedEventArgs e)
        {
            processExpected();
        }

        String expectedDelimiter = "";
        List<String> expectedLines = new List<string>();
        List<List<String>> allExpectedFields = new List<List<String>>();
        List<List<String>> usefulExpectedFields = new List<List<String>>();
        public void processExpected()
        {
            String workingText = expectedTextBox.Text;
            expectedDelimiter = expectedDelimiterTextBox.Text;
            //purge all instances of EOL delimiter
            String eolDelimiter = expectedEOLDelimiterTextBox.Text;

            Regex eolRemover = new Regex(eolDelimiter);

            workingText = eolRemover.Replace(workingText,"");

            //clear previous values
            expectedLines.Clear();

            //EXPECTED OUTPUT MUST BE delimited
            String[] lines = workingText.Split(new string[] { Environment.NewLine, "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (String line in lines)
            {
                expectedLines.Add(line);
                //process the line

                //add a line

                List<String> usefulFields = new List<string>();
                List<String> allFields = new List<string>();

                
                String[] fieldsUseful = line.Split(new string[] { expectedDelimiter }, StringSplitOptions.RemoveEmptyEntries);
                foreach (String field in fieldsUseful)
                {
                    //exclude blank fields
                    if (field != "")
                    {
                        usefulFields.Add(field);
                    }
                }

                String[] fieldsAll = line.Split(new string[] { expectedDelimiter }, StringSplitOptions.None);
                foreach (String field in fieldsAll)
                {
                    allFields.Add(field);
                }

                allExpectedFields.Add(allFields);
                usefulExpectedFields.Add(usefulFields);
            }
            sffSearchForCodeComboBox.Items.Clear();
            //populate combo boxes
            foreach (List<String> list in usefulExpectedFields)
            {
                sffSearchForCodeComboBox.Items.Add(list[0]);
            }
            sffSearchForCodeComboBox.SelectedIndex = 3;

            


        }
        public void codeSelected()
        {
            int index = sffSearchForCodeComboBox.SelectedIndex;
            List<String> fields = usefulExpectedFields[index];

            //clear fields
            sffSearchForItemComboBox.Items.Clear();

            bool firstRun = true;
            foreach (String field in fields)
            {
                //Exclude the first field
                if (!firstRun)
                {
                    sffSearchForItemComboBox.Items.Add(field);
                } else
                {
                    firstRun = false;
                }
            }
            if (!sffMutex)
            {
                sffSearchForItemComboBox.SelectedIndex = 0;
            }
        }
        private void sffCodeSelected(object sender, SelectionChangedEventArgs e)
        {
            codeSelected();
        }
        int sffCodeIndex = 4;
        int sffItemIndex = 0;

        /// <summary>
        /// finds the true index of a search term
        /// </summary>
        /// <param name="code"></param>
        /// <param name="itemValue"></param>
        /// <param name="usefulIndex"></param>
        /// <returns>the true index</returns>
        public int sffTrueIndexFind(String itemValue)
        {
            //find the number of times the item comes up before the useful index
            int timesToSkip = 0;
            //safety check
            if (sffItemIndex >= usefulExpectedFields[sffCodeIndex].Count)
                return 999999999;
            //skip the first item as that's the field name
            for (int i = 1; i < sffItemIndex; i++)
            {
                String testString = usefulExpectedFields[sffCodeIndex][i];
                if (testString==itemValue)
                {
                    timesToSkip++;
                }

            }
            for (int i = 1; i < allExpectedFields[sffCodeIndex].Count; i++)
            {
                String testString = allExpectedFields[sffCodeIndex][i];
                if (testString == itemValue)
                {
                    timesToSkip--;
                    if (timesToSkip<0)
                    {
                        return i;
                    }
                }
            }

            return 0;

        }

        public void sffFind()
        {
            //update indexs
            if (!sffMutex)
            {
                sffCodeIndex = sffSearchForCodeComboBox.SelectedIndex;
                sffItemIndex = sffSearchForItemComboBox.SelectedIndex + 1;
            }
            //get text to find
            String searchTerm = usefulExpectedFields[sffCodeIndex][sffItemIndex];
            String output = findString(searchTerm);

            output += Environment.NewLine;
            output += LINE + Environment.NewLine;

            int trueIndex = sffTrueIndexFind(searchTerm);
            output += usefulExpectedFields[sffCodeIndex][0] + " " + trueIndex + Environment.NewLine;



            output += LINE + Environment.NewLine;
            output += "sff Code Index: " + sffCodeIndex + Environment.NewLine;
            output += "sff Item Index: " + sffItemIndex + Environment.NewLine;

            sffConsole.Text = output;

            //ADD way to match first N characters!

        }
        bool sffMutex = false;
        public void sffNext()
        {
            sffMutex = true;
            sffItemIndex++;
            Console.Out.WriteLine("SFF INDEX: " + sffItemIndex + " / " + usefulExpectedFields[sffCodeIndex].Count) ;

            if (sffItemIndex>=usefulExpectedFields[sffCodeIndex].Count)
            {
                
                //go to the next code
                sffCodeIndex++;
                sffItemIndex = 1;//skip the first field as it's an EDI code

                sffSearchForCodeComboBox.SelectedIndex = sffCodeIndex;
            }
            //sffFind();
            //update combo box
            sffSearchForItemComboBox.SelectedIndex = sffItemIndex-1; // should fire the find event
            sffMutex = false;
        }

        private void sffItemSelected(object sender, SelectionChangedEventArgs e)
        {
            sffFind();
        }

        private void sffNextClick(object sender, RoutedEventArgs e)
        {
            sffNext();
        }

        private void sffFindNextInstanceClick(object sender, RoutedEventArgs e)
        {
            sffFind();
        }

        //todo: make this copy the fieldname to clipboard if that checkbox is checked
    }
}
