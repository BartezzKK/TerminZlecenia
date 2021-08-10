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
            //lastOrder = lastDate.SelectedDate ?? DateTime.Now;
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
            catch(ArgumentNullException e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private void btnOpenDialog_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if(openFileDialog.ShowDialog() == true)
            {
                FilePath = openFileDialog.FileName;
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

        //private IEnumerable<Order> calculateDates()
        //{
        //    int tempQuantity = averageQuantity;
        //    int leftQuantityFromPreviousDay = 0;
        //    DateTime tempDate = lastOrder ?? DateTime.Now;
        //    try
        //    {
        //        orders = ReadOrdersFromCSV(FilePath).ToList();
        //    }
        //    catch (ArgumentNullException e)
        //    {
        //        Console.WriteLine("Nullowa wartość: " + e);
        //    }
        //    if (tempDate.DayOfWeek == DayOfWeek.Saturday) { tempDate.AddDays(2); }
        //    if (tempDate.DayOfWeek == DayOfWeek.Sunday) { tempDate.AddDays(1); }
        //    foreach (var x in orders)
        //    {
        //        if (tempQuantity - x.Quantity > 0 || tempQuantity - x.Quantity == 0)
        //        {
        //            if (leftQuantityFromPreviousDay < 0)
        //            {
        //                x.EstimatedEndTime = tempDate;
        //                leftQuantityFromPreviousDay = 0;
        //            }
        //            else
        //            {
        //                x.EstimatedEndTime = tempDate;
        //                tempQuantity -= x.Quantity;
        //            }
        //        }
        //        else
        //        {
        //            leftQuantityFromPreviousDay = tempQuantity - x.Quantity;
        //            if (Math.Abs(leftQuantityFromPreviousDay) > averageQuantity)
        //            {
        //                if(tempDate.DayOfWeek == DayOfWeek.Thursday)
        //                {
        //                    tempDate = tempDate.AddDays(4);
        //                    x.EstimatedEndTime = tempDate;
        //                }
        //                else if(tempDate.DayOfWeek == DayOfWeek.Friday)
        //                {
        //                    tempDate = tempDate.AddDays(3);
        //                    x.EstimatedEndTime = tempDate;
        //                }
        //                else if(tempDate.DayOfWeek == DayOfWeek.Saturday)
        //                {
        //                    tempDate = tempDate.AddDays(2);
        //                    x.EstimatedEndTime = tempDate;
        //                }
        //                else
        //                {
        //                    tempDate = tempDate.AddDays(2);
        //                    x.EstimatedEndTime = tempDate;
        //                }
        //            }
        //            else
        //            {
        //                tempDate = tempDate.AddDays(1);
        //                x.EstimatedEndTime = tempDate;
        //            }
        //        }
        //        if (leftQuantityFromPreviousDay < 0) { tempQuantity = averageQuantity + leftQuantityFromPreviousDay; }
        //        if (leftQuantityFromPreviousDay > 0) { tempQuantity -= x.Quantity; }
        //    }
        //    ((MainWindow)Application.Current.MainWindow).UpdateLayout();
        //    DataGridTable.ItemsSource = orders;
        //    return orders;
        //}

        private IEnumerable<Order> calculateDates()
        {
            int currentQuantity = averageQuantity;
            int leftQuantityFromPreviousDay = averageQuantity;
            int tempDayOfWeek;
            int counterOfDays;
            //storing temporary date for calculation
            DateTime tempDate = lastOrder ?? DateTime.Now;
            orders = ReadOrdersFromCSV(FilePath).ToList();
            
            //if (tempDate.DayOfWeek == DayOfWeek.Saturday) { tempDate.AddDays(2); }
            //if (tempDate.DayOfWeek == DayOfWeek.Sunday) { tempDate.AddDays(1); }
            
            foreach (var x in orders)
            {
                tempDate = tempDate.AddDays(AddDaysIfWeekend(tempDate));
                if (x.Quantity <= currentQuantity)
                {
                    if(x.Quantity < currentQuantity)
                    {
                        x.EstimatedEndTime = tempDate;
                        currentQuantity -= x.Quantity;
                        leftQuantityFromPreviousDay = currentQuantity;
                    }
                    else
                    {
                        x.EstimatedEndTime = tempDate;
                        //tempDate = tempDate.AddDays(CountHowManyDaysShouldAdd(x.Quantity, averageQuantity, leftQuantityFromPreviousDay));
                        tempDate = tempDate.AddDays(1);
                        currentQuantity = averageQuantity;
                    }
                }
                
                //przewidywany czas równa się tempdate jeżeli ilość na zleceniu jest większa od średniej ilości
                else
                {
                    if(currentQuantity == 0) { currentQuantity = averageQuantity; }
                    counterOfDays = CountHowManyDaysShouldAdd(x.Quantity, averageQuantity, leftQuantityFromPreviousDay);
                    for (int i = 1; i <= counterOfDays; i++)
                    {
                        
                        if(AddDaysIfWeekend(tempDate) > 0)
                        {
                            tempDate = tempDate.AddDays(AddDaysIfWeekend(tempDate));
                            i--;
                        }
                        else
                        {
                            tempDate = tempDate.AddDays(1);
                            tempDate = tempDate.AddDays(AddDaysIfWeekend(tempDate));
                        }
                    }
                    if(counterOfDays > 0)
                    {
                        currentQuantity = Math.Abs(currentQuantity) + (counterOfDays * averageQuantity) - x.Quantity;
                    }
                    else
                    {
                        currentQuantity = currentQuantity + averageQuantity - x.Quantity;
                    }
                    x.EstimatedEndTime = tempDate;
                    //tempDate.AddDays(AddDaysIfWeekend(tempDate));
                    //tempDayOfWeek = CheckWhatIsCurrentDayOfWeek(tempDate);
                    //tempDate.AddDays(CountHowManyDaysShouldAdd(x.Quantity, averageQuantity, leftQuantityFromPreviousDay));
                }
            }
    ((MainWindow)Application.Current.MainWindow).UpdateLayout();
            DataGridTable.ItemsSource = orders;
            return orders;
        }

        private void calculateDates_Click(object sender, RoutedEventArgs e)
        {
            lastOrder = lastDate.SelectedDate;
            calculateDates();
        }

        private int AddDaysIfWeekend(DateTime orderDate)
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

        private int CountHowManyDaysShouldAdd(int quantity, int averageQuantity, int leftFromPrvDay)
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

        private int CheckWhatIsCurrentDayOfWeek(DateTime date)
        {
            return (int)date.DayOfWeek;
        }

        private string FilePath;
        private int averageQuantity;
        private IEnumerable<Order> orders;
        public DateTime? lastOrder;

        //public event PropertyChangedEventHandler PropertyChanged;

    }
}
