﻿using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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


//ASCII ART GENERATED BY:
//http://patorjk.com/software/taag/#p=display&f=Letters


namespace EDI_Utilities
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {



        //super field finder variables
        private int sffCodeIndex = 4;
        private int sffItemIndex = 0;
        private bool sffMutex = false;
        //delimiters
        String expectedDelimiter = "";
        String sourceDelimiter;
        //finder variables
        private String finderString = "";
        private int finderLineOn = 0;
        private int finderSkips = 0;//skip this many hits before accepting the result
        private int finderResultNumber = 1;
        //data structures
        //source (IDOC)
        public List<String> sourceLines = new List<String>();
        public List<String> delimitedLines = new List<String>();
        private Dictionary<String, int> fieldCounts = new Dictionary<string, int>();
        //expected results (X12)
        List<String> expectedLines = new List<string>();
        List<List<String>> allExpectedFields = new List<List<String>>();
        List<List<String>> usefulExpectedFields = new List<List<String>>();

        //constants
        public const string LINE = "-------------------------";

        //data bound elements
        public bool fallbackSearchEnabled { get; set; }
        public string conversionDelimiter { get; set; }
        public int conversionIdocFieldCol { get; set; }
        public int conversionIdocSegmentCol { get; set; }
        public int conversionX12Col { get; set; }
        public int conversionSkipFirstX { get; set; }
        public bool conversionHoldSegValue { get; set; }


        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        

        /*
         *  ________________________________________________
         * EEEEEEE VV     VV EEEEEEE NN   NN TTTTTTT  SSSSS  
         * EE      VV     VV EE      NNN  NN   TTT   SS      
         * EEEEE    VV   VV  EEEEE   NN N NN   TTT    SSSSS  
         * EE        VV VV   EE      NN  NNN   TTT        SS 
         * EEEEEEE    VVV    EEEEEEE NN   NN   TTT    SSSSS   
         * -------------------------------------------------
         */

        /// <summary>
        /// Button Click event
        /// Finds the next instance of the search term.
        /// This is only intended to be used be the regular
        /// field finder, not the super field finder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void findNextClick(object sender, RoutedEventArgs e)
        {
            //perform a basic search for the string
            String findThis = findTextbox.Text;

            string output = findString(findThis);

            fieldFinderResultsTextbox.Text = output;

            //need the number of times the field is present
            //find the field name
            //find the line number
            //find the distance from the line.

        }
        private void processIdocButton_Click(object sender, RoutedEventArgs e)
        {
            processIdoc();

        }

        public void processIdoc()
        {
            String workingText = sourceTextbox.Text;

            Regex rgx = new Regex("[ ]{2,}");
            String rtrned = "";
            sourceDelimiter = delimiterTextbox.Text;
            rtrned = rgx.Replace(workingText, sourceDelimiter);

            delimitedTextbox.Text = rtrned;

            parseSourceLines();
            parseFields();
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //may use this for warnings
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

        private void processExpected(object sender, RoutedEventArgs e)
        {
            processExpected();
        }
        private void sffCodeSelected(object sender, SelectionChangedEventArgs e)
        {
            sffCodeSelected();
        }
        private void sourceFormatProcessClick(object sender, RoutedEventArgs e)
        {
            processIdocFormat();
        }

        public void processIdocFormat()
        {
            Thread process = new Thread(new ThreadStart(processSourceFormat));
            //processSourceFormat();
            this.workingSourceFormatText = formatTextBox.Text;
            process.Start();
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
            for (int i = 0; i < lineNumber; i++)
            {
                String lineField = parseFieldName(delimitedLines[i]);
                if (lineField.Equals(fieldName))
                {
                    count++;
                }

            }

            return count;
        }


        /*
         * FFFFFFF    IIIII    NN   NN    DDDDD      EEEEEEE    RRRRRR  
         * FF          III     NNN  NN    DD  DD     EE         RR   RR 
         * FFFF        III     NN N NN    DD   DD    EEEEE      RRRRRR  
         * FF          III     NN  NNN    DD   DD    EE         RR  RR  
         * FF         IIIII    NN   NN    DDDDDD     EEEEEEE    RR   RR 
         */

        /// <summary>
        /// Finds the next instance of the search term.
        /// returns relevant output of where that string was
        /// found as a string.
        /// generally you just want to output this.
        /// works on both the delimited and source lines lists
        /// 
        /// </summary>
        /// <param name="str">The search term</param>
        /// <returns>String version of results. </returns>
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

            }
            else
            {
                if (debugFunction)
                {
                    output += " Search continue: " + Environment.NewLine;
                    output += " finderLineOn: " + finderLineOn + Environment.NewLine;
                    output += " finderSkips: " + finderSkips + Environment.NewLine;
                }
            }
            Regex regex = new Regex(str);

            for (int i = finderLineOn; i < delimitedLines.Count; i++)
            {
                String delimitedLine = delimitedLines[i];
                String sourceLine = sourceLines[i];


                int toSkip = finderSkips;
                foreach (Match match in regex.Matches(sourceLine))
                {
                    if (toSkip <= 0)
                    {
                        finderSkips++;
                        finderLineOn = i;
                        int position = match.Index;
                        String fieldName = parseFieldName(delimitedLine);
                        output += "Found : " + finderString + " [" + finderResultNumber + "]" + Environment.NewLine;
                        output += "FieldName: " + fieldName + Environment.NewLine;
                        output += "Occurance: " + determineFieldNameCount(i) + Environment.NewLine;
                        output += "Field Length: " + fieldName.Length + Environment.NewLine;
                        //determine position in line
                        output += "Position: " + (match.Index + 1) + Environment.NewLine;

                        output += LINE + Environment.NewLine;
                        output += "Found on Line: " + finderLineOn + Environment.NewLine;
                        //SET CLIPBOARD
                        if (sffAutoClipboardCheckBox.IsChecked.Value)
                        {
                            Clipboard.SetText(fieldName);
                        }

                        //IDOC INFO
                        if (IdocLoaded)
                        {
                            output += LINE + Environment.NewLine;
                            output += getIdocInfo(fieldName, position);
                        }

                        return output;
                    }
                    else
                    {
                        toSkip--;
                    }

                }

                finderSkips = 0;
            }
            resetFinder();
            output = NO_RESULTS_FOUND + Environment.NewLine;
            return output;
        }
        public const string NO_RESULTS_FOUND = "No further matches found.";
        public void resetFinder()
        {
            finderString = "";
            finderLineOn = 0;
            finderSkips = 0;
            finderResultNumber = 1;
        }

        /***
 *     SSSSS     UU   UU    PPPPPP     EEEEEEE    RRRRRR           
 *    SS         UU   UU    PP   PP    EE         RR   RR          
 *     SSSSS     UU   UU    PPPPPP     EEEEE      RRRRRR           
 *         SS    UU   UU    PP         EE         RR  RR           
 *     SSSSS      UUUUU     PP         EEEEEEE    RR   RR          
 *                                                                 
 *    FFFFFFF    IIIII    EEEEEEE    LL         DDDDD              
 *    FF          III     EE         LL         DD  DD             
 *    FFFF        III     EEEEE      LL         DD   DD            
 *    FF          III     EE         LL         DD   DD            
 *    FF         IIIII    EEEEEEE    LLLLLLL    DDDDDD             
 *                                                                 
 *    FFFFFFF    IIIII    NN   NN    DDDDD      EEEEEEE    RRRRRR  
 *    FF          III     NNN  NN    DD  DD     EE         RR   RR 
 *    FFFF        III     NN N NN    DD   DD    EEEEE      RRRRRR  
 *    FF          III     NN  NNN    DD   DD    EE         RR  RR  
 *    FF         IIIII    NN   NN    DDDDDD     EEEEEEE    RR   RR                                                           
 */
        /// <summary>
        /// Process data for changing the sffCode that is currently being worked on
        /// example code: BIG
        /// This updates the item 
        /// </summary>
        public void sffCodeSelected()
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
                }
                else
                {
                    firstRun = false;
                }
            }
            if (!sffMutex)
            {
                sffSearchForItemComboBox.SelectedIndex = 0;
            }
        }



        /// <summary>
        /// finds the true index of a search term
        /// This must be used during a search
        /// as it calls on sffCodeIndex and sffItemIndex
        /// to determine the information
        /// 
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
                if (testString == itemValue)
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
                    if (timesToSkip < 0)
                    {
                        return i;
                    }
                }
            }

            return 0;

        }
        /// <summary>
        /// Executes a find command (findString)
        /// with the sff variables as arguments
        /// </summary>
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
            String output = "searching";
            output = findString(searchTerm);
            //keep searching until a smart result is found
            //(smart result = a known idoc field is found)
            if (sffSmartModeCheckBox.IsChecked.Value) {
                int otherResultsFound = 0;
                while (!idocFieldFound && !output.Contains(NO_RESULTS_FOUND))
                {
                    otherResultsFound++;
                    output = findString(searchTerm);

                }
                //output how many results the smart filter skipped
                if (otherResultsFound >=1)
                {
                    output += "Skipped results: " + otherResultsFound + Environment.NewLine;
                }
            }
            //output += Environment.NewLine;
            output += LINE + Environment.NewLine;

            int trueIndex = sffTrueIndexFind(searchTerm);
            output += usefulExpectedFields[sffCodeIndex][0] + " " + trueIndex + Environment.NewLine;

            output += "Fallback search enabled: " + fallbackSearchEnabled + Environment.NewLine;

            /*
            output += LINE + Environment.NewLine;
            output += "sff Code Index: " + sffCodeIndex + Environment.NewLine;
            output += "sff Item Index: " + sffItemIndex + Environment.NewLine;
            */
            sffConsole.Text = output;

            //ADD way to match first N characters!

        }
        /// <summary>
        /// Gets the next item (or move to the next "code") and performs a
        /// search.
        /// No functionality is currently present at the end of a file,
        /// so this will crash.
        /// </summary>
        public void sffNext()
        {
            sffMutex = true;
            sffItemIndex++;
            Console.Out.WriteLine("SFF INDEX: " + sffItemIndex + " / " + usefulExpectedFields[sffCodeIndex].Count);

            if (sffItemIndex >= usefulExpectedFields[sffCodeIndex].Count)
            {

                //go to the next code
                sffCodeIndex++;
                if (sffCodeIndex >= usefulExpectedFields.Count)
                {
                    sffCodeIndex = 0;
                }
                sffItemIndex = 1;//skip the first field as it's an EDI code

                sffSearchForCodeComboBox.SelectedIndex = sffCodeIndex;
            }
            //sffFind();
            //update combo box
            sffSearchForItemComboBox.SelectedIndex = sffItemIndex - 1; // should fire the find event
            sffMutex = false;
        }



        /***
 *    PPPPPP     RRRRRR      OOOOO      CCCCC     EEEEEEE     SSSSS      SSSSS     EEEEEEE    RRRRRR      SSSSS  
 *    PP   PP    RR   RR    OO   OO    CC    C    EE         SS         SS         EE         RR   RR    SS      
 *    PPPPPP     RRRRRR     OO   OO    CC         EEEEE       SSSSS      SSSSS     EEEEE      RRRRRR      SSSSS  
 *    PP         RR  RR     OO   OO    CC    C    EE              SS         SS    EE         RR  RR          SS 
 *    PP         RR   RR     OOOO0      CCCCC     EEEEEEE     SSSSS      SSSSS     EEEEEEE    RR   RR     SSSSS  
 *                                                                                                                                                                                                                            
 */

        /// <summary>
        /// parses the source text into individual lines.
        /// fills both the source lines and delimited lines.
        /// </summary>
        public void parseSourceLines()
        {
            String workingText = sourceTextbox.Text;
            sourceLines.Clear();
            delimitedLines.Clear();

            String[] lines = workingText.Split(NEW_LINE_SPLIT, StringSplitOptions.None);

            foreach (String line in lines)
            {
                sourceLines.Add(line);
                delimitedLines.Add(toDelimited(line));
                // this adds all the next lines
            }

        }

        /// <summary>
        /// Converts source text to psudo-delimited text
        /// based on finding 2 or more spaces.
        /// this is not a true delimited format, 
        /// but helps improve the readibility of the text.
        /// </summary>
        /// <param name="sourceText"></param>
        /// <returns></returns>
        public String toDelimited(String sourceText)
        {
            Regex rgx = new Regex("[ ]{2,}");
            String delimitedText = "";
            String delimiter = delimiterTextbox.Text;
            delimitedText = rgx.Replace(sourceText, delimiter);
            return delimitedText;
        }

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
            sourceDelimiter = delimiterTextbox.Text;
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
            var dFields = line.Split(new String[] { sourceDelimiter }, StringSplitOptions.None);
            if (dFields.Length >= 1)
            {
                String fieldName = dFields[0];
                return fieldName;
            }

            return null;
        }
        /// <summary>
        /// Process information from the "expected format"
        /// [X12] or delimited information is expected to be
        /// in the expected textbox
        /// and delimiters or regular expressions are expected to be in
        /// the delimiter and EOL textboxes.
        /// </summary>
        public void processExpected()
        {
            String workingText = expectedTextBox.Text;
            expectedDelimiter = expectedDelimiterTextBox.Text;
            //purge all instances of EOL delimiter
            String eolDelimiter = expectedEOLDelimiterTextBox.Text;

            Regex eolRemover = new Regex(eolDelimiter);

            workingText = eolRemover.Replace(workingText, "");

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

        private List<String> formatLines;
        //IDOC constants
        //starting
        public const String IDOC_BEGIN_SEGMENT_SECTION = "BEGIN_SEGMENT_SECTION";//starts a document [ignore?]
        public const String IDOC_BEGIN_SEGMENT = "BEGIN_SEGMENT";//value is the field name
        public const String IDOC_BEGIN_IDOC = "BEGIN_IDOC";
        public const String IDOC_BEGIN_FIELDS = "BEGIN_FIELDS";
        //public const String IDOC_ = "";

        //SEGMENT
        public const String IDOC_SEG_STATUS = "STATUS";
        public const String IDOC_SEG_TYPE = "TYPE";
        public const String IDOC_SEG_LEVEL = "LEVEL";
        public const String IDOC_SEG_LOOPMIN = "LOOPMIN";
        public const String IDOC_SEG_LOOPMAX = "LOOPMAX";
        //FIELDS
        public const String IDOC_NAME = "NAME";
        public const String IDOC_TEXT = "TEXT";
        public const String IDOC_TYPE = "TYPE";
        public const String IDOC_FIELD_POS = "FIELD_POS";
        public const String IDOC_LENGTH = "LENGTH";
        public const String IDOC_CHARACTER_FIRST = "CHARACTER_FIRST";
        public const String IDOC_CHARACTER_LAST = "CHARACTER_LAST";
        //ending
        public const String IDOC_END_SEGMENT = "END_SEGMENT";
        public const String IDOC_END_FIELDS = "END_FIELDS";
        public const String IDOC_END_IDOC = "END_IDOC";
        public const String IDOC_END_SEGMENT_SECTION = "END_SEGMENT_SECTION";
        private static readonly int IDOC_FIELD_LENGTH = 20;
        public String idocName;
        private idocField workingField;
        private idocSegment workingSegment;
        List<idocSegment> idocSegments = new List<idocSegment>();//holds all the loaded idoc segments
        private String workingSourceFormatText;
        private bool IdocLoaded = false;
        private bool idocFieldFound = false;

        public void processSourceFormat()
        {
            idocSegments.Clear();
            allSegments.Clear();
            //read all lines in, and split into lines
            formatLines = seperateToLines(workingSourceFormatText);
            int i = 0;
            //begin processing
            foreach (String line in formatLines)
            {
                i++;
                processIdocFormatLine(line);
                
            }




        }

        private void processIdocFormatLine(string line)
        {
            //the method can simply return if the format is skippable
            //divide line up into fields (we are only expecting two)
            List<String> delimited = toFormatDelimited(line);
            //first value is the key, 2nd is the value
            if (delimited.Count < 1)
            {
                //too few fields
                return;
            }

            String key = delimited[0];

            String value;
            if (delimited.Count >= 2)
            {
                value = delimited[1];
            }
            else
            {
                value = "";
            }
            Console.WriteLine("key: " + key + " Value: " + value);
            if (workingSegment == null)
            {
                workingSegment = new idocSegment();
                workingSegment.name = "no segment name dump";
            }

            switch (key)
            {
                case null:
                    System.Console.WriteLine("Null value detected.");
                    break;
                case IDOC_BEGIN_SEGMENT_SECTION:
                    IdocLoaded = false;
                    return;
                case IDOC_END_IDOC:
                    return;
                case IDOC_BEGIN_IDOC:
                    idocName = value;
                    break;
                case IDOC_BEGIN_SEGMENT:
                    //create a new statement
                    idocSegment seg = new idocSegment();
                    //idocSegments.Add(seg);
                    seg.name = value;
                    workingSegment = seg;
                    break;
                case IDOC_SEG_LEVEL:
                    workingSegment.level = value;
                    break;
                case IDOC_SEG_STATUS:
                    workingSegment.status = value;
                    break;
                case IDOC_SEG_LOOPMIN:
                    workingSegment.loopMin = value;
                    break;
                case IDOC_SEG_LOOPMAX:
                    workingSegment.loopMax = value;
                    break;
                case IDOC_END_SEGMENT:
                    idocSegments.Add(workingSegment);
                    allSegments.Add(workingSegment.name, workingSegment);
                    Console.WriteLine(workingSegment.name + "|" + workingSegment);
                    break;

                    //fields
                case IDOC_BEGIN_FIELDS:
                    
                    break;
                case IDOC_NAME:
                    idocField field = new idocField();
                    workingField = field;
                    workingField.name = value;
                    break;
                case IDOC_TEXT:
                    workingField.text = value;
                    break;
                case IDOC_TYPE:
                    workingField.type = value;
                    break;
                case IDOC_LENGTH:
                    workingField.length = Int32.Parse(value);
                    break;
                case IDOC_FIELD_POS:
                    workingField.fieldPos = Int32.Parse(value);
                    break;
                case IDOC_CHARACTER_FIRST:
                    workingField.charFirst = Int32.Parse(value);
                    break;
                case IDOC_CHARACTER_LAST:
                    workingField.charLast = Int32.Parse(value);
                    workingSegment.fields.Add(workingField);
                    break;
                case IDOC_END_FIELDS:
                    break;

                case IDOC_END_SEGMENT_SECTION:
                    IdocLoaded = true;
                    break;
            }


            //CASE based on text matching
        }

        public static List<String> toFormatDelimited(String line)
        {
            //trim leading and following spaces
            line = line.Trim();
            List<String> fields = new List<string>();
            if (line.Contains(IDOC_BEGIN_FIELDS) 
                || line.Contains(IDOC_BEGIN_SEGMENT_SECTION)
                || line.Contains(IDOC_END_FIELDS)
                || line.Contains(IDOC_END_IDOC)
                || line.Contains(IDOC_END_SEGMENT_SECTION)
                || line.Contains(IDOC_END_SEGMENT)
                || line.Length < IDOC_FIELD_LENGTH)
            {
                fields.Add(line);
            } else {
                
                String key = line.Substring(0, IDOC_FIELD_LENGTH).Trim();
                String value = line.Substring(IDOC_FIELD_LENGTH, line.Length-IDOC_FIELD_LENGTH).Trim();
                fields.Add(key);
                fields.Add(value);
            }
            return fields;

        }

        public static List<String> seperateToLines(String multiLineText)
        {
            List<String> lineList = new List<string>();
            String[] lines = multiLineText.Split(new string[] { Environment.NewLine, "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (String line in lines)
            {
                if (line != "")
                {
                    lineList.Add(line);
                }
                // this adds all the next lines
            }
            return lineList;
        }

        //preprocess, get each line's segment name?
        Dictionary<String, idocSegment> allSegments = new Dictionary<string, idocSegment>();
        public readonly String[] NEW_LINE_SPLIT = new string[] { Environment.NewLine, "\r\n", "\n" };

        public static List<List<String>> toTrueDelimitedIdoc(String idocIn)
        {
            //TODO add this.
            return null;
        }

        public String getIdocInfo(String segName,int position)
        {
            idocFieldFound = false;
            String output = "";
            try
            {
                idocSegment seg = allSegments[segName];

                output += seg.ToString() + Environment.NewLine;
                //find field in segment
                idocField field = seg.getFieldInPosition(position);
                if (field != null)
                {
                    idocFieldFound = true;
                    output += field;
                } else
                {
                    output += "(no field found)" + Environment.NewLine;
                }
            } catch (Exception e)
            {
                Console.WriteLine("ERROR finding: " + segName);
                output += "(no IDOC information found)" + Environment.NewLine;
            }
            return output;
        }

        private void uploadIdocClick(object sender, RoutedEventArgs e)
        {
            //open dialog box
            sourceTextbox.Text = dialogUpload();
            if (sourceTextbox.Text != "")
            {
                loaderIdocStatusLabel.Content = "IDOC Loaded.";
            }
            //process idoc
            processIdoc();
            if (delimitedTextbox.Text!="")
            {
                loaderIdocStatusLabel.Content = "IDOC Processed.";
            }
        }

        public static String dialogUpload()
        {
            String refString = "";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                refString = File.ReadAllText(openFileDialog.FileName);
            }
            return refString;
        }

        private void uploadIdocFormatClick(object sender, RoutedEventArgs e)
        {
            //open dialog box
            formatTextBox.Text = dialogUpload();
            if (formatTextBox.Text != "")
            {
                loaderIdocFormatStatusLabel.Content = "IDOC Format Loaded.";
            }
            //process idoc format
            processIdocFormat();

        }

        private void uploadX12ButtonClick(object sender, RoutedEventArgs e)
        {
            //open dialog box
            expectedTextBox.Text = dialogUpload();
            if (expectedTextBox.Text != "")
            {
                loaderX12StatusLabel.Content = "X12 Loaded.";
            }
            //process x12 format
            processExpected();
        }

        private void testDataBindingClick(object sender, RoutedEventArgs e)
        {
            System.Console.WriteLine("Data bindings: Delimiter: " + conversionDelimiter + " seg col: " + conversionIdocSegmentCol);

            processConversion();

        }

        private void uploadConversionButtonClick(object sender, RoutedEventArgs e)
        {
            //open dialog box
            conversionSourceTextBox.Text = dialogUpload();
            if (conversionSourceTextBox.Text != "")
            {
                loaderConversionStatusLabel.Content = "Conversion Loaded.";
            }
            //process conversion format
            processConversion();
        }
        List<ConversionObject> conversionObjects = new List<ConversionObject>();
        /// <summary>
        /// Go through the csv, looking for essential fields
        /// populate objects based on the lines
        /// goal is to have two maps (x12 sorted and Idoc sorted)
        /// </summary>
        private void processConversion()
        {
            //convert to lines
            String[] lines = conversionSourceTextBox.Text.Split(NEW_LINE_SPLIT,StringSplitOptions.RemoveEmptyEntries);

            String consoleOut = "";

            String currentSegment = "";
            int i = 0;
            foreach(String line in lines)
            {
                i++;
                if (i < conversionSkipFirstX)
                {

                } else {
                    //divide it into fields
                    String[] fields = line.Split(new String[] { conversionDelimiter }, StringSplitOptions.None);

                    String X12Value = fields[conversionX12Col];
                    if (X12Value != "")
                    {

                        String idocField = fields[conversionIdocFieldCol];
                        String idocSeg = currentSegment;
                        if (!conversionHoldSegValue)
                        {
                            idocSeg = fields[conversionIdocSegmentCol];
                        }
                        //create a conversion object and store it
                        ConversionObject co = new ConversionObject();
                        co.x12 = X12Value;
                        co.idocField = idocField;
                        co.idocSeg = idocSeg;
                        co.line = line;
                        consoleOut += co.toString();

                        conversionObjects.Add(co);

                    }
                    else
                    {
                        if (conversionHoldSegValue)
                        {
                            //check to see if the Idoc segment is in the correct location
                            String seg = fields[conversionIdocSegmentCol];
                            if (seg != "")
                            {
                                currentSegment = seg;
                            }
                        }
                    }

                }
            }

            conversionConsoleTextBox.Text = consoleOut;

        }
    }
}
