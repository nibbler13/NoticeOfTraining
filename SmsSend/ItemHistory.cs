using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoticeOfTraining {
    public class ItemHistory {
		public bool SendNow { get; set; }
		public string SendNowString {
			get {
				return SendNow ? "Да" : "Нет";
			}
		}

		public bool SendLater { get; set; }
		public string SendLaterString {
			get {
				return SendLater ? "Да" : "Нет";
			}
		}

		public DateTime DateTimeCreate { get; set; }
		public DateTime? DateTimeSelected { get; set; }
		public String MessageText { get; set; }
		public List<ItemSendResult> Results { get; set; }
		public List<ItemPhoneNumber> PhoneNumbers { get; set; }

		public string DateTimeCreateString {
			get {
				return DateTimeCreate.ToString();
			}
		}

		public string DateTimeSelectedString {
			get {
				if (DateTimeSelected == null)
					return "Сразу";
				else
					return ((DateTime)DateTimeSelected).ToString();
			}
		}

		public ItemHistory() {
			SendNow = false;
			SendLater = false;
			DateTimeCreate = new DateTime();
			DateTimeSelected = null;
			MessageText = string.Empty;
			Results = new List<ItemSendResult>();
			PhoneNumbers = new List<ItemPhoneNumber>();
		}

		public ItemHistory(bool sendNow, bool sendLater, DateTime? dateTimeSelected, 
			string messageText, List<ItemPhoneNumber> phoneNumbers) {
			SendNow = sendNow;
			SendLater = sendLater;
			DateTimeCreate = DateTime.Now;
			DateTimeSelected = dateTimeSelected;
			MessageText = messageText;
			Results = new List<ItemSendResult>();
			PhoneNumbers = phoneNumbers;
		}
    }
}
