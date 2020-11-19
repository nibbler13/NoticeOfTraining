using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SmsSend {
	class SmsGate {
		public enum Provider { SvyaznoyZagruzka, Intellin }

		public async static Task<ItemSendResult> SendMessage(string phoneNumber, string message, DateTime? dateTime = null, Provider provider = Provider.SvyaznoyZagruzka ) {
			ItemSendResult itemSendResult = new ItemSendResult();

			try {
				HttpClient client = new HttpClient();
				Uri uri;

				switch (provider) {
					case Provider.SvyaznoyZagruzka:
						uri = new Uri(
							"http://lk.zagruzka.com:9002/budzdorov?" +
							"msisdn=" + phoneNumber + "&" +
							"message=" + message);

						if (dateTime != null)
							uri = new Uri(uri.ToString() + "&send_time=" + 
								((DateTime)dateTime).ToString("yyyyMMddHHmm") + "00");

						break;
					case Provider.Intellin:
						uri = new Uri(
							"http://sms.intellin.ru/sendsms.cgi?http_username=clinic_lms&http_password=32oW8BJ4&" +
							"phone_list=" + phoneNumber + "&" +
							"message=" + message);

						if (dateTime != null)
							uri = new Uri(uri.ToString() + "&time=" +
								((DateTime)dateTime).ToString("ddMMyyHHmm"));

						break;
					default:
						return new ItemSendResult();
				}

				client.BaseAddress = uri;
				HttpResponseMessage responseMessage = client.GetAsync(uri).Result;
				string content = await responseMessage.Content.ReadAsStringAsync();
				
				itemSendResult.IsSuccessStatusCode = responseMessage.IsSuccessStatusCode;
				itemSendResult.Content = content;
				itemSendResult.DateTimeSelected = dateTime;

				if (!itemSendResult.IsSuccessStatusCode)
					return itemSendResult;

				if (provider == Provider.Intellin)
					if (!itemSendResult.Content.Contains("error_num=0")) {
						itemSendResult.Content = itemSendResult.Content.Replace("<br>", Environment.NewLine);
						itemSendResult.IsSuccessStatusCode = false;
						return itemSendResult;
					}

				switch (provider) {
					case Provider.SvyaznoyZagruzka:
						itemSendResult.MessageId = content.Replace("\r\n", "");
						break;
					case Provider.Intellin:
						itemSendResult.MessageId = content.Split(new string[] { "<br>" }, StringSplitOptions.None)[3].Replace("message_id=", "");
						break;
					default:
						break;
				}
			} catch (Exception e) {
				itemSendResult.Content = e.Message + Environment.NewLine + e.StackTrace;
			}

			return itemSendResult;
		}

		public static ItemDeliveryState GetDeliveryStateContent(string messageId, Provider provider = Provider.SvyaznoyZagruzka) {
			ItemDeliveryState itemDeliveryState = new ItemDeliveryState();

			try {
				HttpClient client = new HttpClient();
				Uri uri;

				switch (provider) {
					case Provider.SvyaznoyZagruzka:
						uri = new Uri("http://lk.zagruzka.com:9002/budzdorov/delivery_report?mt_num=" + messageId + "&show_date=Y");
						break;
					case Provider.Intellin:
						uri = new Uri("http://sms.intellin.ru/getstatus.cgi?http_username=" + IntellinPrivateData.USER_NAME +
							"&http_password=" + IntellinPrivateData.PASSWORD + "&message_id=" + messageId);
						break;
					default:
						return new ItemDeliveryState();
				}

				client.BaseAddress = uri;
				HttpResponseMessage responseMessage = client.GetAsync(string.Empty).Result;
				string content = responseMessage.Content.ReadAsStringAsync().Result;

				itemDeliveryState.Provider = provider;
				itemDeliveryState.IsSuccessStatusCode = responseMessage.IsSuccessStatusCode;
				itemDeliveryState.Content = content.Replace("\r\n", "");
				itemDeliveryState.ParseContent();
			} catch (Exception e) {
				itemDeliveryState.Content = e.Message + Environment.NewLine + e.StackTrace;
			}

			return itemDeliveryState;
		}
	}
}
