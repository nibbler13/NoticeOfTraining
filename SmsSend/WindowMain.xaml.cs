using SmsSend.Items;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Serialization;

namespace SmsSend {
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class WindowMain : Window, INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}


		private readonly string userFolder = Path.Combine(Directory.GetCurrentDirectory(), "UserData", Environment.UserName);
		private readonly string savedPhoneNumbersFile = "PhoneNumberSavedItems.xml";
		private readonly string templatesFile = "TemplateItems.xml";
		private readonly string historyFile = "HistoryItems.xml";
		private readonly string savedPhoneNumbersPath;
		private readonly string templatePersonalPath;
		private readonly string templateGeneralPath;
		private readonly string historyPath;
		private uint mailingMessagesTotalQuantity = 0;


		public ObservableCollection<ItemPhoneNumber> PhoneNumberSavedItems { get; set; }
		public ObservableCollection<ItemPhoneNumber> PhoneNumberReceiversItems { get; set; }
		public ObservableCollection<ItemTemplate> TemplateItems { get; set; }
		public ObservableCollection<ItemHistory> HistoryItems { get; set; }
		public ObservableCollection<ItemPhoneNumber> PhoneNumberMailingItems { get; set; }
		public ObservableCollection<ItemDateTime> DateTimeMailingItems { get; set; }


		private string textMailingSymbolsCount;
		public string TextMailingSymbolsCount { 
			get { return textMailingSymbolsCount; }
			set {
				if (value != textMailingSymbolsCount) {
					textMailingSymbolsCount = value;
					NotifyPropertyChanged();
				}
			}
		}


		private string textMailing;
		public string TextMailing { 
			get { return textMailing; }
			set {
				if (value != textMailing) {
					textMailing = value;
					NotifyPropertyChanged();
					TextMailingSymbolsCount = "Символов: " + (TextMailing == null ? "0" : TextMailing.Length.ToString());
					UpdateMailingMessageCounter();
				}
			}
		}


		private string textPhoneNumberReceiversCount;
		public string TextPhoneNumberReceiversCount {
			get { return textPhoneNumberReceiversCount; }
			set {
				if (value != textPhoneNumberReceiversCount) {
					textPhoneNumberReceiversCount = value;
					NotifyPropertyChanged();
				}
			}
		}


		private string textMessageSymbolsCount;
		public string TextMessageSymbolsCount {
			get { return textMessageSymbolsCount; }
			set {
				if (value != textMessageSymbolsCount) {
					textMessageSymbolsCount = value;
					NotifyPropertyChanged();
				}
			}
		}


		private string textMessage;
		public string TextMessage {
			get { return textMessage; }
			set {
				if (value != textMessage) {
					textMessage = value;
					NotifyPropertyChanged();
					TextMessageSymbolsCount = "Символов: " + (TextMessage == null ? "0" : TextMessage.Length.ToString());
				}
			}
		}


		private string textMailingSendingCount;
		public string TextMailingSendingCount {
			get { return textMailingSendingCount; }
			set {
				if (value != textMailingSendingCount) {
					textMailingSendingCount = value;
					NotifyPropertyChanged();
				}
			}
		}


		private string textPhoneNumberMailingCount;
		public string TextPhoneNumberMailingCount {
			get { return textPhoneNumberMailingCount; }
			set {
				if (value != textPhoneNumberMailingCount) {
					textPhoneNumberMailingCount = value;
					NotifyPropertyChanged();
				}
			}
		}


		private string mailingMessageCounter;
		public string MailingMessageCounter {
			get { return mailingMessageCounter; }
			set {
				if (value != mailingMessageCounter) {
					mailingMessageCounter = value;
					NotifyPropertyChanged();
				}
			}
		}


		private uint mailingQuantityAtTime;
		public uint MailingQuantityAtTime { 
			get { return mailingQuantityAtTime; }
			set {
				if (value != mailingQuantityAtTime) {
					mailingQuantityAtTime = value;
					NotifyPropertyChanged();
					UpdateMailingMessageCounter();
				}
			}
		}

		private string textMailingAdminPhone;
		public string TextMailingAdminPhone {
			get { return textMailingAdminPhone; }
			set {
				if (value != textMailingAdminPhone) {
					textMailingAdminPhone = value;
					NotifyPropertyChanged();
				}
			}
		}


		private ItemPhoneNumber phoneNumberAdmin;




		public WindowMain() {
			InitializeComponent();

			DataContext = this;

			PhoneNumberSavedItems = new ObservableCollection<ItemPhoneNumber>();
			PhoneNumberReceiversItems = new ObservableCollection<ItemPhoneNumber>();
			TemplateItems = new ObservableCollection<ItemTemplate>();
			HistoryItems = new ObservableCollection<ItemHistory>();
			PhoneNumberMailingItems = new ObservableCollection<ItemPhoneNumber>();
			DateTimeMailingItems = new ObservableCollection<ItemDateTime>();

			savedPhoneNumbersPath = Path.Combine(userFolder, savedPhoneNumbersFile);
			templatePersonalPath = Path.Combine(userFolder, templatesFile);
			templateGeneralPath = Path.Combine(Directory.GetCurrentDirectory(), templatesFile);
			historyPath = Path.Combine(userFolder, historyFile);

			PhoneNumberSavedItems.CollectionChanged += (s, e) => {
				SetButtonsReceiversAllEnable();
			};

			PhoneNumberReceiversItems.CollectionChanged += (s, e) => {
				SetButtonsReceiversAllEnable();
				TextPhoneNumberReceiversCount = "Количество: " + PhoneNumberReceiversItems.Count;
			};

			HistoryItems.CollectionChanged += (s, e) => {
				buttonClearHistory.IsEnabled = HistoryItems.Count > 0;
			};

			PhoneNumberMailingItems.CollectionChanged += PhoneNumberMailingItems_CollectionChanged;
			DateTimeMailingItems.CollectionChanged += DateTimeMailingItems_CollectionChanged;
			
			dateTimePicker.DefaultValue = DateTime.Now;
			dateTimePicker.ValueChanged += DateTimePicker_ValueChanged;

			buttonClearHistory.IsEnabled = HistoryItems.Count > 0;

			TextMailing = string.Empty;
			TextMessage = string.Empty;

			TextPhoneNumberReceiversCount = "Количество: 0";
			TextPhoneNumberMailingCount = "Количество: 0";
			TextMailingSendingCount = "Количество запусков рассылки: 0";
			MailingMessageCounter = "Итого будет отправлено сообщений: 0";

			phoneNumberAdmin = new ItemPhoneNumber();

			Loaded += async (s, e) => {
				await Task.Run(new Action(() => {
					LoadCollections();
				}));

				CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(DataGridPhoneNumbersSaved.ItemsSource);
				view.Filter += UserFilter;
			};

			Closed += MainWindow_Closed;
		}

		private void LoadCollections() {
			try {
				if (!Directory.Exists(userFolder))
					Directory.CreateDirectory(userFolder);
			} catch (Exception exception) {
				Console.WriteLine(exception.Message + Environment.NewLine + exception.StackTrace);
			}

			XmlSerializer serializerPhoneItem = new XmlSerializer(typeof(ObservableCollection<ItemPhoneNumber>));
			XmlSerializer serializerTemplatePersonalItem = new XmlSerializer(typeof(ObservableCollection<ItemTemplate>));
			XmlSerializer serializerTemplateGeneralItem = new XmlSerializer(typeof(ObservableCollection<ItemTemplate>));
			XmlSerializer serializerHistoryItem = new XmlSerializer(typeof(ObservableCollection<ItemHistory>));

			try {
				using (FileStream fileStream = new FileStream(savedPhoneNumbersPath, FileMode.Open)) {
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
				using (FileStream fileStream = new FileStream(templatePersonalPath, FileMode.Open)) {
					ObservableCollection<ItemTemplate> observableCollection =
						serializerTemplatePersonalItem.Deserialize(fileStream) as ObservableCollection<ItemTemplate>;
					foreach (ItemTemplate item in observableCollection)
						Application.Current.Dispatcher.Invoke(() => {
							TemplateItems.Add(item);
						});
				}
			} catch (Exception exception) {
				Console.WriteLine(exception.Message + Environment.NewLine + exception.StackTrace);
			}

			try {
				using (FileStream fileStream = new FileStream(templateGeneralPath, FileMode.Open)) {
					ObservableCollection<ItemTemplate> observableCollection =
						serializerTemplateGeneralItem.Deserialize(fileStream) as ObservableCollection<ItemTemplate>;
					foreach (ItemTemplate item in observableCollection)
						Application.Current.Dispatcher.Invoke(() => {
							IEnumerable<ItemTemplate> existingItems =
								from templateItem in TemplateItems
								where templateItem.Name.Equals(item.Name) && templateItem.Message.Equals(item.Message)
								select templateItem;

							if (existingItems.Count() == 0)
								TemplateItems.Add(item);
						});
				}
			} catch (Exception exception) {
				Console.WriteLine(exception.Message + Environment.NewLine + exception.StackTrace);
			}

			try {
				using (FileStream fileStream = new FileStream(historyPath, FileMode.Open)) {
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
				using (StreamWriter writer = new StreamWriter(savedPhoneNumbersPath))
					serializerPhoneItem.Serialize(writer, PhoneNumberSavedItems);
			} catch (Exception exception) {
				Console.WriteLine(exception.Message + Environment.NewLine + exception.StackTrace);
			}

			try {
				using (StreamWriter writer = new StreamWriter(templatePersonalPath))
					serializerTemplateItem.Serialize(writer, TemplateItems);
			} catch (Exception exception) {
				Console.WriteLine(exception.Message + Environment.NewLine + exception.StackTrace);
			}

			try {
				using (StreamWriter writer = new StreamWriter(historyPath))
					serializerHistoryItem.Serialize(writer, HistoryItems);
			} catch (Exception exception) {
				Console.WriteLine(exception.Message + Environment.NewLine + exception.StackTrace);
			}
		}




		#region TabItem - General SMS Sending

		#region PhoneNumberSaved
		private void ButtonPhoneNumberAdd_Click(object sender, RoutedEventArgs e) {
			ItemPhoneNumber itemPhoneNumber = new ItemPhoneNumber();
			WindowPhoneNumber windowPhoneNumber = new WindowPhoneNumber(itemPhoneNumber);
			windowPhoneNumber.Owner = this;
			windowPhoneNumber.ShowDialog();

			if (windowPhoneNumber.DialogResult == true) {
				if (sender == buttonPhoneNumberAdd)
					PhoneNumberSavedItems.Add(windowPhoneNumber.PhoneNumberItem);
				else
					PhoneNumberMailingItems.Add(windowPhoneNumber.PhoneNumberItem);
			}
		}

		private void ButtonPhoneNumberEdit_Click(object sender, RoutedEventArgs e) {
			ItemPhoneNumber phoneNumberItem;

			if (sender == buttonPhoneNumberEdit)
				phoneNumberItem = DataGridPhoneNumbersSaved.SelectedItem as ItemPhoneNumber;
			else
				phoneNumberItem = DataGridPhoneNumbersMailing.SelectedItem as ItemPhoneNumber;

			if (phoneNumberItem == null)
				return;

			WindowPhoneNumber windowPhoneNumber = new WindowPhoneNumber(phoneNumberItem);
			windowPhoneNumber.Owner = this;
			windowPhoneNumber.ShowDialog();
		}

		private void ButtonPhoneNumberRemove_Click(object sender, RoutedEventArgs e) {
			if (MessageBox.Show(
				this,
				"Вы уверены, что хотите удалить выбранные элементы?",
				"Удаление элементов",
				MessageBoxButton.YesNo,
				MessageBoxImage.Question) == MessageBoxResult.No)
				return;

			List<ItemPhoneNumber> itemsToRemove = new List<ItemPhoneNumber>();

			if (sender == buttonPhoneNumberRemove) {
				foreach (ItemPhoneNumber item in DataGridPhoneNumbersSaved.SelectedItems)
					itemsToRemove.Add(item);
				foreach (ItemPhoneNumber item in itemsToRemove)
					PhoneNumberSavedItems.Remove(item);
			} else {
				foreach (ItemPhoneNumber item in DataGridPhoneNumbersMailing.SelectedItems)
					itemsToRemove.Add(item);
				foreach (ItemPhoneNumber item in itemsToRemove)
					PhoneNumberMailingItems.Remove(item);
			}
		}

		private void ButtonPhoneNumberImport_Click(object sender, RoutedEventArgs e) {
			WindowImportExcel windowImportExcel = new WindowImportExcel();
			windowImportExcel.Owner = this;
			windowImportExcel.ShowDialog();

			if (windowImportExcel.DialogResult != true)
				return;

			int added = 0;
			foreach (ItemPhoneNumber item in windowImportExcel.PhoneNumbers) {
				List<ItemPhoneNumber> matches;

				if (sender == buttonPhoneNumberImport)
					matches = PhoneNumberSavedItems.Where(p => p.PhoneNumber.Equals(item.PhoneNumber)).ToList();
				else
					matches = PhoneNumberMailingItems.Where(p => p.PhoneNumber.Equals(item.PhoneNumber)).ToList();

				if (matches != null && matches.Count != 0)
					continue;

				if (sender == buttonPhoneNumberImport)
					PhoneNumberSavedItems.Add(item);
				else
					PhoneNumberMailingItems.Add(item);

				added++;
			}

			if (added > 0)
				MessageBox.Show("В список номеров добавлено записей: " + added, "",
					MessageBoxButton.OK, MessageBoxImage.Information);
			else
				MessageBox.Show("Считанные номера уже добавлены ранее", "",
					MessageBoxButton.OK, MessageBoxImage.Warning);
		}

		private bool UserFilter(object item) {
			if (string.IsNullOrEmpty(textBoxFilter.Text))
				return true;
			else
				return ((item as ItemPhoneNumber).Name.IndexOf(textBoxFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0);
		}

		private void TextBoxFilter_TextChanged(object sender, TextChangedEventArgs e) {
			CollectionViewSource.GetDefaultView(DataGridPhoneNumbersSaved.ItemsSource).Refresh();
			ButtonClearFilter.IsEnabled = textBoxFilter.Text.Length > 0;
		}

		private void DataGridPhoneNumberSavedItem_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			ButtonReceiversAddSelected_Click(DataGridPhoneNumbersSaved, new RoutedEventArgs());
		}

		private void DataGridPhoneNumbersSaved_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			bool isItemsSelected = DataGridPhoneNumbersSaved.SelectedItems.Count > 0;
			buttonPhoneNumberRemove.IsEnabled = isItemsSelected;
			buttonReceiversAddSelected.IsEnabled = isItemsSelected;
			buttonPhoneNumberEdit.IsEnabled = DataGridPhoneNumbersSaved.SelectedItems.Count == 1;
		}

		private void ButtonClearFilter_Click(object sender, RoutedEventArgs e) {
			textBoxFilter.Text = string.Empty;
		}

		private void ButtonPhoneNumberSavedClear_Click(object sender, RoutedEventArgs e) {
			if (MessageBox.Show(
				this,
				"Вы уверены, что хотите очистить список номеров? Это действие нельзя отменить.",
				"",
				MessageBoxButton.YesNo,
				MessageBoxImage.Question) == MessageBoxResult.No)
				return;

			if (sender == ButtonPhoneNumberSavedClear)
				PhoneNumberSavedItems.Clear();
			else
				PhoneNumberMailingItems.Clear();
		}
		#endregion


		#region Moving buttons between saved and receivers DataGrids
		private void ButtonReceiversAddSelected_Click(object sender, RoutedEventArgs e) {
			List<ItemPhoneNumber> itemsToMove = new List<ItemPhoneNumber>();
			foreach (ItemPhoneNumber item in DataGridPhoneNumbersSaved.SelectedItems)
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
			foreach (ItemPhoneNumber item in DataGridPhoneNumbersReceivers.SelectedItems)
				itemsToMove.Add(item);
			foreach (ItemPhoneNumber item in itemsToMove) {
				PhoneNumberSavedItems.Add(item);
				PhoneNumberReceiversItems.Remove(item);
			}
		}

		private void SetButtonsReceiversAllEnable() {
			buttonReceiversRemoveAll.IsEnabled = PhoneNumberReceiversItems.Count > 0;
			buttonReceiversAddAll.IsEnabled = PhoneNumberSavedItems.Count > 0;
			ButtonPhoneNumberSavedClear.IsEnabled = PhoneNumberSavedItems.Count > 0;
		}
		#endregion


		#region PhoneNumberReceivers
		private void DataGridPhoneNumbersReceivers_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			buttonReceiversRemoveSelected.IsEnabled = DataGridPhoneNumbersReceivers.SelectedItems.Count > 0;
		}

		private void DataGridPhoneNumberReceiversItem_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			ButtonReceiversRemoveSelected_Click(DataGridPhoneNumbersReceivers, new RoutedEventArgs());
		}
		#endregion


		#region MessageTemplate
		private void ButtonTemplateAdd_Click(object sender, RoutedEventArgs e) {
			WindowTemplate windowTemplate = new WindowTemplate();
			windowTemplate.Owner = this;
			windowTemplate.ShowDialog();

			if (windowTemplate.DialogResult == true)
				TemplateItems.Add(windowTemplate.TemplateItem);
		}

		private void ButtonTemplateEdit_Click(object sender, RoutedEventArgs e) {
			WindowTemplate windowTemplate = new WindowTemplate(DataGridTemplates.SelectedItem as ItemTemplate);
			windowTemplate.Owner = this;
			windowTemplate.ShowDialog();
		}

		private void ButtonTemplateRemove_Click(object sender, RoutedEventArgs e) {
			if (MessageBox.Show("Вы уверены, что хотите удалить выбранные элементы?", "Удаление элементов",
				MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
				return;

			List<ItemTemplate> itemsToRemove = new List<ItemTemplate>();
			foreach (ItemTemplate item in DataGridTemplates.SelectedItems)
				itemsToRemove.Add(item);
			foreach (ItemTemplate item in itemsToRemove)
				TemplateItems.Remove(item);
		}

		private void ButtonTemplateUse_Click(object sender, RoutedEventArgs e) {
			if (TextMessage.Length > 0) {
				if (MessageBox.Show("В поле для ввода сообщения уже имеется текст. " +
					"Использование шаблона заменит этот текст. Продолжить?", "Использование шаблона",
					MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
					return;
			}

			ItemTemplate itemTemplate = DataGridTemplates.SelectedItem as ItemTemplate;

			if (itemTemplate == null)
				return;

			TextMessage = itemTemplate.Message;
		}

		private void DataGridTemplates_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			bool isOneItemSelected = DataGridTemplates.SelectedItems.Count == 1;
			buttonTemplateUse.IsEnabled = isOneItemSelected;
			buttonTemplateEdit.IsEnabled = isOneItemSelected;
			buttonTemplateRemove.IsEnabled = DataGridTemplates.SelectedItems.Count > 0;
		}

		private void DataGridTemplateItem_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			ButtonTemplateUse_Click(buttonTemplateUse, new RoutedEventArgs());
		}
		#endregion


		#region Message Sending
		private void ButtonMessageSend_Click(object sender, RoutedEventArgs e) {
			if (PhoneNumberReceiversItems.Count == 0) {
				if (MessageBox.Show(
					this,
					"В списке получателей отсутствуют адресаты, хотите добавить сейчас?",
					"Отсутствуют получатели",
					MessageBoxButton.YesNo,
					MessageBoxImage.Question) == MessageBoxResult.No)
					return;

				ItemPhoneNumber itemPhoneNumber = new ItemPhoneNumber();
				WindowPhoneNumber windowPhoneNumber = new WindowPhoneNumber(itemPhoneNumber) {
					Owner = this
				};

				windowPhoneNumber.ShowDialog();
				if (windowPhoneNumber.DialogResult != true)
					return;

				PhoneNumberReceiversItems.Add(windowPhoneNumber.PhoneNumberItem);
			}

			if (TextMessage.Length == 0) {
				MessageBox.Show(
					this,
					"Не задан текст сообщения",
					"",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);
				return;
			}

			ItemHistory itemHistory = new ItemHistory(
				checkBoxNow.IsChecked == true ? true : false,
				checkBoxSelectedTime.IsChecked == true ? true : false,
				checkBoxSelectedTime.IsChecked == true ? dateTimePicker.Value : null,
				TextMessage,
				PhoneNumberReceiversItems.ToList()
			);

			WindowSend windowSend = new WindowSend(itemHistory) {
				Owner = this
			};

			windowSend.ShowDialog();

			if (windowSend.DialogResult == true)
				HistoryItems.Add(itemHistory);
		}

		private void TextBoxMessage_TextChanged(object sender, TextChangedEventArgs e) {
			bool isTextEntered = TextMessage.Length > 0;
			checkBoxNow.IsEnabled = isTextEntered;
			checkBoxSelectedTime.IsEnabled = isTextEntered;
			CheckBox_CheckedChanged(checkBoxNow, new RoutedEventArgs());
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

		private void DateTimePicker_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
			if (dateTimePicker == null || e.NewValue == null || !IsLoaded)
				return;

			try {
				if ((DateTime)e.NewValue < DateTime.Now) {
					dateTimePicker.Value = DateTime.Now;
					e.Handled = true;
				}

				if ((DateTime)e.NewValue > DateTime.Now.AddDays(2)) {
					MessageBox.Show("Отложенная отправка СМС допускает разницу во времени не более 48 часов. " +
						"Сброс до максимально возможного времени.", "Ошибка времени", MessageBoxButton.OK, MessageBoxImage.Warning);

					dateTimePicker.Value = DateTime.Now.AddDays(2).AddMinutes(-1);
					e.Handled = true;
				}
			} catch (Exception) { }
		}
		#endregion

		#endregion




		#region TabItem - Mailing

		#region PhoneNumbers
		private void PhoneNumberMailingItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
			UpdateMailingMessageCounter();
			ButtonPhoneNumberMailingClear.IsEnabled = PhoneNumberMailingItems.Count > 0;
		}

		private void DataGridPhoneNumbersMailing_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			ButtonPhoneNumberMailingEdit.IsEnabled = DataGridPhoneNumbersMailing.SelectedItems.Count == 1;
			ButtonPhoneNumberMailingRemove.IsEnabled = DataGridPhoneNumbersMailing.SelectedItems.Count > 0;
		}

		private void DataGridPhoneNumbersMailing_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			ButtonPhoneNumberEdit_Click(ButtonPhoneNumberMailingEdit, null);
		}
		#endregion


		#region DateTime
		private void DateTimeMailingItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
			UpdateMailingMessageCounter();
		}

		private void ButtonDateTimeMailingClear_Click(object sender, RoutedEventArgs e) {
			if (MessageBox.Show(
				this,
				"Вы уверены, что хотите очистить список дат рассылки? Это действие нельзя отменить.",
				"",
				MessageBoxButton.YesNo,
				MessageBoxImage.Question) == MessageBoxResult.No)
				return;

			DateTimeMailingItems.Clear();
		}

		private void ButtonDateTimeMailingRemove_Click(object sender, RoutedEventArgs e) {
			if (MessageBox.Show(
				this,
				"Вы уверены, что хотите удалить выбранные элементы?",
				"Удаление элементов",
				MessageBoxButton.YesNo,
				MessageBoxImage.Question) == MessageBoxResult.No)
				return;

			List<ItemDateTime> itemsToRemove = new List<ItemDateTime>();

			foreach (ItemDateTime item in DataGridDateTimeMailingSelect.SelectedItems)
				itemsToRemove.Add(item);

			foreach (ItemDateTime item in itemsToRemove)
				DateTimeMailingItems.Remove(item);
		}

		private void ButtonDateTimeMailingEdit_Click(object sender, RoutedEventArgs e) {
			ItemDateTime itemDateTime = DataGridDateTimeMailingSelect.SelectedItem as ItemDateTime;

			if (itemDateTime == null)
				return;

			WindowSelectDateTime windowSelectDateTime = new WindowSelectDateTime(itemDateTime) {
				Owner = this
			};

			windowSelectDateTime.ShowDialog();
			DateTimeMailingItems_CollectionChanged(null, null);
		}

		private void ButtonDateTimeMailingAdd_Click(object sender, RoutedEventArgs e) {
			ItemDateTime itemDateTime = new ItemDateTime();

			WindowSelectDateTime windowSelectDateTime = new WindowSelectDateTime(itemDateTime) {
				Owner = this
			};

			if (windowSelectDateTime.ShowDialog() == true)
				DateTimeMailingItems.Add(itemDateTime);
		}

		private void DataGridDateTimeMailingSelect_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			ButtonDateTimeMailingEdit.IsEnabled = DataGridDateTimeMailingSelect.SelectedItems.Count == 1;
			ButtonDateTimeMailingCopy.IsEnabled = DataGridDateTimeMailingSelect.SelectedItems.Count == 1;
			ButtonDateTimeMailingRemove.IsEnabled = DataGridDateTimeMailingSelect.SelectedItems.Count > 0;
		}

		private void DataGridDateTimeMailingSelect_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			ButtonDateTimeMailingEdit_Click(null, null);
		}

		private void ButtonDateTimeMailingCopy_Click(object sender, RoutedEventArgs e) {
			ItemDateTime itemDateTime = DataGridDateTimeMailingSelect.SelectedItem as ItemDateTime;
			if (itemDateTime == null)
				return;

			ItemDateTime itemDateTimeNew = new ItemDateTime();
			itemDateTimeNew.SelectedDate = itemDateTime.SelectedDate.AddDays(1);
			foreach (KeyValuePair<int, bool> time in itemDateTime.Times)
				itemDateTimeNew.Times[time.Key] = time.Value;

			WindowSelectDateTime windowSelectDateTime = new WindowSelectDateTime(itemDateTimeNew) {
				Owner = this
			};

			windowSelectDateTime.ShowDialog();

			if (windowSelectDateTime.DialogResult == true) {
				DateTimeMailingItems.Add(itemDateTimeNew);
				DataGridDateTimeMailingSelect.SelectedItem = itemDateTimeNew;
			}

		}
		#endregion

		#region Mailing Message Send

		private void CheckBoxAddAdmin_Checked(object sender, RoutedEventArgs e) {
			TextBoxAdminPhone.IsEnabled = CheckBoxAddAdmin.IsChecked == true;
			ButtonEditAdminPhone.IsEnabled = CheckBoxAddAdmin.IsChecked == true;
		}

		private void ButtonEditAdminPhone_Click(object sender, RoutedEventArgs e) {
			WindowPhoneNumber windowPhoneNumber = new WindowPhoneNumber(phoneNumberAdmin) {
				Owner = this
			};

			windowPhoneNumber.ShowDialog();

			if (windowPhoneNumber.DialogResult == true) 
				TextMailingAdminPhone = phoneNumberAdmin.PhoneNumber + " (" + phoneNumberAdmin.Name + ")";
		}

		private void UpdateMailingMessageCounter() {
			int dateTimeCount = 0;
			foreach (ItemDateTime itemDateTime in DateTimeMailingItems)
				foreach (KeyValuePair<int, bool> pair in itemDateTime.Times)
					if (pair.Value)
						dateTimeCount++;

			TextMailingSendingCount = "Количество запусков рассылки: " + dateTimeCount;

			ButtonDateTimeMailingClear.IsEnabled = DateTimeMailingItems.Count > 0;

			int phoneNumbersCount = PhoneNumberMailingItems.Count;
			TextPhoneNumberMailingCount = "Количество: " + phoneNumbersCount;

			int messageCount = TextMailing.Length == 0 ? 0 : 1;
			int totalMessageAtSchedule = dateTimeCount * (int)MailingQuantityAtTime;
			mailingMessagesTotalQuantity = (uint)(totalMessageAtSchedule <= phoneNumbersCount ? totalMessageAtSchedule : phoneNumbersCount);
			mailingMessagesTotalQuantity *= (uint)messageCount;

			MailingMessageCounter = "Итого будет отправлено сообщений: " + mailingMessagesTotalQuantity;
		}

		private void ButtonMailingMessageSend_Click(object sender, RoutedEventArgs e) {
			string msgError = string.Empty;

			if (PhoneNumberMailingItems.Count == 0)
				msgError = "Не указаны получатели рассылки";

			if (textMailing.Length == 0)
				msgError = "Не указан текст рассылки";

			if (DateTimeMailingItems.Count == 0)
				msgError = "Не указаны даты и время рассылки";

			if (MailingQuantityAtTime == 0)
				msgError = "Не указано количество отправляемых сообщений за один раз";

			if (mailingMessagesTotalQuantity == 0)
				msgError = "Итоговое количество отправляемых сообщений равно 0";

			if (!string.IsNullOrEmpty(msgError)) {
				MessageBox.Show(
					this,
					msgError,
					string.Empty,
					MessageBoxButton.OK,
					MessageBoxImage.Warning);

				return;
			}

			if (MessageBox.Show(
				this,
				"Будет сформировано задание" + Environment.NewLine +
				"для рассылки сообщений (" + mailingMessagesTotalQuantity + " шт.)" + 
				Environment.NewLine + Environment.NewLine +
				"Продолжить выполнение?",
				string.Empty,
				MessageBoxButton.YesNo,
				MessageBoxImage.Question) == MessageBoxResult.No)
				return;

			ItemHistory itemHistory = new ItemHistory(
				TextMailing,
				PhoneNumberMailingItems.ToList(),
				DateTimeMailingItems.ToList(),
				MailingQuantityAtTime,
				mailingMessagesTotalQuantity
			);

			if (CheckBoxAddAdmin.IsChecked == true)
				itemHistory.PhoneNumberAdmin = phoneNumberAdmin;

			WindowSend windowSend = new WindowSend(itemHistory) {
				Owner = this
			};

			windowSend.ShowDialog();

			if (windowSend.DialogResult == true)
				foreach (ItemHistory item in windowSend.MailingHistory)
					HistoryItems.Add(item);
		}
		#endregion

		#endregion




		#region TabItem - History
		private void DataGridHistory_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			buttonDetails.IsEnabled = DataGridHistory.SelectedItems.Count == 1;
		}

		private void DataGridHistoryItem_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			ButtonDetails_Click(sender, new RoutedEventArgs());
		}

		private void ButtonClearHistory_Click(object sender, RoutedEventArgs e) {
			if (MessageBox.Show("Это действие нельзя отменить. Вы уверены, что хотите продолжить?",
				"Очистка истории",
				MessageBoxButton.YesNo,
				MessageBoxImage.Question) == MessageBoxResult.No)
				return;

			HistoryItems.Clear();
		}

		private void ButtonDetails_Click(object sender, RoutedEventArgs e) {
			ItemHistory itemHistory = DataGridHistory.SelectedItem as ItemHistory;

			if (itemHistory == null)
				return;

			WindowHistoryItemDetail itemDetail = new WindowHistoryItemDetail(itemHistory) {
				Owner = this
			};

			itemDetail.ShowDialog();
		}
		#endregion
	}
}
