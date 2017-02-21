using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Xaml.Media;
using PureMusicPlayer.ViewModel;

namespace PureMusicPlayer.Model
{
	public class SongListOperator:DbOperator
	{
		private int _count;
		/// <summary> 通过目录读取器选择文件夹 </summary>
		/// <returns>选择的文件夹</returns>
		public async Task<StorageFolder> GetSongsFolder()
		{
			var folderPicker = new FolderPicker
			{
				SuggestedStartLocation = PickerLocationId.ComputerFolder
			};
			folderPicker.FileTypeFilter.Add(".mp3");

			var folder = await folderPicker.PickSingleFolderAsync();
			return folder;
		}
		/// <summary>
		/// 读取歌曲
		/// </summary>
		/// <param name="folder">歌曲目录</param>
		/// <returns>用于歌曲列表显示的歌曲集合</returns>
		public async Task<List<SongListItem>> LoadSongs(StorageFolder folder)
		{
			var songs = new List<Song>();
			await LoadSongsExecute(folder, songs);

			var songListItems = EditSongList(songs);
			DbConnect();
			InsertTable(songs);
			return songListItems;
		}

		/// <summary>
		/// 递归读取目录中的歌曲
		/// </summary>
		/// <param name="storageFolder">歌曲目录</param>
		/// <param name="listItems">歌曲集合</param>
		/// <param name="isCount"></param>
		/// <returns></returns>
		private async Task LoadSongsExecute(IStorageFolder storageFolder, List<Song> listItems ,bool isCount=false)
		{
			var items = await storageFolder.GetItemsAsync();
			foreach (var storageItem in items)
			{
				if (storageItem.IsOfType(StorageItemTypes.File))
				{
					var storageFile = storageItem as StorageFile;

					if (storageFile != null && storageFile.FileType == ".mp3")
					{
						if (!isCount)
						{
							var song = await Song.GetSong(storageFile);
							listItems.Add(song);
						}
						_count++;
					}
				}
				else if (storageItem.IsOfType(StorageItemTypes.Folder))
				{
					await LoadSongsExecute(storageItem as StorageFolder, listItems,isCount);
				}
			}
		}

		private async Task CountSongsExecute(IStorageFolder storageFolder, int count)
		{
			var items= await storageFolder.GetItemsAsync();
			foreach (var storageItem in items)
			{
				if (storageItem.IsOfType(StorageItemTypes.File))
				{
					var storageFile = storageItem as StorageFile;

					if (storageFile != null && storageFile.FileType == ".mp3")
					{
						count++;
					}
				}
				else if (storageItem.IsOfType(StorageItemTypes.Folder))
				{
					await CountSongsExecute(storageItem as StorageFolder, count);
				}
			}
		}
		/// <summary>
		/// 将歌曲集合封装为歌曲列表显示项
		/// </summary>
		/// <param name="songs">歌曲集合</param>
		/// <returns>歌曲列表显示项</returns>
		private List<SongListItem> EditSongList(List<Song> songs)
		{
			var songItemList = new List<SongListItem>();
			for (int i = 0; i < songs.Count; i++)
			{
				songItemList.Add(new SongListItem
				{
					Num = i + 1,
					Song = songs[i],
					ColorBrush = i % 2 == 1
						? new SolidColorBrush(Colors.White)
						: new SolidColorBrush(Colors.AliceBlue)
				});
			}
			return songItemList;
		}
		/// <summary>
		/// 从音乐库读取歌曲
		/// </summary>
		/// <returns>音乐库中的目录</returns>
		public async Task<List<StorageFolder>> InitialSongList()
		{
			var myMusic = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
			var musicFolders=myMusic.Folders.ToList();
			return musicFolders;
		}

		public async Task<int> MusicLibCount(List<StorageFolder> folders)
		{
			_count = 0;
			foreach (var storageFolder in folders)
			{
				await LoadSongsExecute(storageFolder, null, true);
			}
			return _count;
		}
		/// <summary>
		/// 从数据库读取歌曲
		/// </summary>
		/// <returns>歌曲列表显示项集合</returns>
		public List<SongListItem> LoadSongTable()
		{
			var songList = LoadTable();
			return EditSongList(songList);
		}

		public bool IsMusicLibChanged()
		{
			return false;
		}
	}
}
