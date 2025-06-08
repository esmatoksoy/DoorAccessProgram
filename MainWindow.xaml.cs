using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Data.SqlClient;


namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        string connectionString = "Server=DESKTOP-VE8VSKQ\\SQLEXPRESS01;Database=DoorScan;Trusted_Connection=True;TrustServerCertificate=True;";
        int selectedRecordId = -1;

        public MainWindow()
        {
            InitializeComponent();
            dgRecords.AutoGeneratingColumn += (s, e) =>
            {
                if (e.PropertyName == "AccessTime" && e.Column is DataGridTextColumn col)
                {
                    (col.Binding as System.Windows.Data.Binding).StringFormat = "yyyy-MM-dd";
                }
            };
            ListAll();
        }
        private static readonly Regex OnlyLetters = new Regex("^[a-zA-ZğüşöçıİĞÜŞÖÇ ]+$");

        private void txtPersonName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !OnlyLetters.IsMatch(e.Text);
        }

        private void txtPersonName_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!OnlyLetters.IsMatch(text))
                    e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }
        private void txtAccessOnlyTime_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsValidTimeFormat(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        // Helper method to validate time format (hh:mm)
        private bool IsValidTimeFormat(string text)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(text, @"^\d{2}:\d{2}$");
        }
        private static readonly Regex OnlyDigits = new Regex("^[0-9]+$");
        private void txtPersonID_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !OnlyDigits.IsMatch(e.Text);
        }

        private void txtPersonID_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!OnlyDigits.IsMatch(text))
                    e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }

        private bool TryGetAccessOnlyTime(out TimeSpan accessOnlyTime)
        {
            accessOnlyTime = TimeSpan.Zero;
            if (string.IsNullOrWhiteSpace(txtAccessOnlyTime.Text))
                return false;
            return TimeSpan.TryParse(txtAccessOnlyTime.Text, out accessOnlyTime);
        }

        private void dgRecords_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgRecords.SelectedItem is DataRowView row) // Ensure 'row' is assigned here
            {
                selectedRecordId = Convert.ToInt32(row["RecordID"]);
                txtPersonID.Text = row["PersonID"].ToString();
                txtPersonName.Text = row["PersonName"].ToString();
                dpAccessTime.SelectedDate = Convert.ToDateTime(row["AccessTime"]);

                if (row["AccessOnlyTime"] != DBNull.Value) // 'row' is guaranteed to be assigned
                    txtAccessOnlyTime.Text = TimeSpan.Parse(row["AccessOnlyTime"].ToString()).ToString(@"hh\:mm");
                else
                    txtAccessOnlyTime.Text = "";
            }
        }

        private void Button_Click_Add(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPersonID.Text) || string.IsNullOrWhiteSpace(txtPersonName.Text) || !dpAccessTime.SelectedDate.HasValue)
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }
            if (!IsNameValid(txtPersonName.Text))
            {
                MessageBox.Show("Name should be a string");
                return;
            }
            if (!TryGetAccessOnlyTime(out TimeSpan accessOnlyTime))
            {
                MessageBox.Show("Please enter a valid Access Only Time (HH:mm).");
                return;
            }
            try
            {
                // Random ID oluştur
                var random = new Random();
                int recordId = random.Next(1, int.MaxValue);

                // Bağlantı başlat
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    var cmd = new SqlCommand("INSERT INTO ACCESSRECORD (RecordID, PersonID, PersonName, AccessTime, AccessOnlyTime) VALUES (@rid, @pid, @pname, @atime, @aotime)", conn);
                    cmd.Parameters.AddWithValue("@aotime", accessOnlyTime);
                    cmd.Parameters.AddWithValue("@rid", recordId);
                    cmd.Parameters.AddWithValue("@pid", txtPersonID.Text);
                    cmd.Parameters.AddWithValue("@pname", txtPersonName.Text);
                    cmd.Parameters.AddWithValue("@atime", dpAccessTime.SelectedDate.Value);

                    cmd.ExecuteNonQuery(); 
                }

                MessageBox.Show("Record added!");
                 ListAll(); // eğer listeliyorsan buradan çağır
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message);
            }
        }

        private void ListAll()
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM ACCESSRECORD", conn);
                var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);
                dgRecords.ItemsSource = dt.DefaultView;

            }
        }


        private void Button_Click_List(object sender, RoutedEventArgs e)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM ACCESSRECORD", conn);

                // BURAYA EKLE:
                var adapter = new SqlDataAdapter(cmd);
                var table = new DataTable();
                adapter.Fill(table);
                dgRecords.ItemsSource = table.DefaultView;
            }
        }


        private void Button_Click_Search(object sender, RoutedEventArgs e)
        {
            string personId = txtPersonID.Text.Trim();

            if (string.IsNullOrWhiteSpace(personId))
            {
                MessageBox.Show("Please enter a PersonID to search.");
                return;
            }

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM ACCESSRECORD WHERE PersonID = @pid", conn);
                cmd.Parameters.AddWithValue("@pid", personId);

                var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    dgRecords.ItemsSource = dt.DefaultView;
                }
                else
                {
                    dgRecords.ItemsSource = null; // temizle
                    MessageBox.Show("Person with this ID does not exist.");
                }
            }

            ClearFields(); // arama sonrası alanları temizle (isteğe bağlı)
        }


        private void Button_Click_Delete(object sender, RoutedEventArgs e)
             {
                 if (selectedRecordId == -1)
                 {
                     MessageBox.Show("Please enter a record ID to delete.");
                     return;
                 }
                 var result = MessageBox.Show("Are you sure you want to delete this record?", "Confirm", MessageBoxButton.YesNo);
                 if (result == MessageBoxResult.Yes)
                 {
                     using (var conn = new SqlConnection(connectionString))
                     {
                         conn.Open();
                         var cmd = new SqlCommand("DELETE FROM ACCESSRECORD WHERE RecordID=@rid", conn); // Corrected table name
                         cmd.Parameters.AddWithValue("@rid", selectedRecordId);
                         cmd.ExecuteNonQuery();
                     }
                     MessageBox.Show("Record deleted!");
                     ClearFields();
                     ListAll();
            }
             }

        private void Button_Click_Update(object sender, RoutedEventArgs e)
        {
            if (selectedRecordId == -1)
            {
                MessageBox.Show("Please select a record to update.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPersonID.Text) || string.IsNullOrWhiteSpace(txtPersonName.Text) || !dpAccessTime.SelectedDate.HasValue)
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }
            if (!TryGetAccessOnlyTime(out TimeSpan accessOnlyTime))
            {
                MessageBox.Show("Please enter a valid Access Only Time (HH:mm).");
                return;
            }
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    var cmd = new SqlCommand("UPDATE ACCESSRECORD SET PersonID=@pid, PersonName=@pname, AccessTime=@atime, AccessOnlyTime=@aotime WHERE RecordID=@rid", conn);
                    cmd.Parameters.AddWithValue("@aotime", accessOnlyTime);
                    cmd.Parameters.AddWithValue("@pid", txtPersonID.Text);
                    cmd.Parameters.AddWithValue("@pname", txtPersonName.Text);
                    cmd.Parameters.AddWithValue("@atime", dpAccessTime.SelectedDate.Value);
                    cmd.Parameters.AddWithValue("@rid", selectedRecordId);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                        MessageBox.Show("Record updated!");
                    else
                        MessageBox.Show("Update failed!");
                }
                // Listeyi güncelle
                Button_Click_List(null, null);
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

        private bool IsNameValid(string name)
        {
            // Only letters and spaces allowed
            foreach (char c in name)
            {
                if (!char.IsLetter(c) && c != ' ')
                    return false;
            }
            return true;
        }
private void Button_Click_Graphics(object sender, RoutedEventArgs e)
{
    // Example: pass all records from the DataGrid's DataSource
    var dataView = dgRecords.ItemsSource as DataView;
    DataTable table = dataView?.Table;

    var graphicsWindow = new GraphicsWindow(table);
    graphicsWindow.Show();
}
        private void ClearFields()
             {
                 txtAccessOnlyTime.Text = "";
                 txtPersonID.Text = "";
                 txtPersonName.Text = "";
                 dpAccessTime.SelectedDate = null;
                 selectedRecordId = -1;
                 dgRecords.SelectedItem = null;
             }

             private void txtPersonID_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
             {

             }
        private void txtAccessOnlyTime_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Allow only numeric input and colon (for time format hh:mm)
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9:]$");
        }


    }
}