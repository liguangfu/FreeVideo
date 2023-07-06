using System;
using System.Collections.Generic;
using System.Linq;
using FreeVideo.Services;
using FreeVideo.ViewModels;

namespace FreeVideo
{
    public class ServiceLocator
    {
        private IServiceProvider _serviceProvider;

        public MainViewModel MainViewModel => _serviceProvider.GetService<MainViewModel>();
        public PlayVideoViewModel PlayVideoViewModel => _serviceProvider.GetService<PlayVideoViewModel>();
        public ShowVideoModel ShowVideoModel => _serviceProvider.GetService<ShowVideoModel>();

        public IHistoryVideoService HistoryVideoService => _serviceProvider.GetService<IHistoryVideoService>();
        public ISearchVideoServiceFactory SearchVideoServiceFactory => _serviceProvider.GetService<ISearchVideoServiceFactory>();

        public ServiceLocator()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<IHistoryVideoService, HistoryVideoService>();
            serviceCollection.AddSingleton<ISearchVideoServiceFactory, SearchVideoServiceFactory>();

            serviceCollection.AddSingleton<MainViewModel>();
            serviceCollection.AddSingleton<PlayVideoViewModel>();
            serviceCollection.AddSingleton<ShowVideoModel>();


            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

    }
}
