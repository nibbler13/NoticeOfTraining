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

namespace NoticeOfTraining {
	/// <summary>
	/// Логика взаимодействия для WindowTemplate.xaml
	/// </summary>
	public partial class WindowTemplate : Window {
		public ItemTemplate TemplateItem { get; set; }
		private string originalName;
		private string originalMessage;

		public WindowTemplate() {
			InitializeComponent();
			TemplateItem = new ItemTemplate();
			textBoxName.DataContext = this;
			textBoxName.TextChanged += TextBox_TextChanged;
			textBoxMessage.DataContext = this;
			textBoxMessage.TextChanged += TextBox_TextChanged;
			Closed += WindowTemplate_Closed;
			textBoxName.Focus();
		}

		public WindowTemplate(ItemTemplate templateItem) : this() {
			TemplateItem = templateItem;
			originalName = templateItem.Name;
			originalMessage = templateItem.Message;
		}

		private void WindowTemplate_Closed(object sender, EventArgs e) {
			if (DialogResult != true) {
				TemplateItem.Name = originalName;
				TemplateItem.Message = originalMessage;
			}
		}

		private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {
			buttonSave.IsEnabled = textBoxName.Text.Length > 0 && textBoxMessage.Text.Length > 0;
		}

		private void buttonSave_Click(object sender, RoutedEventArgs e) {
			DialogResult = true;
			Close();
		}
	}
}
