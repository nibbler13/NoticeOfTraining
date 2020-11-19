using System;
using System.Data;
using System.Collections.Generic;
using FirebirdSql.Data.FirebirdClient;
using System.Windows;

namespace SmsSend {
	public partial class FirebirdClient {
		private FbConnection connection;

		private static FirebirdClient instance = null;
		private static readonly object padlock = new object();

		public static FirebirdClient Instance {
			get {
				lock (padlock) {
					if (instance == null)
						instance = new FirebirdClient(
							DB_ADDRESS,
							DB_NAME,
							DB_USER,
							DB_PASSWORD);

					return instance;
				}
			}
		}

		public FirebirdClient(string ipAddress, string baseName, string user, string pass) {
			FbConnectionStringBuilder cs = new FbConnectionStringBuilder {
				DataSource = ipAddress,
				Database = baseName,
				UserID = user,
				Password = pass,
				Charset = "NONE",
				Pooling = false
			};

			connection = new FbConnection(cs.ToString());
			IsConnectionOpened();
		}

		public void Close() {
			connection.Close();
		}

		private bool IsConnectionOpened() {
			if (connection.State != ConnectionState.Open) {
				try {
					connection.Open();
				} catch (Exception e) {
					MessageBox.Show(
						Application.Current.MainWindow, 
						e.Message + Environment.NewLine + e.StackTrace, 
						"Ошибка подключения к БД", 
						MessageBoxButton.OK, 
						MessageBoxImage.Error);
				}
			}

			return connection.State == ConnectionState.Open;
		}

		public DataTable GetDataTable(string query, Dictionary<string, object> parameters) {
			DataTable dataTable = new DataTable();

			if (!IsConnectionOpened())
				return dataTable;
			
			try {
				using (FbCommand command = new FbCommand(query, connection)) { 
					if (parameters.Count > 0) 
						foreach (KeyValuePair<string, object> parameter in parameters)
							command.Parameters.AddWithValue(parameter.Key, parameter.Value);
				
					using (FbDataAdapter fbDataAdapter = new FbDataAdapter(command)) 
						fbDataAdapter.Fill(dataTable);
				}
			} catch (Exception e) {
				if (Application.Current.MainWindow != null)
					MessageBox.Show(
						Application.Current.MainWindow,
						e.Message + Environment.NewLine + e.StackTrace,
						"Ошибка выполнения запроса к БД",
						MessageBoxButton.OK,
						MessageBoxImage.Error);
				else
					Logging.ToLog(e.Message + Environment.NewLine + e.StackTrace);

				connection.Close();
			}

			return dataTable;
		}

		public bool ExecuteUpdateQuery(string query, Dictionary<string, object> parameters) {
			bool updated = false;

			if (!IsConnectionOpened())
				return updated;

			try {
				FbCommand update = new FbCommand(query, connection);

				if (parameters.Count > 0) {
					foreach (KeyValuePair<string, object> parameter in parameters)
						update.Parameters.AddWithValue(parameter.Key, parameter.Value);
				}

				updated = update.ExecuteNonQuery() > 0 ? true : false;
			} catch (Exception e) {
				if (Application.Current.MainWindow != null)
				MessageBox.Show(
					e.Message + Environment.NewLine + e.StackTrace,
					"Ошибка выполнения запроса к БД",
					MessageBoxButton.OK,
					MessageBoxImage.Error);
				else {
					Logging.ToLog(e.Message + Environment.NewLine + e.StackTrace);
				}

				connection.Close();
			}

			return updated;
		}
	}
}
