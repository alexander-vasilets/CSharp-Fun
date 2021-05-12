using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Syncfusion.XlsIO;

namespace NaiveBayesClassifier
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

        private void loadCsvFileBtn_Click(object sender, RoutedEventArgs e)
        {
            string fileName = GetFileNameFromDialog("csv");
            //string fileName = @"C:\Users\Kojima\Desktop\Таблиця слів.csv";
            if (fileName == null)
                return;
            string rawText = String.Empty;
            using (TextFieldParser csvParser = new TextFieldParser(fileName))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { ";" });
                csvParser.HasFieldsEnclosedInQuotes = true;

                csvParser.ReadLine();
                StringBuilder sb = new StringBuilder();

                while (!csvParser.EndOfData)
                {
                    string[] fields = csvParser.ReadFields();
                    sb.Append(fields[1] + " ");
                }
                rawText = sb.ToString();
            }
            textBox1.Text = Regex.Replace(rawText, @"#", "");
        }
        private void loadTxtFileBtn_Click(object sender, RoutedEventArgs e)
        {
            string fileName = GetFileNameFromDialog("txt");
            //string fileName = @"C:\Users\Kojima\Desktop\Текст.txt";
            if (fileName == null)
                return;
            textBox2.Text = System.IO.File.ReadAllText(fileName);
        }
        private void createXlsxFileBtn_Click(object sender, RoutedEventArgs e)
        {
            warningTxt.Visibility = Visibility.Collapsed;
            string text1 = textBox1.Text;
            string text2 = textBox2.Text;
            if (string.IsNullOrEmpty(text1) || string.IsNullOrEmpty(text2))
            {
                ShowWarning("Тексти обох класів не мають бути порожніми!");
                return;
            }
            Dictionary<string, int> dict1 = GetWordFrequencies(text1);
            var topWords1 = dict1.OrderByDescending(p => p.Value).Take(30);
            Dictionary<string, int> dict2 = GetWordFrequencies(text2);
            var topWords2 = dict2.OrderByDescending(p => p.Value).Take(30);
            Dictionary<string, int> dict3 = new Dictionary<string, int>(dict1);
            foreach (var pair in dict2)
            {
                if (dict3.ContainsKey(pair.Key))
                    dict3[pair.Key] += pair.Value;
                else
                    dict3[pair.Key] = pair.Value;
            }
            var mostFrequent = dict3.OrderByDescending(p => p.Value).Take(30);

            using (ExcelEngine excelEngine = new ExcelEngine())
            {
                IApplication application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(3);
                workbook.Worksheets[0].Name = "Перший текст";
                CreateFrequenciesStatistics(workbook.Worksheets[0], topWords1);
                workbook.Worksheets[1].Name = "Другий текст";
                CreateFrequenciesStatistics(workbook.Worksheets[1], topWords2);
                workbook.Worksheets[2].Name = "Загальна частотність";
                CreateFrequenciesStatistics(workbook.Worksheets[2], mostFrequent);

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "xlsx файл|*.xlsx";
                saveFileDialog.Title = "Зберегти звіт частотності";
                saveFileDialog.ShowDialog();
                if (saveFileDialog.FileName != "")
                    workbook.SaveAs(saveFileDialog.FileName);
            }
        }
        private void PerformTestBtn_Click(object sender, RoutedEventArgs e)
        {
            warningTxt.Visibility = Visibility.Collapsed;
            string text1 = textBox1.Text;
            string text2 = textBox2.Text;
            if (string.IsNullOrEmpty(text1) || string.IsNullOrEmpty(text2))
            {
                ShowWarning("Тексти обох класів не мають бути порожніми!");
                return;
            }
            if (string.IsNullOrEmpty(inputTxtBox.Text))
            {
                ShowWarning("Тестовий текст не має бути порожнім!");
                return;
            }
            List<Document> trainText = new List<Document>
            { 
                new Document("class1", text1),
                new Document("class2", text2)
            };

            Classifier c = new Classifier(trainText);
            string text = inputTxtBox.Text;
            double classResult1 = c.IsInClassProbability("class1", text);
            double classResult2 = c.IsInClassProbability("class2", text);
            testResultTextBox.Text = $"Вірогідність належності до першого класу: {classResult1}\nВірогідність належності до другого класу: {classResult2}";
        }
        private Dictionary<string, int> GetWordFrequencies(string text)
        {
            string[] words = text.Split(new char[] { '.', '?', '!', ' ', ';', ':', ',', '—' }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, int> dict = new Dictionary<string, int>();
            foreach (string word in words)
            {
                string w = word.ToLower();
                if (dict.ContainsKey(w))
                    ++dict[w];
                else
                    dict[w] = 1;
            }
            return dict;
        }
        private string GetFileNameFromDialog(string extension)
        {
            string fileName = String.Empty;
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Оберіть файл для завантаження";
                openFileDialog.InitialDirectory = $"{Environment.CurrentDirectory}";
                openFileDialog.Filter = $"{extension} файлы (*.{extension})|*.{extension}";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    fileName = openFileDialog.FileName;
                }
                else
                    fileName = null;
            }
            return fileName;
        }
        private void CreateFrequenciesStatistics(IWorksheet sheet, IEnumerable<KeyValuePair<string, int>> dict)
        {
            sheet.Range["A2"].Text = "Слово";
            sheet.Range["B2"].Text = "Частота";
            int cnt = 3;
            foreach (var pair in dict)
            {
                sheet.Range[$"A{cnt}"].Text = pair.Key;
                sheet.Range[$"B{cnt}"].Number = pair.Value;
                ++cnt;
            }

            IChartShape chart = sheet.Charts.Add();
            chart.ChartType = ExcelChartType.Histogram;
            chart.DataRange = sheet[$"A3:B{dict.Count() + 2}"];
            chart.ChartType = ExcelChartType.Column_Clustered;
            chart.PrimaryCategoryAxis.BinWidth = 5;
            chart.ChartTitle = "Гістограма частотності";
            chart.HasLegend = false;
            chart.TopRow = 5;
            chart.LeftColumn = 5;
            chart.BottomRow = 25;
            chart.RightColumn = 20;

        }
        private void ShowWarning(string text)
        {
            warningTxt.Visibility = Visibility.Visible;
            warningTxt.Text = text;
        }
    }
}
