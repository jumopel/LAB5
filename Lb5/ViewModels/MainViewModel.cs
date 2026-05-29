using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using HorseRacingSimulation.Models;

namespace HorseRacingSimulation.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private double _balance;
        private Horse _selectedHorseToBet;
        private double _betAmount;

        public ObservableCollection<Horse> Horses { get; set; }

        public double Balance
        {
            get => _balance;
            set
            {
                _balance = value;
                OnPropertyChanged();
            }
        }

        public double BetAmount
        {
            get => _betAmount;
            set
            {
                _betAmount = value;
                OnPropertyChanged();
            }
        }

        public Horse SelectedHorseToBet
        {
            get => _selectedHorseToBet;
            set
            {
                _selectedHorseToBet = value;
                OnPropertyChanged();
            }
        }

        public ICommand StartSimulationCommand { get; }
        public ICommand PlaceBetCommand { get; }

        public MainViewModel()
        {
            Balance = 250; 
            BetAmount = 20; 

            InitializeHorses();

            StartSimulationCommand = new RelayCommand(StartSimulation, CanStartSimulation);
            PlaceBetCommand = new RelayCommand(PlaceBet, CanPlaceBet);
        }

        private void InitializeHorses()
        {
            Horses = new ObservableCollection<Horse>
            {
                new Horse("1. Lucky", Colors.Red),
                new Horse("2. Ranger", Colors.Green),
                new Horse("3. Willow", Colors.Blue),
                new Horse("4. Tucker", Colors.Orange)
            };
        }

        private void PlaceBet(object parameter)
        {
            if (Balance >= BetAmount && SelectedHorseToBet != null)
            {
                Balance -= BetAmount;
                SelectedHorseToBet.MoneyBet += BetAmount;
            }
        }

        private bool CanPlaceBet(object parameter)
        {
            return Balance >= BetAmount && SelectedHorseToBet != null;
        }

        private void StartSimulation(object parameter)
        {
        }

        private bool CanStartSimulation(object parameter)
        {
            return true;
        }
    }
}