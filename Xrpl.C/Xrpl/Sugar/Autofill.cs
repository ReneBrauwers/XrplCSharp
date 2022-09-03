﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Ocsp;
using Ripple.Address.Codec;
using Xrpl.Client;
using Xrpl.Client.Exceptions;
using Xrpl.Client.Models.Common;
using Xrpl.Client.Models.Ledger;
using Xrpl.Client.Models.Methods;
using Xrpl.Client.Models.Utils;
using System.Numerics;
using static Ripple.Address.Codec.AddressCodec;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;

// https://github.com/XRPLF/xrpl.js/blob/main/packages/xrpl/src/sugar/autofill.ts

namespace Xrpl.Sugar
{
    public class AutofillSugar
    {

        static readonly int LEDGER_OFFSET = 20;

        public class AddressNTag
        {
            public string ClassicAddress { get; set; }
            public int? Tag { get; set; }
        }

        public static async Task<Dictionary<string, dynamic>> Autofill(IRippleClient client, Dictionary<string, dynamic> transaction, int? signersCount)
        {
            Dictionary<string, dynamic> tx = transaction;

            SetValidAddresses(tx);

            //Flags.SetTransactionFlagsToNumber(tx);
            List<Task> promises = new List<Task>();
            if (!tx.ContainsKey("Sequence"))
            {
                promises.Add(SetNextValidSequenceNumberAsync(client, tx));
            }
            //if (tx["Fee"] == null)
            //{
            //    promises.push(CalculateFeePerTransactionType(client, tx, signersCount))
            //}
            if (!tx.ContainsKey("LastLedgerSequence"))
            {
                promises.Append(SetLatestValidatedLedgerSequence(client, tx));
            }
            //if (tx.TransactionType === 'AccountDelete')
            //{
            //    promises.push(CheckAccountDeleteBlockers(client, tx))
            //}
            Task.WaitAll(promises.ToArray());
            return tx;
        }
        public static void SetValidAddresses(Dictionary<string, dynamic> tx)
        {
            ValidateAccountAddress(tx, "Account", "SourceTag");
            if (tx.ContainsKey("Destination"))
            {
                ValidateAccountAddress(tx, "Destination", "DestinationTag");
            }

            // DepositPreauth:
            ConvertToClassicAddress(tx, "Authorize");
            ConvertToClassicAddress(tx, "Unauthorize");
            // EscrowCancel, EscrowFinish:
            ConvertToClassicAddress(tx, "Owner");
            // SetRegularKey:
            ConvertToClassicAddress(tx, "RegularKey");
        }

        public static void ValidateAccountAddress(Dictionary<string, dynamic> tx, string accountField, string tagField)
        {
            // if X-address is given, convert it to classic address
            AddressNTag classicAccount = GetClassicAccountAndTag((string)tx[accountField], null);
            tx[accountField] = classicAccount.ClassicAddress;

            // XRPL: Does bool or int. Smells.
            //if (classicAccount.Tag != null && classicAccount.Tag != false)
            if (classicAccount.Tag != null)
            {
                if (tx[tagField] != null && (int)tx[tagField] != classicAccount.Tag)
                {
                    throw new ValidationError($"The { tagField }, if present, must match the tag of the { accountField} X - address");
                }
                // eslint-disable-next-line no-param-reassign -- param reassign is safe
                tx[tagField] = classicAccount.Tag;
            }
        }

        public static AddressNTag GetClassicAccountAndTag(string account, int? expectedTag)
        {
            if (AddressCodec.IsValidXAddress(account))
            {
                CodecAddress codecAddress = AddressCodec.XAddressToClassicAddress(account);
                if (expectedTag != null && codecAddress.Tag != expectedTag)
                {
                    throw new ValidationError("address includes a tag that does not match the tag specified in the transaction");
                }
                return new AddressNTag { ClassicAddress = codecAddress.ClassicAddress, Tag = codecAddress.Tag };
            }
            return new AddressNTag { ClassicAddress = account, Tag = expectedTag };
        }

        public static void ConvertToClassicAddress(Dictionary<string, dynamic> tx, string fieldName)
        {
            if (tx.ContainsKey(fieldName))
            {
                string account = (string)tx[fieldName];
                if (account is string)
                {
                    AddressNTag addressntag = GetClassicAccountAndTag(account, null);
                    tx[fieldName] = addressntag.ClassicAddress;
                }
            }
        }

        public static async Task SetNextValidSequenceNumberAsync(IRippleClient client, Dictionary<string, dynamic> tx)
        {
            LedgerIndex index = new LedgerIndex(LedgerIndexType.Current);
            AccountInfoRequest request = new AccountInfoRequest((string)tx["Account"]) { LedgerIndex = index };
            AccountInfo data = await client.AccountInfo(request);
            tx["Sequence"] = data.AccountData.Sequence;
        }

        public static async Task<BigInteger> FetchAccountDeleteFee(IRippleClient client)
        {
            ServerInfoRequest request = new ServerInfoRequest();
            ServerInfo data = await client.ServerInfo(request);
            uint? fee = data.Info.ValidatedLedger.ReserveIncXrp;

            if (fee == null)
            {
                Task.FromException(new XrplError("Could not fetch Owner Reserve."));
            }
            return new BigInteger(Convert.ToByte(fee));
        }

        // ....

        public static async Task SetLatestValidatedLedgerSequence(IRippleClient client, Dictionary<string, dynamic> tx)
        {
            uint ledgerSequence = await client.GetLedgerIndex();
            Debug.WriteLine($"LAST LEDGER: {ledgerSequence}");
            tx["LastLedgerSequence"] = ledgerSequence + LEDGER_OFFSET;
        }
    }
}