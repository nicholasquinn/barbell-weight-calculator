using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BarbellWeightCalculator
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        // I know this is a dependency on a concrete type, it's fine for this project scope
        private PersistenceService _persistenceService;

        private WeightConfiguration _weightConfiguration;

        private string _resultText = "This text comes from the view model";
        
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
            Task.Run(async () =>
            {
                await Task.Delay(2000);
                ResultText = $"weight config has tw={_weightConfiguration.TargetWeight}, " +
                    $"bw={_weightConfiguration.BarbellWeight}, " +
                    $"metric={_weightConfiguration.IsMetric}";
            });
            
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
