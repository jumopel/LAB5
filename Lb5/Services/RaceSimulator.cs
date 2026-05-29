using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using HorseRacingSimulation.Models;

namespace HorseRacingSimulation.Services
{
    public class RaceSimulator
    {
        private readonly ObservableCollection<Horse> _horses;
        private readonly Random _random = new Random();
        private bool _isRacing;

        public event Action OnRenderNeeded;
        public event Action OnRaceFinished;

        public RaceSimulator(ObservableCollection<Horse> horses)
        {
            _horses = horses;
        }
        public async Task StartRaceAsync(int trackLength)
        {
            _isRacing = true;

            foreach (var horse in _horses)
            {
                horse.PositionX = 0;
                horse.RaceTime = TimeSpan.Zero;
                horse.IsFinished = false;
                horse.CurrentFrameIndex = 0;
                horse.BaseSpeed = Random.Shared.Next(7, 12);
            }

            DateTime startTime = DateTime.Now;

            while (_isRacing && _horses.Any(h => !h.IsFinished))
            {
                List<Task> tasks = new List<Task>();
                foreach (var horse in _horses)
                {
                    if (!horse.IsFinished)
                    {
                       tasks.Add(ChangeAccelerationAsync(horse, trackLength, startTime));
                    }
                }

                await Task.WhenAll(tasks);
                OnRenderNeeded?.Invoke();
                await Task.Delay(33); 
            }

            _isRacing = false;
            OnRaceFinished?.Invoke();
        }

        private async Task ChangeAccelerationAsync(Horse horse, int trackLength, DateTime startTime)
        {
            await Task.Run(() =>
            {
                double randomFactor = 0.6 + (Random.Shared.NextDouble() * 0.4);
                horse.Acceleration = horse.BaseSpeed * randomFactor;
                horse.PositionX += (int)horse.Acceleration;
                horse.CurrentFrameIndex = (horse.CurrentFrameIndex + 1) % 12;
                if (horse.PositionX >= trackLength)
                {
                    horse.PositionX = trackLength;
                    horse.IsFinished = true;
                    horse.RaceTime = DateTime.Now - startTime;
                }
            });
        }
    }
}
