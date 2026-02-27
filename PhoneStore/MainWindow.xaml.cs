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
using System.Data.SqlClient;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PhoneStore
{
        public partial class MainWindow : Window
        {
            // Chuỗi kết nối tới SQL Server
            string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=GenericStoreDB;Integrated Security=True";

            public MainWindow()
            {
                InitializeComponent();
                LoadData();
            }

            private void LoadData(string key = "")
            {
                try
                {
                    List<Customer> list = new List<Customer>();
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        string sql = "SELECT * FROM Customers WHERE FullName LIKE @k OR PhoneNumber LIKE @k";
                        SqlCommand cmd = new SqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@k", "%" + key + "%");
                        conn.Open();
                        SqlDataReader r = cmd.ExecuteReader();
                        while (r.Read())
                        {
                            list.Add(new Customer
                            {
                                Id = (int)r["Id"],
                                FullName = r["FullName"].ToString(),
                                PhoneNumber = r["PhoneNumber"].ToString(),
                                Email = r["Email"].ToString(),
                                Address = r["Address"].ToString(),
                                Note = r["Note"].ToString()
                            });
                        }
                    }
                    dgCustomer.ItemsSource = list;
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            }

            private void BtnAdd_Click(object sender, RoutedEventArgs e) => Execute("INSERT INTO Customers (FullName, PhoneNumber, Email, Address, Note) VALUES (@n, @p, @e, @a, @no)");
            private void BtnUpdate_Click(object sender, RoutedEventArgs e) { if (dgCustomer.SelectedItem is Customer s) Execute("UPDATE Customers SET FullName=@n, PhoneNumber=@p, Email=@e, Address=@a, Note=@no WHERE Id=@id", s.Id); }
            private void BtnDelete_Click(object sender, RoutedEventArgs e) { if (dgCustomer.SelectedItem is Customer s) Execute("DELETE FROM Customers WHERE Id=@id", s.Id); }
            private void BtnSearch_Click(object sender, RoutedEventArgs e) => LoadData(txtSearch.Text);

            private void Execute(string sql, int id = 0)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        SqlCommand cmd = new SqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@n", txtFullName.Text);
                        cmd.Parameters.AddWithValue("@p", txtPhone.Text);
                        cmd.Parameters.AddWithValue("@e", txtEmail.Text);
                        cmd.Parameters.AddWithValue("@a", txtAddress.Text);
                        cmd.Parameters.AddWithValue("@no", txtNote.Text);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                    LoadData();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }

            private void DgCustomer_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
            {
                if (dgCustomer.SelectedItem is Customer s)
                {
                    txtFullName.Text = s.FullName; txtPhone.Text = s.PhoneNumber;
                    txtEmail.Text = s.Email; txtAddress.Text = s.Address; txtNote.Text = s.Note;
                }
            }
        }

        public class Customer
        {
            public int Id { get; set; }
            public string FullName { get; set; }
            public string PhoneNumber { get; set; }
            public string Email { get; set; }
            public string Address { get; set; }
            public string Note { get; set; }
        }
    }
