using HorseRacingSimulation.Models;
using HorseRacingSimulation.Services;
using HorseRacingSimulation.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HorseRacingSimulation.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private double _balance;
        private Horse _selectedHorseToBet;
        private string _betAmount = "20";
        private ImageSource _renderedFrame;
        private bool _isSimulationRunning;

        private RaceSimulator _raceSimulator;
        private readonly RenderService _renderService;

        private Dictionary<Color, List<ImageSource>> _horseAnimations;
        private BitmapImage _backgroundImage;

        private ObservableCollection<Horse> _horses;
        public ObservableCollection<Horse> Horses
        {
            get => _horses;
            set { _horses = value; OnPropertyChanged(); }
        }

        private int _selectedHorseCount;
        public ObservableCollection<int> AvailableHorseCounts { get; } = new ObservableCollection<int> { 2, 3, 4 };

        public int SelectedHorseCount
        {
            get => _selectedHorseCount;
            set
            {
                if (_selectedHorseCount != value)
                {
                    _selectedHorseCount = value;
                    OnPropertyChanged();
                    UpdateHorseCount();
                }
            }
        }

        public double Balance
        {
            get => _balance;
            set { _balance = value; OnPropertyChanged(); }
        }

        public string BetAmount
        {
            get => _betAmount;
            set
            {
                string allowedChars = new string(value.Where(c => char.IsDigit(c) || c == '.').ToArray());

                int firstDot = allowedChars.IndexOf('.');
                if (firstDot != -1)
                {
                    string beforeDot = allowedChars.Substring(0, firstDot + 1);
                    string afterDot = allowedChars.Substring(firstDot + 1).Replace(".", "");
                    allowedChars = beforeDot + afterDot;
                }

                _betAmount = allowedChars;
                OnPropertyChanged();
            }
        }

        public Horse SelectedHorseToBet
        {
            get => _selectedHorseToBet;
            set { _selectedHorseToBet = value; OnPropertyChanged(); }
        }

        public ImageSource RenderedFrame
        {
            get => _renderedFrame;
            set { _renderedFrame = value; OnPropertyChanged(); }
        }

        public bool IsSimulationRunning
        {
            get => _isSimulationRunning;
            set
            {
                _isSimulationRunning = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotSimulationRunning)); 
            }
        }

        public bool IsNotSimulationRunning => !IsSimulationRunning;

        public ICommand StartSimulationCommand { get; }
        public ICommand PlaceBetCommand { get; }

        public MainViewModel()
        {
            Balance = 250;
            BetAmount = "20";
            _renderService = new RenderService();
            StartSimulationCommand = new RelayCommand(StartSimulation, CanStartSimulation);
            PlaceBetCommand = new RelayCommand(PlaceBet, CanPlaceBet);
            _selectedHorseCount = 4;
            UpdateHorseCount();
        }

        private void UpdateHorseCount()
        {
            var allColors = new[] { Colors.Red, Colors.Green, Colors.Blue, Colors.Orange };
            var allNames = new[] { "1. Lucky", "2. Ranger", "3. Willow", "4. Tucker" };

            var newHorses = new ObservableCollection<Horse>();
            for (int i = 0; i < SelectedHorseCount; i++)
            {
                newHorses.Add(new Horse(allNames[i], allColors[i], Math.Round(1.1 + (i * 0.3), 2)));
            }

            Horses = newHorses;
            SelectedHorseToBet = null;

            if (_raceSimulator != null)
            {
                _raceSimulator.OnRenderNeeded -= UpdateFrame;
                _raceSimulator.OnRaceFinished -= HandleRaceFinish;
            }

            _raceSimulator = new RaceSimulator(Horses);
            _raceSimulator.OnRenderNeeded += UpdateFrame;
            _raceSimulator.OnRaceFinished += HandleRaceFinish;

            if (_renderService != null)
            {
                LoadGraphics();
                UpdateFrame();
            }
        }

        private void LoadGraphics()
        {
            _horseAnimations = new Dictionary<Color, List<ImageSource>>();
            foreach (var horse in Horses)
            {
                _horseAnimations[horse.Color] = _renderService.GetHorseAnimation(horse.Color);
            }
            _backgroundImage = new BitmapImage(new Uri("pack://application:,,,/Images/Track.png"));
        }

        private void PlaceBet(object parameter)
        {
            if (double.TryParse(BetAmount, out double currentBet))
            {
                if (Balance >= currentBet && SelectedHorseToBet != null)
                {
                    Balance -= currentBet;
                    SelectedHorseToBet.MoneyBet += currentBet;
                }
            }
        }

        private bool CanPlaceBet(object parameter)
        {
            if (double.TryParse(BetAmount, out double currentBet))
            {
                return currentBet > 0 && Balance >= currentBet && SelectedHorseToBet != null && !IsSimulationRunning;
            }
            return false;
        }
        private async void StartSimulation(object parameter)
        {
            IsSimulationRunning = true;
            CommandManager.InvalidateRequerySuggested(); 

            await _raceSimulator.StartRaceAsync(700);
        }

        private bool CanStartSimulation(object parameter) => !IsSimulationRunning;

        private void UpdateFrame()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var bitmap = new RenderTargetBitmap(800, 400, 96, 96, PixelFormats.Pbgra32);
                var drawingVisual = new DrawingVisual();

                using (DrawingContext dc = drawingVisual.RenderOpen())
                {
                    dc.DrawImage(_backgroundImage, new Rect(0, 0, 800, 400));

                    for (int i = 0; i < Horses.Count; i++)
                    {
                        var horse = Horses[i];
                        var frame = _horseAnimations[horse.Color][horse.CurrentFrameIndex];
                        double stepY = 320.0 / Horses.Count;
                        double yPosition = 20 + (i * stepY);
                        dc.DrawImage(frame, new Rect(horse.PositionX, yPosition, 100, 100));
                    }
                }

                bitmap.Render(drawingVisual);
                RenderedFrame = bitmap;
            });
        }

        private void HandleRaceFinish()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                IsSimulationRunning = false;
                CommandManager.InvalidateRequerySuggested();

                var sortedHorses = Horses.OrderBy(h => h.RaceTime).ToList();
                var winner = sortedHorses.First();

                if (winner.MoneyBet > 0)
                {
                    double winAmount = winner.MoneyBet * winner.Coefficient;
                    Balance += winAmount;
                    MessageBox.Show($"Вітаємо! Ваш кінь {winner.Name} переміг!\nВиграш: {winAmount}$", "Перемога");
                }
                else if (Horses.Any(h => h.MoneyBet > 0))
                {
                    MessageBox.Show($"На жаль, ваші ставки програли. Переміг {winner.Name}.", "Поразка");
                }

                for (int i = 0; i < sortedHorses.Count; i++)
                {
                    sortedHorses[i].MoneyBet = 0;
                    sortedHorses[i].Coefficient = Math.Round(1.1 + (i * 0.3), 2);
                }
            });
        }
    }
}