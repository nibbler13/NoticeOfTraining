using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SmsSend {
	public class ItemPhoneNumber : INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private string name = string.Empty;
		public string Name {
			get { return name; }
			set {
				if (value != name) {
					name = value;
					NotifyPropertyChanged();
				}
			}
		}

		private string phoneNumber = string.Empty;
		public string PhoneNumber {
			get { return phoneNumber; }
			set {
				if (value != phoneNumber) {
					phoneNumber = value;
					NotifyPropertyChanged();
				}
			}
		}

		public string GetClearedNumber() {
			return new string(phoneNumber.Where(char.IsDigit).ToArray());
		}

		public string MailingId { get; set; }
	}
}
