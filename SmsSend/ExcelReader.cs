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

namespace SmsSend {
    class ExcelReader {
		public static List<ItemPhoneNumber> ReadPhoneNumbers(
			string fileName, string sheetName, string columnName, 
			string columnPhoneNumber, Action<double, string> UpdateProgress) {
			List<ItemPhoneNumber> phoneNumbers = new List<ItemPhoneNumber>();

			DataTable dt = new DataTable();
			using (OleDbConnection conn = new OleDbConnection()) {
				conn.ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName + ";" + "Extended Properties='Excel 12.0 Xml;HDR=YES;IMEX=1;MAXSCANROWS=0;Mode=Read;'";
				using (OleDbCommand comm = new OleDbCommand()) {
					comm.CommandText = "Select * from [" + sheetName + "$]";
					comm.Connection = conn;
					using (OleDbDataAdapter da = new OleDbDataAdapter()) {
						da.SelectCommand = comm;
						da.Fill(dt);
					}
				}
			}

			double progressCurrent = 0;

			UpdateProgress(progressCurrent, "На странице '" + sheetName + "' имеется строк: " + dt.Rows.Count);

			progressCurrent += 10;
			UpdateProgress(progressCurrent, "Считывание значений из указанных столбцов");

			double progressStep = (100 - progressCurrent) / dt.Rows.Count;
			int columnNameIndex = GetExcelColumnNumber(columnName) - 1;
			int columnPhoneNumberIndex = GetExcelColumnNumber(columnPhoneNumber) - 1;

			foreach (DataRow dr in dt.Rows) {
					try {
					progressCurrent += progressStep;

					string name = dr[columnNameIndex].ToString();
					string phoneNumber = dr[columnPhoneNumberIndex].ToString();

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
					UpdateProgress(progressCurrent, "Не удалось разобрать строку, " + e.Message);
				}
			}

			GC.Collect();
			GC.WaitForPendingFinalizers();

			UpdateProgress(100, "Считывание завершено");

			return phoneNumbers;
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

		public static List<string> ReadSheetNames(string file) {
			List<string> sheetNames = new List<string>();

			using (OleDbConnection conn = new OleDbConnection()) {
				conn.ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + file + ";Mode=Read;" +
					"Extended Properties='Excel 12.0 Xml;HDR=NO;'";

				using (OleDbCommand comm = new OleDbCommand()) {
					conn.Open();
					DataTable dtSchema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables,
						new object[] { null, null, null, "TABLE" });
					foreach (DataRow row in dtSchema.Rows) {
						string name = row.Field<string>("TABLE_NAME");
						if (name.Contains("FilterDatabase"))
							continue;

						sheetNames.Add(name.Replace("$", "").TrimStart('\'').TrimEnd('\''));
					}
				}
			}

			return sheetNames;
		}
	}
}