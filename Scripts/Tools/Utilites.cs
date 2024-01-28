using DG.Tweening;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GrandDevs.Tavern
{
    public static class Utilites
    {
        #region UnixTime

        public static long ConvertToUnixFormat(DateTime? dateTime)
        {
            if (dateTime == null) return 0;

            return (long)(dateTime.Value - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }

        public static DateTime ConvertFromUnixFormat(long? unixTime)
        {
            if (unixTime == null) return DateTime.Now;

            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return start.AddSeconds(unixTime.Value);
        }

        #endregion

        public static Sequence DoActionDelayed(TweenCallback callback, float delay = 0f)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.PrependInterval(delay);
            sequence.AppendCallback(callback);
            sequence.Play();

            return sequence;
        }

        public static void SetLayerRecursively(this GameObject obj, int layer)
        {
            obj.layer = layer;

            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
        
        public static string LimitStringLength(string str, int maxLength)
        {
            if (str.Length < maxLength)
                return str;

            return str.Substring(0, maxLength);
        }

        public static double GetTimestamp()
        {
            return new TimeSpan(DateTime.UtcNow.Ticks).TotalSeconds;
        }

        #region cryptography

        public static string Encrypt(string value, string key)
        {
            return Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(value), key));
        }

        [DebuggerNonUserCode]
        public static string Decrypt(string value, string key)
        {
            string result;

            try
            {
                using (CryptoStream cryptoStream = InternalDecrypt(Convert.FromBase64String(value), key))
                {
                    using (StreamReader streamReader = new StreamReader(cryptoStream))
                    {
                        result = streamReader.ReadToEnd();
                    }
                }
            }
            catch (CryptographicException e)
            {
                UnityEngine.Debug.LogException(e);
                return null;
            }

            return result;
        }

        private static byte[] Encrypt(byte[] key, string value)
        {
            SymmetricAlgorithm symmetricAlgorithm = Rijndael.Create();
            ICryptoTransform cryptoTransform =
                symmetricAlgorithm.CreateEncryptor(new Rfc2898DeriveBytes(value, new byte[16]).GetBytes(16), new byte[16]);

            byte[] result;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream =
                    new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(key, 0, key.Length);
                    cryptoStream.FlushFinalBlock();

                    result = memoryStream.ToArray();

                    memoryStream.Close();
                    memoryStream.Dispose();
                }
            }

            return result;
        }

        private static CryptoStream InternalDecrypt(byte[] key, string value)
        {
            SymmetricAlgorithm symmetricAlgorithm = Rijndael.Create();
            ICryptoTransform cryptoTransform =
                symmetricAlgorithm.CreateDecryptor(new Rfc2898DeriveBytes(value, new byte[16]).GetBytes(16),
                    new byte[16]);

            MemoryStream memoryStream = new MemoryStream(key);
            return new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read);
        }

        public static byte[] Base64UrlDecode(string input)
        {
            string output = input;
            output = output.Replace('-', '+');
            output = output.Replace('_', '/');
            switch (output.Length % 4)
            {
                case 0: 
                    break; 
                case 2: 
                    output += "=="; 
                    break;
                case 3: 
                    output += "="; 
                    break; 
                default: 
                    throw new Exception("Illegal base64url string!");
            }
            byte[] converted = Convert.FromBase64String(output); 
            return converted;
        }

        #endregion cryptography
    }
}

static class CanvasExtensions
{
    public static Vector2 SizeToParent(this RawImage image, float padding = 0)
    {
        var parent = image.transform.parent.GetComponent<RectTransform>();
        var imageTransform = image.GetComponent<RectTransform>();
        if (!parent) { return imageTransform.sizeDelta; } //if we don't have a parent, just return our current width;
        padding = 1 - padding;
        float w = 0, h = 0;
        float ratio = image.texture.width / (float)image.texture.height;
        var bounds = new Rect(0, 0, parent.rect.width, parent.rect.height);
        if (Mathf.RoundToInt(imageTransform.eulerAngles.z) % 180 == 90)
        {
            //Invert the bounds if the image is rotated
            bounds.size = new Vector2(bounds.height, bounds.width);
        }
        //Size by height first
        h = bounds.height * padding;
        w = h * ratio;
        if (w > bounds.width * padding)
        { //If it doesn't fit, fallback to width;
            w = bounds.width * padding;
            h = w / ratio;
        }
        imageTransform.sizeDelta = new Vector2(w, h);
        return imageTransform.sizeDelta;
    }
}
