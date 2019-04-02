﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для WindowImportExcel.xaml
    /// </summary>
    public partial class WindowImportExcel : Window {
		public List<ItemPhoneNumber> PhoneNumbers { get; set; }

        public WindowImportExcel() {
            InitializeComponent();
			PhoneNumbers = new List<ItemPhoneNumber>();
		}

		private void ButtonSelectFile_Click(object sender, RoutedEventArgs e) {
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Книга Excel (*.xls*)|*.xls*";
			openFileDialog.CheckFileExists = true;
			openFileDialog.CheckPathExists = true;
			openFileDialog.Multiselect = false;
			openFileDialog.RestoreDirectory = true;

			if (openFileDialog.ShowDialog() == true) {
				textBoxSelectedFile.Text = openFileDialog.FileName;
				List<TextBox> textBoxes = new List<TextBox>() {
					textBoxSheetName,
					textBoxColumnName,
					textBoxColumnPhoneNumber
				};

				foreach (TextBox textBox in textBoxes)
					textBox.IsEnabled = true;

				TextBox_TextChanged(null, null);
			}
		}

		private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {
			if (buttonImport == null)
				return;

			buttonImport.IsEnabled =
				textBoxSheetName.Text.Length > 0 &&
				textBoxColumnName.Text.Length > 0 &&
				textBoxColumnPhoneNumber.Text.Length > 0;
		}

		private async void ButtonImport_Click(object sender, RoutedEventArgs e) {
			buttonImport.IsEnabled = false;
			stackPanel.Visibility = Visibility.Hidden;
			textBox.Visibility = Visibility.Visible;
			Cursor = Cursors.Wait;

			string fileName = textBoxSelectedFile.Text;
			string sheetName = textBoxSheetName.Text;
			string columnName = textBoxColumnName.Text;
			string columnPhoneNumber = textBoxColumnPhoneNumber.Text;

			List<ItemPhoneNumber> results = new List<ItemPhoneNumber>();

			await Task.Run(new Action(() => {
				results = ExcelReader.ReadPhoneNumbers(
					fileName,
					sheetName,
					columnName,
					columnPhoneNumber,
					UpdateProgress);
			}));
			
			Cursor = Cursors.Arrow;

			if (results.Count == 0) {
				MessageBox.Show("Не удалось считать ни одного номера телефона", "", 
					MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			MessageBox.Show("Считано номеров телефонов: " + results.Count, "Завершено", 
				MessageBoxButton.OK, MessageBoxImage.Information);

			PhoneNumbers = results;
			DialogResult = true;
		}

		private void UpdateProgress(double progress, string text) {
			Application.Current.Dispatcher.Invoke(new Action(() => {
				progressBar.Value = (int)progress;
				if (!string.IsNullOrEmpty(text))
					textBox.Text = text + Environment.NewLine + textBox.Text;
			}));
		}
	}
}