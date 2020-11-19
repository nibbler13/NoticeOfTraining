using SmsSend.Items;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace SmsSend {
	/// <summary>
	/// Interaction logic for WindowSelectDateTime.xaml
	/// </summary>
	public partial class WindowSelectDateTime : Window {
		private ItemDateTime itemDateTime;

		private Dictionary<string, CheckBox> checkBoxes = new Dictionary<string, CheckBox>();

		public WindowSelectDateTime(ItemDateTime itemDateTime) {
			InitializeComponent();
			this.itemDateTime = itemDateTime;

			DatePickerSelected.DisplayDateStart = DateTime.Now;

			checkBoxes.Add("0", CheckBox0);
			checkBoxes.Add("1", CheckBox1);
			checkBoxes.Add("2", CheckBox2);
			checkBoxes.Add("3", CheckBox3);
			checkBoxes.Add("4", CheckBox4);
			checkBoxes.Add("5", CheckBox5);
			checkBoxes.Add("6", CheckBox6);
			checkBoxes.Add("7", CheckBox7);
			checkBoxes.Add("8", CheckBox8);
			checkBoxes.Add("9", CheckBox9);
			checkBoxes.Add("10", CheckBox10);
			checkBoxes.Add("11", CheckBox11);
			checkBoxes.Add("12", CheckBox12);
			checkBoxes.Add("13", CheckBox13);
			checkBoxes.Add("14", CheckBox14);
			checkBoxes.Add("15", CheckBox15);
			checkBoxes.Add("16", CheckBox16);
			checkBoxes.Add("17", CheckBox17);
			checkBoxes.Add("18", CheckBox18);
			checkBoxes.Add("19", CheckBox19);
			checkBoxes.Add("20", CheckBox20);
			checkBoxes.Add("21", CheckBox21);
			checkBoxes.Add("22", CheckBox22);
			checkBoxes.Add("23", CheckBox23);

			foreach (KeyValuePair<int, bool> time in itemDateTime.Times) 
				try {
					checkBoxes[time.Key.ToString()].IsChecked = time.Value;
				} catch (Exception) { }

			DatePickerSelected.SelectedDate = itemDateTime.SelectedDate;
		}

		private void ButtonSave_Click(object sender, RoutedEventArgs e) {
			string msg = string.Empty;

			if (DatePickerSelected.SelectedDate == null)
				msg = "Не выбрана дата";

			if (DatePickerSelected.SelectedDate.Value.Date < DateTime.Now.Date)
				msg = "Выбрана прошедшая дата";

			if (string.IsNullOrEmpty(msg) && DatePickerSelected.SelectedDate.Value.Date == DateTime.Now.Date) {
				foreach (KeyValuePair<string, CheckBox> checkBox in checkBoxes) {
					if (checkBox.Value.IsChecked != true)
						continue;

					if (int.Parse(checkBox.Key) < DateTime.Now.Hour) {
						msg = "Выбраны прошедшие часы";
						break;
					}
				}
			}

			if (string.IsNullOrEmpty(msg)) {
				bool isAnySelected = false;
				foreach (KeyValuePair<string, CheckBox> checkBox in checkBoxes) {
					if (checkBox.Value.IsChecked == true) {
						isAnySelected = true;
						break;
					}
				}

				if (!isAnySelected)
					msg = "Не выбрано ни одного времени";
			}

			if (!string.IsNullOrEmpty(msg)) {
				MessageBox.Show(
					this,
					msg,
					"Ошибка сохранения",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);
				return;
			}

			itemDateTime.SelectedDate = DatePickerSelected.SelectedDate.Value;
			foreach (KeyValuePair<string, CheckBox> checkBox in checkBoxes)
				itemDateTime.Times[int.Parse(checkBox.Key)] = checkBox.Value.IsChecked.Value;

			itemDateTime.NotifyPropertyChanged("SelectedTime");

			DialogResult = true;
		}

		private void ButtonSelectAllTime_Click(object sender, RoutedEventArgs e) {
			foreach (KeyValuePair<string, CheckBox> checkbox in checkBoxes)
				checkbox.Value.IsChecked = true;
		}

		private void ButtonSelectWorkingTime_Click(object sender, RoutedEventArgs e) {
			ButtonSelectNoneTime_Click(null, null);
			for (int i = 8; i < 21; i++)
				checkBoxes[i.ToString()].IsChecked = true;
		}

		private void ButtonSelectNoneTime_Click(object sender, RoutedEventArgs e) {
			foreach (KeyValuePair<string, CheckBox> checkbox in checkBoxes)
				checkbox.Value.IsChecked = false;
		}
	}
}
