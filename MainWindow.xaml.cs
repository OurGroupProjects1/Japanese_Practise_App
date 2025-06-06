using Microsoft.Win32;
using System.Diagnostics;
using System.Drawing;
using System.IO;

using System.Windows;
using System.Windows.Controls;

using System.Windows.Media;
using static Japanese_Practise_App.MainWindow;


namespace Japanese_Practise_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Button[] btns = new Button[4];
        
        Dictionary<string, string> JapaneseWordsWithItsEnglishMeans = new Dictionary<string, string>();
        List<string> Questions = new List<string>();
        int QuestionCount;
        List<string> Allkey;
        TextBlock DisplayTextBlock;
        TextBlock TotalQuestion;
        int dup = 0;
        public MainWindow()
        {
            
            InitializeComponent();
            DisplayTextBlock = (TextBlock)FindName("Text_Display");
            
        }

        string FilePath;

        void FileBroswer(object sender,RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Title = "Open Words List File";
            openFile.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            if (openFile.ShowDialog() == true)
            {
                FilePath = openFile.FileName;
            }
            else
            {
                MessageBox.Show("No file selected. Using default file path.");
                FilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", "Words_List.txt");
            }
                JapaneseWordsWithItsEnglishMeans.Clear();
                Questions.Clear();
                QuestionCount = 0;
                InitializeDictiionary();
                GenerateQuesitonWithMCQs();
                DisplayTextBlock.Text = Questions[0];
                IntializeBottonackGroundColor();
                InitializeButtons();
            
        }
        
        void InitializeDictiionary()
        {
            
            foreach (string str in File.ReadLines(FilePath))
            {
                string trimed = str.Trim();
                
                if (trimed.Contains(':'))
                {
                    if (!JapaneseWordsWithItsEnglishMeans.ContainsKey(trimed.Substring(0, trimed.IndexOf(':'))))
                    {
                        JapaneseWordsWithItsEnglishMeans.Add(trimed.Substring(0, trimed.IndexOf(':')), trimed.Substring(trimed.IndexOf(':') + 1));
                        QuestionCount++;
                    }
                    
                }
                
            }
            
            TotalQuestion = (TextBlock)FindName("TotalQuestions");
            TotalQuestion.Text = $"{Pointer+1}/{QuestionCount}";
        }
        void GenerateQuesitonWithMCQs()
        {
            ShuffleWordsAndAssignToQuestions();
            string Correctkey="";
            
            for (int i =0; i< QuestionCount; i++)
            {
                JapaneseWordsWithItsEnglishMeans.TryGetValue(Questions[i], out Correctkey);
                GenerateMCQ(Correctkey);
                MarkedStatus.Add(0); // Initialize with 0 (no answer marked)
            }
        }
        void ShuffleWordsAndAssignToQuestions()
        {
            Random rng = new Random();
            Allkey = JapaneseWordsWithItsEnglishMeans.Keys.ToList();
            Allkey = Allkey.OrderBy(x => rng.Next()).ToList();
            
            foreach(string str in Allkey)
            {
                Questions.Add(str);
            }
        }

        public struct MCQset {
            public List<string> MCQs;
            
            public MCQset()
            {
                MCQs = new List<string>();
            }
        }

        List<MCQset> AllMCQsets = new List<MCQset>();
        List<int> MarkedStatus = new List<int>();
        

        void GenerateMCQ(string Correctkey)
        {
            List<string> AllValues = JapaneseWordsWithItsEnglishMeans.Values.ToList();
            Random rng = new Random();
            MCQset MCQ = new MCQset();

            AllValues.Remove(Correctkey);

            AllValues = AllValues.OrderBy(x=> rng.Next()).ToList();
            for (int i = 0; i < 3; i++)
            {   
                    MCQ.MCQs.Add(AllValues[i]);
            }
            MCQ.MCQs.Add(Correctkey);
            MCQ.MCQs = MCQ.MCQs.OrderBy(x=>rng.Next()).ToList();

            AllMCQsets.Add(MCQ);
        }
         
        void InitializeButtons()
        {
            btns[0] = (Button)FindName("Btn1");
            btns[1] = (Button)FindName("Btn2");
            btns[2] = (Button)FindName("Btn3");
            btns[3] = (Button)FindName("Btn4");

            for (int i = 0; i < 4; i++)
            {
                if (MarkedStatus[Pointer] != 0)
                {
                    btns[i].IsEnabled = false; 
                }
                btns[i].Content = AllMCQsets[Pointer].MCQs[i];
                if(ColoredChoice[Pointer][i] == 1)
                {
                    btns[i].Background = Brushes.Green;
                    if (MarkedStatus[Pointer] == 0)
                    {
                        MarkedStatus.Add(1);
                    }
                }
                else if (ColoredChoice[Pointer][i] == -1)
                {
                    btns[i].Background = Brushes.Red;
                    if (MarkedStatus[Pointer] == 0)
                    {
                        MarkedStatus.Add(-1);
                    }
                }
                else
                {
                    btns[i].Background = Brushes.LightGray; 
                }
            }
            
        }
        
        
        int[] MCQColorArray;
        List<int[]> ColoredChoice = new List<int[]>();
        void IntializeBottonackGroundColor()
        {
            for(int i = 0; i<Questions.Count; i++)
            {
                MCQColorArray = new int[4] { 0, 0, 0, 0 };
                ColoredChoice.Add(MCQColorArray); 
            }
        }

        void MCQ(object sender, RoutedEventArgs e)
        {

            string CheckCorrectAnswer;
            if (sender is Button btn)
            {
                var tag = btn.Tag;
                JapaneseWordsWithItsEnglishMeans.TryGetValue(Questions[Pointer],out CheckCorrectAnswer);
                if(btn.Content.ToString() == CheckCorrectAnswer)
                {
                    btn.Background = Brushes.Green;
                    ColoredChoice[Pointer][Convert.ToInt32(tag)] = 1; // Correct answer
                }
                else
                {
                    btn.Background = Brushes.Red;
                    ColoredChoice[Pointer][Convert.ToInt32(tag)] = -1; // Incorrect answer
                }
            }
            
            
        }
        int Pointer=0;
        void MoveThroughList(object sender, RoutedEventArgs e)
        {

            Button btn = (Button)sender;
            if (btn.Name == "Left_Move")
            {
                Pointer--;
                if (Pointer <= 0) { Pointer = 0; }
                InitializeButtons();

                TotalQuestion.Text = $"{Pointer+1}/{QuestionCount}";
                DisplayTextBlock.Text = Questions[Pointer];
                
            }else if(btn.Name == "Right_Move")
            {
                Pointer++;
                if (Pointer >= Questions.Count) { Pointer = Questions.Count - 1; }
                InitializeButtons();
                
                TotalQuestion.Text = $"{Pointer + 1}/{QuestionCount}";
                DisplayTextBlock.Text = Questions[Pointer];
            }
        }

        private void LoadWordsFromFile(object sender, RoutedEventArgs e)
        {

        }

        void OnlyOneIsCheckedInMenu(object sender, RoutedEventArgs e)
        {
            CheckBox PrevChecked =(CheckBox)FindName("CheckBox1");
            Menu menu = (Menu)FindName("SelectMenu");


            foreach (var item in menu.Items)
            {
                if(item is CheckBox cb)
                {
                    if(cb.IsChecked ==true)
                    {

                    }        
                }
            }
          

        }

        private void FileDropDown_Click(object sender, RoutedEventArgs e)
        {
            FilePopup.IsOpen = true;
        }

        private void SelectDropDown_Click(object sender, RoutedEventArgs e)
        {
            SelectPopup.IsOpen = true;
            
        }
    }
}