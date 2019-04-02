using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SmsSend {
    public class ItemTemplate : INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private string _name = string.Empty;
		public string Name {
			get {
				return _name;
			}
			set {
				if (value != _name) {
					_name = value;
					NotifyPropertyChanged();
				}
			}
		}

		private string _message = string.Empty;
		public string Message {
			get {
				return _message;
			}
			set {
				if (value != _message) {
					_message = value;
					NotifyPropertyChanged();
				}
			}
		}
	}
}
