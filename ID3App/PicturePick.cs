using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;

namespace ID3App
{
	public class PicturePick
	{
		public async void PicturePicker()
		{
			//打开文件选择器
			FolderPicker pick = new FolderPicker();
			pick.FileTypeFilter.Add(".png");
			pick.FileTypeFilter.Add(".jpg");
			pick.FileTypeFilter.Add(".bmp");
			IAsyncOperation<StorageFolder> folderTask = pick.PickSingleFolderAsync();

			StorageFolder folder = await folderTask;

			//var folder = await pick.PickSingleFolderAsync();
			StorageFolder Folder = null;
			string Address;
			string Token = "";
			if (folder != null)
			{
				Folder = folder;
				Address = folder.Path;
				Token = StorageApplicationPermissions.FutureAccessList.Add(folder);
			}
			await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(Token);

			//获取本地文件夹
			StorageFolder folderLocal = ApplicationData.Current.LocalFolder;

			//创建一个文件夹account
			string folderStr = string.Empty;
			try
			{
				folderLocal = await folderLocal.GetFolderAsync(folderStr);
			}
			catch (FileNotFoundException)
			{
				folderLocal = await folderLocal.CreateFolderAsync(folderStr);
			}

			StorageFile file = await folderLocal.CreateFileAsync(
				folderStr + ".json", CreationCollisionOption.ReplaceExisting);

			//保存选择的文件夹Token
			//var json = JsonSerializer.Create();
			ImagePath imagePath = new ImagePath { Id = DateTime.Now.ToString("yyMMddHHmmss"), Path = Token };
			string imageJson = imagePath.Stringify();
			if (file != null)
			{
				try
				{
					using (StorageStreamTransaction transaction = await file.OpenTransactedWriteAsync())
					{
						using (DataWriter dataWriter = new DataWriter(transaction.Stream))
						{
							dataWriter.WriteInt32(Encoding.UTF8.GetByteCount(imageJson));
							dataWriter.WriteString(imageJson);
							transaction.Stream.Size = await dataWriter.StoreAsync();
							await transaction.CommitAsync();
						}
					}
				}
				catch (Exception ex)
				{
					throw ex;
				}
			}
		}

		public async void GetPicture()
		{
			StorageFile fileLocal = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appdata:///local/account/" + ImageHelper.folderStr + ".json"));
			if (fileLocal != null)
			{
				try
				{
					//读取本地文件内容，并且反序列化
					using (IRandomAccessStream readStream = await fileLocal.OpenAsync(FileAccessMode.Read))
					{
						using (DataReader dataReader = new DataReader(readStream))
						{
							UInt64 size = readStream.Size;
							if (size <= UInt32.MaxValue)
							{
								await dataReader.LoadAsync(sizeof(Int32));
								Int32 stringSize = dataReader.ReadInt32();
								await dataReader.LoadAsync((UInt32)stringSize);
								string fileContent = dataReader.ReadString((uint)stringSize);
								ImagePath imagePath = new ImagePath(fileContent);
								StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(imagePath.Path);
								//筛选图片
								var queryOptions = new Windows.Storage.Search.QueryOptions();
								queryOptions.FileTypeFilter.Add(".png");
								queryOptions.FileTypeFilter.Add(".jpg");
								queryOptions.FileTypeFilter.Add(".bmp");
								var query = folder.CreateFileQueryWithOptions(queryOptions);
								var files = await query.GetFilesAsync();

								ImagePath img;
								var imgList = new ObservableCollection<ImagePath>();
								foreach (var item in files)
								{
									IRandomAccessStream irandom = await item.OpenAsync(FileAccessMode.Read);

									//对图像源使用流源
									BitmapImage bitmapImage = new BitmapImage();
									bitmapImage.DecodePixelWidth = 160;
									bitmapImage.DecodePixelHeight = 100;
									await bitmapImage.SetSourceAsync(irandom);

									img = new ImagePath();
									img.Path = item.Path;
									img.File = bitmapImage;
									img.Storage = item;
									imgList.Add(img);
								}

								//imageView.ItemsSource = imgList;
							}

						}
					}
				}
				catch (Exception exce)
				{
					await new MessageDialog(exce.ToString()).ShowAsync();
					throw exce;
				}
			}
		}
	}

	public class ImageHelper
	{
		public static object folderStr;
	}

	public class ImagePath
	{
		public ImagePath()
		{
		}

		public ImagePath(string fileContent)
		{
			throw new NotImplementedException();
		}

		public string Path { get; set; }
		public BitmapImage File { get; set; }
		public StorageFile Storage { get; set; }
		public string Id { get; set; }

		public string Stringify()
		{
			throw new NotImplementedException();
		}
	}
}
