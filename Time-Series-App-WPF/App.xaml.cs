using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ScottPlot.WPF;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Time_Series_App_WPF.Context;
using Time_Series_App_WPF.Enums;
using Time_Series_App_WPF.Model;
using Time_Series_App_WPF.Services.Analysis;
using Time_Series_App_WPF.Services.Annotations;
using Time_Series_App_WPF.Services.Charts;
using Time_Series_App_WPF.Services.Files;
using Time_Series_App_WPF.View;
using Time_Series_App_WPF.ViewModel;

namespace Time_Series_App_WPF
{
    public partial class App : Application
    {
        public static IHost? AppHost { get; private set; }

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) => 
                {
                    services.AddSingleton<MainWindowViewModel>();
                    services.AddTransient<AnnotationWindowViewModel>();
                    services.AddTransient<AddEditAnnotationWindowViewModel>();
                    services.AddTransient<AnalyzeOptionsWindowViewModel>();
                    services.AddTransient<AnalyzeResultsWindowViewModel>();
                    services.AddSingleton<MainWindow>();
                    services.AddTransient<AnnotationWindow>();
                    services.AddTransient<AddAnnotationWindow>();
                    services.AddTransient<EditAnnotationWindow>();
                    services.AddTransient<ProgramInfoWindow>();
                    services.AddTransient<AnalyzeOptionsWindow>();
                    services.AddTransient<AnalyzeResultsWindow>();
                    services.AddSingleton<IFileService, FileService>();
                    services.AddSingleton<IChartService<SignalChartData>, SignalChartService>();
                    services.AddTransient<IAnnotationService, AnnotationService>();
                    services.AddTransient<IAnalysisService, AnalysisService>();
                    services.AddSingleton<IMessenger, WeakReferenceMessenger>();
                    services.AddSingleton<DataHolder<Annotation>>();
                    services.AddSingleton<DataHolder<AnalysisMethod>>();
                    services.AddSingleton<DataHolder<WpfPlot>>();
                    services.AddSingleton<DataHolder<int>>();
                    services.AddSingleton<ListDataHolder<Annotation>>();
                    services.AddSingleton<ListDataHolder<MadeAnnotation>>();
                    services.AddDbContext<ApplicationDbContext>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost!.StartAsync();

            var startupWindow = AppHost.Services.GetRequiredService<MainWindow>();
            startupWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost!.StopAsync();
            base.OnExit(e);
        }
    }
}
