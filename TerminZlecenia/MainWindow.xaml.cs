using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace TerminZlecenia
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
        }

        public IEnumerable<Order> ReadOrdersFromCSV(string fileName)
        {
            try
            {
                string[] lines = File.ReadAllLines(System.IO.Path.ChangeExtension(fileName, ".csv"));
                return lines.Select(line =>

                {
                    string[] data = line.Split(';');
                    return new Order(data[0], Convert.ToInt32(data[1]), DateTime.Now);
                });
            }
            catch (ArgumentNullException e)
            {
                MessageBox.Show("Nie wskazano żadnego pliku \n" + e.Message);
                return null;
            }
            catch (FileNotFoundException e)
            {
                MessageBox.Show("Nieprawidłowy format pliku, wskaż plik CSV  \n" + e.Message);
                return null;
            }
        }

        private void btnOpenDialog_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if(openFileDialog.ShowDialog() == true)
            {
                filePath = openFileDialog.FileName;
            }
            
        }
        public int AverageQuantity
        {
            get { return averageQuantity;  }
            set
            {
                if(averageQuantity != value)
                {
                    averageQuantity = value;
                }
            }
        }

        private void OnPropertyChanged()
        {
            calculateDates();
        }

        private IEnumerable<Order> calculateDates()
        {
            int currentQuantity = averageQuantity;
            int leftQuantityFromPreviousDay = averageQuantity;
            int counterOfDays;
            //storing temporary date for calculation
            DateTime tempDate = LastOrder ?? DateTime.Now;
            try
            {
            orders = ReadOrdersFromCSV(filePath).ToList();
                MessageBox.Show("Sukces!");
            }
            catch(ArgumentNullException e)
            {
                MessageBox.Show("Wybierz plik CSV żeby kontynuować \n" + e.Message);
            }

            if (orders != null)
            {
                foreach (var x in orders)
                {
                    tempDate = tempDate.AddDays(addDaysIfWeekend(tempDate));
                    if (x.Quantity <= currentQuantity)
                    {
                        if (x.Quantity < currentQuantity)
                        {
                            x.EstimatedEndTime = tempDate;
                            currentQuantity -= x.Quantity;
                            leftQuantityFromPreviousDay = currentQuantity;
                        }
                        else
                        {
                            x.EstimatedEndTime = tempDate;
                            tempDate = tempDate.AddDays(1);
                            currentQuantity = averageQuantity;
                        }
                    }

                    else
                    {
                        if (currentQuantity == 0) { currentQuantity = averageQuantity; }
                        counterOfDays = countHowManyDaysShouldAdd(x.Quantity, averageQuantity, leftQuantityFromPreviousDay);
                        for (int i = 1; i <= counterOfDays; i++)
                        {

                            if (addDaysIfWeekend(tempDate) > 0)
                            {
                                tempDate = tempDate.AddDays(addDaysIfWeekend(tempDate));
                                i--;
                            }
                            else
                            {
                                tempDate = tempDate.AddDays(1);
                                tempDate = tempDate.AddDays(addDaysIfWeekend(tempDate));
                            }
                        }
                        if (counterOfDays > 0)
                        {
                            currentQuantity = Math.Abs(currentQuantity) + (counterOfDays * averageQuantity) - x.Quantity;
                        }
                        else
                        {
                            currentQuantity = currentQuantity + averageQuantity - x.Quantity;
                        }
                        x.EstimatedEndTime = tempDate;
                    }
                }
            }
    ((MainWindow)Application.Current.MainWindow).UpdateLayout();
            DataGridTable.ItemsSource = orders;
            return orders;
        }

        private void calculateDates_Click(object sender, RoutedEventArgs e)
        {
            LastOrder = lastDate.SelectedDate;
            calculateDates();
        }

        private int addDaysIfWeekend(DateTime orderDate)
        {
            if (orderDate.DayOfWeek == DayOfWeek.Saturday)
            { 
                return 2;
            }
            if (orderDate.DayOfWeek == DayOfWeek.Sunday)
            {
                return 1;
            }
            else return 0;
        }

        private int countHowManyDaysShouldAdd(int quantity, int averageQuantity, int leftFromPrvDay)
        {
            if(quantity <= leftFromPrvDay)
            {
                return 0;
            }
            else
            {
                int counter = 0;
                do
                {
                    leftFromPrvDay += averageQuantity;
                    counter++;
                } while (quantity > leftFromPrvDay);
                return counter;
            }
        }

        private int checkWhatIsCurrentDayOfWeek(DateTime date)
        {
            return (int)date.DayOfWeek;
        }

        private string filePath;
        private int averageQuantity;
        private IEnumerable<Order> orders;
        public DateTime? LastOrder;

    }
}
