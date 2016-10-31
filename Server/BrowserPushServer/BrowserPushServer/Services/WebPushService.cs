using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using BrowserPushServer.Models;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace BrowserPushServer.Services
{
    public class WebPushService
    {
        private readonly string _firebaseServerKey;

        public WebPushService(string firebaseServerKey)
        {
            _firebaseServerKey = firebaseServerKey;
        }

        public async Task<HttpStatusCode> SendNotification(Subscription sub, SendRequest data, int ttl = 0, ushort padding = 0,
                                            bool randomisePadding = false)
        {
            var serializedData = JsonConvert.SerializeObject(data);
            return await SendNotification(sub.Endpoint,
                                    data: Encoding.UTF8.GetBytes(serializedData),
                                    userKey: sub.PublicKey,
                                    userSecret: sub.SecretKey,
                                    ttl: ttl,
                                    padding: padding,
                                    randomisePadding: randomisePadding);
        }

        private async Task<HttpStatusCode> SendNotification(string endpoint, byte[] userKey, byte[] userSecret, byte[] data = null,
                                        int ttl = 0, ushort padding = 0, bool randomisePadding = false)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            if (endpoint.StartsWith("https://android.googleapis.com/gcm/send/"))
                request.Headers.TryAddWithoutValidation("Authorization", "key=" + _firebaseServerKey);
            request.Headers.Add("TTL", ttl.ToString());
            if (data != null && userKey != null && userSecret != null)
            {
                EncryptionResult package = EncryptMessage(userKey, userSecret, data, padding, randomisePadding);
                request.Content = new ByteArrayContent(package.Payload);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                request.Content.Headers.ContentLength = package.Payload.Length;
                request.Content.Headers.ContentEncoding.Add("aesgcm");
                request.Headers.Add("Crypto-Key", "keyid=p256dh;dh=" + WebEncoders.Base64UrlEncode(package.PublicKey));
                request.Headers.Add("Encryption", "keyid=p256dh;salt=" + WebEncoders.Base64UrlEncode(package.Salt));
            }
            using (HttpClient hc = new HttpClient())
            {
                var response = await hc.SendAsync(request);
                return response.StatusCode;
            }
        }

        private static EncryptionResult EncryptMessage(byte[] userKey, byte[] userSecret, byte[] data,
                                                      ushort padding = 0, bool randomisePadding = false)
        {
            SecureRandom random = new SecureRandom();
            byte[] salt = new byte[16];
            random.NextBytes(salt);
            X9ECParameters curve = ECNamedCurveTable.GetByName("prime256v1");
            ECDomainParameters spec = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
            ECKeyPairGenerator generator = new ECKeyPairGenerator();
            generator.Init(new ECKeyGenerationParameters(spec, new SecureRandom()));
            AsymmetricCipherKeyPair keyPair = generator.GenerateKeyPair();
            ECDHBasicAgreement agreementGenerator = new ECDHBasicAgreement();
            agreementGenerator.Init(keyPair.Private);
            BigInteger ikm = agreementGenerator.CalculateAgreement(new ECPublicKeyParameters(spec.Curve.DecodePoint(userKey), spec));
            byte[] prk = GenerateHkdf(userSecret, ikm.ToByteArrayUnsigned(), Encoding.UTF8.GetBytes("Content-Encoding: auth\0"), 32);
            byte[] publicKey = ((ECPublicKeyParameters)keyPair.Public).Q.GetEncoded(false);
            byte[] cek = GenerateHkdf(salt, prk, CreateInfoChunk("aesgcm", userKey, publicKey), 16);
            byte[] nonce = GenerateHkdf(salt, prk, CreateInfoChunk("nonce", userKey, publicKey), 12);
            if (randomisePadding && padding > 0) padding = Convert.ToUInt16(Math.Abs(random.NextInt()) % (padding + 1));
            byte[] input = new byte[padding + 2 + data.Length];
            Buffer.BlockCopy(ConvertInt(padding), 0, input, 0, 2);
            Buffer.BlockCopy(data, 0, input, padding + 2, data.Length);
            IBufferedCipher cipher = CipherUtilities.GetCipher("AES/GCM/NoPadding");
            cipher.Init(true, new AeadParameters(new KeyParameter(cek), 128, nonce));
            byte[] message = new byte[cipher.GetOutputSize(input.Length)];
            cipher.DoFinal(input, 0, input.Length, message, 0);
            return new EncryptionResult() { Salt = salt, Payload = message, PublicKey = publicKey };
        }

        private class EncryptionResult
        {
            public byte[] PublicKey { get; set; }
            public byte[] Payload { get; set; }
            public byte[] Salt { get; set; }
        }

        private static byte[] ConvertInt(int number)
        {
            byte[] output = BitConverter.GetBytes(Convert.ToUInt16(number));
            if (BitConverter.IsLittleEndian) Array.Reverse(output);
            return output;
        }

        private static byte[] CreateInfoChunk(string type, byte[] recipientPublicKey, byte[] senderPublicKey)
        {
            List<byte> output = new List<byte>();
            output.AddRange(Encoding.UTF8.GetBytes($"Content-Encoding: {type}\0P-256\0"));
            output.AddRange(ConvertInt(recipientPublicKey.Length));
            output.AddRange(recipientPublicKey);
            output.AddRange(ConvertInt(senderPublicKey.Length));
            output.AddRange(senderPublicKey);
            return output.ToArray();
        }

        private static byte[] GenerateHkdf(byte[] salt, byte[] ikm, byte[] info, int len)
        {
            IMac prkGen = MacUtilities.GetMac("HmacSHA256");
            prkGen.Init(new KeyParameter(MacUtilities.CalculateMac("HmacSHA256", new KeyParameter(salt), ikm)));
            prkGen.BlockUpdate(info, 0, info.Length);
            prkGen.Update(1);
            byte[] result = MacUtilities.DoFinal(prkGen);
            if (result.Length > len) Array.Resize(ref result, len);
            return result;
        }

    }
}