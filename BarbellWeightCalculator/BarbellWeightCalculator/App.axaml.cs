using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;

namespace BarbellWeightCalculator
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var persistenceService = new PersistenceService(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "barbell-weight-calculator-data.json"
                    );
                desktop.MainWindow = new MainWindow()
                {
                     DataContext = new MainWindowViewModel(persistenceService)
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}