using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Id3;
using Id3.Id3v2;
using Id3.Id3v2.v23;
using Prism.Commands;
using Prism.Mvvm;

namespace ID3App
{
	public class MainPageViewModel : BindableBase
	{
		private string _artist;

		public string Artist
		{
			get { return _artist; }
			set
			{
				_artist = value;
				OnPropertyChanged();
			}
		}

		private string _album;

		public string Album
		{
			get { return _album; }
			set
			{
				_album = value;
				OnPropertyChanged();
			}
		}

		private string _title;

		public string Title
		{
			get { return _title; }
			set
			{
				_title = value;
				OnPropertyChanged();
			}
		}

		private BitmapImage _albumCover;

		public BitmapImage AlbumCover
		{
			get { return _albumCover; }
			set
			{
				_albumCover = value;
				OnPropertyChanged();
			}
		}


		public MediaElement MediaElement { get; set; }
		public ICommand ClickCommand { get; set; }

		private async Task ClickExecute()
		{
			FileOpenPicker fileOpenPicker = new FileOpenPicker
			{
				SuggestedStartLocation = PickerLocationId.MusicLibrary,
			};
			fileOpenPicker.FileTypeFilter.Add(".mp3");
			var file = await fileOpenPicker.PickSingleFileAsync();
			var info = await file.Properties.GetMusicPropertiesAsync();

			var fileStream = file.OpenStreamForReadAsync();

			Artist = info.Artist;
			Title = info.Title;
			Album = info.Album;
			AlbumCover = await GetAlbumImage(await fileStream);

			var stream = await file.OpenAsync(FileAccessMode.Read);
			MediaElement.SetSource(stream, file.ContentType);

			MediaElement.Play();
		}

		private async void OnClick()
		{
			//			await Task.Run(() =>
			//			{
			await ClickExecute();
			if (Title == null)
				Title = "null";
			ShowMessage(Title);
			//			});
		}

		private async Task<BitmapImage> GetAlbumImage(Stream path)
		{
			BitmapImage bitmapImage = new BitmapImage(new Uri("ms-appx:///Image/DefaultImage.png", UriKind.RelativeOrAbsolute));
			var mp3 = new Mp3Stream(path);
			var tags = mp3.GetAllTags();
			var picture = tags[0].Pictures[0].PictureData;
			if (picture.Any())
			{
				using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
				{
					await stream.WriteAsync(picture.AsBuffer());
					stream.Seek(0);
					await bitmapImage.SetSourceAsync(stream);

				}
			}
			return bitmapImage;
		}

		private async void ShowMessage(string test)
		{
			MessageDialog messageDialog = new MessageDialog(test) { Title = "Message" };
			await messageDialog.ShowAsync();
		}

		public ICommand PlayCommand { get; set; }

		private async void PlayExecute()
		{
			var folderPicker = new FolderPicker
			{
				SuggestedStartLocation = PickerLocationId.ComputerFolder
			};
			folderPicker.FileTypeFilter.Add(".mp3");

			var folder = await folderPicker.PickSingleFolderAsync();
			var files=new List<string>();
			await GetAllFile(folder,files);

//			ShowMessage(Files.Count.ToString());

			Frame root = Window.Current.Content as Frame;
			root?.Navigate(typeof(SecondPage),files);
		}

		private static readonly List<string> Files = new List<string>();
		private static string _testStr = "test";

		private async Task GetAllFile(IStorageFolder storageFolder, List<string> files)
		{
			var items = await storageFolder.GetItemsAsync();
			foreach (var storageItem in items)
			{
				if (storageItem.IsOfType(StorageItemTypes.File))
				{
					var storageFile = storageItem as StorageFile;
					if (storageFile != null&&storageFile.FileType==".mp3") files.Add(storageFile.Name);
				}
				else if (storageItem.IsOfType(StorageItemTypes.Folder))
				{
					await GetAllFile(storageItem as StorageFolder,files);
				}
			}
			//			ShowMessage(Files.Count.ToString());
		}
		public async Task LoadSongs(string songListPath)
		{
			await Task.Run(async () =>
			{
				if (!Directory.Exists(songListPath)) return;
				foreach (var path in Directory.GetFileSystemEntries(songListPath))
				{
					if (File.Exists(path) &&
						(File.GetAttributes(path) & System.IO.FileAttributes.Hidden) != System.IO.FileAttributes.Hidden &&
						Path.GetExtension(path) == ".mp3")
					{
						Files.Add(path);
						_testStr = path;
					}
					else if (Directory.Exists(path) &&
							 (new DirectoryInfo(path).Attributes & System.IO.FileAttributes.Hidden) != System.IO.FileAttributes.Hidden)
					{
						await LoadSongs(path);
					}
				}
			});
		}

		public MainPageViewModel()
		{
			MediaElement = new MediaElement();
			ClickCommand = new DelegateCommand(OnClick);
			PlayCommand = new DelegateCommand(PlayExecute);
		}
	}
}
