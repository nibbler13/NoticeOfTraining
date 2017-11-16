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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NoticeOfTraining {
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();
		}

		private void ButtonPhoneNumberAdd_Click(object sender, RoutedEventArgs e) {
			WindowPhoneNumber windowPhoneNumber = new WindowPhoneNumber();
			windowPhoneNumber.Owner = this;
			windowPhoneNumber.ShowDialog();
		}

		private void ButtonPhoneNumberEdit_Click(object sender, RoutedEventArgs e) {

		}

		private void ButtonPhoneNumberRemove_Click(object sender, RoutedEventArgs e) {

		}

		private void ButtonPhoneNumberImport_Click(object sender, RoutedEventArgs e) {

		}

		private void ButtonMEssageSendNow_Click(object sender, RoutedEventArgs e) {

		}

		private void ButtonMessageSendLater_Click(object sender, RoutedEventArgs e) {

		}

		private void ButtonMessagesHistory_Click(object sender, RoutedEventArgs e) {

		}

		private void ButtonMessageSaveAsTemplate_Click(object sender, RoutedEventArgs e) {

		}

		private void ButtonTemplateUse_Click(object sender, RoutedEventArgs e) {

		}

		private void ButtonTemplateRemove_Click(object sender, RoutedEventArgs e) {

		}

		private void ButtonTemplateEdit_Click(object sender, RoutedEventArgs e) {

		}

		private void ButtonTemplateAdd_Click(object sender, RoutedEventArgs e) {
			WindowTemplate windowTemplate = new WindowTemplate();
			windowTemplate.Owner = this;
			windowTemplate.ShowDialog();
		}

		private void ButtonReceiversAddSelected_Click(object sender, RoutedEventArgs e) {

		}

		private void ButtonReceiversAddAll_Click(object sender, RoutedEventArgs e) {

		}

		private void ButtonReceiversRemoveAll_Click(object sender, RoutedEventArgs e) {

		}

		private void ButtonReceiversRemoveSelected_Click(object sender, RoutedEventArgs e) {

		}

		private void TextBoxMessage_TextChanged(object sender, TextChangedEventArgs e) {
			bool isEnabled = textBoxMessage.Text.Length > 0;
			buttonMessageSaveAsTemplate.IsEnabled = isEnabled;
			buttonMessageSendLater.IsEnabled = isEnabled;
			buttonMEssageSendNow.IsEnabled = isEnabled;
		}

		private void ListViewPhoneNumbersSaved_SelectionChanged(object sender, SelectionChangedEventArgs e) {

		}

		private void ListViewPhoneNumbersReceivers_SelectionChanged(object sender, SelectionChangedEventArgs e) {

		}

		private void ListViewTemplates_SelectionChanged(object sender, SelectionChangedEventArgs e) {

		}
	}
}
