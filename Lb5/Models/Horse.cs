using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace HorseRacingSimulation.Models
{
    public class Horse : INotifyPropertyChanged
    {
        private static readonly Random _random = new Random();
        private int _positionX;
        private TimeSpan _raceTime;
        private int _currentFrameIndex;
        private double _coefficient;
        private double _moneyBet; 

        public string Name { get; private set; }
        public Color Color { get; private set; }

        public double BaseSpeed { get;  set; }

        public double Acceleration { get; set; }
       
        public double Coefficient
        {
            get => _coefficient;
            set
            {
                _coefficient = value;
                OnPropertyChanged();
            }
        }
        public double MoneyBet
        {
            get => _moneyBet;
            set { _moneyBet = value; OnPropertyChanged(); }
        }

        public int PositionX
        {
            get => _positionX;
            set
            {
                _positionX = value;
                OnPropertyChanged();
            }
        }

        public TimeSpan RaceTime
        {
            get => _raceTime;
            set
            {
                _raceTime = value;
                OnPropertyChanged();
            }
        }

        public int CurrentFrameIndex
        {
            get => _currentFrameIndex;
            set
            {
                _currentFrameIndex = value;
                OnPropertyChanged();
            }
        }

        public bool IsFinished { get; set; }

        public Horse(string name, Color color, double defaultCoefficient = 1.25)
        {
            Name = name;
            Color = color;
            _coefficient = defaultCoefficient;
            BaseSpeed = Random.Shared.Next(7, 12);
            PositionX = 0;
            CurrentFrameIndex = 0;
            RaceTime = TimeSpan.Zero;
            IsFinished = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}