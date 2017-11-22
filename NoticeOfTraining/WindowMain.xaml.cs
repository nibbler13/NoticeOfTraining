using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace NoticeOfTraining {
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class WindowMain : Window {
		private ListSortDirection _sortDirectionSavedItems;
		private GridViewColumnHeader _sortColumnSavedItems;
		private ListSortDirection _sortDirectionReceiversItems;
		private GridViewColumnHeader _sortColumnReceiversItems;
		private ListSortDirection _sortDirectionTemplateItems;
		private GridViewColumnHeader _sortColumnTemplateItems;

		public ObservableCollection<ItemPhoneNumber> PhoneNumberSavedItems { get; set; }
		public ObservableCollection<ItemPhoneNumber> PhoneNumberReceiversItems { get; set; }
		public ObservableCollection<ItemTemplate> TemplateItems { get; set; }
		public ObservableCollection<ItemHistory> HistoryItems { get; set; }

		public WindowMain() {
			InitializeComponent();
			PhoneNumberSavedItems = new ObservableCollection<ItemPhoneNumber>();
			PhoneNumberReceiversItems = new ObservableCollection<ItemPhoneNumber>();
			TemplateItems = new ObservableCollection<ItemTemplate>();
			HistoryItems = new ObservableCollection<ItemHistory>();

			listViewPhoneNumbersSaved.DataContext = this;
			listViewPhoneNumbersReceivers.DataContext = this;
			listViewTemplates.DataContext = this;


			PhoneNumberSavedItems.CollectionChanged += PhoneNumberItems_CollectionChanged;
			PhoneNumberReceiversItems.CollectionChanged += ReceiversPhoneNumberItems_CollectionChanged;
			
			dateTimePicker.DefaultValue = DateTime.Now;
			dateTimePicker.ValueChanged += DateTimePicker_ValueChanged;

			Loaded += MainWindow_Loaded;
			Closed += MainWindow_Closed;
		}

		private bool UserFilter(object item) {
			if (string.IsNullOrEmpty(textBoxFilter.Text))
				return true;
			else
				return ((item as ItemPhoneNumber).Name.IndexOf(textBoxFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0);
		}
		
		private void TextBoxFilter_TextChanged(object sender, TextChangedEventArgs e) {
			CollectionViewSource.GetDefaultView(listViewPhoneNumbersSaved.ItemsSource).Refresh();
		}


		private async void MainWindow_Loaded(object sender, RoutedEventArgs e) {
			await Task.Run(new Action(() => {
				LoadCollections();
			}));

			CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listViewPhoneNumbersSaved.ItemsSource);
			view.Filter += UserFilter;
		}

		private void LoadCollections() {
			XmlSerializer serializerPhoneItem = new XmlSerializer(typeof(ObservableCollection<ItemPhoneNumber>));
			XmlSerializer serializerTemplateItem = new XmlSerializer(typeof(ObservableCollection<ItemTemplate>));
			XmlSerializer serializerHistoryItem = new XmlSerializer(typeof(ObservableCollection<ItemHistory>));

			try {
				using (FileStream fileStream = new FileStream("PhoneNumberSavedItems.xml", FileMode.Open)) {
					ObservableCollection<ItemPhoneNumber> observableCollection =
						serializerPhoneItem.Deserialize(fileStream) as ObservableCollection<ItemPhoneNumber>;
					foreach (ItemPhoneNumber item in observableCollection)
						Application.Current.Dispatcher.Invoke(() => {
							PhoneNumberSavedItems.Add(item);
						});
				}
			} catch (Exception exception) {
				Console.WriteLine(exception.Message + Environment.NewLine + exception.StackTrace);
			}

			try {
				using (FileStream fileStream = new FileStream("TemplateItems.xml", FileMode.Open)) {
					ObservableCollection<ItemTemplate> observableCollection =
						serializerTemplateItem.Deserialize(fileStream) as ObservableCollection<ItemTemplate>;
					foreach (ItemTemplate item in observableCollection)
						Application.Current.Dispatcher.Invoke(() => {
							TemplateItems.Add(item);
						});
				}
			} catch (Exception exception) {
				Console.WriteLine(exception.Message + Environment.NewLine + exception.StackTrace);
			}

			try {
				using (FileStream fileStream = new FileStream("HistoryItems.xml", FileMode.Open)) {
					ObservableCollection<ItemHistory> observableCollection =
						serializerHistoryItem.Deserialize(fileStream) as ObservableCollection<ItemHistory>;
					foreach (ItemHistory item in observableCollection)
						Application.Current.Dispatcher.Invoke(() => {
							HistoryItems.Add(item);
						});
				}
			} catch (Exception exception) {
				Console.WriteLine(exception.Message + Environment.NewLine + exception.StackTrace);
			}
		}

		private void MainWindow_Closed(object sender, EventArgs e) {
			ButtonReceiversRemoveAll_Click(buttonReceiversRemoveAll, new RoutedEventArgs());

			XmlSerializer serializerPhoneItem = new XmlSerializer(typeof(ObservableCollection<ItemPhoneNumber>));
			XmlSerializer serializerTemplateItem = new XmlSerializer(typeof(ObservableCollection<ItemTemplate>));
			XmlSerializer serializerHistoryItem = new XmlSerializer(typeof(ObservableCollection<ItemHistory>));

			try {
				using (StreamWriter writer = new StreamWriter("PhoneNumberSavedItems.xml"))
					serializerPhoneItem.Serialize(writer, PhoneNumberSavedItems);
			} catch (Exception exception) {
				Console.WriteLine(exception.Message + Environment.NewLine + exception.StackTrace);
			}

			try {
				using (StreamWriter writer = new StreamWriter("TemplateItems.xml"))
					serializerTemplateItem.Serialize(writer, TemplateItems);
			} catch (Exception exception) {
				Console.WriteLine(exception.Message + Environment.NewLine + exception.StackTrace);
			}
			
			try {
				using (StreamWriter writer = new StreamWriter("HistoryItems.xml"))
					serializerHistoryItem.Serialize(writer, HistoryItems);
			} catch (Exception exception) {
				Console.WriteLine(exception.Message + Environment.NewLine + exception.StackTrace);
			}
		}


		private void DateTimePicker_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
			if (e.NewValue == null)
				return;

			if (!IsLoaded)
				return;

			if ((DateTime)e.NewValue < DateTime.Now) {
				dateTimePicker.Value = DateTime.Now;
				e.Handled = true;
			}

			if ((DateTime)e.NewValue > DateTime.Now.AddDays(2)) {
				try {
					MessageBox.Show("Отложенная отправка СМС допускает разницу во времени не более 48 часов. " +
						"Сброс до максимально возможного времени.", "Ошибка времени", MessageBoxButton.OK, MessageBoxImage.Warning);
				} catch (Exception) { }

				dateTimePicker.Value = DateTime.Now.AddDays(2).AddMinutes(-1);
				e.Handled = true;
			}
		}

		private void ReceiversPhoneNumberItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
			SetButtonsReceiversAllEnable();
		}

		private void PhoneNumberItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
			SetButtonsReceiversAllEnable();
		}

		private void SetButtonsReceiversAllEnable() {
			buttonReceiversRemoveAll.IsEnabled = PhoneNumberReceiversItems.Count > 0;
			buttonReceiversAddAll.IsEnabled = PhoneNumberSavedItems.Count > 0;

		}


		private void ButtonPhoneNumberAdd_Click(object sender, RoutedEventArgs e) {
			WindowPhoneNumber windowPhoneNumber = new WindowPhoneNumber();
			windowPhoneNumber.Owner = this;
			windowPhoneNumber.ShowDialog();

			if (windowPhoneNumber.DialogResult == true)
				PhoneNumberSavedItems.Add(windowPhoneNumber.PhoneNumberItem);
		}

		private void ButtonPhoneNumberEdit_Click(object sender, RoutedEventArgs e) {
			ItemPhoneNumber phoneNumberItem = listViewPhoneNumbersSaved.SelectedItem as ItemPhoneNumber;
			WindowPhoneNumber windowPhoneNumber = new WindowPhoneNumber(phoneNumberItem);
			windowPhoneNumber.Owner = this;
			windowPhoneNumber.ShowDialog();
		}

		private void ButtonPhoneNumberRemove_Click(object sender, RoutedEventArgs e) {
			if (MessageBox.Show("Вы уверены, что хотите удалить выбранные элементы?", "Удаление элементов", 
				MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
				return;

			List<ItemPhoneNumber> itemsToRemove = new List<ItemPhoneNumber>();
			foreach (ItemPhoneNumber item in listViewPhoneNumbersSaved.SelectedItems)
				itemsToRemove.Add(item);
			foreach (ItemPhoneNumber item in itemsToRemove)
				PhoneNumberSavedItems.Remove(item);
		}

		private void ButtonPhoneNumberImport_Click(object sender, RoutedEventArgs e) {
			WindowImportExcel windowImportExcel = new WindowImportExcel();
			windowImportExcel.Owner = this;
			windowImportExcel.ShowDialog();

			if (windowImportExcel.DialogResult != true)
				return;

			int added = 0;
			foreach (ItemPhoneNumber item in windowImportExcel.PhoneNumbers) {
				List<ItemPhoneNumber> matches = PhoneNumberSavedItems.Where(p => p.PhoneNumber.Equals(item.PhoneNumber)).ToList();
				if (matches != null && matches.Count != 0)
					continue;

				PhoneNumberSavedItems.Add(item);
				added++;
			}

			if (added > 0)
				MessageBox.Show("В список сохраненных номеров добавлено записей: " + added, "",
					MessageBoxButton.OK, MessageBoxImage.Information);
			else
				MessageBox.Show("Считанные номера уже добавлены ранее", "", 
					MessageBoxButton.OK, MessageBoxImage.Warning);
		}


		private void ButtonMessageSend_Click(object sender, RoutedEventArgs e) {
			if (PhoneNumberReceiversItems.Count == 0) {
				if (MessageBox.Show("В списке получателей отсутствуют адресаты, хотите добавить сейчас?", 
					"Отсутствуют получатели", 
					MessageBoxButton.YesNo, 
					MessageBoxImage.Question) == MessageBoxResult.No)
					return;

				WindowPhoneNumber windowPhoneNumber = new WindowPhoneNumber() {
					Owner = this
				};

				windowPhoneNumber.ShowDialog();
				if (windowPhoneNumber.DialogResult != true)
					return;

				PhoneNumberReceiversItems.Add(windowPhoneNumber.PhoneNumberItem);
			}

			ItemHistory itemHistory = new ItemHistory(
				checkBoxNow.IsChecked == true ? true : false,
				checkBoxSelectedTime.IsChecked == true ? true : false,
				checkBoxSelectedTime.IsChecked == true ? dateTimePicker.Value : null,
				textBoxMessage.Text,
				PhoneNumberReceiversItems.ToList()
			);

			WindowSend windowSend = new WindowSend(itemHistory) {
				Owner = this
			};

			windowSend.ShowDialog();

			if (windowSend.DialogResult == true)
				HistoryItems.Add(itemHistory);
		}

		private void ButtonMessagesHistory_Click(object sender, RoutedEventArgs e) {
			WindowHistory windowHistory = new WindowHistory(HistoryItems);
			windowHistory.Owner = this;
			windowHistory.ShowDialog();
		}


		private void ButtonTemplateUse_Click(object sender, RoutedEventArgs e) {
			if (textBoxMessage.Text.Length > 0) {
				if (MessageBox.Show("В поле для ввода сообщения уже имеется текст. " +
					"Использование шаблона заменит этот текст. Продолжить?", "Использование шаблона",
					MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
					return;
			}

			textBoxMessage.Text = (listViewTemplates.SelectedItem as ItemTemplate).Message;
		}

		private void ButtonTemplateRemove_Click(object sender, RoutedEventArgs e) {
			if (MessageBox.Show("Вы уверены, что хотите удалить выбранные элементы?", "Удаление элементов",
				MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
				return;

			List<ItemTemplate> itemsToRemove = new List<ItemTemplate>();
			foreach (ItemTemplate item in listViewTemplates.SelectedItems)
				itemsToRemove.Add(item);
			foreach (ItemTemplate item in itemsToRemove)
				TemplateItems.Remove(item);
		}

		private void ButtonTemplateEdit_Click(object sender, RoutedEventArgs e) {
			WindowTemplate windowTemplate = new WindowTemplate(listViewTemplates.SelectedItem as ItemTemplate);
			windowTemplate.Owner = this;
			windowTemplate.ShowDialog();
		}

		private void ButtonTemplateAdd_Click(object sender, RoutedEventArgs e) {
			WindowTemplate windowTemplate = new WindowTemplate();
			windowTemplate.Owner = this;
			windowTemplate.ShowDialog();

			if (windowTemplate.DialogResult == true)
				TemplateItems.Add(windowTemplate.TemplateItem);
		}


		private void ButtonReceiversAddSelected_Click(object sender, RoutedEventArgs e) {
			List<ItemPhoneNumber> itemsToMove = new List<ItemPhoneNumber>();
			foreach (ItemPhoneNumber item in listViewPhoneNumbersSaved.SelectedItems)
				itemsToMove.Add(item);
			foreach (ItemPhoneNumber item in itemsToMove) {
				PhoneNumberReceiversItems.Add(item);
				PhoneNumberSavedItems.Remove(item);
			}
		}

		private void ButtonReceiversAddAll_Click(object sender, RoutedEventArgs e) {
			foreach (ItemPhoneNumber item in PhoneNumberSavedItems)
				PhoneNumberReceiversItems.Add(item);
			PhoneNumberSavedItems.Clear();
		}

		private void ButtonReceiversRemoveAll_Click(object sender, RoutedEventArgs e) {
			foreach (ItemPhoneNumber item in PhoneNumberReceiversItems)
				PhoneNumberSavedItems.Add(item);
			PhoneNumberReceiversItems.Clear();
		}

		private void ButtonReceiversRemoveSelected_Click(object sender, RoutedEventArgs e) {
			List<ItemPhoneNumber> itemsToMove = new List<ItemPhoneNumber>();
			foreach (ItemPhoneNumber item in listViewPhoneNumbersReceivers.SelectedItems)
				itemsToMove.Add(item);
			foreach (ItemPhoneNumber item in itemsToMove) {
				PhoneNumberSavedItems.Add(item);
				PhoneNumberReceiversItems.Remove(item);
			}
		}
		

		private void TextBoxMessage_TextChanged(object sender, TextChangedEventArgs e) {
			bool isTextEntered = textBoxMessage.Text.Length > 0;
			checkBoxNow.IsEnabled = isTextEntered;
			checkBoxSelectedTime.IsEnabled = isTextEntered;
			CheckBox_CheckedChanged(checkBoxNow, new RoutedEventArgs());
		}


		private void ListViewTemplateItem_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			ButtonTemplateUse_Click(buttonTemplateUse, new RoutedEventArgs());
		}

		private void ListViewPhoneNumbersSaved_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			bool isItemsSelected = listViewPhoneNumbersSaved.SelectedItems.Count > 0;
			buttonPhoneNumberRemove.IsEnabled = isItemsSelected;
			buttonReceiversAddSelected.IsEnabled = isItemsSelected;
			buttonPhoneNumberEdit.IsEnabled = listViewPhoneNumbersSaved.SelectedItems.Count == 1;
		}

		private void ListViewPhoneNumbersReceivers_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			buttonReceiversRemoveSelected.IsEnabled = listViewPhoneNumbersReceivers.SelectedItems.Count > 0;
		}

		private void ListViewTemplates_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			bool isOneItemSelected = listViewTemplates.SelectedItems.Count == 1;
			buttonTemplateUse.IsEnabled = isOneItemSelected;
			buttonTemplateEdit.IsEnabled = isOneItemSelected;
			buttonTemplateRemove.IsEnabled = listViewTemplates.SelectedItems.Count > 0;
		}
		
		private void ListViewPhoneNumberSavedItem_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			ButtonReceiversAddSelected_Click(listViewPhoneNumbersSaved, new RoutedEventArgs());
		}

		private void ListViewPhoneNumberReceiversItem_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			ButtonReceiversRemoveSelected_Click(listViewPhoneNumbersReceivers, new RoutedEventArgs());
		}

		private void ListViewColumnHeader_Click(object sender, RoutedEventArgs e) {
			if (sender == listViewPhoneNumbersSaved) 
				SortListViewColumn(sender, e, ref _sortColumnSavedItems, ref _sortDirectionSavedItems);
			else if (sender == listViewPhoneNumbersReceivers)
				SortListViewColumn(sender, e, ref _sortColumnReceiversItems, ref _sortDirectionReceiversItems);
			else if (sender == listViewTemplates)
				SortListViewColumn(sender, e, ref _sortColumnTemplateItems, ref _sortDirectionTemplateItems);
			else
				return;
		}

		private void SortListViewColumn(object sender, RoutedEventArgs e, ref GridViewColumnHeader columnHeader, ref ListSortDirection sortDirection) {
			GridViewColumnHeader column = e.OriginalSource as GridViewColumnHeader;
			if (column == null)
				return;
			
			if (columnHeader == column)
				sortDirection = sortDirection == ListSortDirection.Ascending ?
												 ListSortDirection.Descending :
												 ListSortDirection.Ascending;
			else {
				if (columnHeader != null) {
					columnHeader.Column.HeaderTemplate = null;
					columnHeader.Column.Width = columnHeader.ActualWidth - 20;
				}

				columnHeader = column;
				sortDirection = ListSortDirection.Ascending;
				column.Column.Width = column.ActualWidth + 20;
			}

			if (sortDirection == ListSortDirection.Ascending)
				column.Column.HeaderTemplate = Resources["ArrowUp"] as DataTemplate;
			else
				column.Column.HeaderTemplate = Resources["ArrowDown"] as DataTemplate;

			string header = string.Empty;

			Binding b = columnHeader.Column.DisplayMemberBinding as Binding;
			if (b != null)
				header = b.Path.Path;

			ICollectionView resultDataView = CollectionViewSource.GetDefaultView((sender as ListView).ItemsSource);
			resultDataView.SortDescriptions.Clear();
			resultDataView.SortDescriptions.Add(new SortDescription(header, sortDirection));
		}

		
		private void CheckBox_CheckedChanged(object sender, RoutedEventArgs e) {
			if (dateTimePicker != null)
				dateTimePicker.IsEnabled =
					checkBoxSelectedTime.IsChecked == true &&
					checkBoxSelectedTime.IsEnabled;
			if (buttonMessageSend != null)
				buttonMessageSend.IsEnabled =
					(checkBoxNow.IsChecked == true && checkBoxNow.IsEnabled) ||
					(checkBoxSelectedTime.IsChecked == true && checkBoxSelectedTime.IsEnabled);
		}
	}
}
