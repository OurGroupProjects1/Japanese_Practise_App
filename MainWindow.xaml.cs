using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using System.DirectoryServices;

//using System.Drawing;
using System.IO;

using System.Windows;
using System.Windows.Controls;

using System.Windows.Media;
using System.Windows.Media.Animation;
using static Japanese_Practise_App.MainWindow;


namespace Japanese_Practise_App
{

    public partial class MainWindow : Window
    {
        Button[] btns = new Button[4];

        Dictionary<string, string> WordsDict = new Dictionary<string, string>();
        List<string> Questions = new List<string>();
        int QuestionCount;
        List<string> Allkey;
        TextBlock DisplayTextBlock, TotalQuestion;
        CheckBox PrevChecked, PrevLevel, PrevQuestionCount,PrevType;
        public MainWindow()
        {

            InitializeComponent();
            DisplayTextBlock = Text_Display;
            PrevChecked = Hiragana;
            PrevLevel = N5;
            PrevQuestionCount = q_5;
            PreQuestionCount = 5;
            PrevType = Nouns;
        }


        string FilePath;

        Color lerp(Color from, Color to, double t)
        {
            byte a = (byte)(from.A + (to.A - from.A) * t);
            byte r = (byte)(from.R + (to.R - from.R) * t);

            byte g = (byte)(from.G + (to.G - from.G) * t);
            byte b = (byte)(from.B + (to.B - from.B) * t);

            return Color.FromArgb(a, r, g, b);
        }




        GradientStop A = new GradientStop(Color.FromArgb(255, 230, 242, 255), 0.8f);
        GradientStop B = new GradientStop(Color.FromArgb(255, 200, 200, 200), 0.1f);
        LinearGradientBrush GradientBrush;
        Style gradientStyle = new Style(typeof(Button));
        public bool AllInitialized = false;
        void GradientButtons(GradientStop A, GradientStop B)
        {

            //Gradient from A->B from (0,0) -> (1,1)
            GradientBrush = new LinearGradientBrush();
            GradientBrush.StartPoint = new Point(0, 0);
            GradientBrush.EndPoint = new Point(1, 1);

            GradientBrush.GradientStops.Add(A);
            GradientBrush.GradientStops.Add(B);

        }
        void FileBroswer(object sender, RoutedEventArgs e)
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
                FilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", "Hiragana_words.txt");
            }

            WordsDict.Clear();
            Questions.Clear();
            QuestionCount = 0;
            NewImprovedInitializeDictionary();
            //InitializeDictiionary();
            GenerateQuesitonWithMCQs();
            DisplayTextBlock.Text = Questions[0];
            IntializeBottonackGroundColor();

            GradientButtons(A, B);
            gradientStyle.Setters.Add(new Setter(Button.BackgroundProperty, GradientBrush));
            Application.Current.Resources[typeof(Button)] = gradientStyle;
            InitializeButtons();

            Button[] TmpButton = new Button[3];
            TmpButton[0] = Left_Move;
            TmpButton[1] = Right_Move;
            TmpButton[2] = Reset;
            for (int i = 0; i < 3; i++)
            {
                TmpButton[i].IsEnabled = true;
            }

