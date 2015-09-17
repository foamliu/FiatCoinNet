using FiatCoinNet.Common;
using FiatCoinNet.Domain;
using FiatCoinNet.Domain.Requests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;
using System.Windows.Controls;

namespace FiatCoinNet.WalletGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string FileName = "wallet.json";

        private Wallet m_Wallet;

        private const string baseUrl = "http://fiatcoinet.azurewebsites.net/";
        public static readonly HttpClient HttpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
        };

        public MainWindow()
        {
            InitializeComponent();      
            Load();
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
                Balance = 0.00m,
                PublicKey = publicKey,
                PrivateKey = privateKey
            };

            // register
            string requestUri = string.Format("issuer/api/{0}/accounts/register", issuerId);
            var registerRequest = new RegisterRequest
            {
                PaymentAccount = account
            };
            HttpContent content = new StringContent(JsonHelper.Serialize(registerRequest));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = HttpClient.PostAsync(requestUri, content).Result;
            response.EnsureSuccessStatusCode();

            this.m_Wallet.PaymentAccounts.Add(account);

            this.UpdateDataGrid();
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

            this.UpdateDataGrid();
            this.Save();
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
            //Nina:Todo
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            this.Save();

            base.OnClosing(e);
        }

        private void UpdateDataGrid()
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

        private void LoadComboBox()
        {
            //load issuer combo box
            string requestUri = "certifier/api/issuers";
            HttpResponseMessage response = HttpClient.GetAsync(requestUri).Result;
            response.EnsureSuccessStatusCode();
            List<Issuer> issuers = response.Content.ReadAsAsync<List<Issuer>>().Result;
            comboBoxIssuer.ItemsSource = issuers;
            comboBoxIssuer.SelectedValuePath = "Id";
            comboBoxIssuer.DisplayMemberPath = "Name";
            comboBoxIssuer.SelectedValue = issuers[0].Id;

            //load currency code combo box
            List<string> currencyCodes = DataAccessor.GetCurrencyCodes();
            comboBoxCurrencyCode.ItemsSource = currencyCodes;
            comboBoxCurrencyCode.SelectedValue = "USD";
        }

        private void Load()
        {
            if (File.Exists(FileName))
            {
                this.m_Wallet = JsonHelper.Deserialize<Wallet>(File.ReadAllText(FileName));
            }
            else
            {
                this.m_Wallet = new Wallet();
            }
            this.UpdateDataGrid();
            this.LoadComboBox();
        }

        private void Save()
        {
            File.WriteAllText(FileName, JsonHelper.Serialize(m_Wallet));
        }
    }
}
