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

namespace NoticeOfTraining {
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
			Console.WriteLine("UpdateState");

			Application.Current.Dispatcher.Invoke(new Action(() => {
				progressBar.Value = percentage;
				textBox.Text += text + Environment.NewLine;
			}));
		}

		private async void SendMessages() {
			Console.WriteLine("SendMessages");

			double progressCurrent = 0;
			double progressStep = 100 / 
				((Convert.ToInt32(ItemHistory.SendNow) + Convert.ToInt32(ItemHistory.SendLater)) * ItemHistory.PhoneNumbers.Count);

			UpdateState(progressCurrent, "Текст сообщения: " + ItemHistory.MessageText);

			Dictionary<string, DateTime?> sendTypes = new Dictionary<string, DateTime?>();

			if (ItemHistory.SendNow)
				sendTypes.Add("Отправка сейчас", null);

			if (ItemHistory.SendLater) {
				sendTypes.Add("Отправка в заданное время", ItemHistory.DateTimeSelected);
				UpdateState(progressCurrent, "Выбранное время: " + ((DateTime)ItemHistory.DateTimeSelected));
			}

			foreach (ItemPhoneNumber number in ItemHistory.PhoneNumbers) {
				UpdateState(progressCurrent, "Получатель: " + number.Name + ", номер телефона: " + number.PhoneNumber);

				foreach (KeyValuePair<string, DateTime?> sendType in sendTypes) {
					UpdateState(progressCurrent, sendType.Key);
					ItemSendResult sendMessageResult = await SvyaznoyZagruzka.SendMessage(
						number.GetClearedNumber(), 
						ItemHistory.MessageText, 
						sendType.Value);
					progressCurrent += progressStep;
					UpdateState(progressCurrent, sendMessageResult.IsSuccessStatusCode == true ? 
						"Успешно" : "Не удалось отправить: " + sendMessageResult.Content);
					sendMessageResult.ItemPhoneNumber = number;
					ItemHistory.Results.Add(sendMessageResult);
				}
			}

			buttonClose.IsEnabled = true;
			MessageBox.Show("Операции завершены", "", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void buttonClose_Click(object sender, RoutedEventArgs e) {
			DialogResult = true;
			Close();
		}
	}
}
