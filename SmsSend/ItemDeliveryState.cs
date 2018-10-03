using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoticeOfTraining {
	class ItemDeliveryState {
		public bool IsSuccessStatusCode { get; set; }
		public DateTime? DateTimeDelivery { get; private set; }
		public string Content { get; set; }
		public SmsGate.Provider Provider { get; set; }

//9.Статусы Интеллин.
//READ - доставлено
//DELIVERED - доставлено
//EXPIRED - период допустимости сообщения истек(оно равно сутки)
//DELETED - сообщение было удалено
//UNDELIV - недоставлено(чаще всего используется для недоставленных сообщений, аналог EXPIRED, REJECTD)
//ACCEPTD - сообщение находиться в принятом состоянии
//UNKNOWN - сообщение находиться в ошибочном состоянии
//REJECTD - отвергнуто
//PROGRESS и SEND - сообщение в статусе отправки


		private string deliveryState;
		public string DeliveryState {
			get {
				switch (deliveryState) {
					case "-1":
						return "В очереди передачи";
					case "0":
						return "Отвергнуто SMS-центром (ошибка доставки)";
					case "1":
						return "Не доставлено";
					case "2":
						return "Отправлено, статус доставки неизвестен";
					case "3":
						return "Доставлено";
					case "READ":
						return "Доставлено";
					case "DELIVERED":
						return "Доставлено";
					case "EXPIRED":
						return "Период допустимости сообщения истек (оно равно сутки)";
					case "DELETED":
						return "Сообщение было удалено";
					case "UNDELIV":
						return "Недоставлено (чаще всего используется для недоставленных сообщений, аналог EXPIRED, REJECTD)";
					case "ACCEPTD":
						return "Сообщение находится в принятом состоянии";
					case "UNKNOWN":
						return "Сообщение находится в ошибочном состоянии";
					case "REJECTD":
						return "Отклонено";
					case "PROGRESS":
						return "Сообщение в статусе отправки";
					case "SEND":
						return "Сообщение в статусе отправки";
					default:
						return "Ошибка обработки: " + Content;
				}
			}
		}

		public ItemDeliveryState() {
			IsSuccessStatusCode = false;
			DateTimeDelivery = null;
			Content = string.Empty;
			deliveryState = string.Empty;
		}

		public void ParseContent() {
			deliveryState = Content;

			if (!Content.Contains(" "))
				return;

			try {
				string[] splitted;
				DateTime dateTime;

				switch (Provider) {
					case SmsGate.Provider.SvyaznoyZagruzka:
						splitted = Content.Split(' ');
						deliveryState = splitted[0];

						if (DateTime.TryParseExact(splitted[1], "yyyyMMddHHmm", null,
							System.Globalization.DateTimeStyles.None, out dateTime))
							DateTimeDelivery = dateTime;

						break;
					case SmsGate.Provider.Intellin:
						splitted = Content.Split(new string[] { "<br>" }, StringSplitOptions.None);
						deliveryState = splitted[9].Replace("deliver_status=", "");

						if (DateTime.TryParseExact(splitted[6].Replace("deliver_date=", ""),
							"yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out dateTime))
							DateTimeDelivery = dateTime;

						break;
					default:
						break;
				}
			} catch (Exception) { }
		}
	}
}
