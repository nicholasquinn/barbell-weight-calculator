using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace BarbellWeightCalculator
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        // I know this is a dependency on a concrete type, it's fine for this project scope
        private PersistenceService _persistenceService;

        private WeightConfiguration _weightConfiguration;

        public List<Tuple<string, uint>> Plates { get; set; } = new();

        private string _targetWeight = string.Empty;

        public string TargetWeight 
        { 
            get => _targetWeight;
            set
            {
                _targetWeight = value;
                NotifyPropertyChanged();
            }
        }

        private string _barbellWeight = string.Empty;

        public string BarbellWeight
        {
            get => _barbellWeight;
            set
            {
                _barbellWeight = value;
                NotifyPropertyChanged();
            }
        }

        private string _resultText 
            = "Enter a target weight and barbell weight, then click calculate...";
        
        public string ResultText {
            get => _resultText;
            set
            {
                _resultText = value;
                NotifyPropertyChanged();
            }
        }

        public MainWindowViewModel(PersistenceService persistenceService)
        {
            _persistenceService = persistenceService;
            _weightConfiguration = persistenceService.ReadWeightConfiguration();

            TargetWeight = _weightConfiguration.TargetWeight.ToString();
            BarbellWeight = _weightConfiguration.BarbellWeight.ToString();

            string massUnitSuffix = _weightConfiguration.IsMetric ? "kg" : "lbs";
            Plates = _weightConfiguration.PlatesPerSide.Select(
                kv => new Tuple<string, uint>($"{kv.Key}{massUnitSuffix}", kv.Value)
            ).ToList();

        }

        public void OnCalculateClicked()
        {
            // try turn the user input string into a double, reverting to 0 on failure
            double.TryParse(TargetWeight, out var targetWeight);
            _weightConfiguration.TargetWeight = targetWeight;
            TargetWeight = targetWeight.ToString();

            // do the same for barbell weight
            double.TryParse(BarbellWeight, out var barbellWeight);
            _weightConfiguration.BarbellWeight = barbellWeight;
            BarbellWeight = barbellWeight.ToString();

            // calculate the solution and set the result message accordingly
            bool canMakeWeight = _weightConfiguration.TryCalculatePlateSet(out var plateSet);

            string message = string.Empty;
            if (canMakeWeight)
            {
                if (plateSet.Count == 0)
                {
                    message = $"You can make {targetWeight} by using just the bar.";
                }
                else
                {
                    message = $"You can make {TargetWeight} by loading the following " +
                        $"plates on each side of the barbell: ";
                    foreach (var kvp in plateSet.Reverse())
                    {
                        message += $"{kvp.Key} x {kvp.Value}, ";
                    }
                    // trim last ", " from string
                    message = message.Remove(message.Length - 2);
                }
            }
            else
            {
                message = $"You cannot make {TargetWeight} with this barbell and plate set.";
            }

            ResultText = message;

            // we save every time a calculation is done
            _persistenceService.WriteWeightConfiguration(_weightConfiguration);
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
