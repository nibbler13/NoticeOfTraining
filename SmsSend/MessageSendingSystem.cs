using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsSend {
	static class MessageSendingSystem {
		public static void RunMailingAtThisTime() {
			Logging.ToLog("Запуск");
			Logging.ToLog("Получение списка рассылки на текущий час");

			string query = FirebirdClient.Instance.sqlGetMailingListAtThisTime;
			query = query.Replace("@date", "'" + DateTime.Now.ToShortDateString() + "'");
			query = query.Replace("@hour", DateTime.Now.Hour.ToString());

			DataTable dataTable = FirebirdClient.Instance.GetDataTable(query, new Dictionary<string, object>());
			Logging.ToLog("Получено строк: " + dataTable.Rows.Count);

			if (dataTable.Rows.Count == 0) {
				Logging.ToLog("Нет данных для обработки, завершение");
				return;
			}

			Dictionary<string, ItemHistory> historyItemsDict = new Dictionary<string, ItemHistory>();

			foreach (DataRow row in dataTable.Rows) {
				try {
					string smsText = row["SMSTEXT"].ToString();

					if (!historyItemsDict.ContainsKey(smsText))
						historyItemsDict.Add(smsText, new ItemHistory(true, false, null, smsText, new List<ItemPhoneNumber>()));

					string id = row["ID"].ToString();
					string recipientName = row["RECIPIENTNAME"].ToString();
					string recipientPhone = row["RECIPIENTPHONE"].ToString();

					ItemPhoneNumber itemPhoneNumber = new ItemPhoneNumber() {
						MailingId = id,
						Name = recipientName,
						PhoneNumber = recipientPhone
					};

					historyItemsDict[smsText].PhoneNumbers.Add(itemPhoneNumber);
				} catch (Exception e) {
					Logging.ToLog(e.Message + Environment.NewLine + e.StackTrace);
				}
			}

			foreach (KeyValuePair<string, ItemHistory> pair in historyItemsDict)
				SendRegularSms(pair.Value, UpdateState);

			Logging.ToLog("Завершение");
		}

		private static void UpdateState(double progress, string status) {
			if (string.IsNullOrEmpty(status))
				return;

			Logging.ToLog(status);
		}

		public static Tuple<bool, string> SendRegularSms(ItemHistory ItemHistoryOriginal, Action<double, string> UpdateState) {
			double progressCurrent = 0;
			double progressStep = (double)100 / (double)((Convert.ToInt32(ItemHistoryOriginal.SendNow) +
					Convert.ToInt32(ItemHistoryOriginal.SendLater)) * ItemHistoryOriginal.PhoneNumbers.Count);

			string finalMessage = "Все сообщения успешно обработаны";
			bool isAllGood = true;
			Dictionary<string, DateTime?> sendTypes = new Dictionary<string, DateTime?>();

			if (ItemHistoryOriginal.SendNow)
				sendTypes.Add("Отправка сейчас", null);

			if (ItemHistoryOriginal.SendLater) {
				sendTypes.Add("Отправка в заданное время", ItemHistoryOriginal.DateTimeSelected);
				UpdateState(progressCurrent, "Выбранное время: " + ((DateTime)ItemHistoryOriginal.DateTimeSelected).ToString("yyyy-MM-dd HH:mm"));
			}

			foreach (ItemPhoneNumber number in ItemHistoryOriginal.PhoneNumbers) {
				UpdateState(progressCurrent, "Получатель: " + number.Name + ", номер телефона: " + number.PhoneNumber);

				foreach (KeyValuePair<string, DateTime?> sendType in sendTypes) {
					UpdateState(progressCurrent, sendType.Key);
					ItemSendResult sendMessageResult = SmsGate.SendMessage(
						number.GetClearedNumber(),
						ItemHistoryOriginal.MessageText,
						sendType.Value).Result;

					progressCurrent += progressStep;

					UpdateState(progressCurrent, sendMessageResult.IsSuccessStatusCode == true ?
						"Успешно, ID: " + sendMessageResult.MessageId : "Не удалось отправить: " + sendMessageResult.Content);

					sendMessageResult.ItemPhoneNumber = number;
					ItemHistoryOriginal.Results.Add(sendMessageResult);

					if (!sendMessageResult.IsSuccessStatusCode) {
						isAllGood = false;
						finalMessage = "Внимание! Имеются проблемы при обработке сообщений";
						continue;
					}

					if (!long.TryParse(sendMessageResult.MessageId, out long smsID))
						continue;

					Dictionary<string, object> param = new Dictionary<string, object> {
						{ "@smsID", smsID },
						{ "@sendDate", DateTime.Now },
						{ "@recipientName", number.Name },
						{ "@recipientPhone", number.PhoneNumber },
						{ "@smsText", ItemHistoryOriginal.MessageText },
						{ "@isRightNow", sendType.Key.Contains("сейчас") ? 1 : 0 },
						{ "@isDelayed", sendType.Key.Contains("заданное") ? 1 : 0 },
						{ "@delayedTime", ItemHistoryOriginal.DateTimeSelected }
					};

					FirebirdClient.Instance.ExecuteUpdateQuery(FirebirdClient.Instance.sqlInsert, param);

					if (!string.IsNullOrEmpty(number.MailingId)) {
						Dictionary<string, object> parameters = new Dictionary<string, object> {
							{ "@smsId", smsID },
							{ "@id", number.MailingId }
						};

						FirebirdClient.Instance.ExecuteUpdateQuery(FirebirdClient.Instance.sqlUpateSmsQueueRecord, parameters);
					}
				}
			}

			return new Tuple<bool, string>(isAllGood, finalMessage);
		}
	}
}
