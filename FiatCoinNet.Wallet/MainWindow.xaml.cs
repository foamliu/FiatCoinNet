using FiatCoinNet.Common;
using FiatCoinNet.Domain;
using FiatCoinNet.Domain.Requests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace FiatCoinNet.WalletGui
{
    public enum TransactionType
    {
        All = 0,

        Sent = 1,

        Received = 2
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string FileName = "wallet.json";

        private Wallet m_Wallet;

        private List<PaymentTransaction> m_Transactions = new List<PaymentTransaction>();

        //private const string baseUrl = "http://localhost:48701/"; 
        private const string baseUrl = "http://fiatcoinet.azurewebsites.net/";
        public static readonly HttpClient HttpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
        };

        public MainWindow()
        {
            InitializeComponent();
            LoadAddresses();
            BindCompanyNameAndCurrencyCode();
            BindAddressForPay();
            BindTransactionType();
        }


        private void miNewAddress_Click(object sender, RoutedEventArgs e)
        {
            string privateKey;
            string publicKey;
            CryptoHelper.GenerateKeyPair(out privateKey, out publicKey);

            string fingerPrint = CryptoHelper.Hash(publicKey);

            int issuerId = (int)comboBoxIssuer.SelectedValue;
            string currencyCode = (string)comboBoxCurrencyCode.SelectedValue;
            var account = new PaymentAccount
            {
                Address = FiatCoinHelper.ToAddress(issuerId, fingerPrint),
                CurrencyCode = currencyCode,
                Balance = 10.00m,
                PublicKey = publicKey,
                PrivateKey = null
            };

            // register
            string requestUri = string.Format("issuer/api/{0}/accounts/register", issuerId);
            var registerRequest = new RegisterRequest
            {
                PaymentAccount = account.Mask()
            };
            HttpContent content = new StringContent(JsonHelper.Serialize(registerRequest));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = HttpClient.PostAsync(requestUri, content).Result;
            response.EnsureSuccessStatusCode();

            account.PrivateKey = privateKey;
            this.m_Wallet.PaymentAccounts.Add(account);

            this.UpdateAddressDataGrid();
            this.Save();
        }

        private void miDelete_Click(object sender, RoutedEventArgs e)
        {
            //Get the clicked MenuItem
            var menuItem = (MenuItem)sender;

            //Get the ContextMenu to which the menuItem belongs
            var contextMenu = (ContextMenu)menuItem.Parent;

            //Find the placementTarget
            var item = (DataGrid)contextMenu.PlacementTarget;

            //Get the underlying item, that you cast to your object that is bound
            //to the DataGrid (and has subject and state as property)
            var toDeleteFromBindedList = (PaymentAccount)item.SelectedCells[0].Item;

            // unregister this account
            int issuerId = 1942;
            string requestUri = string.Format("issuer/api/{0}/accounts/unregister", issuerId);
            var unregisterRequest = new UnregisterRequest
            {
                Address = toDeleteFromBindedList.Address
            };
            unregisterRequest.Signature = CryptoHelper.Sign(toDeleteFromBindedList.PrivateKey, unregisterRequest.ToMessage());
            HttpContent content = new StringContent(JsonHelper.Serialize(unregisterRequest));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = HttpClient.PostAsync(requestUri, content).Result;
            response.EnsureSuccessStatusCode();

            m_Wallet.PaymentAccounts.Remove(toDeleteFromBindedList);

            this.UpdateAddressDataGrid();
            this.Save();
        }

        public void miTabControl_SelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            if (selectionChangedEventArgs.Source is TabControl)
            {
                string tabItem = ((TabItem)TabControls.SelectedItem).Header as string;
                switch (tabItem)
                {
                    case "Addresses":
                        GetBalances();
                        this.UpdateAddressDataGrid();
                        comboBoxIssuer.SelectedIndex = 0;
                        comboBoxCurrencyCode.SelectedValue = "USD";
                        break;
                    case "Pay":
                        payFrom.Items.Refresh();
                        payFrom.SelectedValue = m_Wallet.PaymentAccounts.Count > 0 ? m_Wallet.PaymentAccounts[0].Address : null;
                        break;
                    case "Transactions":
                        comboBoxTransactionType.SelectedIndex = 0;
                        this.UpdateTransactionDataGrid();
                        break;
                }
            }
        }

        private void PayFromSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            if (selectionChangedEventArgs.Source is ComboBox)
            {
                int index = payFrom.SelectedIndex;
                payCurrencyCode.Text = m_Wallet.PaymentAccounts[index].CurrencyCode;
            }
        }

        private void TransactionTypeSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            if (selectionChangedEventArgs.Source is ComboBox)
            {
                string value = comboBoxTransactionType.SelectedValue.ToString();
                switch (value)
                {
                    case "All":
                        m_Transactions = GetAllTransactions();
                        break;
                    case "Sent":
                        break;
                    case "Received":
                        break;
                }
                this.UpdateTransactionDataGrid();
            }
        }

        private void miExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void miSettings_Click(object sender, RoutedEventArgs e)
        {

        }

        private void miAbout_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnPay_Click(object sender, RoutedEventArgs e)
        {
            int issuerId = FiatCoinHelper.GetIssuerId(payFrom.SelectedValue.ToString());
            string requestUri = string.Format("issuer/api/{0}/accounts/pay", issuerId);
            var payRequest = new DirectPayRequest
            {
                PaymentTransaction = new PaymentTransaction
                {
                    Source = payFrom.SelectedValue.ToString(),
                    Dest = payTo.Text,
                    Amount = Convert.ToDecimal(payAmount.Text),
                    CurrencyCode = payCurrencyCode.Text,
                    MemoData = "surface"
                }
            };
            payRequest.Signature = CryptoHelper.Sign(m_Wallet.PaymentAccounts[payFrom.SelectedIndex].PrivateKey, payRequest.ToMessage());
            HttpContent content = new StringContent(JsonHelper.Serialize(payRequest));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = HttpClient.PostAsync(requestUri, content).Result;
            response.EnsureSuccessStatusCode();

            TabControls.SelectedIndex = 2;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            this.Save();

            base.OnClosing(e);
        }

        private void UpdateAddressDataGrid()
        {
            if (null == m_Wallet || null == m_Wallet.PaymentAccounts) return;
            try
            {
                dataGridAddresses.ItemsSource = m_Wallet.PaymentAccounts;
                dataGridAddresses.Items.Refresh();
            }
            catch (Exception ex)
            {
                File.WriteAllText("error.log", ex.ToString());
            }
        }

        private void UpdateTransactionDataGrid()
        {
            if (null == m_Wallet || null == m_Transactions) return;
            try
            {
                dataGridTransactions.ItemsSource = m_Transactions;
                dataGridTransactions.Items.Refresh();
            }
            catch (Exception ex)
            {
                File.WriteAllText("error.log", ex.ToString());
            }
        }

        private void BindCompanyNameAndCurrencyCode()
        {
            //load issuer combo box
            string requestUri = "certifier/api/issuers";
            HttpResponseMessage response = HttpClient.GetAsync(requestUri).Result;
            response.EnsureSuccessStatusCode();
            List<Issuer> issuers = response.Content.ReadAsAsync<List<Issuer>>().Result;
            comboBoxIssuer.ItemsSource = issuers;
            comboBoxIssuer.SelectedValuePath = "Id";
            comboBoxIssuer.DisplayMemberPath = "Name";

            //load currency code combo box
            List<string> currencyCodes = DataAccessor.GetCurrencyCodes();
            comboBoxCurrencyCode.ItemsSource = currencyCodes;
        }

        private void BindAddressForPay()
        {
            payFrom.ItemsSource = m_Wallet.PaymentAccounts;
            payFrom.SelectedValuePath = "Address";
            payFrom.DisplayMemberPath = "Address";
            if (m_Wallet.PaymentAccounts.Count > 0)
            {
                payCurrencyCode.Text = m_Wallet.PaymentAccounts[0].CurrencyCode;

            }
        }

        private void BindTransactionType()
        {
            comboBoxTransactionType.ItemsSource = Enum.GetNames(typeof(TransactionType)).ToList();
        }

        private void LoadAddresses()
        {
            if (File.Exists(FileName))
            {
                this.m_Wallet = JsonHelper.Deserialize<Wallet>(File.ReadAllText(FileName));
                GetBalances();
            }
            else
            {
                this.m_Wallet = new Wallet();
            }
            UpdateAddressDataGrid();
        }

        private void GetBalances()
        {
            foreach (var paymentAccount in m_Wallet.PaymentAccounts)
            {
                int issuerId = FiatCoinHelper.GetIssuerId(paymentAccount.Address);
                string requestUri = string.Format("issuer/api/{0}/accounts/get", issuerId);
                var getRequest = new GetAccountRequest
                {
                    Address = paymentAccount.Address
                };
                HttpContent content = new StringContent(JsonHelper.Serialize(getRequest));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = HttpClient.PostAsync(requestUri, content).Result;
                response.EnsureSuccessStatusCode();
                paymentAccount.Balance = response.Content.ReadAsAsync<PaymentAccount>().Result.Balance;
            }
        }

        private void Save()
        {
            File.WriteAllText(FileName, JsonHelper.Serialize(m_Wallet));
        }

        private List<PaymentTransaction> GetAllTransactions()
        {
            List<PaymentTransaction> result = new List<PaymentTransaction>();
            foreach (var paymentAccount in m_Wallet.PaymentAccounts)
            {
                string requestUri = "issuer/api/transactions/get";
                var getRequest = new GetAccountRequest
                {
                    Address = paymentAccount.Address
                };
                HttpContent content = new StringContent(JsonHelper.Serialize(getRequest));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = HttpClient.PostAsync(requestUri, content).Result;
                if (response.IsSuccessStatusCode)
                {
                    List<PaymentTransaction> transactions = response.Content.ReadAsAsync<List<PaymentTransaction>>().Result;
                    result.AddRange(transactions);
                }
            }
            return result;
        }
    }
}
