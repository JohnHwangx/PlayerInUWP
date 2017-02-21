using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace PureMusicPlayer.Converter
{
	class SongDurationConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var timeSpan = (TimeSpan) value;
			var hours = timeSpan.Hours<10?"0"+ timeSpan.Hours: "" + timeSpan.Hours;
			var minutes = timeSpan.Minutes < 10 ? "0" + timeSpan.Minutes : "" + timeSpan.Minutes;
			var seconds= timeSpan.Seconds < 10 ? "0" + timeSpan.Seconds : "" + timeSpan.Seconds;
			var duration = timeSpan.Hours==0 
				? $"{minutes} : {seconds}" 
				: $"{hours} : {minutes} : {seconds}";
			return duration;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
