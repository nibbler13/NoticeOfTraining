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
	/// Логика взаимодействия для WindowPhoneNumber.xaml
	/// </summary>
	public partial class WindowPhoneNumber : Window {
		public WindowPhoneNumber() {
			InitializeComponent();
		}

		private void buttonSave_Click(object sender, RoutedEventArgs e) {
			Close();
		}
	}
}
