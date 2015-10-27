using FiatCoinNet.Common;
using FiatCoinNet.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FiatCoinNetWeb.Controllers
{
    public class BankApiController : ApiController
    {
        private static readonly Bank s_Bank;
        private static readonly List<HigherLevelBlock> s_Blocks;
        private static readonly List<Tuple<long, string>> s_Hashes;
        private static long s_Period = 0;

        public static List<Issuer> CertifiedIssuers { get; set; }

        static BankApiController()
        {
            s_Bank = new Bank
            {
                PublicKey = "RUNTMSAAAADcmtiQ8GgWydlQseioV45M+ZfjicIF82yCJrjr4bqJEIsaYeMEw7cH8uArUuE9W9cx8pskNIP6AZB23DunLsHs",
                PrivateKey = "RUNTMiAAAADcmtiQ8GgWydlQseioV45M+ZfjicIF82yCJrjr4bqJEIsaYeMEw7cH8uArUuE9W9cx8pskNIP6AZB23DunLsHsYivlMzw31CW+v2G9TKcuNtHp14Y9GRPaRhiJN/eUQcM="
            };

            CertifiedIssuers = new List<Issuer>();
            var issuer1 = new Issuer
            {
                Id = 1010,
                Name = "菲特银行北海分行",
                PublicKey = "RUNTMSAAAAB9kGHlnnUY0FGYBCzd7WdcZifnx+wfPVswjSrxYqAw7sC92RYMLB2iJA9f7utNbhX7WGUgzWwKd+Y4wscGA98G",
                PrivateKey = "RUNTMiAAAAB9kGHlnnUY0FGYBCzd7WdcZifnx+wfPVswjSrxYqAw7sC92RYMLB2iJA9f7utNbhX7WGUgzWwKd+Y4wscGA98G65oCkARn28CZeMfTC9i84DMlfc7vwSABoRVRmWlaLR4=",
                SignatureToCertifyIssuer = "",
                PaymentAccounts = new List<PaymentAccount>()
            };
            issuer1.SignatureToCertifyIssuer = CryptoHelper.Sign(s_Bank.PrivateKey, issuer1.PublicKey);
            CertifiedIssuers.Add(issuer1);

            var issuer2 = new Issuer
            {
                Id = 1942,
                Name = "菲特银行南海分行",
                PublicKey = "RUNTMSAAAADn7HBQqfSnjcD2R3UFKyirGIAqk65+NPWMIlX3Ilp95HpZLWt9DqSYowSbCQ1wUienJ9wQ2GEoYKWOEwMF9jl6",
                PrivateKey = "RUNTMiAAAADn7HBQqfSnjcD2R3UFKyirGIAqk65+NPWMIlX3Ilp95HpZLWt9DqSYowSbCQ1wUienJ9wQ2GEoYKWOEwMF9jl6bLAWC+sFREWvj1rfY97K49tosn9eg5CQ9Q3e1i59zgQ=",
                SignatureToCertifyIssuer = "",
                PaymentAccounts = new List<PaymentAccount>()
            };
            issuer2.SignatureToCertifyIssuer = CryptoHelper.Sign(s_Bank.PrivateKey, issuer2.PublicKey);
            CertifiedIssuers.Add(issuer2);

            s_Blocks = new List<HigherLevelBlock>();
            s_Hashes = new List<Tuple<long, string>>();

            s_Period = 0;
        }

        /// <summary>
        /// 取得所有注册银行列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("certifier/api/issuers")]
        public List<Issuer> GetCertifiedIssuers()
        {
            return CertifiedIssuers;
        }

        /// <summary>
        /// 取得某银行的公钥
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("certifier/api/issuers/{id}/key")]
        public HttpResponseMessage GetKey([FromUri]int id)
        {
            var issuer = CertifiedIssuers.First(i => i.Id == id);
            if (issuer == null)
            {
                var message = string.Format("Issuer with id = {0} not found", id);

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, message);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, issuer.PublicKey);
            }
        }

        /// <summary>
        /// 下载所有的区块
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("certifier/api/blocks")]
        public List<HigherLevelBlock> GetBlocks()
        {
            return s_Blocks;
        }

        /// <summary>
        /// 下载某时间段的区块
        /// </summary>
        /// <param name="period"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("certifier/api/blocks/{period}")]
        public HigherLevelBlock GetBlocks(long period)
        {
            return s_Blocks.FirstOrDefault<HigherLevelBlock>(b => b.Period == period);
        }

        [HttpGet]
        [Route("certifier/api/hashes")]
        public List<Tuple<long, string>> GetHashes()
        {
            return s_Hashes;
        }

        [HttpGet]
        [Route("certifier/api/hashes/{period}")]
        public string GetHash(long period)
        {
            return s_Hashes.FirstOrDefault<Tuple<long, string>>(t => t.Item1 == period).Item2;
        }

        /// <summary>
        /// 取得当前时间段ID
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("certifier/api/period")]
        public long GetPeriod()
        {
            return s_Period;
        }
    }
}
