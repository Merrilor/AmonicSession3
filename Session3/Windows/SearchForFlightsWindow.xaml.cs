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

        //TODO: Cleanup, ReturnDataGrid

        public List<FlightRoute> OutboundRouteList { get; set; }

        public List<FlightRoute> ReturnRouteList { get; set; }

        
        public string CurrentFromFilter = "AUH";

        public string CurrentToFilter = "AUH";

        public DateTime CurrentOutboundDate;

        public DateTime CurrentReturnDate;

        public int CurrentCabinTypeIndex;

        

        public SearchForFlightsWindow()
        {
            LoadValues();
            InitializeComponent();
            DataContext = this;          
        }

        public void LoadValues()
        {

            var entities = new Session3Entities();

            AirportList = entities.Airports.Select(ai => ai.IATACode).ToList();
            CabinTypeList = entities.CabinTypes.Select(ct => ct.Name).ToList();

        }

       

        private void ChangeColumnVisibility()
        {
            EconomyPriceColumn.Visibility = Visibility.Collapsed;
            BusinessPriceColumn.Visibility = Visibility.Collapsed;
            FirstClassPriceColumn.Visibility = Visibility.Collapsed;
            EconomyPriceColumnReturn.Visibility = Visibility.Collapsed;
            BusinessPriceColumnReturn.Visibility = Visibility.Collapsed;
            FirstClassPriceColumnReturn.Visibility = Visibility.Collapsed;

            switch (CurrentCabinTypeIndex)
            {

                case 0:
                    EconomyPriceColumn.Visibility = Visibility.Visible;
                    EconomyPriceColumnReturn.Visibility = Visibility.Visible;
                    break;
                case 1:
                    BusinessPriceColumn.Visibility = Visibility.Visible;
                    BusinessPriceColumnReturn.Visibility = Visibility.Visible;
                    break;
                case 2:
                    FirstClassPriceColumn.Visibility = Visibility.Visible;
                    FirstClassPriceColumnReturn.Visibility = Visibility.Visible;
                    break;
                default:
                    MessageBox.Show("Ошибка");
                    break;


            }


        }

       
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (OutboundTextBox.Text == string.Empty || (ReturnTextBox.Text == string.Empty && (bool)BookingTypeReturn.IsChecked))
            {
                MessageBox.Show("Введены не все значения");
                return;
            }

            CurrentCabinTypeIndex = CabinTypeComboBox.SelectedIndex;

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

                ReturnRouteList = SelectRoutes(CurrentToFilter, CurrentFromFilter,CurrentReturnDate);


                ReturnDataGrid.ItemsSource = ReturnCheckBox.IsChecked == false ?
                    SelectRoutes(ToComboBox.Text, FromComboBox.Text, CurrentReturnDate).Where(f=>f.Date == CurrentReturnDate)
                    : ReturnRouteList;

            }


            OutboundRouteList = SelectRoutes(CurrentFromFilter, CurrentToFilter, CurrentOutboundDate);


            OutboundDataGrid.ItemsSource = OutboundCheckBox.IsChecked == false ?
                OutboundRouteList.Where(f => f.Date == CurrentOutboundDate)
                : OutboundRouteList;



            
            
            

        }

       

        private void OutboundCheckBox_Checked(object sender, RoutedEventArgs e) => OutboundDataGrid.ItemsSource = OutboundRouteList;
        private void OutboundCheckBox_Unchecked(object sender, RoutedEventArgs e) => OutboundDataGrid.ItemsSource = OutboundRouteList is null ? null : OutboundRouteList.Where(f => f.Date == CurrentOutboundDate);
        private void ReturnCheckBox_Checked(object sender, RoutedEventArgs e) => ReturnDataGrid.ItemsSource = ReturnRouteList;
        private void ReturnCheckBox_Unchecked(object sender, RoutedEventArgs e) => ReturnDataGrid.ItemsSource = ReturnRouteList is null ? null : ReturnRouteList.Where(f => f.Date == CurrentReturnDate);
        private void CloseButton_Click(object sender, RoutedEventArgs e) => this.Close();
        


        private List<FlightRoute> SelectRoutes(string startPoint, string endPoint, DateTime outboundDate)
        {


            Session3Entities entities = new Session3Entities();
            List<List<Schedules>> RoutesList = new List<List<Schedules>>();

            string StartPoint = startPoint;
            string EndPoint = endPoint;
            DateTime OutboundDate = outboundDate;

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
                        .Where(s => s.Routes.Airports.IATACode == lastPoint.Routes.Airports1.IATACode && s.Date == lastPoint.Date && s.Routes.Airports1.IATACode != lastPoint.Routes.Airports.IATACode).ToList();

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

            return RoutesList.Select(l => new FlightRoute
            {

                IdList = l.Select(r => r.ID).ToList(),
                FromList = l.Select(r => r.Routes.Airports.IATACode).ToList(),
                ToList = l.Select(r => r.Routes.Airports1.IATACode).ToList(),
                Date = l.First().Date,
                Time = l.First().Time,
                FlightNumber = l.Select(r => r.FlightNumber).ToList(),
                EconomyPrice = l.Last().EconomyPrice,
                BusinessPrice = l.Last().EconomyPrice + (l.Last().EconomyPrice / 100 * 35),
                FirstClassPrice = (l.Last().EconomyPrice + (l.Last().EconomyPrice / 100 * 35)) + ((l.Last().EconomyPrice + (l.Last().EconomyPrice / 100 * 35)) / 100 * 30),
                NumberOfStops = l.Count() - 1,
                DisplayDestination = l.Last().Routes.Airports1.IATACode,
                DisplayFlightNumber = string.Join(" ] - [ ", l.Select(r => r.FlightNumber)),

            }).ToList();

            


        }

        private void BookFlightButton_Click(object sender, RoutedEventArgs e)
        {
            if(PassengerAmountTextBox.Text == string.Empty)
            {
                MessageBox.Show("Не введено количество пассажиров","Ошибка",MessageBoxButton.OK,MessageBoxImage.Error);
                return;
            }
            
            if(OutboundDataGrid.SelectedItem is null)
            {
                MessageBox.Show("Не выбран вариант вылета", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if(BookingTypeOneWay.IsChecked != true && ReturnDataGrid.SelectedItem is null)
            {
                MessageBox.Show("Не выбран вариант возвращения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            FlightRoute SelectedOutboundRoute = (FlightRoute)OutboundDataGrid.SelectedItem;
            FlightRoute SelectedReturnRoute = BookingTypeOneWay.IsChecked == true ? null : (FlightRoute)ReturnDataGrid.SelectedItem;

            

            Session3Entities entities = new Session3Entities();

            foreach (var id in SelectedOutboundRoute.IdList)
            {
                int SeatsTaken = entities.Tickets
                    .Where(ticket => ticket.ScheduleID == id && ticket.CabinTypeID == CurrentCabinTypeIndex + 1)
                    .Count();

                int AvailableSeats;

                switch (CurrentCabinTypeIndex)
                {
                    case 0:
                        AvailableSeats = entities.Schedules.Where(s => s.ID == id).Single().Aircrafts.EconomySeats;
                        break;
                    case 1:
                        AvailableSeats = entities.Schedules.Where(s => s.ID == id).Single().Aircrafts.BusinessSeats;
                        break;
                    case 2:
                        Aircrafts aircraft = entities.Schedules.Where(s => s.ID == id).Single().Aircrafts;
                        AvailableSeats = aircraft.TotalSeats - aircraft.EconomySeats - aircraft.BusinessSeats;
                        break;
                    default:
                        MessageBox.Show("Ошибка БД");
                        return;

                }

                if(AvailableSeats - (int.Parse(PassengerAmountTextBox.Text) + SeatsTaken) <= 0)
                {
                    MessageBox.Show("На одном из выбранных маршрутов не хватает свободных мест", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

            }

            

            var nextWindow = new BookingConfirmation((FlightRoute)OutboundDataGrid.SelectedItem, (FlightRoute)ReturnDataGrid.SelectedItem, CurrentCabinTypeIndex,PassengerAmountTextBox.Text);
            nextWindow.ShowDialog();
           

        }

        private void DigitTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            char character = (char)KeyInterop.VirtualKeyFromKey(e.Key);

            if (!char.IsDigit(character) && e.Key != Key.Back)
                e.Handled = true;

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

        public decimal BusinessPrice { get; set; }

        public decimal FirstClassPrice { get; set; }

        public int NumberOfStops { get; set; }


        public string DisplayDestination { get; set; }

        public string DisplayFlightNumber { get; set; }

        

    }

   

}
