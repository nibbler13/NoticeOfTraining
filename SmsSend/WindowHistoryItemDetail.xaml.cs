using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Логика взаимодействия для WindowHistoryItemDetail.xaml
    /// </summary>
    public partial class WindowHistoryItemDetail : Window {
		public ObservableCollection<ItemHistoryDetail> HistoryDetailItems { get; set; }

		public WindowHistoryItemDetail(ItemHistory itemHistory) {
            InitializeComponent();
			HistoryDetailItems = new ObservableCollection<ItemHistoryDetail>();
			ParseHistoryItem(itemHistory);
			DataContext = this;
			Title += itemHistory.DateTimeCreate;
		}

		private async void ParseHistoryItem(ItemHistory itemHistory) {
			await Task.Run(() => {
				foreach (ItemSendResult result in itemHistory.Results) {
					ItemHistoryDetail itemHistoryDetail = new ItemHistoryDetail(
						result.ItemPhoneNumber.Name,
						result.ItemPhoneNumber.PhoneNumber,
						result.MessageId,
						result.DateTimeSelected);
					Application.Current.Dispatcher.Invoke(() => {
						HistoryDetailItems.Add(itemHistoryDetail);
					});
				}
			});
		}

		private void Button_Click(object sender, RoutedEventArgs e) {
			Close();
		}
	}
}
