using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SmsSend.Items {
	public class ItemDateTime : INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;
		public void NotifyPropertyChanged([CallerMemberName] string propertyName = "") {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private DateTime selectedDate = DateTime.Now;
		public DateTime SelectedDate {
			get { return selectedDate; }
			set {
				if (value != selectedDate) {
					selectedDate = value;
					NotifyPropertyChanged();
					NotifyPropertyChanged("SelectedDateStr");
				}
			}
		}

		[NonSerialized]
		[XmlIgnore]
		public Dictionary<int, bool> Times = new Dictionary<int, bool>();

		public string SelectedDateStr { get { return SelectedDate.ToString("yyyy.MM.dd"); } }
		public string SelectedTime { 
			get {
				string selectedTime = string.Empty;

				foreach (KeyValuePair<int, bool> pair in Times)
					if (pair.Value)
						selectedTime += pair.Key + ", ";

				return selectedTime.TrimEnd(' ').TrimEnd(',');
			} 
		}

		public ItemDateTime() {
			for (int i = 0; i < 24; i++)
				Times.Add(i, false);
		}
	}
}
