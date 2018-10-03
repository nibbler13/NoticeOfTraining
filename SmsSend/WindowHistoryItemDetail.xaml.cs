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

namespace NoticeOfTraining {
    /// <summary>
    /// Логика взаимодействия для WindowHistoryItemDetail.xaml
    /// </summary>
    public partial class WindowHistoryItemDetail : Window {
		public ObservableCollection<ItemHistoryDetail> HistoryDetailItems { get; set; }
		private ListSortDirection sortDirection;
		private GridViewColumnHeader sortColumn;

		public WindowHistoryItemDetail(ItemHistory itemHistory) {
            InitializeComponent();
			HistoryDetailItems = new ObservableCollection<ItemHistoryDetail>();
			ParseHistoryItem(itemHistory);
			listViewHistoryDetail.DataContext = this;
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

		private void ListView_Click(object sender, RoutedEventArgs e) {
			GridViewColumnHeader column = e.OriginalSource as GridViewColumnHeader;
			if (column == null)
				return;

			if (sortColumn == column)
				sortDirection = sortDirection == ListSortDirection.Ascending ?
												 ListSortDirection.Descending :
												 ListSortDirection.Ascending;
			else {
				if (sortColumn != null) {
					sortColumn.Column.HeaderTemplate = null;
					sortColumn.Column.Width = sortColumn.ActualWidth - 20;
				}

				sortColumn = column;
				sortDirection = ListSortDirection.Ascending;
				column.Column.Width = column.ActualWidth + 20;
			}

			if (sortDirection == ListSortDirection.Ascending)
				column.Column.HeaderTemplate = Resources["ArrowUp"] as DataTemplate;
			else
				column.Column.HeaderTemplate = Resources["ArrowDown"] as DataTemplate;

			string header = string.Empty;

			Binding b = sortColumn.Column.DisplayMemberBinding as Binding;
			if (b != null)
				header = b.Path.Path;

			ICollectionView resultDataView = CollectionViewSource.GetDefaultView((sender as ListView).ItemsSource);
			resultDataView.SortDescriptions.Clear();
			resultDataView.SortDescriptions.Add(new SortDescription(header, sortDirection));
		}

		private void Button_Click(object sender, RoutedEventArgs e) {
			Close();
		}
	}
}
