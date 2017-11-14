using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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

namespace saprWpf
{
    class Schema
    {
        private IEnumerable<string> chainName = new List<string>();
        private IEnumerable<string> elemName = new List<string>();
        private IEnumerable<int> elemPin = new List<int>();

        public IEnumerable<string> СhainName
        {
            get { return chainName; }
            set { chainName = value; }
        }

        public IEnumerable<string> ElemName
        {
            get { return elemName; }
            set { elemName = value; }
        }

        public IEnumerable<int> ElemPin
        {
            get { return elemPin; }
            set { elemPin = value; }
        }

        public IEnumerable<string> setChainName(string value)
        {
            chainName = chainName.Concat(new[] { value });
            return chainName;
        }

        public IEnumerable<string> setElemName(string value)
        {
            elemName = elemName.Concat(new[] { value });
            return elemName;
        }

        public IEnumerable<int> setElemPin(string value)
        {
            elemPin = elemPin.Concat(new[] { Convert.ToInt32(value) });
            return elemPin;
        }

        public override string ToString()
        {
            return "Цепь: " + chainName;
        }
    }

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void button2_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        //читаем NET файл
        public List<string> readNetFile()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.FileName = "Document";
            dlg.DefaultExt = ".net";
            dlg.Filter = "NET file (.net)|*.net";

            // открыть диалоговое окно выбора файла
            Nullable<bool> result = dlg.ShowDialog();

            //здесь будет результат
            List<string> split = new List<string>();

            // Если файл прочитан
            if (result == true)
            {
                // открыть файл
                string filename = dlg.FileName;

                //чтение NET файла 
                string textNetFile = "";

                try
                {
                    using (StreamReader sr = new StreamReader(filename))
                    {
                        textNetFile = sr.ReadToEnd();
                    }
                }
                catch (Exception exp)
                {
                    MessageBox.Show(exp.Message);
                }

                /*
                 * Есть два вида NET файла.
                 * В одном из них есть мета-теги $NETS, $END и т.п. 
                 * Это означает, что в NET файле есть много других элементов, которые не нужны.
                 * От них нужно избавиться.
                 * То есть находим $NETS, и если есть, то удаляем лишнее
                 */

                //если NET файл содержит $NETS
                if (textNetFile.Contains("$NETS"))
                {
                    int indexNets = 0;

                    //находим $NETS и запоминаем его позицию
                    foreach (Match m in Regex.Matches(textNetFile, @"[$]NETS"))
                    {
                        indexNets = m.Index;
                    }

                    //удаляем подстроку до $NETS (сам $NETS имеет 5 символов + удаление перевода строки)
                    textNetFile = textNetFile.Remove(0, indexNets + 7);

                    //и удаляем в конце $END
                    textNetFile = textNetFile.Replace("\n$END", "");

                    //удаляем ;
                    textNetFile = textNetFile.Replace(";", "");

                    //удаляем перенос строки в одной цепи
                    textNetFile = Regex.Replace(textNetFile, @",\r?\n", "");

                    //разделяем по переносу строки
                    split = textNetFile.Split('\n').ToList();
                }
                else
                {
                    //удаляем перенос строки в самом конце
                    textNetFile = textNetFile.Substring(0, textNetFile.Length - 4);
                    //разделяем по ;
                    split = textNetFile.Split(';').ToList();
                }
            }

            textBoxNetFile.Text = "-----SPLIT------\n";