            AllInitialized = true;
        }
        Dictionary<string, string> WordsList = new Dictionary<string, string>();
        string GolLvl = "";
        string GolScript = "";
        string GolType = "";
        void NewImprovedInitializeDictionary()
        {
            
            if (N5.IsChecked == true)
            {
                GolLvl = "N5";
                
                if(Nouns.IsChecked == true)
                    GolType = "Nouns";
                else if (Verbs.IsChecked == true)
                    GolType = "Verbs";
                else if (い_Adjectives.IsChecked == true)
                    GolType = "い_Adjectives";
                else if (な_Adjectives.IsChecked == true)
                    GolType = "な_Adjectives";

                if (Hiragana.IsChecked == true)
                    GolScript = "Hiragana";
                else if (Katakana.IsChecked == true)
                    GolScript = "Katakana";
                else if (Kanji.IsChecked == true)
                    GolScript = "Kanji";
            }
            else if (N4.IsChecked == true)
            {
                GolLvl = "N4";
                if (Hiragana.IsChecked == true)
                    GolScript = "Hiragana";
            }
            
            string curLvl = "";
            string CurScript = "";
            string  CurType = "";
            QuestionCount = PreQuestionCount;
            bool lvlselected = false;
            bool scriptselected = false;
            bool typeSelected = false;
            foreach (string str in File.ReadLines(FilePath))
            {   
                Debug.WriteLine(str);
                string line = str.Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (line.StartsWith("N") && line.EndsWith(":"))
                {
                    curLvl = line.TrimEnd(':');
                    lvlselected = (curLvl == GolLvl);
                    scriptselected = false;
                    typeSelected = false;
                }
                else if (line.TrimStart().StartsWith("_") && line.EndsWith(":") && lvlselected)
                {
                    CurScript = line.TrimStart('_').TrimEnd(':').Trim();
                    scriptselected = (CurScript == GolScript);
                }
                else if (line.Contains(':') && lvlselected && scriptselected && typeSelected)
                {
                    string[] parts = line.Split(':');
                    if (parts.Length == 2)
                    {

                        string key = parts[0].Trim();
                        string value = parts[1].Trim();
                        if (!WordsDict.ContainsKey(key))
                        {
                            WordsDict.Add(key, value);
                        }
                    }
                }
                else if(line.TrimStart().StartsWith("[") && line.TrimEnd().EndsWith("]") && lvlselected) 
                {
                    CurType = line.Trim().TrimStart('[').TrimEnd(']');
                    typeSelected = (CurType == GolType);
                }
                else if (
                    (line.StartsWith("N") || line.StartsWith("_") || line.StartsWith("[") || line == "--")
                        && lvlselected && scriptselected && typeSelected
                )
                {
                    if(WordsDict.Count == 0)
                    {
                        MessageBox.Show("No words found for the selected level, script, and type combination.");
                        
                    }
                    break;
                }
            }
            MessageBox.Show(lvlselected.ToString());
            TotalQuestion = (TextBlock)FindName("TotalQuestions");
            TotalQuestion.Text = $"{Pointer + 1}/{QuestionCount}";
            MessageBox.Show(WordsDict.Count.ToString() + " words loaded from file.");
        }
        void GenerateQuesitonWithMCQs()
        {
            ShuffleWordsAndAssignToQuestions();
            string Correctkey = "";

            for (int i = 0; i < QuestionCount; i++)
            {
                //MessageBox.Show(WordsDict.Count.ToString());
                WordsDict.TryGetValue(Questions[i], out Correctkey);

                GenerateMCQ(Correctkey);
                MarkedStatus.Add(0);
            }
        }
        void ShuffleWordsAndAssignToQuestions()
        {
            Random rng = new Random();
            Allkey = WordsDict.Keys.ToList();
            Allkey = Allkey.OrderBy(x => rng.Next()).ToList();

            foreach (string str in Allkey)
            {
                Questions.Add(str);
            }
        }

        public struct MCQset
        {
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
            List<string> AllValues = WordsDict.Values.ToList();
            Random rng = new Random();
            MCQset MCQ = new MCQset();

            AllValues.Remove(Correctkey);

            AllValues = AllValues.OrderBy(x => rng.Next()).ToList();
            for (int i = 0; i < 3; i++)
            {
                MCQ.MCQs.Add(AllValues[i]);
            }
            MCQ.MCQs.Add(Correctkey);
            MCQ.MCQs = MCQ.MCQs.OrderBy(x => rng.Next()).ToList();

            AllMCQsets.Add(MCQ);
        }

        void InitializeButtons()
        {

            btns[0] = Btn1;
            btns[1] = Btn2;
            btns[2] = Btn3;
            btns[3] = Btn4;

            for (int i = 0; i < 4; i++)
            {
                if (MarkedStatus[Pointer] != 0)
                {
                    btns[i].IsEnabled = false;
                }
                btns[i].Content = AllMCQsets[Pointer].MCQs[i];
                if (ColoredChoice[Pointer][i] == 1)
                {
                    A = new GradientStop(Color.FromArgb(255, 200, 255, 200), 0.8f);
                    B = new GradientStop(Color.FromArgb(255, 200, 200, 200), 0.1f);
                    GradientButtons(A, B);
                    btns[i].Background = GradientBrush;
                    if (MarkedStatus[Pointer] == 0)
                    {
                        MarkedStatus.Add(1);
                    }
                }
                else if (ColoredChoice[Pointer][i] == -1)
                {
                    A = new GradientStop(Color.FromArgb(255, 255, 200, 200), 0.8f);
                    B = new GradientStop(Color.FromArgb(255, 200, 200, 200), 0.1f);
                    GradientButtons(A, B);
                    btns[i].Background = GradientBrush;
                    if (MarkedStatus[Pointer] == 0)
                    {
                        MarkedStatus.Add(-1);
                    }
                }
                else
                {
                    A = new GradientStop(Color.FromArgb(255, 230, 242, 255), 0.8f);
                    B = new GradientStop(Color.FromArgb(255, 200, 200, 200), 0.1f);
                    GradientButtons(A, B);
                    btns[i].Background = GradientBrush;
                }
            }

        }


        int[] MCQColorArray;
        List<int[]> ColoredChoice = new List<int[]>();
        void IntializeBottonackGroundColor()
        {
            for (int i = 0; i < Questions.Count; i++)
            {
                MCQColorArray = new int[4] { 0, 0, 0, 0 };
                ColoredChoice.Add(MCQColorArray);
            }
        }
        Color Start, End;

        void MCQ(object sender, RoutedEventArgs e)
        {

            string CheckCorrectAnswer;
            if (sender is Button btn)
            {
                var tag = btn.Tag;
                WordsDict.TryGetValue(Questions[Pointer], out CheckCorrectAnswer);
                if (btn.Content.ToString() == CheckCorrectAnswer)
                {

                    A = new GradientStop(Color.FromArgb(255, 200, 255, 200), 0.8f);
                    B = new GradientStop(Color.FromArgb(255, 200, 242, 200), 0.1f);

                    GradientButtons(A, B);
                    btn.Background = GradientBrush;
                    ColoredChoice[Pointer][Convert.ToInt32(tag)] = 1;
                }
                else
                {
                    A = new GradientStop(Color.FromArgb(255, 255, 200, 200), 0.8f);
                    B = new GradientStop(Color.FromArgb(255, 200, 200, 200), 0.1f);
                    GradientButtons(A, B);
                    btn.Background = GradientBrush;
                    ColoredChoice[Pointer][Convert.ToInt32(tag)] = -1;
                }
            }


        }
        int Pointer = 0;
        void MoveThroughList(object sender, RoutedEventArgs e)
        {

            if (AllInitialized)
            {
                Button btn = (Button)sender;
                if (btn.Name == "Left_Move")
                {
                    Pointer--;
                    if (Pointer <= 0) { Pointer = 0; }
                    InitializeButtons();

                    TotalQuestion.Text = $"{Pointer + 1}/{QuestionCount}";
                    DisplayTextBlock.Text = Questions[Pointer];

                }
                else if (btn.Name == "Right_Move")
                {
                    Pointer++;
                    if (Pointer >= QuestionCount) { Pointer = QuestionCount - 1; }
                    InitializeButtons();

                    TotalQuestion.Text = $"{Pointer + 1}/{QuestionCount}";
                    DisplayTextBlock.Text = Questions[Pointer];
                }

            }

        }

        void OnlyOneIsCheckedInMenu(object sender, RoutedEventArgs e)
        {

            Menu menu = SelectMenu;
            CheckBox box = (CheckBox)sender;
            if (box.IsChecked == true && box != PrevChecked)
            {
                PrevChecked.IsChecked = false;
                box.IsChecked = true;
                PrevChecked = box;
            }
            else if (box == PrevChecked)
            {
                box.IsChecked = true;
            }

        }

        private void FileDropDown_Click(object sender, RoutedEventArgs e)
        {
            FilePopup.IsOpen = true;
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            AllMCQsets.Clear();
            MarkedStatus.Clear();
            ColoredChoice.Clear();
            Questions.Clear();
            WordsDict.Clear();


            QuestionCount = 0;
            Pointer = 0;
            NewImprovedInitializeDictionary();
            //InitializeDictiionary();
            GenerateQuesitonWithMCQs();
            DisplayTextBlock.Text = Questions[0];
            IntializeBottonackGroundColor();
            InitializeButtons();
            TotalQuestion = (TextBlock)FindName("TotalQuestions");

            TotalQuestion.Text = $"{Pointer + 1}/{QuestionCount}";

        }

        private void SelectDropDown_Click(object sender, RoutedEventArgs e)
        {
            SelectPopup.IsOpen = true;

        }


        private void Level_Select(object sender, RoutedEventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            StackPanel stk = LevelStack;
            if (box.IsChecked == true && box != PrevLevel)
            {
                box.IsChecked = true;
                PrevLevel.IsChecked = false;
                PrevLevel = box;
            }
            else
            {
                box.IsChecked = true;
            }
        }

        private void LevelSelect_Click(object sender, RoutedEventArgs e)
        {
            LevelSelectPopUp.IsOpen = true;
        }
        int PreQuestionCount = 0;
        private void SelectQuestionCount(object sender, RoutedEventArgs e)
        {

            CheckBox box = (CheckBox)sender;
            StackPanel stk = LevelStack;
            if (box.IsChecked == true && box != PrevQuestionCount)
            {
                PrevQuestionCount.IsChecked = false;
                box.IsChecked = true;
                PrevQuestionCount = box;
            }
            else if (box == PrevQuestionCount)
            {
                box.IsChecked = true;
            }
            QuestionCount = int.Parse(box.Content.ToString());

            if (QuestionCount < Questions.Count)
            {
                PreQuestionCount = QuestionCount;
            }
            else
            {
                MessageBox.Show("The List Cotains Less Number of Questions then Selected \n " +
                    "Select Less Question or increase Number of Question in the List. \n" +
                    "(Setting to max Availabe questions)");

                PreQuestionCount = Questions.Count;
                Reset_Click(sender, e);
            }
        }

        private void CustomQuestionCount(object sender, RoutedEventArgs e)
        {

            Window CustomWin = new Window();
            CustomWin.Width = 300;
            CustomWin.Height = 200;
            CustomWin.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            CustomWin.ResizeMode = ResizeMode.NoResize;
            CustomWin.WindowStyle = WindowStyle.ToolWindow;


            Grid grid = new Grid();

            Label label = new Label();
            label.Content = "Enter Number of Questions";
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.VerticalAlignment = VerticalAlignment.Top;
            label.Margin = new Thickness(0, 20, 0, 0);

            TextBox IN_Box = new TextBox();
            IN_Box.HorizontalAlignment = HorizontalAlignment.Center;
            IN_Box.VerticalAlignment = VerticalAlignment.Center;
            IN_Box.Width = 150;
            IN_Box.Height = 25;
            IN_Box.BorderThickness = new Thickness(1);
            IN_Box.Margin = new Thickness(50);
            IN_Box.FontSize = 18;

            Button b_OK = new Button();
            b_OK.Width = 50; b_OK.Height = 25;
            b_OK.Content = "Done";
            b_OK.HorizontalAlignment = HorizontalAlignment.Center;
            b_OK.VerticalAlignment = VerticalAlignment.Bottom;
            b_OK.Margin = new Thickness(0, 0, 0, 15);
            b_OK.BorderThickness = new Thickness(0);

            b_OK.Click += (sender, e) =>
            {
                if (int.TryParse(IN_Box.Text, out int result) && result > 0)
                {
                    if (result > Questions.Count)
                    {
                        MessageBox.Show("The List Cotains more Number of Questions then Selected \n " +
                            "Select Less Question or increase Number of Question in the List. \n" +
                            "(Setting to max Availabe questions)");
                        result = Questions.Count;
                    }
                    PreQuestionCount = result;
                    CustomWin.Close();
                }
                else
                {
                    MessageBox.Show("Please enter a valid number greater than 0.");
                }
            };

            grid.Children.Add(IN_Box);
            grid.Children.Add(b_OK);
            grid.Children.Add(label);

            CustomWin.Content = grid;
            CustomWin.ShowDialog();

        }

        private void TypeDropDown_Click(object sender, RoutedEventArgs e)
        {
            TypePopup.IsOpen = true;

        }
        
        private void TypeSelect(object sender, RoutedEventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            StackPanel stk = TypeStack;
            if (box.IsChecked == true && box != PrevType)
            {
                PrevType.IsChecked = false;
                box.IsChecked = true;
                PrevType = box;
            }
            else if (box == PrevType)
            {
                box.IsChecked = true;
            }

        }
    }
}