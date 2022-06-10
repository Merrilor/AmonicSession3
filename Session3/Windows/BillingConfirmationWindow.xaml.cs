using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace Session3
{
    /// <summary>
    /// Interaction logic for BillingConfirmationWindow.xaml
    /// </summary>
    public partial class BillingConfirmationWindow : Window
    {
        List<Flight> _FlightList;

        List<Passenger> _PassengerList;


        public BillingConfirmationWindow(List<Flight> flightList, List<Passenger> passengerList)
        {
            InitializeComponent();
            _FlightList = flightList;
            _PassengerList = passengerList;

            Session3Entities entities = new Session3Entities();
            decimal TotalPrice = 0;
            foreach (var item in _FlightList)
            {

                string CabinType = item.CabinType;
                decimal EconomyPrice = entities.Schedules.Where(s => s.ID == item.Id).Single().EconomyPrice;

                switch (CabinType)
                {
                    case "Economy":
                        TotalPrice += EconomyPrice;
                        break;
                    case "Business":
                        TotalPrice += EconomyPrice + (EconomyPrice / 100 * 35);
                        break;
                    case "FirstClass":
                        TotalPrice += EconomyPrice + (EconomyPrice / 100 * 35) + ((EconomyPrice + (EconomyPrice / 100 * 35)) / 100 * 30);
                        break;
                    default:
                        MessageBox.Show("Ошибка");
                        break;
                }

            }

            TotalAmountTextBox.Text = $" $ {TotalPrice * _PassengerList.Count}";

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void IssueTicketsButton_Click(object sender, RoutedEventArgs e)
        {
            Session3Entities entities = new Session3Entities();

            foreach (var passenger in _PassengerList)
            {

                foreach (var flight in _FlightList)
                {
                    int CabinTypeIndex;

                    switch (flight.CabinType)
                    {
                        case "Economy":
                            CabinTypeIndex = 1;
                            break;
                        case "Business":
                            CabinTypeIndex = 2;
                            break;
                        case "First Class":
                            CabinTypeIndex = 3;
                            break;
                        default:
                            MessageBox.Show("Cabin Type do not exist");
                            return;
                            
                    }

                    string BookingReference = passenger.Firstname[0].ToString().ToUpper() + (passenger.Lastname.Length <= 5 ? passenger.Lastname : passenger.Lastname.Substring(0,5)).ToUpper();
                    BookingReference = BookingReference.PadRight(6, '1');

                    string AmountOfRepeats = entities.Tickets.Where(t=>t.PassportNumber == passenger.PassportNumber).Count().ToString();

                    BookingReference = BookingReference.Remove(BookingReference.Length - AmountOfRepeats.Length -1);

                    BookingReference += AmountOfRepeats;

                    entities.Tickets.Add(new Tickets
                    {

                        UserID = 1,
                        ScheduleID = flight.Id,
                        CabinTypeID = CabinTypeIndex,
                        Firstname = passenger.Firstname,
                        Lastname = passenger.Lastname,
                        Phone = passenger.Phone,
                        PassportNumber = passenger.PassportNumber,
                        PassportCountryID = entities.Countries.Where(c => c.Name == passenger.PassportCountry).Single().ID,
                        BookingReference = BookingReference,
                        Confirmed = false

                    });

                    entities.SaveChanges();

                }
            }


            MessageBox.Show("Confirmation was successfull");
            this.Close();

        }
    }
}
