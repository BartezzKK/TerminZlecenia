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
    public partial class MainWindow : INotifyPropertyChanged
    {
        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            //lastOrder = lastDate.SelectedDate ?? DateTime.Now;
        }

        public IEnumerable<Order> ReadOrdersFromCSV(string fileName)
        {
            string[] lines = File.ReadAllLines(System.IO.Path.ChangeExtension(fileName, ".csv"));

            return lines.Select(line =>
            {
                string[] data = line.Split(';');
                return new Order(data[0], Convert.ToInt32(data[1]), DateTime.Now);
            });
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

        private IEnumerable<Order> calculateDates()
        {
            int tempQuantity = averageQuantity;
            int leftQuantityFromPreviousDay = 0;
            DateTime tempDate = lastOrder ?? DateTime.Now;
            orders = ReadOrdersFromCSV(FilePath).ToList();
            if (tempDate.DayOfWeek == DayOfWeek.Saturday) { tempDate.AddDays(2); }
            if (tempDate.DayOfWeek == DayOfWeek.Sunday) { tempDate.AddDays(1); }
            foreach (var x in orders)
            {
                if (tempQuantity - x.Quantity > 0)
                {
                    if (leftQuantityFromPreviousDay < 0)
                    {
                        x.EstimatedEndTime = tempDate;
                        leftQuantityFromPreviousDay = 0;
                    }
                    else
                    {
                        x.EstimatedEndTime = tempDate;
                        tempQuantity -= x.Quantity;
                    }
                }
                else
                {
                    if (tempDate.DayOfWeek == DayOfWeek.Friday)
                    {
                        tempDate = tempDate.AddDays(2);
                        x.EstimatedEndTime = tempDate;
                    }
                    tempDate = tempDate.AddDays(1);
                    x.EstimatedEndTime = tempDate;
                    leftQuantityFromPreviousDay = tempQuantity - x.Quantity;
                }
                if (leftQuantityFromPreviousDay < 0) { tempQuantity = averageQuantity + leftQuantityFromPreviousDay; }
                if (leftQuantityFromPreviousDay > 0) { tempQuantity -= x.Quantity; }
                //if(leftQuantityFromPreviousDay == 0) { tempQuantity = averageQuantity; }
            }
            ((MainWindow)Application.Current.MainWindow).UpdateLayout();
            ListViewOrder.ItemsSource = orders;
            return orders;
        }
        private void calculateDates_Click(object sender, RoutedEventArgs e)
        {
            lastOrder = lastDate.SelectedDate;
            calculateDates();
        }

        private string FilePath;
        private int averageQuantity;
        private IEnumerable<Order> orders;
        public DateTime? lastOrder;

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
