using System;
using System.Diagnostics;

// https://github.com/XRPLF/xrpl.js/blob/main/packages/ripple-address-codec/src/xrp-codec.ts

namespace Ripple.Address.Codec
{
    public class XrplCodec
    {
        public const string Alphabet = "rpshnaf39wBUDNEGHJKLM4PQRST7VWXYZ2bcdeCg65jkm8oFqi1tuvAxyz";

        // Account address (20 bytes)
        public static B58.Version AccountID = B58.Version.With(versionByte: 0, expectedLength: 20);
        // Account public key (33 bytes)
        public static B58.Version PublicKey = B58.Version.With(versionByte: 35, expectedLength: 33);
        // 33; Seed value (for secret keys) (16 bytes)
        public static B58.Version K256Seed = B58.Version.With(versionByte: 33, expectedLength: 16);
        // [1, 225, 75]
        public static B58.Version Ed25519Seed = B58.Version.With(versionBytes: new byte[] { 0x1, 0xe1, 0x4b }, expectedLength: 16);
        // 28; Validation public key (33 bytes)
        public static B58.Version NodePublic = B58.Version.With(versionByte: 28, expectedLength: 33);

        public static B58.Versions AnySeed = B58.Versions.With("secp256k1", K256Seed).And("ed25519", Ed25519Seed);

        private static readonly B58 B58;
        static XrplCodec()
        {
            B58 = new B58(Alphabet);
        }

        public class DecodedSeed
        {
            public readonly string Type;
            public readonly byte[] Bytes;

            public DecodedSeed(string type, byte[] payload)
            {
                Type = type;
                Bytes = payload;
            }
        }

        /// <summary>
        /// Returns an encoded seed.
        /// </summary>
        /// <param entropy="byte[]">Entropy bytes of SEED_LENGTH.</param>
        /// <param type="string">Either ED25519 or SECP256K1.</param>
        /// <returns>An encoded seed.</returns>
        /// <throws> AddressCodecError: If entropy is not of length SEED_LENGTH
        /// or the encoding type is not one of CryptoAlgorithm.</throws>
        public static string EncodeSeed(byte[] bytes, string type)
        {
            return B58.Encode(bytes, type, AnySeed);
        }

        /// <summary>
        /// Returns (decoded seed, its algorithm).
        /// </summary>
        /// <param seed="string">The b58 encoding of a seed.</param>
        /// <returns>A(decoded seed, its algorithm).</returns>
        /// <throws>SeedError: If the seed is invalid.</throws>
        public static DecodedSeed DecodeSeed(string seed)
        {
            var decoded = B58.Decode(seed, AnySeed);
            return new DecodedSeed(decoded.Type, decoded.Payload);
        }

        /// <summary>
        /// Returns the classic address encoding of these bytes as a base58 string.
        /// </summary>
        /// <param bytes="byte[]">Bytes to be encoded.</param>
        /// <returns>The classic address encoding of these bytes as a base58 string.</returns>
        public static string EncodeAccountID(byte[] bytes)
        {
            return B58.Encode(bytes, AccountID);
        }

        /// <summary>
        /// Returns the decoded bytes of the classic address.
        /// </summary>
        /// <param classicAddress="string">Classic address to be decoded.</param>
        /// <returns>The decoded bytes of the classic address.</returns>
        public static byte[] DecodeAccountID(string accountId)
        {
            return B58.Decode(accountId, AccountID);
        }

        /// <summary>
        /// Returns the account public key encoding of these bytes as a base58 string.
        /// </summary>
        /// <param bytes="byte[]">Bytes to be encoded.</param>
        /// <returns>The account public key encoding of these bytes as a base58 string.</returns>
        public static string EncodeAccountPublic(byte[] bytes)
        {
            return B58.Encode(bytes, PublicKey);
        }

        /// <summary>
        /// Returns the decoded bytes of the account public key.
        /// </summary>
        /// <param accountPublicKey="string">Account public key to be decoded.</param>
        /// <returns>The decoded bytes of the account public key.</returns>
        public static byte[] DecodeAccountPublic(string address)
        {

            return B58.Decode(address, PublicKey);
        }

        /// <summary>
        /// Returns the node public key encoding of these bytes as a base58 string.
        /// </summary>
        /// <param bytes="byte[]">Bytes to be encoded.</param>
        /// <returns>The node public key encoding of these bytes as a base58 string.</returns>
        public static string EncodeNodePublic(byte[] bytes)
        {
            return B58.Encode(bytes, NodePublic);
        }

        /// <summary>
        /// Returns the decoded bytes of the node public key
        /// </summary>
        /// <param nodePublicKey="string">Node public key to be decoded.</param>
        /// <returns>The decoded bytes of the node public key.</returns>
        public static byte[] DecodeNodePublic(string publicKey)
        {
            return B58.Decode(publicKey, NodePublic);
        }

        /// <summary>
        /// Returns a bool representing if the classic address is valid.
        /// </summary>
        /// <param address="string">Classic address to validate.</param>
        /// <returns>A bool representing if the classic address is valid.</returns>
        public static bool IsValidClassicAddress(string address)
        {
            return B58.IsValid(address, AccountID);
        }
    }
}
