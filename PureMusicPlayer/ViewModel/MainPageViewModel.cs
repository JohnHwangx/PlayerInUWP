using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage.AccessCache;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Prism.Commands;
using Prism.Mvvm;
using PureMusicPlayer.Model;

namespace PureMusicPlayer.ViewModel
{
	public class MainPageViewModel:BindableBase
	{
		private Song _playingSong;

		public Song PlayingSong
		{
			get { return _playingSong; }
			set
			{
				_playingSong = value; 
				OnPropertyChanged();
			}
		}
		
		private bool _isWaitting;

		public bool IsWaitting
		{
			get { return _isWaitting; }
			set
			{
				_isWaitting = value; 
				OnPropertyChanged();
			}
		}

		public SongListOperator SongListOperator { get; set; }
		private List<SongListItem> _disSongList;

		public List<SongListItem> DisSongList
		{
			get { return _disSongList; }
			set
			{
				_disSongList = value; 
				OnPropertyChanged();
				PlayingSong = _disSongList.FirstOrDefault().Song;
			}
		}

		public ICommand AddCommand { get; set; }

		private async void AddExecute()
		{
			try
			{
				var folder =await SongListOperator.GetSongsFolder();
				StorageApplicationPermissions.FutureAccessList.Add(folder);
				IsWaitting = true;
				DisSongList = await SongListOperator.LoadSongs(folder);
			}
			finally
			{
				IsWaitting = false;
			}
		}

		private async void InitialSongList()
		{
			try
			{
				//IsWaitting = true;
				//var folders = await SongListOperator.InitialSongList();
				//var tempList=new List<SongListItem>();
				//foreach (var folder in folders)
				//{
				//	StorageApplicationPermissions.FutureAccessList.Add(folder);
				//	var songListItems = await SongListOperator.LoadSongs(folder);
				//	tempList.AddRange(songListItems);
				//}
				//DisSongList = tempList;

				IsWaitting = true;
				var folders = await SongListOperator.InitialSongList();
				var count = await SongListOperator.MusicLibCount(folders);

				DisSongList = SongListOperator.LoadSongTable();
			}
			finally
			{
				IsWaitting = false;
			}
		}

		public MainPageViewModel()
		{
			SongListOperator=new SongListOperator();
			IsWaitting = false;
			InitialSongList();
			AddCommand=new DelegateCommand(AddExecute);
			ShowDialog = new DelegateCommand<object>(ShowMessage);
		}

		public DelegateCommand<object> ShowDialog { get; set; }
		private async void ShowMessage(object listBox)
		{
			DisSongList = new List<SongListItem>();
			var item = listBox as SongListItem;
			MessageDialog messageDialog = new MessageDialog(item.Song.Path) { Title = "Message" };
			await messageDialog.ShowAsync();
		}
	}

	public class SongListItem
	{
		public int Num { get; set; }
		public Song Song { get; set; }
		public SolidColorBrush ColorBrush { get; set; }
	}
}
