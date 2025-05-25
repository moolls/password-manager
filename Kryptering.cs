using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Passwordmanager
{
    public class Kryptering
    {

        byte[] key = new byte[16];


        public byte[] getiv()
        {
            byte[] iv = new byte[16];

            RandomNumberGenerator rng = RandomNumberGenerator.Create();

            rng.GetBytes(iv);

            return iv;
        }


        public byte[] CreateVaultKey(string masterpassword, byte[] secretkey)
        {
            Rfc2898DeriveBytes vaultkey = new Rfc2898DeriveBytes(masterpassword, secretkey);

            return vaultkey.GetBytes(16);
        }

        static byte[] EncryptStringToBytes_Aes(string plainText, byte[] vaultkey, byte[] IV)
        {

            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (vaultkey == null || vaultkey.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = vaultkey;
                aesAlg.IV = IV;
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return encrypted;
        }



        public string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] vaultkey, byte[] IV)
        {

            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (vaultkey == null || vaultkey.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            string plaintext = null;


            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = vaultkey;
                aesAlg.IV = IV;
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }


                return plaintext;
            }


        }


        public string Encryptvault(byte[] vaultkey, byte[] iv, Dictionary<string, string> vaultContent)
        {


            string vaultContentToJson = JsonSerializer.Serialize(vaultContent);
            byte[] encryptedContent = EncryptStringToBytes_Aes(vaultContentToJson, vaultkey, iv);
            string resultString = Convert.ToBase64String(encryptedContent);

            return resultString;

        }

        public string Decryptvault(Dictionary<string, string> sameData, string masterPassword, byte[] secretKey)
        {
            try
            {
                string dictionaryVault = sameData["vault"];
                byte[] vault = Convert.FromBase64String(dictionaryVault);
                byte[] vaultKey = CreateVaultKey(masterPassword, secretKey);
                string dictionaryIV = sameData["IV"];
                byte[] IV = Convert.FromBase64String(dictionaryIV);


                string decryptedContent = DecryptStringFromBytes_Aes(vault, vaultKey, IV);
                return decryptedContent;

            }

            catch
            {
                return "Gick ej att decrypta vaulten";
            }
        }

        //public string Decryptvault(Dictionary<string, string> sameData, string masterPassword, byte[] secretKey)
        //{
        //    try
        //    {
        //        string dictionaryVault = sameData["vault"];
        //        Console.WriteLine("Encrypted Vault: " + dictionaryVault); // Lägg till för att logga värdet

        //        byte[] vault = Convert.FromBase64String(dictionaryVault);
        //        byte[] vaultKey = CreateVaultKey(masterPassword, secretKey);
        //        string dictionaryIV = sameData["IV"];
        //        Console.WriteLine("IV: " + dictionaryIV); // Lägg till för att logga värdet

        //        byte[] IV = Convert.FromBase64String(dictionaryIV);

        //        string decryptedContent = DecryptStringFromBytes_Aes(vault, vaultKey, IV);
        //        Console.WriteLine("Decrypted Content: " + decryptedContent); // Lägg till för att logga värdet

        //        return decryptedContent;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Error during decryption: " + ex.Message); // Lägg till för att logga felmeddelandet
        //        return "Gick ej att decrypta vaulten";
        //    }
        //}




    }
}