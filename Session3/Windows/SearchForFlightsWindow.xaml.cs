using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace Session3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SearchForFlightsWindow : Window
    {

        public List<string> AirportList { get; set; }
        public List<string> CabinTypeList { get; set; }

        //
        public List<FlightRoute> FlightRouteList { get; set; }
        //

        public List<Flight> FlightList;

        public List<Flight> FilteredFlightList { get; set; }

        public string CurrentFromFilter = "AUH";

        public string CurrentToFilter = "AUH";

        public DateTime CurrentOutboundDate;

        public DateTime CurrentReturnDate;

        public List<Flight> OutboundFlightList;

        public List<Flight> ReturnFlightList;


        public SearchForFlightsWindow()
        {
            LoadValues();
            InitializeComponent();
            DataContext = this;

            SelectFlights();


        }

        public void LoadValues()
        {

            var entities = new Session3Entities();

            AirportList = entities.Airports.Select(ai => ai.IATACode).ToList();
            CabinTypeList = entities.CabinTypes.Select(ct => ct.Name).ToList();

        }

        public void SelectFlights()
        {
            Session3Entities entities = new Session3Entities();

            FlightList = entities.Schedules.Select(f => new Flight
            {
                Id = f.ID,
                From = f.Routes.Airports.IATACode,
                To = f.Routes.Airports1.IATACode,
                Date = f.Date,
                Time = f.Time,
                FlightNumber = f.FlightNumber,
                EconomyPrice = f.EconomyPrice,
                BusinessPrice = f.EconomyPrice + (f.EconomyPrice / 100 * 35),
                FirstClassPrice = (f.EconomyPrice + (f.EconomyPrice / 100 * 35)) + ((f.EconomyPrice + (f.EconomyPrice / 100 * 35)) / 100 * 30),
                NumberOfStops = 0

            }).ToList();

            FilteredFlightList = FlightList;

            


        }

        private void ChangeColumnVisibility()
        {
            EconomyPriceColumn.Visibility = Visibility.Collapsed;
            BusinessPriceColumn.Visibility = Visibility.Collapsed;
            FirstClassPriceColumn.Visibility = Visibility.Collapsed;


            switch (CabinTypeComboBox.SelectedIndex)
            {

                case 0:
                    EconomyPriceColumn.Visibility = Visibility.Visible;
                    break;
                case 1:
                    BusinessPriceColumn.Visibility = Visibility.Visible;
                    break;
                case 2:
                    FirstClassPriceColumn.Visibility = Visibility.Visible;
                    break;
                default:
                    MessageBox.Show("Ошибка");
                    break;


            }


        }

        //private void FromComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => CurrentFromFilter = e.AddedItems[0].ToString();
        //private void ToComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => CurrentToFilter = e.AddedItems[0].ToString();

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (OutboundTextBox.Text == string.Empty || (ReturnTextBox.Text == string.Empty && (bool)BookingTypeReturn.IsChecked))
            {
                MessageBox.Show("Введены не все значения");
                return;
            }

            //Get Destination filters
            CurrentFromFilter = FromComboBox.Text;
            CurrentToFilter = ToComboBox.Text;

            if(CurrentFromFilter == CurrentToFilter)
            {
                MessageBox.Show("Места вылета и прилета должны быть различными");
                return;
            }

            //Change visibility
            ChangeColumnVisibility();


            //Get dates
            try
            {
                CurrentOutboundDate = DateTime.Parse(OutboundTextBox.Text);
                CurrentReturnDate = BookingTypeReturn.IsChecked == true ? DateTime.Parse(ReturnTextBox.Text) : default;
                
            }
            catch
            {
                MessageBox.Show("Неправильный формат даты", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if(BookingTypeReturn.IsChecked == true)
            {

                if (CurrentOutboundDate > CurrentReturnDate && BookingTypeReturn.IsChecked == true)
                {
                    MessageBox.Show("Дата возвращения должна быть больше даты вылета");
                    return;
                }

                ReturnFlightList = 
                FlightList
                .Where(f => f.From == CurrentToFilter && f.To == CurrentFromFilter)
                .Where(f => f.Date >= CurrentReturnDate.AddDays(-3) && f.Date <= CurrentReturnDate.AddDays(3))
                .ToList();


                ReturnDataGrid.ItemsSource = ReturnCheckBox.IsChecked == false ?
                    ReturnFlightList.Where(f => f.Date == CurrentReturnDate)
                    : ReturnFlightList;

            }

            
            OutboundFlightList = FlightList
                .Where(f => f.From == CurrentFromFilter && f.To == CurrentToFilter)
                .Where(f => f.Date >= CurrentOutboundDate.AddDays(-3) && f.Date <= CurrentOutboundDate.AddDays(3))
                .ToList();


            OutboundDataGrid.ItemsSource = OutboundCheckBox.IsChecked == false ?
                OutboundFlightList.Where(f => f.Date == CurrentOutboundDate)
                : OutboundFlightList;



            //TODO Route search
            
            

        }

        //TODO Change datagrid highlight color

        private void OutboundCheckBox_Checked(object sender, RoutedEventArgs e) => OutboundDataGrid.ItemsSource = OutboundFlightList;
        private void OutboundCheckBox_Unchecked(object sender, RoutedEventArgs e) => OutboundDataGrid.ItemsSource = OutboundFlightList is null ? null : OutboundFlightList.Where(f => f.Date == CurrentOutboundDate);
        private void ReturnCheckBox_Checked(object sender, RoutedEventArgs e) => ReturnDataGrid.ItemsSource = ReturnFlightList;
        private void ReturnCheckBox_Unchecked(object sender, RoutedEventArgs e) => ReturnDataGrid.ItemsSource = ReturnFlightList is null ? null : ReturnFlightList.Where(f => f.Date == CurrentReturnDate);
        private void CloseButton_Click(object sender, RoutedEventArgs e) => this.Close();
        

        private void ApplyButton_Click_1(object sender, RoutedEventArgs e)
        {
            Session3Entities entities = new Session3Entities();
            DateTime outboundDate = DateTime.Parse(OutboundTextBox.Text);

            //1 Create list of possible start destinations
            //Iterate throught list to find routes that lead to destination

            // ?
            //var relations2 = entities.Schedules.Where(s => relations.Any(g => g == s.Routes.Airports.IATACode && s.Routes.Airports1.IATACode != FromComboBox.Text && s.Date == outboundDate)).ToList();

            var relations = entities.Schedules.Where(s => s.Routes.Airports.IATACode == FromComboBox.Text && s.Date == outboundDate).Select(r=>r.Routes.Airports1.IATACode).ToList();
            var relations2 = entities.Schedules.Where(s => relations.Any(g => g == s.Routes.Airports.IATACode && s.Routes.Airports1.IATACode != FromComboBox.Text && s.Date == outboundDate)).Select(r=>r.Routes.Airports1.IATACode).ToList();
            var relations3 = entities.Schedules.Where(s => relations2.Any(g => g == s.Routes.Airports.IATACode && s.Routes.Airports1.IATACode != FromComboBox.Text && s.Date == outboundDate)).ToList();

            List<Schedules> VisitedSchedule = new List<Schedules>();

        }

        private void ApplyButton_Click_2(object sender, RoutedEventArgs e)
        {

            Session3Entities entities = new Session3Entities();
            List<List<Schedules>> RoutesList = new List<List<Schedules>>();

            string StartPoint = FromComboBox.Text;
            string EndPoint = ToComboBox.Text;
            DateTime OutboundDate = DateTime.Parse(OutboundTextBox.Text);

            DateTime MaxDate = OutboundDate.AddDays(3);
            DateTime MinDate = OutboundDate.AddDays(-3);

            int AirportAmount = entities.Airports.Count();

            var Connections1 = entities.Schedules.Where(s => s.Routes.Airports.IATACode == StartPoint && s.Date >= MinDate && s.Date <= MaxDate).ToList();

            //Стартовые точки
            foreach (var item in Connections1)
            {
                List<Schedules> startList = new List<Schedules>();
                startList.Add(item);
                RoutesList.Add(startList);
            }

            #region v1
            //for (int i = 0; i < AirportAmount; i++)
            //{

            //    foreach (var ScheduleList in RoutesList)
            //    {
            //        var lastPoint = ScheduleList.Last();

            //        var connections = entities.Schedules
            //            .Where(s => s.Routes.Airports.IATACode == lastPoint.Routes.Airports1.IATACode && s.Date == OutboundDate && lastPoint.Routes.Airports1.IATACode != EndPoint).ToList();

            //        bool Inserted = false;
            //        foreach (var schedule in connections)
            //        {
            //            if(Inserted == false)
            //            {
            //                ScheduleList.Add(schedule);
            //                Inserted = true;
            //            }
            //            else
            //            {
            //                List<Schedules> list = new List<Schedules>();
            //                list.AddRange(ScheduleList);
            //                list.Add(schedule);

            //                RoutesList.Add(list);
            //            }


            //        }

            //    }


            //}
            #endregion

            #region v2
            //for (int i = 0; i < AirportAmount; i++)
            //{

            //    for (int j = RoutesList.Count -1; j>=0; j--)
            //    {
            //        //get last point of the route
            //        var lastPoint = RoutesList[j].Last();

            //        // if it's completed route,continue
            //        if(lastPoint.Routes.Airports1.IATACode == EndPoint)
            //        {
            //            continue;
            //        }


            //        var connections = entities.Schedules
            //            .Where(s => s.Routes.Airports.IATACode == lastPoint.Routes.Airports1.IATACode && s.Date == OutboundDate && s.Routes.Airports1.IATACode != lastPoint.Routes.Airports.IATACode).ToList();

            //        bool Inserted = false;
            //        foreach (var schedule in connections)
            //        {
            //            //modify current route
            //            if (Inserted == false)
            //            {
            //                RoutesList[j].Add(schedule);
            //                Inserted = true;
            //            }
            //            //add new route if already modified
            //            else
            //            {
            //                List<Schedules> list = new List<Schedules>();
            //                list.AddRange(RoutesList[j]);
            //                list.Add(schedule);

            //                RoutesList.Add(list);
            //            }


            //        }

            //    }


            //}
            #endregion


            for (int i = 0; i < AirportAmount; i++)
            {

                for (int j = RoutesList.Count - 1; j >= 0; j--)
                {
                    //get last point of the route
                    var lastPoint = RoutesList[j].Last();

                    // if it's completed route,continue
                    if (lastPoint.Routes.Airports1.IATACode == EndPoint)
                    {
                        continue;
                    }


                    var connections = entities.Schedules
                        .Where(s => s.Routes.Airports.IATACode == lastPoint.Routes.Airports1.IATACode && s.Date == lastPoint.Date  && s.Routes.Airports1.IATACode != lastPoint.Routes.Airports.IATACode).ToList();

                    bool Inserted = false;
                    foreach (var schedule in connections)
                    {
                        //modify current route
                        if (Inserted == false)
                        {
                            RoutesList[j].Add(schedule);
                            Inserted = true;
                        }
                        //add new route if already modified
                        else
                        {
                            List<Schedules> list = new List<Schedules>();
                            list.AddRange(RoutesList[j]);
                            list.Add(schedule);

                            RoutesList.Add(list);
                        }


                    }

                }


            }

            //Discard routes with wrong end point
            RoutesList = RoutesList.Where(l => l.Last().Routes.Airports1.IATACode == EndPoint).ToList();

            

            FlightRouteList = RoutesList.Select(l => new FlightRoute
            {

                IdList = l.Select(r => r.ID).ToList(),

                FromList = l.Select(r => r.Routes.Airports.IATACode).ToList(),

                ToList = l.Select(r => r.Routes.Airports1.IATACode).ToList(),

                Date = l.First().Date,

                Time = l.First().Time,

                FlightNumber = l.Select(r => r.FlightNumber).ToList(),

                EconomyPrice = l.Sum(r => r.EconomyPrice),

                NumberOfStops = l.Count() - 1


            }).ToList();


            

        }

    }


    public class FlightRoute
    {


        public List<int> IdList { get; set; }

        public List<string> FromList { get; set; }

        public List<string> ToList { get; set; }

        public DateTime Date { get; set; }

        public TimeSpan Time { get; set; }

        public List<string> FlightNumber { get; set; }

        public decimal EconomyPrice { get; set; }

        public int NumberOfStops { get; set; }

    }

    public class Flight
    {

        public int Id { get; set; }
        public string From { get; set; }

        public string To { get; set; }

        public DateTime Date { get; set; }

        public TimeSpan Time { get; set; }

        public string FlightNumber { get; set; }

        public decimal EconomyPrice { get; set; }
        public decimal BusinessPrice { get; set; }

        public decimal FirstClassPrice { get; set; }


        public int NumberOfStops { get; set; }

    }

}
