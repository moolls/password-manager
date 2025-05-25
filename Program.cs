using Passwordmanager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Passwordmanager
{
    public class Program
    {
        public static void Main(string[] args)
        {

            if (args == null || args.Length == 0)
            {
                //dotnet run userInput
                PrintHelp();
                return;
            }

            userInput(args);

            Kryptering kryptering = new Kryptering();

            kryptering.getiv();
        }


  public static void userInput(string[] userInput)
{
    try
    {
        if (userInput.Length == 0)
        {
            Console.WriteLine("Inga argument angavs. Använd 'help' för en lista med kommandon.");
            return;
        }

        switch (userInput[0].ToLower())
        {
            case "init":
                if (userInput.Length != 3)
                {
                    Console.WriteLine("Fel: 'init' kräver exakt 2 argument: <clientPath> <serverPath>");
                    return;
                }
                Init(userInput[1], userInput[2]);
                break;

            case "create":
                if (userInput.Length != 3)
                {
                    Console.WriteLine("Fel: 'create' kräver exakt 2 argument: <clientPath> <serverPath>");
                    return;
                }
                Create(userInput[1], userInput[2]);
                break;

            case "set":
                if (userInput.Length == 4)
                {
                    Set(userInput[1], userInput[2], userInput[3]);
                }
                else if (userInput.Length == 5)
                {
                    SetG(userInput[1], userInput[2], userInput[3], userInput[4]);
                }
                else
                {
                    Console.WriteLine("Fel: 'set' kräver antingen 3 eller 4 argument:\n- set <clientPath> <serverPath> <property>\n- set <clientPath> <serverPath> <property> -g");
                }
                break;

            case "get":
                if (userInput.Length == 3)
                {
                    Getlist(userInput[1], userInput[2]);
                }
                else if (userInput.Length == 4)
                {
                    Getprop(userInput[1], userInput[2], userInput[3]);
                }
                else
                {
                    Console.WriteLine("Fel: 'get' kräver 2 eller 3 argument:\n- get <clientPath> <serverPath>\n- get <clientPath> <serverPath> <property>");
                }
                break;

            case "delete":
                if (userInput.Length != 4)
                {
                    Console.WriteLine("Fel: 'delete' kräver exakt 3 argument: <clientPath> <serverPath> <property>");
                    return;
                }
                Delete(userInput[1], userInput[2], userInput[3]);
                break;

            case "secret":
                if (userInput.Length != 2)
                {
                    Console.WriteLine("Fel: 'secret' kräver exakt 1 argument: <clientPath>");
                    return;
                }
                string secret = Secret(userInput[1]);
                Console.WriteLine(secret);
                break;

            case "help":
                PrintHelp();
                break;

            default:
                Console.WriteLine("Okänt kommando. Använd 'help' för en lista med kommandon.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Ett fel uppstod: " + ex.Message);
    }
}


private static void PrintHelp()
{
    Console.WriteLine("Tillgängliga kommandon:");
    Console.WriteLine("  init <clientPath> <serverPath>       - Initierar nya klient- och serverfiler");
    Console.WriteLine("  create <clientPath> <serverPath>     - Återställer klientfil från server");
    Console.WriteLine("  set <clientPath> <serverPath> <property> [-g]  - Lägger till lösenord (valfritt slumpgenererat)");
    Console.WriteLine("  get <clientPath> <serverPath>        - Visar alla egenskaper");
    Console.WriteLine("  get <clientPath> <serverPath> <property>  - Hämtar specifik property");
    Console.WriteLine("  delete <clientPath> <serverPath> <property> - Tar bort en property");
    Console.WriteLine("  secret <clientPath>                  - Visar SecretKey för en klientfil");
    Console.WriteLine("  help                                 - Visar denna hjälptext");
}

        static void Init(string Client, string Server)
        {
            Kryptering kryptering = new Kryptering();


            var ivv = kryptering.getiv();


            string client = Client;
            string server = Server;


            byte[] secretkey = GenerateSecretKey();



            var vaultkey = kryptering.CreateVaultKey(Masterpassword(), secretkey);

            CreateClient(client, secretkey);
            CreateServer(server, vaultkey, ivv);

        }


        static void CreateClient(string Client, byte[] secretkey)
        {

            var dictClient = new Dictionary<string, string>();
            string result = Convert.ToBase64String(secretkey);
            dictClient.Add("SecretKey", result);
            string dictionaryClient = JsonSerializer.Serialize(dictClient);
            File.WriteAllText(Client, dictionaryClient);

            if (File.Exists(Client))
            {

            }
            else
            {
                Console.WriteLine("Filen misslyckades att skapas");
            }

        }


        static void CreateServer(string Server, byte[] vaultkey, byte[] iv)
        {

            Kryptering Kryptering = new Kryptering();
            var dictServer = new Dictionary<string, string>();

            dictServer.Add("vault", Kryptering.Encryptvault(vaultkey, iv, dictServer));
            string IVstring = Convert.ToBase64String(iv);
            dictServer.Add("IV", IVstring);
            string dictionaryServer = JsonSerializer.Serialize(dictServer);
            File.WriteAllText(Server, dictionaryServer);

            if (File.Exists(Server))
            {

            }
            else
            {
                Console.WriteLine("Filen misslyckades att skapas");
            }
        }




        static byte[] GenerateSecretKey()
        {
            byte[] key = new byte[16];

            try
            {
                RandomNumberGenerator secretnumber = RandomNumberGenerator.Create();

                secretnumber.GetBytes(key);
                return key;
            }
            catch
            {
                Console.WriteLine("Ett fel uppstod vid genereringen av hemlig nyckel");
                return null;
            }


        }

        static byte[] SecretKey()

        {
            try
            {
                Console.WriteLine("Skriv in din secret key");

                string secretKey = Console.ReadLine();
                byte[] secretKeyByte = Convert.FromBase64String(secretKey);
                return secretKeyByte;

            }


            catch 
            {;
                return null;
            }


        }

        static string Masterpassword()
        {
            try
            {
                Console.WriteLine("Skriv in ditt masterpassword");
                 return ReadPassword();
            }
            catch (Exception ex)
            {
                return "Ett fel uppstod" + ex.Message;
            }


        }

        static void Create(string clientPath, string serverPath)
        {
            Kryptering kryptering = new Kryptering();

            string masterPassword = Masterpassword();
            byte[] secretKeyByte = SecretKey();

            if (File.Exists(serverPath))
            {
                string serverData = File.ReadAllText(serverPath);
                Dictionary<string, string> sameData = JsonSerializer.Deserialize<Dictionary<string, string>>(serverData);

                try
                {
                    string decryptedVault = kryptering.Decryptvault(sameData, masterPassword, secretKeyByte);

                    if (decryptedVault != "Gick ej att decrypta vaulten")
                    {
                        CreateClient(clientPath, secretKeyByte);
                    }
                    else Console.WriteLine(decryptedVault);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ett fel uppstod" + ex.Message);
                }
            }
            else Console.WriteLine("Fil finns inte");

        }

        static void Set(string clientPath, string serverPath, string property)
        {
            Kryptering kryptering = new Kryptering();

            string masterPassword = Masterpassword();

            if (File.Exists(serverPath))
            {

                string clientData = File.ReadAllText(clientPath);
                Dictionary<string, string> clientSameData = JsonSerializer.Deserialize<Dictionary<string, string>>(clientData);
                string secretkey = clientSameData["SecretKey"];
                byte[] secretKeyByte = Convert.FromBase64String(secretkey);

                string serverData = File.ReadAllText(serverPath);
                Dictionary<string, string> sameData = JsonSerializer.Deserialize<Dictionary<string, string>>(serverData);
                string IV = sameData["IV"];
                byte[] IVbyte = Convert.FromBase64String(IV);


                byte[] vaultKey = kryptering.CreateVaultKey(masterPassword, secretKeyByte);

                try
                {
                    Console.WriteLine("Vänligen ange lösenord till property");
                    string propPassword = ReadPassword();

                    if (string.IsNullOrWhiteSpace(propPassword))
                    {
                        Console.WriteLine("Lösenordet får inte vara tomt. Åtgärden avbröts");
                        return;
                    }

                    string decryptedVault = kryptering.Decryptvault(sameData, masterPassword, secretKeyByte);
                    Dictionary<string, string> dezerializedVault = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedVault);
                    dezerializedVault.Add(property, propPassword);


                    string encryptedVault = kryptering.Encryptvault(vaultKey, IVbyte, dezerializedVault);

                    Dictionary<string, string> serverDictionary = new Dictionary<string, string>();
                    serverDictionary.Add("IV", IV);
                    serverDictionary.Add("vault", encryptedVault);

                    string serializedVault = JsonSerializer.Serialize(serverDictionary);

                    File.WriteAllText(serverPath, serializedVault);

                }

                catch (Exception ex)
                {
                    Console.WriteLine("Ett fel uppstod" + ex.Message);
                }
            }

        }


        public static string GenerateRandomAlphabeticText()
        {
            string randomString = "";
            Random random = new Random();
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            for (int i = 0; i < 20; i++)
            {
                randomString += chars[random.Next(chars.Length)];
            }

            return randomString;
        }


        static void SetG(string clientPath, string serverPath, string property, string generate)
        {

            Kryptering kryptering = new Kryptering();

            string masterPassword = Masterpassword();



            if (File.Exists(serverPath))
            {

                string clientData = File.ReadAllText(clientPath);
                Dictionary<string, string> clientSameData = JsonSerializer.Deserialize<Dictionary<string, string>>(clientData);
                string secretkey = clientSameData["SecretKey"];
                byte[] secretKeyByte = Convert.FromBase64String(secretkey);

                string serverData = File.ReadAllText(serverPath);
                Dictionary<string, string> sameData = JsonSerializer.Deserialize<Dictionary<string, string>>(serverData);
                string IV = sameData["IV"];
                byte[] IVbyte = Convert.FromBase64String(IV);


                byte[] vaultKey = kryptering.CreateVaultKey(masterPassword, secretKeyByte);

                try
                {
                    if (generate == "-g" || generate == "--generate")
                    {
                        string propPassword = GenerateRandomAlphabeticText();


                        string decryptedVault = kryptering.Decryptvault(sameData, masterPassword, secretKeyByte);
                        Dictionary<string, string> dezerializedVault = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedVault);
                        dezerializedVault.Add(property, propPassword);


                        string encryptedVault = kryptering.Encryptvault(vaultKey, IVbyte, dezerializedVault);

                        Dictionary<string, string> serverDictionary = new Dictionary<string, string>();
                        serverDictionary.Add("IV", IV);
                        serverDictionary.Add("vault", encryptedVault);



                        string serializedVault = JsonSerializer.Serialize(serverDictionary);

                        File.WriteAllText(serverPath, serializedVault);


                        Console.WriteLine(propPassword);

                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine("Ett fel uppstod" + ex.Message);
                }
            }

        }


        static void Getlist(string clientPath, string serverPath)
        {
            Kryptering kryptering = new Kryptering();

            string masterPassword = Masterpassword();


            if (File.Exists(serverPath))
            {
                string clientData = File.ReadAllText(clientPath);
                Dictionary<string, string> clientSameData = JsonSerializer.Deserialize<Dictionary<string, string>>(clientData);
                string secretkey = clientSameData["SecretKey"];
                byte[] secretKeyByte = Convert.FromBase64String(secretkey);

                string serverData = File.ReadAllText(serverPath);
                Dictionary<string, string> sameData = JsonSerializer.Deserialize<Dictionary<string, string>>(serverData);

                try
                {
                    string decryptedVault = kryptering.Decryptvault(sameData, masterPassword, secretKeyByte);
                    Dictionary<string, string> dictionaryVault = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedVault);

                    foreach (var key in dictionaryVault.Keys)
                    {
                        Console.WriteLine(key);
                    }

                }

                catch (Exception ex)
                {
                    Console.WriteLine("Ett fel uppstod" + ex.Message);
                }
            }
            else Console.WriteLine("Fil finns inte");

        }


        static void Getprop(string clientPath, string serverPath, string getProp)
        {
            Kryptering kryptering = new Kryptering();

            string masterPassword = Masterpassword();


            if (File.Exists(serverPath))
            {
                string clientData = File.ReadAllText(clientPath);
                Dictionary<string, string> clientSameData = JsonSerializer.Deserialize<Dictionary<string, string>>(clientData);
                string secretkey = clientSameData["SecretKey"];
                byte[] secretKeyByte = Convert.FromBase64String(secretkey);

                string serverData = File.ReadAllText(serverPath);
                Dictionary<string, string> sameData = JsonSerializer.Deserialize<Dictionary<string, string>>(serverData);



                try
                {
                    string decryptedVault = kryptering.Decryptvault(sameData, masterPassword, secretKeyByte);
                    Dictionary<string, string> dictionaryVault = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedVault);

                    if (dictionaryVault.ContainsKey(getProp))
                    {
                        string value = dictionaryVault[getProp];
                        Console.WriteLine(value);
                    }
                }

                catch
                {
                    Console.WriteLine("Gick inte att inhämta property");
                }


            }

        }

        static void Delete(string clientPath, string serverPath, string prop)
        {
            Kryptering kryptering = new Kryptering();

            string masterPassword = Masterpassword();
            //byte[] secretKeyByte = SecretKey(clientPath);

            if (File.Exists(serverPath))
            {
                string clientData = File.ReadAllText(clientPath);
                Dictionary<string, string> clientSameData = JsonSerializer.Deserialize<Dictionary<string, string>>(clientData);
                string secretkey = clientSameData["SecretKey"];
                byte[] secretKeyByte = Convert.FromBase64String(secretkey);

                string serverData = File.ReadAllText(serverPath);
                Dictionary<string, string> sameData = JsonSerializer.Deserialize<Dictionary<string, string>>(serverData);
                string IV = sameData["IV"];
                byte[] IVbyte = Convert.FromBase64String(IV);


                byte[] vaultKey = kryptering.CreateVaultKey(masterPassword, secretKeyByte);

                try
                {
                    string decryptedVault = kryptering.Decryptvault(sameData, masterPassword, secretKeyByte);
                    Dictionary<string, string> dictionaryVault = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedVault);
                    Dictionary<string, string> serverDictionary = new Dictionary<string, string>();


                    if (dictionaryVault.ContainsKey(prop))
                    {
                        dictionaryVault.Remove(prop);
                        serverDictionary.Add("IV", IV);
                        serverDictionary.Add("vault", kryptering.Encryptvault(vaultKey, IVbyte, dictionaryVault));

                    }


                    string serializedVault = JsonSerializer.Serialize(serverDictionary);
                    File.WriteAllText(serverPath, serializedVault);
                }


                catch (Exception ex)
                {
                    Console.WriteLine("Ett fel uppstod" + ex.Message);
                }
            }

        }


        static string Secret(string clientPath)
        {
            if (File.Exists(clientPath))
            {
                string clientData = File.ReadAllText(clientPath);
                Dictionary<string, string> sameData = JsonSerializer.Deserialize<Dictionary<string, string>>(clientData);

                string secretkey = sameData["SecretKey"];

                return secretkey;
            }

            return "Gick inte att hämta secret-key";
        }

    static string ReadPassword()
{
    StringBuilder password = new StringBuilder();
    ConsoleKeyInfo key;

    do
    {
        key = Console.ReadKey(true);
        if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
        {
            password.Append(key.KeyChar);
            Console.Write("*");
        }
        else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
        {
            password.Length--;
            Console.Write("\b \b");
        }
    } while (key.Key != ConsoleKey.Enter);

    Console.WriteLine();
    return password.ToString();
}

    }
}



