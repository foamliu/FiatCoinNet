using FiatCoinNet.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiatCoinNetWeb.DataAccess
{
    public static class ConvertHelper
    {
        public static PaymentAccount FromRow(this PaymentAccount acct, DataRow row)
        {
            acct.Address = row.GetStringField("Address");
            acct.IssuerId = row.GetIntField("IssuerId");
            acct.CurrencyCode = row.GetStringField("CurrencyCode");
            acct.PublicKey = row.GetStringField("PublicKey");
            return acct;
        }

        public static PaymentTransaction FromRow(this PaymentTransaction trx, DataRow row)
        {
            trx.IssuerId = row.GetIntField("IssuerId");
            trx.Source = row.GetStringField("Source");
            trx.Dest = row.GetStringField("Dest");
            trx.Amount = row.GetDecimalField("Amount");
            trx.CurrencyCode = row.GetStringField("CurrencyCode");
            trx.MemoData = row.GetStringField("MemoData");
            return trx;
        }

        public static LowerLevelBlock FromRow(this LowerLevelBlock llb, DataRow row)
        {
            llb.blockSize = row.GetIntField("BlockSize");
            llb.blockHeader.Version = row.GetIntField("Version");
            llb.blockHeader.hashPrevBlock = row.GetStringField("HashPrevBlock");
            llb.blockHeader.hashMerkleRoot = row.GetStringField("HashMerkleRoot");
            llb.blockHeader.Time = row.GetIntField("Time");
            llb.blockHeader.Bits = row.GetIntField("Bits");
            llb.TransactionCounter = row.GetIntField("TransactionCounter");
            //TO DO: Finish the code
            //llb.TransactionSet. = row.Get
            return llb;
        }
        
        public static HigherLevelBlock FromRow(this HigherLevelBlock hlb, DataRow row)
        {
            hlb.blockSize = row.GetIntField("BlockSize");
            hlb.blockHeader.Version = row.GetIntField("Version");
            hlb.blockHeader.hashPrevBlock = row.GetStringField("HashPrevBlock");
            hlb.blockHeader.hashMerkleRoot = row.GetStringField("HashMerkleRoot");
            hlb.blockHeader.Time = row.GetIntField("Time");
            hlb.blockHeader.Bits = row.GetIntField("Bits");
            hlb.LowLevelBlockCounter = row.GetIntField("LowLevelBlockCounter");
            //TO DO: Finish the code

            return hlb;
        }

        public static string GetStringField(this DataRow row, string columnName)
        {
            return row[columnName] == DBNull.Value ? null : Convert.ToString(row[columnName]);
        }

        public static Byte GetByteField(this DataRow row, string columnName)
        {
            return Convert.ToByte(row[columnName]);
        }

        public static int GetIntField(this DataRow row, string columnName)
        {
            return Convert.ToInt32(row[columnName]);
        }

        public static long GetInt64Field(this DataRow row, string columnName)
        {
            return Convert.ToInt64(row[columnName]);
        }

        public static decimal GetDecimalField(this DataRow row, string columnName)
        {
            return Convert.ToDecimal(row[columnName]);
        }

        public static byte[] GetByteArray(this DataRow row, string columnName)
        {
            return (byte[])row[columnName];
        }
    }
}
