using FiatCoinNet.Common;
using FiatCoinNet.Domain;
using FiatCoinNet.Domain.Requests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace FiatCoinNet.WalletGui
{
    public enum TransactionType
    {
        所有交易 = 0,

        转出 = 1,

        转入 = 2

    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string FileName = "wallet.json";

        private Wallet m_Wallet;

        private List<PaymentTransaction> m_Transactions = new List<PaymentTransaction>();

        private const string baseUrl = "http://localhost:48701/"; 
        //private const string baseUrl = "http://fiatcoinet.azurewebsites.net/";
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
            BindAddressForExchange();
            BindCurrencyCodeForExchange();
        }


        private void miNewAddress_Click(object sender, RoutedEventArgs e)
        {
            string privateKey;
            string publicKey;
            CryptoHelper.GenerateKeyPair(out privateKey, out publicKey);

            string fingerPrint = CryptoHelper.Hash(publicKey);
            int issuerId = 0;
            try
            {
                issuerId = (int)comboBoxIssuer.SelectedValue;
            }
            catch(Exception)
            {
                MessageBox.Show("请选择开户银行","警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string currencyCode = (string)comboBoxCurrencyCode.SelectedValue;
            if(currencyCode == null)
            {
                MessageBox.Show("请选择交易货币代码", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var account = new PaymentAccount
            {
                Address = FiatCoinHelper.ToAddress(issuerId, fingerPrint),
                CurrencyCode = currencyCode,
                Balance = 0.00m,
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

            //Allocate initial balance
            string baseAccount = "8gMAAA==+u3qZ1H9Ha0dOT6WX3d7Hr9npKQRreoFdGp4VourKtQ=";
            requestUri = string.Format("issuer/api/{0}/accounts/pay", issuerId);
            Random ran = new Random();
            int i_ranAmount = ran.Next(1, 499);
            float f_ranAmount = (float)(i_ranAmount * 0.01);
            var payRequest = new DirectPayRequest
            {
                PaymentTransaction = new PaymentTransaction
                {
                    Source = baseAccount,
                    Dest = Convert.ToBase64String(BitConverter.GetBytes(issuerId)) + fingerPrint,
                    Amount = Convert.ToDecimal(f_ranAmount),
                    CurrencyCode = currencyCode,
                    MemoData = "Initial-balance"
                }
            };
            payRequest.Signature = CryptoHelper.Sign("RUNTMiAAAAA7Fyutk/Pd2VotgUewM7QpS0lTMUwZC0PewDg47HFhIoq0rjlnUTraDpS5gurmvVybU357HBOZkX3aKon4FcSdrLKIvEgjHUbRuUt2bze5HNflkQRitCWbxYc7FVGxlog=", payRequest.ToMessage());
            content = new StringContent(JsonHelper.Serialize(payRequest));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            response = HttpClient.PostAsync(requestUri, content).Result;
            response.EnsureSuccessStatusCode();

            GetAccountBalances();
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
            int issuerId = toDeleteFromBindedList.IssuerId;
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
                        GetAccountBalances();
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

        private void exchangePayFromSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            if (selectionChangedEventArgs.Source is ComboBox)
            {
                int index = exchangePayFrom.SelectedIndex;
                exchangeAccountCurrency.Text = m_Wallet.PaymentAccounts[index].CurrencyCode;
                exchangePayTo.Text = "稍后我们会为您创建新的账户";
            }
        }

        private void TransactionTypeSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            if (selectionChangedEventArgs.Source is ComboBox)
            {
                string value = comboBoxTransactionType.SelectedValue.ToString();
                switch (value)
                {
                    case "所有交易":
                        m_Transactions = GetAllTransactions();
                        break;
                    case "转出":
                        break;
                    case "转入":
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
            Settings settings = new Settings();
            settings.Show();
        }

        private void miAbout_Click(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.Show();
        }

        private void btnPay_Click(object sender, RoutedEventArgs e)
        {
            int issuerId = 0;
            try
            {
                issuerId = FiatCoinHelper.GetIssuerId(payFrom.SelectedValue.ToString());
            }
            catch (Exception)
            {
                MessageBox.Show("请选择付款账户", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!validateTransaction(payFrom.SelectedValue.ToString(), payTo.Text, payAmount.Text))
            {
                return;
            }
            string requestUri = string.Format("issuer/api/{0}/accounts/pay", issuerId);
            var payRequest = new DirectPayRequest
            {
                PaymentTransaction = new PaymentTransaction
                {
                    Source = payFrom.SelectedValue.ToString(),
                    Dest = payTo.Text,
                    Amount = Convert.ToDecimal(payAmount.Text),
                    CurrencyCode = payCurrencyCode.Text,
                    MemoData = MemoData.Text,
                }
            };
            payRequest.Signature = CryptoHelper.Sign(m_Wallet.PaymentAccounts[payFrom.SelectedIndex].PrivateKey, payRequest.ToMessage());
            HttpContent content = new StringContent(JsonHelper.Serialize(payRequest));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = HttpClient.PostAsync(requestUri, content).Result;
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch(Exception ex)
            {
                MessageBox.Show("付款失败,错误码:" + ex);
                return;
            }

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
        
        private void BindAddressForExchange()
        {
            exchangePayFrom.ItemsSource = m_Wallet.PaymentAccounts;
            exchangePayFrom.SelectedValuePath = "Address";
            exchangePayFrom.DisplayMemberPath = "Address";
        }

        private void BindCurrencyCodeForExchange()
        {
            List<string> currencyCodes = DataAccessor.GetCurrencyCodes();
            exchangeCurrency.ItemsSource = currencyCodes;
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
                GetAccountBalances();
                GetTotalBalances();
            }
            else
            {
                this.m_Wallet = new Wallet();
            }
            UpdateAddressDataGrid();
        }

        private void GetAccountBalances()
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
                //response.EnsureSuccessStatusCode();
                paymentAccount.Balance = response.Content.ReadAsAsync<PaymentAccount>().Result.Balance;
            }
        }

        private void GetTotalBalances()
        {
            decimal result = 0;
            foreach (var paymentAccount in m_Wallet.PaymentAccounts)
            {
                result += paymentAccount.Balance;
            }
            labelBalanceAvailable.Content = result;
            //TODO: work on balance not available
            labelBalanceNotAvailable.Content = 0;
            labelBalanceAmount.Content = result;
        }

        private void Save()
        {
            File.WriteAllText(FileName, JsonHelper.Serialize(m_Wallet));
        }

        private List<PaymentTransaction> GetAllTransactions()
        {
            List<PaymentTransaction> result = new List<PaymentTransaction>();
            string requestUri = "certifier/api/blocks";
            HttpResponseMessage response = HttpClient.GetAsync(requestUri).Result;
            if (response.IsSuccessStatusCode)
            {
                List<HigherLevelBlock> transactions = response.Content.ReadAsAsync<List<HigherLevelBlock>>().Result;
                foreach (var block in transactions)
                {
                    PaymentTransaction trans = new PaymentTransaction();
                    trans = block.LowerLevelBlockSet[0].TransactionSet[0];
                    result.Add(trans);
                }
            }

            return result;
        }


        private bool validateTransaction(string PayFrom, string PayTo, string payAmount)
        {
            //TO DO: check the balance available for transaction

            if (payAmount == "")
            {
                MessageBox.Show("请填写付款金额", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if(!IsNumeric(payAmount))
            {
                MessageBox.Show("请填写正确的付款金额", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            decimal balance = GetAccountBalance(PayFrom);
            if(Convert.ToDecimal(payAmount) > balance)
            {
                MessageBox.Show("余额不足", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            //TO DO: check the payTo account for available
            if (payTo.Text == "")
            {
                MessageBox.Show("请填写收款账户", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            try
            {
                int issuerId = FiatCoinHelper.GetIssuerId(PayTo);
            }
            catch (Exception)
            {
                MessageBox.Show("此收款账户不存在", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
            
        }

        private static bool IsNumeric(string value)
        {
            return Regex.IsMatch(value, @"^[+-]?\d*[.]?\d*$");
        }

        private void exchangeCurrencySelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            if (selectionChangedEventArgs.Source is ComboBox)
            {
                int index = exchangeCurrency.SelectedIndex;
                string exchangeFrom = m_Wallet.PaymentAccounts[index].CurrencyCode;
                string exchangeTo = exchangeCurrency.SelectedValue.ToString();
                if(exchangeFrom == exchangeTo)
                {
                    exchangeRate.Text = "1";
                    return;
                }
                string baseUrlForExchange = "http://api.fixer.io/";
                HttpClient HttpClientForExchange = new HttpClient{
                    BaseAddress = new Uri(baseUrlForExchange),
                };
                string requestUri = "latest?base=" + exchangeFrom;
                HttpResponseMessage response = HttpClientForExchange.GetAsync(requestUri).Result;
                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    JObject jo = (JObject)JsonConvert.DeserializeObject(result);
                    exchangeRate.Text = (string)jo["rates"][exchangeTo];
                }
            }
        }

        private void btnExchange_Click(object sender, RoutedEventArgs e)
        {
            int issuerId = 0;
            try
            {
                issuerId = FiatCoinHelper.GetIssuerId(exchangePayFrom.SelectedValue.ToString());
            }
            catch (Exception)
            {
                MessageBox.Show("请选择付款账户", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            //TODO:create new account for exchange currency
            string destAccount = NewAddressForExchange();
            MessageBox.Show("您的新账户为 " + destAccount);

            string requestUri = string.Format("issuer/api/{0}/accounts/pay", issuerId);
            decimal balance = GetAccountBalance(exchangePayFrom.Text);
            decimal exchangeAmount = Convert.ToDecimal(exchangeRate.Text) * balance;
            var payRequest = new DirectPayRequest
            {
                PaymentTransaction = new PaymentTransaction
                {
                    Source = exchangePayFrom.SelectedValue.ToString(),
                    Dest = destAccount,
                    Amount = exchangeAmount,
                    CurrencyCode = exchangeCurrency.SelectedValue.ToString(),
                    MemoData = "Exchange from " + exchangeAccountCurrency.Text + " to " + exchangeCurrency.Text,
                }
            };
            payRequest.Signature = CryptoHelper.Sign(m_Wallet.PaymentAccounts[exchangePayFrom.SelectedIndex].PrivateKey, payRequest.ToMessage());
            HttpContent content = new StringContent(JsonHelper.Serialize(payRequest));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = HttpClient.PostAsync(requestUri, content).Result;
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                MessageBox.Show("兑换失败,错误码:" + ex);
                return;
            }
            MessageBox.Show("账户兑换为 " + exchangeCurrency.Text + " 余额为 " + GetAccountBalance(destAccount));
            //TODO: Delete former account
            RemoveOldAddressForExchange(exchangePayFrom.SelectedValue.ToString());

            MessageBox.Show("兑换成功");
            exchangePayTo.Text = destAccount;

            GetAccountBalances();
            this.UpdateAddressDataGrid();
            this.Save();
        }

        private void RemoveOldAddressForExchange(string account)
        {
            // unregister this account
            int issuerId = FiatCoinHelper.GetIssuerId(account);
            string requestUri = string.Format("issuer/api/{0}/accounts/unregister", issuerId);
            var unregisterRequest = new UnregisterRequest
            {
                Address = account
            };
            unregisterRequest.Signature = CryptoHelper.Sign(m_Wallet.PaymentAccounts[exchangePayFrom.SelectedIndex].PrivateKey, unregisterRequest.ToMessage());
            HttpContent content = new StringContent(JsonHelper.Serialize(unregisterRequest));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = HttpClient.PostAsync(requestUri, content).Result;
            response.EnsureSuccessStatusCode();

            m_Wallet.PaymentAccounts.Remove(m_Wallet.PaymentAccounts[exchangePayFrom.SelectedIndex]);

            this.UpdateAddressDataGrid();
            this.Save();
        }

        private string NewAddressForExchange()
        {
            string privateKey;
            string publicKey;
            CryptoHelper.GenerateKeyPair(out privateKey, out publicKey);

            string fingerPrint = CryptoHelper.Hash(publicKey);
            int issuerId = FiatCoinHelper.GetIssuerId(exchangePayFrom.SelectedValue.ToString());
            string currencyCode = exchangeCurrency.SelectedValue.ToString();
            var account = new PaymentAccount
            {
                Address = FiatCoinHelper.ToAddress(issuerId, fingerPrint),
                CurrencyCode = currencyCode,
                Balance = 0.00m,
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
            return account.Address;

        }

        private decimal GetAccountBalance(string Account)
        {
            int issuerId = FiatCoinHelper.GetIssuerId(Account);
            string requestUri = string.Format("issuer/api/{0}/accounts/get", issuerId);
            var getRequest = new GetAccountRequest
            {
                Address = Account
            };
            HttpContent content = new StringContent(JsonHelper.Serialize(getRequest));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = HttpClient.PostAsync(requestUri, content).Result;
            decimal balance = response.Content.ReadAsAsync<PaymentAccount>().Result.Balance;
            return balance;
        }

        private void receiveCurrency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
