using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace NoticeOfTraining {
    class ExcelReader {
		public static List<ItemPhoneNumber> ReadPhoneNumbers(
			string fileName, string sheetName, string columnName, 
			string columnPhoneNumber, Action<double, string> UpdateProgress) {
			List<ItemPhoneNumber> phoneNumbers = new List<ItemPhoneNumber>();

			double progressCurrent = 0;
			UpdateProgress(progressCurrent, "Запуск приложения Excel");
			Excel.Application xlApp = new Excel.Application();

			if (xlApp == null) {
				UpdateProgress(progressCurrent, "Не удалось запустить");
				return phoneNumbers;
			}

			progressCurrent += 10;
			UpdateProgress(progressCurrent, "Открытие книги Excel");
			Excel.Workbook xlWorbook = xlApp.Workbooks.Open(fileName, ReadOnly: true);
			if (xlWorbook == null) {
				UpdateProgress(progressCurrent, "Не удалось открыть книгу: " + fileName);
				return phoneNumbers;
			}

			progressCurrent += 10;
			UpdateProgress(progressCurrent, string.Empty);
			Excel._Worksheet xlWorksheet = xlWorbook.Sheets[sheetName];

			if (xlWorksheet == null) {
				UpdateProgress(progressCurrent, "Не удалось найти лист с именем: " + sheetName);
				return phoneNumbers;
			}

			progressCurrent += 10;
			UpdateProgress(progressCurrent, string.Empty);
			Excel.Range xlRange = xlWorksheet.UsedRange;

			if (xlRange.Rows.Count == 0) {
				UpdateProgress(progressCurrent, "На указанном листе отсутствуют данные");
				return phoneNumbers;
			}

			UpdateProgress(progressCurrent, "На странице '" + sheetName + "' имеется строк: " + xlRange.Rows.Count);

			progressCurrent += 10;
			UpdateProgress(progressCurrent, "Считывание значений из указанных столбцов");

			double progressStep = (100 - progressCurrent) / xlRange.Rows.Count;
			int columnNameIndex = GetExcelColumnNumber(columnName);
			int columnPhoneNumberIndex = GetExcelColumnNumber(columnPhoneNumber);

			for (int i = 1; i < xlRange.Rows.Count; i++) {
				try {
					progressCurrent += progressStep;
					UpdateProgress(progressCurrent, "Разбор строки " + i);

					string name = xlRange.Cells[i, columnNameIndex].Value2;
					string phoneNumber = xlRange.Cells[i, columnPhoneNumberIndex].Value2;

					if (string.IsNullOrEmpty(phoneNumber))
						continue;

					ItemPhoneNumber itemPhoneNumber = new ItemPhoneNumber() {
						Name = name,
						PhoneNumber = phoneNumber
					};

					phoneNumber = itemPhoneNumber.GetClearedNumber();

					if (phoneNumber.Length < 10 ||
						phoneNumber.Length > 11)
						continue;

					if (!phoneNumber.StartsWith("9") &&
						!phoneNumber.StartsWith("79") &&
						!phoneNumber.StartsWith("89"))
						continue;

					if (phoneNumber.Length == 11)
						phoneNumber = phoneNumber.Substring(1, 10);

					phoneNumber = "+7 (" + phoneNumber.Substring(0, 3) +
						") " + phoneNumber.Substring(3, 3) +
						"-" + phoneNumber.Substring(6, 2) +
						"-" + phoneNumber.Substring(8, 2);

					itemPhoneNumber.PhoneNumber = phoneNumber;
					phoneNumbers.Add(itemPhoneNumber);
				} catch (Exception e) {
					UpdateProgress(progressCurrent, "Не удалось разобрать строку " + i + ", " + e.Message);
				}
			}

			GC.Collect();
			GC.WaitForPendingFinalizers();

			Marshal.ReleaseComObject(xlRange);
			Marshal.ReleaseComObject(xlWorksheet);

			xlWorbook.Close();
			Marshal.ReleaseComObject(xlWorbook);

			xlApp.Quit();
			Marshal.ReleaseComObject(xlApp);

			UpdateProgress(100, "Считывание завершено");

			return phoneNumbers;
		}

		private static DataTable ReadExcelFile(string fileName, string sheetName) {
			DataTable dataTable = new DataTable();

			if (!File.Exists(fileName))
				return dataTable;

			try {
				using (OleDbConnection conn = new OleDbConnection()) {
					conn.ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName + ";" +
						"Extended Properties='Excel 12.0 Xml;HDR=NO;'";

					using (OleDbCommand comm = new OleDbCommand()) {
						if (string.IsNullOrEmpty(sheetName)) {
							conn.Open();
							DataTable dtSchema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, 
								new object[] { null, null, null, "TABLE" });
							sheetName = dtSchema.Rows[0].Field<string>("TABLE_NAME");
							conn.Close();
						} else
							sheetName += "$";

						comm.CommandText = "Select * from [" + sheetName + "]";
						comm.Connection = conn;

						using (OleDbDataAdapter oleDbDataAdapter = new OleDbDataAdapter()) {
							oleDbDataAdapter.SelectCommand = comm;
							oleDbDataAdapter.Fill(dataTable);
						}
					}
				}
			} catch (Exception) { }

			return dataTable;
		}


		private static int GetExcelColumnNumber(string columnName) {
			if (string.IsNullOrEmpty(columnName)) throw new ArgumentNullException("columnName");

			columnName = columnName.ToUpperInvariant();

			int sum = 0;

			for (int i = 0; i < columnName.Length; i++) {
				sum *= 26;
				sum += (columnName[i] - 'A' + 1);
			}

			return sum;
		}

		private static string GetExcelColumnName(int columnNumber) {
			int dividend = columnNumber;
			string columnName = String.Empty;
			int modulo;

			while (dividend > 0) {
				modulo = (dividend - 1) % 26;
				columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
				dividend = (int)((dividend - modulo) / 26);
			}

			return columnName;
		}
	}
}
