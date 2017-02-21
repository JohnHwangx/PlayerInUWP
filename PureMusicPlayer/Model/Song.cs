using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Id3;
using SQLite.Net.Attributes;

namespace PureMusicPlayer.Model
{
	//[Table("MusicList")]
	public class Song
	{
		/// <summary>
		/// 歌曲路径
		/// </summary>
		//[Column("path")]
		//[PrimaryKey]
		public string Path { get; set; }

		/// <summary>
		/// 歌曲名称
		/// </summary>
		//		[Column("title")]
		public string Title { get; set; }

		/// <summary>
		/// 作者
		/// </summary>
		//		[Column("artist")]
		public string Artist { get; set; }
		/// <summary>
		/// 专辑
		/// </summary>
		//		[Column("album")]
		public string Album { get; set; }
		/// <summary>
		/// 时长
		/// </summary>
		//		[Column("duration")]
		public TimeSpan Duration { get; set; }
		/// <summary>
		/// 标签
		/// </summary>
		public List<string> Tags { get; set; }
		/// <summary>
		/// 歌曲封面
		/// </summary>
		public BitmapImage AlbumCover { get; set; }

		private static Song _song = null;

		private Song()
		{

		}

		public static async Task<Song> GetSong(StorageFile file)
		{
			if (_song == null || _song.Path != file.Path)
			{
				var properties = await GetProperty(file);
				_song = new Song
				{
					Path = file.Path,
					Title = properties.Item1,
					Artist = properties.Item2,
					Album = properties.Item3,
					Duration = properties.Item4,
					AlbumCover = properties.Item5,
					Tags = new List<string>()
				};

			}
			return _song;
		}

		public static Song GetSong(SongListData songListData)
		{
			_song = new Song
			{
				Path = songListData.Path,
				Title = songListData.Title,
				Artist = songListData.Artist,
				Album = songListData.Album,
				Duration = songListData.Duration
			};
			return _song;
		}

		public static async Task<Tuple<string, string, string, TimeSpan, BitmapImage>> GetProperty(StorageFile storageFile/*file*/)
		{
			var file = await StorageFile.GetFileFromPathAsync(storageFile.Path);//Access Denied Error
																				//PathToStream(file.Path);
			var info = await file.Properties.GetMusicPropertiesAsync();
			var fileStream = file.OpenStreamForReadAsync();

			var albumCover = await GetAlbumImage(await fileStream);
			var title = info.Title;
			var artist = info.Artist;
			var album = info.Album;
			var duration = info.Duration;
			if (title == string.Empty)
			{
				title = file.DisplayName;
			}
			if (artist == string.Empty)
			{
				artist = "未知作者";
			}
			if (album == string.Empty)
			{
				album = "未知专辑";
			}
			if (duration == TimeSpan.Zero)
			{
				return null;
			}

			Tuple<string, string, string, TimeSpan, BitmapImage> properties
				= new Tuple<string, string, string, TimeSpan, BitmapImage>(title, artist, album, info.Duration, albumCover);
			return properties;
		}

		public static async Task<Tuple<string, string, string, TimeSpan, BitmapImage>> GetProperty(string filePath)
		{
			var file = await StorageFile.GetFileFromPathAsync(filePath);
			var info = await file.Properties.GetMusicPropertiesAsync();
			var fileStream = file.OpenStreamForReadAsync();

			var albumCover = await GetAlbumImage(await fileStream);
			//var albumCover = new BitmapImage();

			Tuple<string, string, string, TimeSpan, BitmapImage> properties
				= new Tuple<string, string, string, TimeSpan, BitmapImage>(info.Title, info.Artist, info.Album, info.Duration, albumCover);
			return properties;
		}

		private static async Task<BitmapImage> GetAlbumImage(Stream path)
		{
			BitmapImage bitmapImage = new BitmapImage(new Uri("ms-appx:///Image/DefaultImage.png", UriKind.RelativeOrAbsolute));
			var mp3 = new Mp3Stream(path);
			var tags = mp3.GetAllTags();
			var pictures = tags[0].Pictures;
			if (pictures.Count != 0)
			{
				var picture = pictures[0].PictureData;
				if (picture.Any())
				{
					using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
					{
						await stream.WriteAsync(picture.AsBuffer());
						stream.Seek(0);
						await bitmapImage.SetSourceAsync(stream);

					}
				}
			}
			return bitmapImage;
		}

		private static async void PathToStream(string path)
		{
			//if (File.Exists(path))
			{
				//var storageFile = new StorageFile();
				await Task.Run(async () =>
				{
					var fileStream = File.Open(path, FileMode.Open);
					var binaryReader = new BinaryReader(fileStream);
					binaryReader.BaseStream.Seek(0, SeekOrigin.Begin);
					var readByte = binaryReader.ReadBytes((int)binaryReader.BaseStream.Length);
					using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
					{
						await stream.WriteAsync(readByte.AsBuffer());
						stream.Seek(0);
						var mediaElement = new MediaElement();
						string contentType = string.Empty;
						mediaElement.SetSource(stream, contentType);
					}
				});
			}
		}
	}
}
