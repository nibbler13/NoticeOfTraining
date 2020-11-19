using SmsSend.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SmsSend {
    public class ItemHistory {
		public bool SendNow { get; set; }
		public string SendNowString {
			get { return SendNow ? "Да" : "Нет"; }
		}

		public bool SendLater { get; set; }
		public string SendLaterString {
			get { return SendLater ? "Да" : "Нет"; }
		}

		public bool SendMailing { get; set; }
		public string SendMailinigString {
			get { return SendMailing ? "Да" : "Нет"; }
		}

		public DateTime DateTimeCreate { get; set; }
		public DateTime? DateTimeSelected { get; set; }

		public List<ItemDateTime> DateTimeMailing { get; set; }

		public uint MailingQuantityAtTime { get; set; }
		public string MessageText { get; set; }
		public List<ItemSendResult> Results { get; set; }
		public List<ItemPhoneNumber> PhoneNumbers { get; set; }
		public uint MailingMessagesTotalCount { get; set; }
		public string DateTimeCreateString {
			get {
				return DateTimeCreate.ToString();
			}
		}
		public string DateTimeSelectedString {
			get {
				if (DateTimeSelected == null)
					return string.Empty;
				else
					return ((DateTime)DateTimeSelected).ToString();
			}
		}

		public ItemPhoneNumber PhoneNumberAdmin { get; set; }

		public ItemHistory() {
			SendNow = false;
			SendLater = false;
			DateTimeCreate = new DateTime();
			DateTimeSelected = null;
			MessageText = string.Empty;
			Results = new List<ItemSendResult>();
			PhoneNumbers = new List<ItemPhoneNumber>();
		}

		public ItemHistory(bool sendNow,
					 bool sendLater,
					 DateTime? dateTimeSelected,
					 string messageText,
					 List<ItemPhoneNumber> phoneNumbers) {
			SendNow = sendNow;
			SendLater = sendLater;
			DateTimeCreate = DateTime.Now;
			DateTimeSelected = dateTimeSelected;
			MessageText = messageText;
			Results = new List<ItemSendResult>();
			PhoneNumbers = phoneNumbers;
		}

		public ItemHistory(string messageText,
					 List<ItemPhoneNumber> phoneNumbers,
					 List<ItemDateTime> dateTimeMailing,
					 uint mailingQuantityAtTime,
					 uint mailingMessagesTotalCount) {
			SendMailing = true;
			DateTimeCreate = DateTime.Now;
			MessageText = messageText;
			Results = new List<ItemSendResult>();
			PhoneNumbers = phoneNumbers;
			DateTimeMailing = dateTimeMailing;
			MailingQuantityAtTime = mailingQuantityAtTime;
			MailingMessagesTotalCount = mailingMessagesTotalCount;
		}
	}
}
