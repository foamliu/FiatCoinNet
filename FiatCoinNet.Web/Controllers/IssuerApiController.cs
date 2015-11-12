using FiatCoinNet.Common;
using FiatCoinNet.Domain;
using FiatCoinNet.Domain.Requests;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace FiatCoinNetWeb.Controllers
{
    /// <summary>
    /// (making operations) faster, more efficient and more transparent.
    /// </summary>
    public class IssuerApiController : ApiController
    {
        // TODO: persistence
        //private static readonly ConcurrentDictionary<int, List<LowerLevelBlock>> s_Blocks;

        public BankApiController bankapi;

        public IssuerApiController()
        {
            bankapi = new BankApiController();
            //s_Blocks = new ConcurrentDictionary<int, List<LowerLevelBlock>>();

            //BankApiController.CertifiedIssuers.ForEach(i =>
            //{
            //    s_Blocks[i.Id] = new List<LowerLevelBlock>();
            //});
        }


        /// <summary>
        /// Get Registered Accounts - DEBUG ONLY
        /// </summary>
        /// <param name="issuerId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("issuer/api/{issuerId}/accounts")]
        public HttpResponseMessage GetAccounts([FromUri]int issuerId)
        {
            this.Validate(issuerId);

            var list = DataAccess.DataAccessor.FiatCoinRepository.GetAccounts(issuerId);
            return Request.CreateResponse(HttpStatusCode.OK, list);
        }

        /// <summary>
        /// Get Registered Accounts
        /// </summary>
        /// <param name="issuerId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("issuer/api/{issuerId}/accounts/get")]
        public HttpResponseMessage GetAccount([FromUri]int issuerId, [FromBody]GetAccountRequest request)
        {
            this.Validate(issuerId, request);

            var account = DataAccess.DataAccessor.FiatCoinRepository.GetAccount(issuerId, request.Address);
            var transactions = DataAccess.DataAccessor.FiatCoinRepository.GetTransactions(issuerId, request.Address);
            account.Balance = CalculateBalance(transactions, request.Address);
            return Request.CreateResponse(HttpStatusCode.OK, account);
        }

        /// <summary>
        /// Register
        /// </summary>
        /// <param name="issuerId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("issuer/api/{issuerId}/accounts/register")]
        public HttpResponseMessage Register([FromUri]int issuerId, [FromBody]RegisterRequest request)
        {
            this.Validate(issuerId, request);

            request.PaymentAccount.IssuerId = issuerId;
            DataAccess.DataAccessor.FiatCoinRepository.AddAccount(request.PaymentAccount);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// Unregister
        /// </summary>
        /// <param name="issuerId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("issuer/api/{issuerId}/accounts/unregister")]
        public HttpResponseMessage Unregister([FromUri]int issuerId, [FromBody]UnregisterRequest request)
        {
            this.Validate(issuerId, request);

            DataAccess.DataAccessor.FiatCoinRepository.CloseAccount(request.Address);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// DirectPay
        /// </summary>
        /// <param name="issuerId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("issuer/api/{issuerId}/accounts/pay")]
        public HttpResponseMessage DirectPay([FromUri]int issuerId, [FromBody]DirectPayRequest request)
        {
            this.Validate(issuerId, request);

            request.PaymentTransaction.IssuerId = issuerId;
            int srcIssuerId = FiatCoinHelper.GetIssuerId(request.PaymentTransaction.Source);
            int dstIssuerId = FiatCoinHelper.GetIssuerId(request.PaymentTransaction.Dest);

            if (srcIssuerId != dstIssuerId)
            {
                string requestUri = string.Format("issuer/api/{0}/federation/pay", dstIssuerId);
                var payRequest = new FederatedPayRequest
                {
                    PaymentTransaction = request.PaymentTransaction
                };
                var me = bankapi.bankService.bank.CertifiedIssuers.FirstOrDefault<Issuer>(i => i.Id == issuerId);
                payRequest.Signature = CryptoHelper.Sign(me.PrivateKey, payRequest.ToMessage());
                HttpContent content = new StringContent(JsonHelper.Serialize(payRequest));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = RestApiHelper.HttpClient.PostAsync(requestUri, content).Result;
                response.EnsureSuccessStatusCode();
            }
            DataAccess.DataAccessor.FiatCoinRepository.AddTransaction(request.PaymentTransaction);

            //CreateLowerLevelBlock(issuerId, request.PaymentTransaction);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("issuer/api/{issuerId}/federation/pay")]
        public HttpResponseMessage FederatedPay([FromUri]int issuerId, [FromBody]FederatedPayRequest request)
        {
            this.Validate(issuerId, request);

            request.PaymentTransaction.IssuerId = issuerId;
            DataAccess.DataAccessor.FiatCoinRepository.AddTransaction(request.PaymentTransaction);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// Fund
        /// </summary>
        /// <param name="issuerId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("issuer/api/{issuerId}/accounts/fund")]
        public HttpResponseMessage Fund([FromUri]int issuerId, [FromBody]FundRequest request)
        {
            this.Validate(issuerId, request);

            request.PaymentTransaction.IssuerId = issuerId;
            DataAccess.DataAccessor.FiatCoinRepository.AddTransaction(request.PaymentTransaction);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        #region Private Methods
        private static decimal CalculateBalance(List<PaymentTransaction> journal, string address)
        {
            return
                journal.Where(trx => trx.Dest == address).Sum(trx => trx.Amount) -
                journal.Where(trx => trx.Source == address).Sum(trx => trx.Amount);
        }

        private void Validate(int issuerId, BaseRequest baseReq = null)
        {
            if (baseReq is GetAccountRequest)
            {
                var request = (GetAccountRequest)baseReq;
                int isserId = FiatCoinHelper.GetIssuerId(request.Address);
                var account = DataAccess.DataAccessor.FiatCoinRepository.GetAccount(isserId, request.Address);
                if (account == null)
                {
                    var message = string.Format("Account with address = {0} not found", request.Address);
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, message));
                }
            }
            else if (baseReq is RegisterRequest)
            {

            }
            else if (baseReq is UnregisterRequest)
            {
                var request = (UnregisterRequest)baseReq;
                int isserId = FiatCoinHelper.GetIssuerId(request.Address);
                var account = DataAccess.DataAccessor.FiatCoinRepository.GetAccount(isserId, request.Address);
                if (account == null)
                {
                    var message = string.Format("Account with address = {0} not found", request.Address);
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NoContent, message));
                }
                ValidateRequestor(request, account);
            }
            else if (baseReq is DirectPayRequest)
            {
                var request = (DirectPayRequest)baseReq;
                int srcIsserId = FiatCoinHelper.GetIssuerId(request.PaymentTransaction.Source);
                if (srcIsserId != issuerId)
                {
                    var message = string.Format("Source's issuer Id = {0}, but the request was sent to issuer Id = {1}", srcIsserId, issuerId);
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest, message));
                }
                var account = DataAccess.DataAccessor.FiatCoinRepository.GetAccount(srcIsserId, request.PaymentTransaction.Source);
                if (account == null)
                {
                    var message = string.Format("Account with address = {0} not found", request.PaymentTransaction.Source);
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NoContent, message));
                }
                ValidateRequestor(request, account);

                var transactions = DataAccess.DataAccessor.FiatCoinRepository.GetTransactions(srcIsserId, request.PaymentTransaction.Source);
                var balance = CalculateBalance(transactions, request.PaymentTransaction.Source);
                
                if (request.PaymentTransaction.Amount > balance)
                {
                    var message = string.Format("Insufficient funds, balance = {0}, to pay = {1}", balance, request.PaymentTransaction.Amount);
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, message));
                }
            }
            else if (baseReq is FundRequest)
            {
                var request = (FundRequest)baseReq;
                int destIsserId = FiatCoinHelper.GetIssuerId(request.PaymentTransaction.Dest);
                var account = DataAccess.DataAccessor.FiatCoinRepository.GetTransactions(destIsserId, request.PaymentTransaction.Dest);
                if (account == null)
                {
                    var message = string.Format("Account with address = {0} not found", request.PaymentTransaction.Dest);
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound, message));
                }
            }
        }

        public void ValidateRequestor(BaseRequest request, PaymentAccount account)
        {
            string publicKey = account.PublicKey;
            string signature = request.Signature;
            request.Signature = null;
            string jsonString = JsonHelper.Serialize(request);
            request.Signature = signature;
            bool authorized = CryptoHelper.Verify(publicKey, jsonString, signature);

            if (!authorized)
            {
                var message = string.Format("User is not authorized to operate on the object.");
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, message));
            }
        }

        
        #endregion
    }
}