            foreach (var it in split)
            {
                textBoxNetFile.Text += it;
            }
            return split;
        }

        //заполняем DataGrid матрицей цепей
        public void setDataGridMatrixChain(IEnumerable<string> elemName, IEnumerable<int> elemPin, string[][] stepArray)
        {
            DataTable dt = new DataTable();

            //формируем размерность DataGrid
            dt.Columns.Add("#");
            
            //для файла allegro_5.NET раскоментировать эти строки
            /*
             * dt.Columns.Add("");
             * dt.Columns.Add("");
             */

            foreach (var itemPin in elemPin)
            {
                dt.Columns.Add(itemPin.ToString());
            } 

            foreach (var itemElem in elemName)
            {
                dt.Rows.Add(itemElem);
            }

            //заполняем DataGrid
            string tempChain = "";
            int tempCount = 0;

            foreach (var itemElem in elemName)
            {
                for (int i = 0, count = 0; i < stepArray.Length; i++)
                {
                    count = 0;
                    tempChain = stepArray[i][0].ToString();

                    for (int k = 0; k < stepArray[i].Length; k++)
                    {
                        if (count != 0)
                        {
                            if (count % 2 != 0)
                            {
                                if (itemElem.ToString() == stepArray[i][k])
                                {
                                    

                                    //вместо названий цепей вставляем их "номер"
                                    dt.Rows[tempCount][Convert.ToInt32(stepArray[i][k + 1])] = (i + 1);
                                }
                            }
                        }
                        count++;
                    }

                }
                tempCount++;
            }
            dataGrid.ItemsSource = dt.DefaultView;
        }

        //считаем количество цепей, элементов и пинов в сумме
        public List<int> getCountSplit(List<string> split)
        {  
            List<int> countSplit = new List<int>();

            string regex = @"\w+";
            int tmp = 0;
            foreach (var item in split)
            {
                tmp = 0;
                foreach (Match m in Regex.Matches(item, regex))
                {
                    tmp++;
                }
                countSplit.Add(tmp);
            }
            return countSplit;
        }

        //формируем comboBox
        public void setComboBox(IEnumerable<string> elemName)
        {
            int temp = 1;
            foreach (var item in elemName)
            {
                comboBox.Items.Add(temp);
                temp++;
            }
        }

        //КНОПКА "OPEN NET FILE" ---------------------------------
        private void button_Click(object sender, RoutedEventArgs e)
        {
            dataGrid.Columns.Clear();
            comboBox.Items.Clear();
            textBoxChainName.Text = "";
            textBoxNetFile.Text = "";

            //полчаем NET файл
            List<string> split = readNetFile();

            //считаем количество цепей, элементов и пинов в сумме
            List<int> countSplit = getCountSplit(split);    

            //формируем ступенчатый массив
            /*
             * Первый элемент подмассива равен названию цепи 
             * Несчитая первый элемент массива, каждый нечетный - это элемент схемы 
             * А каждый четный это номер пина элемента
             */

            //ступенчатый массив (здесь хранится каждое найденное вхождение в NET файле)
            string[][] stepArray = new string[countSplit.Count][];
            for (int i = 0; i < countSplit.Count; i++)
            {
                stepArray[i] = new string[countSplit[i]];
            }

            //получаем отдельно названия цепей, элементов и номера пинов
            //и выводим полученные данные в textBox
            Schema sh = new Schema();

            textBoxNetFile.Text += "-----Schema------";

            for (int i = 0, count; i < split.Count; i++)
            {
                count = 0;
                foreach (Match m in Regex.Matches(split[i], @"\w+"))
                {
                    //если первый элемент в подмассиве, то это название цепи
                    if (count == 0)
                    {
                        stepArray[i][count] = m.Value;
                        sh.СhainName = sh.setChainName(m.Value);
                        textBoxNetFile.Text += "\n" + m.Value + ": ";
                        textBoxChainName.Text += m.Value + "\t= " + (i + 1) + "\n";
                    }
                    //если нечетный элемент в подмассиве, то это название элемента схемы
                    else if (count % 2 != 0)
                    {
                        stepArray[i][count] = m.Value;
                        sh.ElemName = sh.setElemName(m.Value);
                        textBoxNetFile.Text += m.Value + " - ";
                    }
                    //если четный элемент в подмассиве, то это номер пина
                    else if (count % 2 == 0)
                    {
                        stepArray[i][count] = m.Value;
                        sh.ElemPin = sh.setElemPin(m.Value);
                        textBoxNetFile.Text += m.Value + " ";
                    }
                    count++;
                }
            }

            //удаляем повторяющиеся названия элементов
            sh.ElemName = sh.ElemName.Distinct();
            //сортируем
            sh.ElemName = sh.ElemName.OrderBy(s => s);

            //удаляем повторяющиеся номера пинов
            sh.ElemPin = sh.ElemPin.Distinct();
            //сортируем
            sh.ElemPin = sh.ElemPin.OrderBy(s => s);

            //формируем comboBox
            setComboBox(sh.ElemName);

            //заполняем DataGrid матрицей цепей
            setDataGridMatrixChain(sh.ElemName, sh.ElemPin, stepArray);

            //отображаем кол-во элементов в NET-файле
            countElemLabel.Content = "Кол-во элементов: " + sh.ElemName.Count().ToString();
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}

