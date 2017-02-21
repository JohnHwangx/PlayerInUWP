using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;
using Prism.Mvvm;

namespace ID3App
{
	public class SecondPageViewModel:BindableBase
	{
		private string _msg;

		public string Msg
		{
			get { return _msg; }
			set
			{
				_msg = value;
				OnPropertyChanged();
			}
		}

		private List<ListStyle> _songList;

		public List<ListStyle> SongList
		{
			get { return _songList; }
			set
			{
				_songList = value;
				OnPropertyChanged();
			}
		}

	}
}
