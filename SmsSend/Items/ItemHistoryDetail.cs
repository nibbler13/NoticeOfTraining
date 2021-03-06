﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsSend {
    public class ItemHistoryDetail {
		public string Name { get; private set; }
		public string PhoneNumber { get; private set; }
		public string DateSelected { get; private set; }
		private string MessageId { get; set; }
		public string DeliveryState { get; private set; }

		private ItemDeliveryState itemDeliveryState;
		public string DeliveryDateTimeString { get; private set; }

		public ItemHistoryDetail(string name, string phoneNumber, string messageId, DateTime? dateSelected) {
			Name = name;
			PhoneNumber = phoneNumber;
			MessageId = messageId;
			DateSelected = dateSelected != null ? ((DateTime)dateSelected).ToString() : "Сразу";

			if (string.IsNullOrEmpty(messageId)) {
				DeliveryState = "Неизвестно";
				DeliveryDateTimeString = string.Empty;
			} else {
				itemDeliveryState = SmsGate.GetDeliveryStateContent(MessageId);

				DeliveryState = itemDeliveryState.IsSuccessStatusCode ?
					itemDeliveryState.DeliveryState :
					"Не удалось получить статус, " + itemDeliveryState.Content;

				DeliveryDateTimeString = itemDeliveryState.DateTimeDelivery == null ?
					string.Empty :
					((DateTime)itemDeliveryState.DateTimeDelivery).ToString();
			}
		}
    }
}
