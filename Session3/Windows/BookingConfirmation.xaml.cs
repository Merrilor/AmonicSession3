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
using System.Windows.Shapes;

namespace Session3
{
    /// <summary>
    /// Interaction logic for BookingConfirmation.xaml
    /// </summary>
    /// 
    public partial class BookingConfirmation : Window
    {
        private FlightRoute _OutboundFlightRoute;
        private FlightRoute _ReturnFlightRoute;
        private int _CabinTypeIndex;

        private int _PassengerAmount;

        public List<Flight> OutboundFlightList { get; set; } = new List<Flight>();

        public List<Flight> ReturnFlightList { get; set; } = new List<Flight>();

        public BookingConfirmation(FlightRoute outboundFlightRoute, FlightRoute returnFlightRoute, int cabinTypeIndex, string passengerAmount)
        {
            InitializeComponent();
            _OutboundFlightRoute = outboundFlightRoute;
            _ReturnFlightRoute = returnFlightRoute;
            _CabinTypeIndex = cabinTypeIndex;
            _PassengerAmount = int.Parse(passengerAmount);

            OutboundFlightList.Clear();
            ReturnFlightList.Clear();

            SelectFlights();
            LoadValues();

            DataContext = this;
            OutboundFlightItemControl.ItemsSource = OutboundFlightList;
            ReturnFlightItemControl.ItemsSource = ReturnFlightList;

            if (ReturnFlightItemControl.Items.Count == 0)
            {
                ReturnFlightRow.Height = new GridLength(0);
                ReturnFlightHeaderRow.Height = new GridLength(0);
            }

        }

        private void SelectFlights()
        {
            var entities = new Session3Entities();

            foreach (var id in _OutboundFlightRoute.IdList)
            {
                Schedules schedule = entities.Schedules.Where(s => s.ID == id).Single();

                string Cabin = entities.CabinTypes.Where(ct => ct.ID == _CabinTypeIndex + 1).Single().Name;

                OutboundFlightList.Add(new Flight
                {
                    Id = id,
                    From = schedule.Routes.Airports.IATACode,
                    To = schedule.Routes.Airports1.IATACode,
                    CabinType = entities.CabinTypes.Where(ct => ct.ID == _CabinTypeIndex + 1).Single().Name,
                    Date = schedule.Date,
                    FlightNumber = schedule.FlightNumber

                }) ;
            }

            if(_ReturnFlightRoute is null)
            {
                ReturnDataGridHeader.Visibility = Visibility.Collapsed;
                ReturnFlightItemControl.Visibility = Visibility.Collapsed;
                return;
            }

            foreach (var id in _ReturnFlightRoute.IdList)
            {
                Schedules schedule = entities.Schedules.Where(s => s.ID == id).Single();

                ReturnFlightList.Add(new Flight
                {
                    Id = id,
                    From = schedule.Routes.Airports.IATACode,
                    To = schedule.Routes.Airports1.IATACode,
                    CabinType = entities.CabinTypes.Where(ct => ct.ID == _CabinTypeIndex + 1).Single().Name,
                    Date = schedule.Date,
                    FlightNumber = schedule.FlightNumber

                });


            }

        }


        private void LoadValues()
        {
            Session3Entities entities = new Session3Entities();

            PassportCountryComboBox.ItemsSource = entities.Countries.Select(c => c.Name).ToList();

        }


        private void DigitTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            char character = (char)KeyInterop.VirtualKeyFromKey(e.Key);

            if (!char.IsDigit(character) && e.Key != Key.Back)
                e.Handled = true;

        }

        private void AddPassengerButton_Click(object sender, RoutedEventArgs e)
        {

            if (FirstNameTextBox.Text == String.Empty || LastNameTextBox.Text == String.Empty || PhoneTextBox.Text == String.Empty || PassportNumberTextBox.Text == String.Empty)
            {
                MessageBox.Show("Введены не все данные", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if(BirthdateDatePicker.SelectedDate > DateTime.Now.Date)
            {
                MessageBox.Show("День рождения должен быть раньше текущей даты", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (PassengerDataGrid.Items.Count == _PassengerAmount)
            {
                MessageBox.Show("Данные для всех пассажиров уже введены", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }



            Passenger NewPassenger = new Passenger
            {
                Firstname = FirstNameTextBox.Text,
                Lastname = LastNameTextBox.Text,
                Birthdate = BirthdateDatePicker.SelectedDate.Value.Date,
                PassportNumber = PassportNumberTextBox.Text,
                PassportCountry = PassportCountryComboBox.Text,
                Phone = PhoneTextBox.Text
                        
            
            };


            

            PassengerDataGrid.Items.Add(NewPassenger);

        }

        private void RemovePassengerButton_Click(object sender, RoutedEventArgs e)
        {

            if(PassengerDataGrid.SelectedItem != null)
            {
                PassengerDataGrid.Items.Remove(PassengerDataGrid.SelectedItem);
            }


        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
           
            this.Close();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if(PassengerDataGrid.Items.Count < _PassengerAmount)
            {
                MessageBox.Show("Введены данные не для всех пассажиров", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            List<Flight> AllFlightList = OutboundFlightList;

            if(ReturnFlightList.Count > 0)
            {
                AllFlightList.AddRange(ReturnFlightList);
            }


            var newWindow = new BillingConfirmationWindow(AllFlightList, PassengerDataGrid.Items.Cast<Passenger>().ToList());
            newWindow.ShowDialog();


            //TODO pass all flights list and passenger list to next window and generate unique code in the next window

        }
    }

    public class Flight
    {

        public int Id { get; set; }

        public string From { get; set; }
        public string To { get; set; }

        public string CabinType { get; set; }

        public DateTime Date { get; set; }

        public string FlightNumber { get; set; }

    }

    public class Passenger
    {

        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public DateTime Birthdate { get; set; }

        public string PassportNumber { get; set; }

        public string PassportCountry { get; set; }

        public string Phone { get; set; }

    }

}
