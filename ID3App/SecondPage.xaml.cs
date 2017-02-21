using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace ID3App
{
	/// <summary>
	/// 可用于自身或导航至 Frame 内部的空白页。
	/// </summary>
	public sealed partial class SecondPage: Page
	{
		public SecondPage()
		{
			this.InitializeComponent();
			DataContext = new SecondPageViewModel();
		}
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			//这个e.Parameter是获取传递过来的参数，其实大家应该再次之前判断这个参数是否为null的，我偷懒了
			var info = (List<string>)e.Parameter;
			if (info == null) return;

			var songList=new List<ListStyle>();
			for (int i = 0; i < info.Count; i++)
			{
				songList.Add(new ListStyle
				{
					Num=i,
					Song=info[i],
					Color = i % 2 == 1
						? new SolidColorBrush(Colors.White)
						: new SolidColorBrush(Colors.AliceBlue)
				});
			}
			((SecondPageViewModel) DataContext).SongList = songList;
		}

		private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems != null)
			{
				foreach (var item in e.AddedItems)
				{
					Debug.WriteLine(item);
					ListBoxItem litem = (sender as ListBox).ContainerFromItem(item) as ListBoxItem;
					if (litem != null)
					{
						VisualStateManager.GoToState(litem, "CustomSelected", true);
					}
				}
			}
			if (e.RemovedItems != null)
			{
				foreach (var item in e.RemovedItems)
				{
					ListBoxItem litem = (sender as ListBox).ContainerFromItem(item) as ListBoxItem;
					if (litem != null)
					{
						VisualStateManager.GoToState(litem, "Unselected", true);
					}
				}
			}
		}
	}

public class ListStyle
{
	public int Num { get; set; }
	public string Song { get; set; }
	public SolidColorBrush Color { get; set; }
}
}
