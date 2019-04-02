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
	/// Логика взаимодействия для WindowPhoneNumber.xaml
	/// </summary>
	public partial class WindowPhoneNumber : Window {
		public ItemPhoneNumber PhoneNumberItem { get; set; }
		private string originalName;
		private string originalPhoneNumber;

		public WindowPhoneNumber() {
			InitializeComponent();
			PhoneNumberItem = new ItemPhoneNumber();
			textBoxName.DataContext = this;
			textBoxPhoneNumber.DataContext = this;
			textBoxPhoneNumber.TextChanged += TextBoxPhoneNumber_TextChanged;
			textBoxName.TextChanged += TextBoxPhoneNumber_TextChanged;
			Closed += WindowPhoneNumber_Closed;
			textBoxName.Focus();
		}

		private void WindowPhoneNumber_Closed(object sender, EventArgs e) {
			if (DialogResult != true) {
				PhoneNumberItem.Name = originalName;
				PhoneNumberItem.PhoneNumber = originalPhoneNumber;
			}
		}

		public WindowPhoneNumber(ItemPhoneNumber phoneNumberItem) : this() {
			PhoneNumberItem = phoneNumberItem;
			originalName = phoneNumberItem.Name;
			originalPhoneNumber = phoneNumberItem.PhoneNumber;
		}

		private void ButtonSave_Click(object sender, RoutedEventArgs e) {
			if (textBoxName.Text.Length == 0 ||
				!textBoxPhoneNumber.IsMaskCompleted)
				return;

			DialogResult = true;
			Close();
		}

		private void TextBoxPhoneNumber_TextChanged(object sender, TextChangedEventArgs e) {
			buttonSave.IsEnabled = textBoxPhoneNumber.IsMaskCompleted && textBoxName.Text.Length > 0;
		}

		private void TextBox_KeyDown(object sender, KeyEventArgs e) {
			if (e.Key != Key.Enter)
				return;

			ButtonSave_Click(buttonSave, new RoutedEventArgs());
		}
	}
}
