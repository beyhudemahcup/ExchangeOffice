using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ExchangeOffice
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public SqlConnection sqlConnection = new SqlConnection();
        public SqlCommand sqlCommand = new SqlCommand();
        public SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();

        Root value = new Root();

        public int CurrencyId = 0;
        public double FromAmount = 0;
        public double ToAmount = 0;

        public MainWindow()
        {
            InitializeComponent();
            GetValue();
            GetData();
        }

        public class Root
        {
            //open exchange rate ID
            //3255c05823ed41c081b0a85cf6349d6e
            public Rate rates { get; set; }
            public long timestamp;
            public string license;
        }

        public class Rate
        {
            //those values comes from a website that provides APIs
            public double INR { get; set; }
            public double JPY { get; set; }
            public double USD { get; set; }
            public double EUR { get; set; }
            public double NZD { get; set; }
            public double CAD { get; set; }
            public double ISK { get; set; }
            public double PHP { get; set; }
            public double DKK { get; set; }
            public double CZK { get; set; }
        }

        public static async Task<Root> GetData<T>(string url)
        {
            var root = new Root();
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(1);
                    HttpResponseMessage httpResponseMessage = await client.GetAsync(url);

                    if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var responseString = await httpResponseMessage.Content.ReadAsStringAsync();
                        var responseObject = JsonConvert.DeserializeObject<Root>(responseString);

                        MessageBox.Show($"Timestamp: {responseObject.timestamp}, Information: {MessageBoxButton.OK}");

                        return responseObject;
                    }
                    return root;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return root;
            }
        }

        public async void GetValue()
        {
            value = await GetData<Root>("https://openexchangerates.org/api/latest.json?app_id=3255c05823ed41c081b0a85cf6349d6e");
            BindCurrency();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ClearTextBoxes();
        }

        private void txtCurrency_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) { }

        private void cmbToCurrency_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) { }

        private void txtAmount_TextChanged(object sender, TextChangedEventArgs e) { }

        private void txtCurrencyName_TextChanged(object sender, TextChangedEventArgs e) { }

        public void BindCurrency()
        {
            //created datatable using manual data 
            // ManualCurrencies();

            //created datatable using data comes from my database
            //CreateConnection();
            //DatabaseCurrencies();

            //created datatable using data comes from website as a API
            APICurrencies();
        }

        private void cmbFromCurrency_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                if (cmbToCurrency.SelectedValue != null && int.Parse(cmbToCurrency.SelectedValue.ToString()) != 0 && cmbToCurrency.SelectedIndex != 0)
                {
                    int CurrencyToId = int.Parse(cmbToCurrency.SelectedValue.ToString());

                    CreateConnection();
                    DataTable dataTable = new DataTable();
                    sqlCommand = new SqlCommand("SELECT Amount FROM Currency WHERE Id = @CurrencyToId", sqlConnection);
                    sqlCommand.CommandType = CommandType.Text;

                    if (CurrencyToId != 0)
                    {
                        sqlCommand.Parameters.AddWithValue("@CurrencyToId", CurrencyToId);
                    }
                    sqlDataAdapter = new SqlDataAdapter(sqlCommand);

                    sqlDataAdapter.Fill(dataTable);

                    if (dataTable != null && dataTable.Rows.Count > 0)
                    {
                        ToAmount = double.Parse(dataTable.Rows[0]["Amount"].ToString());
                    }
                    sqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NumberValidationTextBox(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            double ConvertedValue;

            if (txtCurrency.Text == null || txtCurrency.Text.Trim() == "")
            {
                MessageBox.Show("Please enter amount", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                //selector is set to the text box
                txtCurrency.Focus();
                return;
            }
            else if (cmbFromCurrency.SelectedValue == null || cmbFromCurrency.SelectedIndex == 0)
            {
                MessageBox.Show("Please select currency from", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                //selector is set to the currency button
                cmbFromCurrency.Focus();
                return;
            }
            else if (cmbToCurrency.SelectedValue == null || cmbToCurrency.SelectedIndex == 0)
            {
                MessageBox.Show("Please select currency to", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                cmbToCurrency.Focus();
                return;
            }

            //check if From and To combobox selected values are the same
            if (cmbToCurrency.Text == cmbFromCurrency.Text)
            {
                ConvertedValue = double.Parse(txtCurrency.Text);
                lblCurrency.Content = cmbToCurrency.Text + " " + ConvertedValue.ToString("N3");
            }
            else
            {
                ConvertedValue = double.Parse(cmbToCurrency.SelectedValue.ToString())
                    * double.Parse(txtCurrency.Text) / double.Parse(cmbFromCurrency.SelectedValue.ToString());

                lblCurrency.Content = cmbToCurrency.Text + " " + ConvertedValue.ToString("N3");
            }
        }

        private void APICurrencies()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Text");
            dataTable.Columns.Add("Value");
            dataTable.Rows.Add("SELECT", 0);
            dataTable.Rows.Add("INR", value.rates.INR);
            dataTable.Rows.Add("JPY", value.rates.JPY);
            dataTable.Rows.Add("USD", value.rates.USD);
            dataTable.Rows.Add("EUR", value.rates.EUR);
            dataTable.Rows.Add("NZD", value.rates.NZD);
            dataTable.Rows.Add("CAD", value.rates.CAD);
            dataTable.Rows.Add("ISK", value.rates.ISK);
            dataTable.Rows.Add("PHP", value.rates.PHP);
            dataTable.Rows.Add("DKK", value.rates.DKK);
            dataTable.Rows.Add("CZK", value.rates.CZK);

            cmbFromCurrency.ItemsSource = dataTable.DefaultView;
            cmbFromCurrency.DisplayMemberPath = "Text";
            cmbFromCurrency.SelectedValuePath = "Value";
            cmbFromCurrency.SelectedIndex = 0;

            cmbToCurrency.ItemsSource = dataTable.DefaultView;
            cmbToCurrency.DisplayMemberPath = "Text";
            cmbToCurrency.SelectedValuePath = "Value";
            cmbToCurrency.SelectedIndex = 0;
        }

        public void GetData()
        {
            CreateConnection();
            DataTable dataTable = new DataTable();
            sqlCommand = new SqlCommand("SELECT * FROM Currency", sqlConnection);
            sqlCommand.CommandType = CommandType.Text;
            sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            sqlDataAdapter.Fill(dataTable);

            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                dgvCurrency.ItemsSource = dataTable.DefaultView;
            }
            else
            {
                dgvCurrency.ItemsSource = null;
            }

            sqlConnection.Close();
        }

        private void DatabaseCurrencies()
        {
            DataTable dataTable = new DataTable();
            sqlCommand = new SqlCommand("SELECT Id, CurrencyName FROM Currency", sqlConnection);
            sqlCommand.CommandType = CommandType.Text;

            sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            sqlDataAdapter.Fill(dataTable);

            //create an object for dataRow
            DataRow firstRow = dataTable.NewRow();
            firstRow["Id"] = 0;
            firstRow["CurrencyName"] = "SELECT";

            //insert that object to dataTable
            dataTable.Rows.InsertAt(firstRow, 0);

            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                cmbFromCurrency.ItemsSource = dataTable.DefaultView;

                cmbToCurrency.ItemsSource = dataTable.DefaultView;
            }

            cmbFromCurrency.DisplayMemberPath = "CurrencyName";
            cmbFromCurrency.SelectedValuePath = "Id";
            cmbFromCurrency.SelectedIndex = 0;

            cmbToCurrency.DisplayMemberPath = "CurrencyName";
            cmbToCurrency.SelectedValuePath = "Id";
            cmbToCurrency.SelectedIndex = 0;
        }

        private void ManualCurrencies()
        {
            DataTable dtCurrency = new DataTable();
            dtCurrency.Columns.Add("Text");
            dtCurrency.Columns.Add("Value");
            //adding rows which are text and value:

            dtCurrency.Rows.Add("SELECT", 0);
            dtCurrency.Rows.Add("DOLAR", 31.5);
            dtCurrency.Rows.Add("EURO", 33.3);
            dtCurrency.Rows.Add("POUND", 39.8);
            dtCurrency.Rows.Add("LIRA", 1);
            dtCurrency.Rows.Add("ZLOTY", 7.8);

            cmbFromCurrency.ItemsSource = dtCurrency.DefaultView;
            cmbFromCurrency.DisplayMemberPath = "Text";
            cmbFromCurrency.SelectedValuePath = "Value";
            cmbFromCurrency.SelectedIndex = 0;

            cmbToCurrency.ItemsSource = dtCurrency.DefaultView;
            cmbToCurrency.DisplayMemberPath = "Text";
            cmbToCurrency.SelectedValuePath = "Value";
            cmbToCurrency.SelectedIndex = 0;
        }

        private void ClearTextBoxes()
        {
            txtCurrency.Text = string.Empty;
            if (cmbFromCurrency.Items.Count > 0)
                cmbFromCurrency.SelectedIndex = 0;
            if (cmbToCurrency.Items.Count > 0)
                cmbToCurrency.SelectedIndex = 0;

            lblCurrency.Content = "";
            txtCurrency.Focus();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtAmount.Text == null || txtAmount.Text.Trim() == "")
                {
                    MessageBox.Show("Please enter amount", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtAmount.Focus();
                    return;
                }
                else if (txtCurrencyName.Text == null || txtCurrencyName.Text.Trim() == "")
                {
                    MessageBox.Show("Please select currency from", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtCurrencyName.Focus();
                    return;
                }
                else
                {
                    if (CurrencyId > 0)
                    {
                        //Show the confirmation message
                        if (MessageBox.Show("Are you sure you want to update ?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            CreateConnection();
                            DataTable dt = new DataTable();

                            //Update Query Record update using Id
                            sqlCommand = new SqlCommand("UPDATE Currency SET Amount = @Amount, CurrencyName = @CurrencyName WHERE Id = @Id", sqlConnection);
                            sqlCommand.CommandType = CommandType.Text;
                            sqlCommand.Parameters.AddWithValue("@Id", CurrencyId);
                            sqlCommand.Parameters.AddWithValue("@Amount", txtAmount.Text);
                            sqlCommand.Parameters.AddWithValue("@CurrencyName", txtCurrencyName.Text);
                            sqlCommand.ExecuteNonQuery();

                            sqlConnection.Close();

                            MessageBox.Show("Data updated successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        if (MessageBox.Show("Are you sure you want to save ?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            CreateConnection();
                            //Insert query to Save data in the table
                            sqlCommand = new SqlCommand("INSERT INTO Currency (Amount, CurrencyName) VALUES(@Amount, @CurrencyName)", sqlConnection);
                            sqlCommand.CommandType = CommandType.Text;
                            sqlCommand.Parameters.AddWithValue("@Amount", txtAmount.Text);
                            sqlCommand.Parameters.AddWithValue("@CurrencyName", txtCurrencyName.Text);
                            sqlCommand.ExecuteNonQuery();
                            sqlConnection.Close();

                            MessageBox.Show("Data saved successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    ClearMaster();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error occured!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void CreateConnection()
        {
            string connection = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            sqlConnection = new SqlConnection(connection);
            sqlConnection.Open();
        }
        public void ClearMaster()
        {
            try
            {
                txtAmount.Text = string.Empty;
                txtCurrencyName.Text = string.Empty;
                btnSave.Content = "Save";
                GetData();
                CurrencyId = 0;
                BindCurrency();
                txtAmount.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearMaster();
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }

        private void dgvCurrency_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                DataGrid dataGrid = (DataGrid)sender;

                DataRowView selectedRow = dataGrid.CurrentItem as DataRowView;

                if (selectedRow != null)
                {
                    if (dgvCurrency.Items.Count > 0 && dataGrid.SelectedCells.Count > 0)
                    {
                        CurrencyId = Int32.Parse(selectedRow["Id"].ToString());

                        if (dataGrid.SelectedCells[0].Column.DisplayIndex == 0)
                        {
                            txtAmount.Text = selectedRow["Amount"].ToString();
                            txtCurrencyName.Text = selectedRow["CurrencyName"].ToString();
                            btnSave.Content = "Update";
                        }
                        else if (dataGrid.SelectedCells[0].Column.DisplayIndex == 1)
                        {
                            if (MessageBox.Show("Are you sure you want to delete ?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                CreateConnection();
                                DataTable dt = new DataTable();
                               sqlCommand = new SqlCommand("DELETE FROM Currency WHERE Id = @Id", sqlConnection);
                               sqlCommand.CommandType = CommandType.Text;

                                //CurrencyId set in @Id parameter and send it in delete statement
                                sqlCommand.Parameters.AddWithValue("@Id", CurrencyId);
                                sqlCommand.ExecuteNonQuery();
                                sqlConnection.Close();

                                MessageBox.Show("Data deleted successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                                ClearMaster();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }

        private void ErrorMessage(Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
