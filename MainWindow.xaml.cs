using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
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
            ListAll();
        }
        private void dgRecords_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgRecords.SelectedItem is DataRowView row)
            {
                selectedRecordId = Convert.ToInt32(row["RecordID"]);
                txtPersonID.Text = row["PersonID"].ToString();
                txtPersonName.Text = row["PersonName"].ToString();
                dpAccessTime.SelectedDate = Convert.ToDateTime(row["AccessTime"]);
            }
        }

        private void Button_Click_Add(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPersonID.Text) || string.IsNullOrWhiteSpace(txtPersonName.Text) || !dpAccessTime.SelectedDate.HasValue)
            {
                MessageBox.Show("Please fill in all fields.");
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

                    var cmd = new SqlCommand("INSERT INTO ACCESSRECORD (RecordID, PersonID, PersonName, AccessTime) VALUES (@rid, @pid, @pname, @atime)", conn);
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

            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    var cmd = new SqlCommand("UPDATE ACCESSRECORD SET PersonID=@pid, PersonName=@pname, AccessTime=@atime WHERE RecordID=@rid", conn);
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



        private void ClearFields()
             {
                 txtPersonID.Text = "";
                 txtPersonName.Text = "";
                 dpAccessTime.SelectedDate = null;
                 selectedRecordId = -1;
                 dgRecords.SelectedItem = null;
             }

             private void txtPersonID_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
             {

             }

             

    }
}