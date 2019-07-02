using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SmsSend {
	/// <summary>
	/// Логика взаимодействия для WindowSend.xaml
	/// </summary>
	public partial class WindowSend : Window {
		public ItemHistory ItemHistory { get; private set; }
		
		public WindowSend(ItemHistory itemHistory) {
            InitializeComponent();
			ItemHistory = itemHistory;
			SendMessages();
		}

		private void UpdateState(double percentage, string text) {
			Console.WriteLine("UpdateState: " + text);

			Application.Current.Dispatcher.Invoke(new Action(() => {
				progressBar.Value = percentage;
				textBox.Text = text + Environment.NewLine + textBox.Text;
			}));
		}

		private async void SendMessages() {
			Console.WriteLine("SendMessages");

			double progressCurrent = 0;
			double progressStep = (double)100 / 
				(double)((Convert.ToInt32(ItemHistory.SendNow) + Convert.ToInt32(ItemHistory.SendLater)) * ItemHistory.PhoneNumbers.Count);

			UpdateState(progressCurrent, "Текст сообщения: " + ItemHistory.MessageText);

			Dictionary<string, DateTime?> sendTypes = new Dictionary<string, DateTime?>();

			if (ItemHistory.SendNow)
				sendTypes.Add("Отправка сейчас", null);

			if (ItemHistory.SendLater) {
				sendTypes.Add("Отправка в заданное время", ItemHistory.DateTimeSelected);
				UpdateState(progressCurrent, "Выбранное время: " + ((DateTime)ItemHistory.DateTimeSelected).ToString("yyyy-MM-dd HH:mm"));
			}

			bool isAllGood = true;
			string finalMessage = "Все сообщения успешно обработаны";

			foreach (ItemPhoneNumber number in ItemHistory.PhoneNumbers) {
				UpdateState(progressCurrent, "Получатель: " + number.Name + ", номер телефона: " + number.PhoneNumber);

				foreach (KeyValuePair<string, DateTime?> sendType in sendTypes) {
					UpdateState(progressCurrent, sendType.Key);
					ItemSendResult sendMessageResult = await SmsGate.SendMessage(
						number.GetClearedNumber(), 
						ItemHistory.MessageText, 
						sendType.Value);

					progressCurrent += progressStep;

					UpdateState(progressCurrent, sendMessageResult.IsSuccessStatusCode == true ? 
						"Успешно, ID: " + sendMessageResult.MessageId : "Не удалось отправить: " + sendMessageResult.Content);

					sendMessageResult.ItemPhoneNumber = number;
					ItemHistory.Results.Add(sendMessageResult);

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
						{ "@smsText", ItemHistory.MessageText },
						{ "@isRightNow", sendType.Key.Contains("сейчас") ? 1 : 0 },
						{ "@isDelayed", sendType.Key.Contains("заданное") ? 1 : 0 },
						{ "@delayedTime", ItemHistory.DateTimeSelected }
					};

					FirebirdClient.Instance.ExecuteUpdateQuery(FirebirdClient.Instance.sqlInsert, param);						
				}
			}

			buttonClose.IsEnabled = true;
			MessageBox.Show(
				Application.Current.MainWindow,
				finalMessage,
				"Операции завершены",
				MessageBoxButton.OK,
				isAllGood ? MessageBoxImage.Information : MessageBoxImage.Warning);
		}

		private void buttonClose_Click(object sender, RoutedEventArgs e) {
			DialogResult = true;
			Close();
		}
	}
}
