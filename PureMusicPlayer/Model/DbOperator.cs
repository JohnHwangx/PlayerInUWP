using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using SQLite.Net;
using SQLite.Net.Attributes;
using SQLite.Net.Interop;
using SQLite.Net.Platform.WinRT;

namespace PureMusicPlayer.Model
{
	public class DbOperator
	{
		private SQLiteConnection _conn;
		private readonly string _dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Music.db");
		public void DbConnect()
		{
			string fdlocal = ApplicationData.Current.LocalFolder.Path;
			string filename = "Music.db";
			string strConn = Path.Combine(fdlocal, filename);

			ISQLitePlatform platform = new SQLitePlatformWinRT();
			_conn = new SQLiteConnection(platform, strConn);
			var dbExist = IsExistDb(filename);
		}

		public bool IsExistDb(string dbName)
		{
			if (File.Exists(_dbPath))
			{
				return true;
			}
			return false;
		}

		public void InsertTable(List<Song> songs)
		{
			int rn = _conn.CreateTable<SongListData>();

			using (_conn = new SQLiteConnection(new SQLitePlatformWinRT(), _dbPath))
			{
				_conn.DeleteAll<SongListData>();
				var songList=new List<SongListData>();
				foreach (var song in songs)
				{
					songList.Add(new SongListData
					{
						Path=song.Path,
						Title=song.Title,
						Artist=song.Artist,
						Album=song.Album,
						Duration=song.Duration,
					});
				}

				int n = _conn.InsertAll(songList);
			}
		}

		public List<Song> LoadTable()
		{
			var songList=new List<Song>();
			using (_conn = new SQLiteConnection(new SQLitePlatformWinRT(), _dbPath))
			{
				// 获取列表
				var t = _conn.Table<SongListData>();
				var data = t.AsParallel();
				var q = from s in t.AsParallel()
						select s;
				foreach (var song in q)
				{
					songList.Add(Song.GetSong(song));
				}
			}
			return songList;
		}

		
	}


	[Table("MusicList")]
	public class SongListData
	{
		[Column("path")]
		[PrimaryKey]
		public string Path { get; set; }

		/// <summary>
		/// 歌曲名称
		/// </summary>
		[Column("title")]
		public string Title { get; set; }

		/// <summary>
		/// 作者
		/// </summary>
		[Column("artist")]
		public string Artist { get; set; }
		/// <summary>
		/// 专辑
		/// </summary>
		[Column("album")]
		public string Album { get; set; }
		/// <summary>
		/// 时长
		/// </summary>
		[Column("duration")]
		public TimeSpan Duration { get; set; }
	}
}
