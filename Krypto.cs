using System;
using System.Security.Cryptography;
using System.Text;

namespace mdh
{
    public class Krypto
    {
        /// <summary>
        /// Takes a string input and string key and encrypts the data
        /// </summary>
        /// <returns>
        /// A base64 encrypted string
        /// </returns>
        public string Encrypt(string input, string secret)
        {
            // Initialize RijndaelManaged
            RijndaelManaged rm = new RijndaelManaged();

            // Setup settings
            rm.Mode = CipherMode.CBC;
            rm.Padding = PaddingMode.PKCS7;
            rm.KeySize = 128; // 128-bit keysize
            rm.BlockSize = 128; // 128-bit blocksize
            byte[] pass = Encoding.UTF8.GetBytes(secret); // Grab the pass
            byte[] text = Encoding.UTF8.GetBytes(input); // Grab the input
            int length = pass.Length; // get the size of pass array

            // initialize a byte array
            var bytes = new byte[16];

            // if the length is greater than the array size
            // set the length equal to the array.
            if (length > bytes.Length)
            {
                length = bytes.Length;
            }

            // copy the secret to the byte array
            Array.Copy(pass, bytes, length);
            
            // setup our key and IV
            rm.Key = bytes;
            rm.IV = bytes;

            // Create our encryptor
            ICryptoTransform ict = rm.CreateEncryptor();

            // Use the encryptor to transform and covert it to Base64
            string enctext = Convert.ToBase64String(ict.TransformFinalBlock(text,0,text.Length));

            return enctext; // return the base64 string
        }

        /// <summary>
        /// Takes a string input and string key and decrypts the data
        /// </summary>
        /// <returns>
        /// A decrypted string
        /// </returns>
        public string Decrypt(string input, string secret)
        {
            // Initialize RijndaelManaged
            RijndaelManaged rm = new RijndaelManaged();

            // Setup settings
            rm.Mode = CipherMode.CBC;
            rm.Padding = PaddingMode.PKCS7;
            rm.KeySize = 128; // 128-bit keysize
            rm.BlockSize = 128; // 128-bit blocksize
            byte[] pass = Encoding.UTF8.GetBytes(secret); // Grab the pass
            byte[] text = Convert.FromBase64String(input); // Grab the input from base64
            int length = pass.Length; // get the size of pass array

            // initialize a byte array
            var bytes = new byte[16];

            // if the length is greater than the array size
            // set the length equal to the array.
            if (length > bytes.Length)
            {
                length = bytes.Length;
            }

            // copy the secret to the byte array
            Array.Copy(pass, bytes, length);
            
            // setup our key and IV
            rm.Key = bytes;
            rm.IV = bytes;

            // Create our decryptor
            ICryptoTransform ict = rm.CreateDecryptor();

            // Use the decryptor to transform it to a byte array
            byte[] dectext = ict.TransformFinalBlock(text,0,text.Length);

            // format as a string
            string returnString = Encoding.UTF8.GetString(dectext);

            return returnString; // return the string
        }
    }
}