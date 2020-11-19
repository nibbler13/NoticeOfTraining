using SmsSend.Items;
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
		public ItemHistory ItemHistoryOriginal { get; private set; }
		public List<ItemHistory> MailingHistory { get; private set; }
		
		public WindowSend(ItemHistory itemHistory) {
            InitializeComponent();
			ItemHistoryOriginal = itemHistory;
			MailingHistory = new List<ItemHistory>();
			Loaded += WindowSend_Loaded;
		}

		private async void WindowSend_Loaded(object sender, RoutedEventArgs e) {
			await Task.Run(() => { SendMessages(); });
		}

		private void SendMessages() {
			UpdateState(0, "Текст сообщения: " + ItemHistoryOriginal.MessageText);

			Tuple<bool, string> results;
			if (ItemHistoryOriginal.SendMailing)
				results = WriteMailingInfoToDB();
			else
				results = MessageSendingSystem.SendRegularSms(ItemHistoryOriginal, UpdateState);

			Application.Current.Dispatcher.Invoke(new Action(() => {
				buttonClose.IsEnabled = true;
				MessageBox.Show(
					this,
					results.Item2,
					"Операции завершены",
					MessageBoxButton.OK,
					results.Item1 ? MessageBoxImage.Information : MessageBoxImage.Warning);
			}));
		}

		private Tuple<bool, string> WriteMailingInfoToDB() {
			double progressCurrent = 0;
			double progressStep = (double)100 / (double)ItemHistoryOriginal.MailingMessagesTotalCount;

			string finalMessage = "Все сообщения успешно обработаны";
			bool isAllGood = true;

			int currentPhoneNumberCounter = 0;

			foreach (ItemDateTime itemDateTime in ItemHistoryOriginal.DateTimeMailing) {
				foreach (KeyValuePair<int, bool> time in itemDateTime.Times) {
					if (!time.Value)
						continue;

					UpdateState(progressCurrent, "Запись данных, дата: " + itemDateTime.SelectedDate.ToShortDateString() + ", время: " + time.Key + ":00");

					List<ItemPhoneNumber> itemPhoneNumbers = new List<ItemPhoneNumber>();
					for (int i = 0; i < ItemHistoryOriginal.MailingQuantityAtTime; i++) {
						itemPhoneNumbers.Add(ItemHistoryOriginal.PhoneNumbers[i]);
						currentPhoneNumberCounter++;

						if (currentPhoneNumberCounter == ItemHistoryOriginal.PhoneNumbers.Count)
							break;
					}

					if (ItemHistoryOriginal.PhoneNumberAdmin != null)
						itemPhoneNumbers.Add(ItemHistoryOriginal.PhoneNumberAdmin);

					DateTime dateTimeCurrent = itemDateTime.SelectedDate.Date.AddHours(time.Key);
					ItemHistory itemHistory = new ItemHistory(ItemHistoryOriginal.MessageText, itemPhoneNumbers, null, 0, 0) {
						SendLater = true,
						DateTimeSelected = dateTimeCurrent };

					foreach(ItemPhoneNumber itemPhoneNumber in itemPhoneNumbers) {
						Dictionary<string, object> param = new Dictionary<string, object> {
							{ "@ID",  DateTime.Now.ToString("yyyyMMddHHmmss") + DateTime.Now.Millisecond},
							{ "@createDate", DateTime.Now },
							{ "@recipientName", itemPhoneNumber.Name },
							{ "@recipientPhone", itemPhoneNumber.PhoneNumber },
							{ "@smsText", ItemHistoryOriginal.MessageText },
							{ "@delayedTime", itemDateTime.SelectedDate },
							{ "@delayedHour", time.Key }
						};

						bool isSuccess = true;

						try {
							FirebirdClient.Instance.ExecuteUpdateQuery(FirebirdClient.Instance.sqlInsertQueue, param);
						} catch (Exception e) {
							isAllGood = false;
							isSuccess = false;
							finalMessage = "Внимание! Имеются проблемы при обработке сообщений";
							UpdateState(progressCurrent, "Ошибка записи: " + e.Message);
						}

						ItemSendResult itemSendResult = new ItemSendResult() { 
							DateTimeSelected = dateTimeCurrent,
							ItemPhoneNumber = itemPhoneNumber,
							MessageId = string.Empty,
							IsSuccessStatusCode = isSuccess,
							Content = string.Empty};

						itemHistory.Results.Add(itemSendResult);

						progressCurrent += progressStep;
					}

					MailingHistory.Add(itemHistory);
					if (currentPhoneNumberCounter == ItemHistoryOriginal.PhoneNumbers.Count)
						return new Tuple<bool, string>(isAllGood, finalMessage);
				}
			}

			return new Tuple<bool, string>(isAllGood, finalMessage);
		}


		private void UpdateState(double percentage, string text) {
			Console.WriteLine("UpdateState: " + text);

			Application.Current.Dispatcher.Invoke(new Action(() => {
				progressBar.Value = percentage;
				textBox.Text += DateTime.Now.ToLongTimeString() + ": " + text + Environment.NewLine;
				textBox.ScrollToEnd();
			}));
		}

		private void ButtonClose_Click(object sender, RoutedEventArgs e) {
			DialogResult = true;
			Close();
		}
	}
}
