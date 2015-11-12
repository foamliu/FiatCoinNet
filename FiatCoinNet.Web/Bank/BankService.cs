using FiatCoinNet.Common;
using FiatCoinNet.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace FiatCoinNetWeb.Controllers
{
    public class BankService
    {
        private const int MAX_LOWERLEVELBLOCK = 100;
        public Bank bank;

        public BankService()
        {
            string PublicKey = "RUNTMSAAAADcmtiQ8GgWydlQseioV45M+ZfjicIF82yCJrjr4bqJEIsaYeMEw7cH8uArUuE9W9cx8pskNIP6AZB23DunLsHs";
            string PrivateKey = "RUNTMiAAAADcmtiQ8GgWydlQseioV45M+ZfjicIF82yCJrjr4bqJEIsaYeMEw7cH8uArUuE9W9cx8pskNIP6AZB23DunLsHsYivlMzw31CW+v2G9TKcuNtHp14Y9GRPaRhiJN/eUQcM=";
            Bank bank = new Bank(PublicKey, PrivateKey);
        }

        public void CreateHigherLevelBlock(object source, ElapsedEventArgs e)
        {

            if (bank.s_LowerLevelBlockPool.Count == 0)
                return;
            //Construct high level block
            HigherLevelBlock block = new HigherLevelBlock();
            block.LowLevelBlockCounter = 0;

            //Construct the block header
            //block.blockHeader.hashPrevBlock = GetPreviousTransactionHash();
            block.blockHeader.Time = GetTime(DateTime.Now);
            block.blockHeader.Version = bank.version;

            while (bank.s_LowerLevelBlockPool.Count != 0 && block.LowLevelBlockCounter < MAX_LOWERLEVELBLOCK)
            {
                LowerLevelBlock llb = bank.s_LowerLevelBlockPool.Dequeue();
                if (CheckingLedger(llb) == true)
                {
                    block.LowerLevelBlockSet.Add(llb);
                    block.LowLevelBlockCounter++;
                }
            }

            //Add the hash & size of this block
            block.Hash = CryptoHelper.Hash(JsonHelper.Serialize(block));
            block.blockSize = Marshal.SizeOf(block);

            //TODO: Call api to send ACK message to Issuer
            
        }

        private bool CheckingLedger(LowerLevelBlock llb)
        {
            //TODO:check ledger
            throw new NotImplementedException();
        }

        private static double GetTime(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            double result = (time - startTime).TotalSeconds;
            return result;
        }
    }
}
