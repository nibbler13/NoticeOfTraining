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
    /// Логика взаимодействия для WindowHistory.xaml
    /// </summary>
    public partial class WindowHistory : Window {
		public ObservableCollection<ItemHistory> HistoryItems { get; set; }
		private ListSortDirection sortDirection;
		private GridViewColumnHeader sortColumn;

		public WindowHistory(ObservableCollection<ItemHistory> historyItems) {
            InitializeComponent();
			HistoryItems = historyItems;
			listViewHistory.DataContext = this;
			buttonClearHistory.IsEnabled = HistoryItems.Count > 0;
        }

		private void buttonClose_Click(object sender, RoutedEventArgs e) {
			Close();
		}

		private void ListViewColumnHeader_Click(object sender, RoutedEventArgs e) {
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

		private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			buttonDetails.IsEnabled = listViewHistory.SelectedItems.Count == 1;
		}

		private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			buttonDetails_Click(sender, new RoutedEventArgs());
		}

		private void buttonClearHistory_Click(object sender, RoutedEventArgs e) {
			if (MessageBox.Show("Это действие нельзя отменить. Вы уверены, что хотите продолжить?",
				"Очистка истории", 
				MessageBoxButton.YesNo, 
				MessageBoxImage.Question) == MessageBoxResult.No)
				return;

			HistoryItems.Clear();
		}

		private void buttonDetails_Click(object sender, RoutedEventArgs e) {
			WindowHistoryItemDetail itemDetail = new WindowHistoryItemDetail(
				listViewHistory.SelectedItem as ItemHistory) {
				Owner = this
			};

			itemDetail.ShowDialog();
		}
	}
}
