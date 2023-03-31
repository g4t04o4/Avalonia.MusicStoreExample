using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using iTunesSearch.Library.Models;
using ReactiveUI;
using Album = MusicStore.Models.Album;

namespace MusicStore.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private bool _collectionEmpty;

    public MainWindowViewModel()
    {
        ShowDialog = new Interaction<MusicStoreViewModel, AlbumViewModel?>();

        BuyMusicCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var store = new MusicStoreViewModel();

            var result = await ShowDialog.Handle(store);

            if (result != null)
            {
                Albums.Add(result);

                await result.SaveToDiskAsync();
            }
        });

        this.WhenAnyValue(x => x.Albums.Count)
            .Subscribe(x => CollectionEmpty = x == 0);

        RxApp.MainThreadScheduler.Schedule(LoadAlbums);
    }

    private async void LoadAlbums()
    {
        //var albums = (await Album.LoadCachedAsync()).Select(x => new AlbumViewModel(x));

        var albums = await AlbumViewModel.LoadCached();

        foreach (var album in albums)
        {
            Albums.Add(album);
        }

        // foreach (var album in Albums.ToList())
        // {
        //     await album.LoadCover();
        // }

        LoadCovers();
    }

    private async void LoadCovers()
    {
        foreach (var album in Albums.ToList())
        {
            await album.LoadCover();
        }
    }

    public bool CollectionEmpty
    {
        get => _collectionEmpty;
        set => this.RaiseAndSetIfChanged(ref _collectionEmpty, value);
    }

    public ObservableCollection<AlbumViewModel> Albums { get; } = new();


    public ICommand BuyMusicCommand { get; }

    public Interaction<MusicStoreViewModel, AlbumViewModel?> ShowDialog { get; }
}