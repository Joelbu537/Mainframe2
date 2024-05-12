using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Mainframe2
{
    internal class Program
    {
        static string version = "2.0.1";
        static int port = 33533;
        static string hash = "00";
        public static byte[] responseData;
        static void Main(string[] args)
        {
            Console.Title = "Mainframe " + version;
            Console.Write("Enter target ip: ");
            string ip = Console.ReadLine();
            Console.Clear();
            try
            {
                TcpClient client = new TcpClient();
                client.Connect(ip, port);
                NetworkStream stream = client.GetStream();
                while (true)
                {
                    Console.Clear();
                    Console.Write("Enter command: ");
                    string command = Console.ReadLine();
                    string[] commands = command.Split(' ');
                    SHA256 sha_hash = SHA256.Create();
                    byte[] hashBytes;
                    string response = "";
                    byte[] responseData;
                    byte[] buffer = new byte[1024];
                    int bytesRead = 0;
                    switch (commands[0])
                    {
                        case "help":
                            Console.WriteLine("");
                            Console.ReadKey();
                            break;
                        case "register":
                            string username = "";
                            do
                            {
                                Console.Write("Choose a username: ");
                                username = Console.ReadLine();
                            } while (username.Length < 8);
                            Console.WriteLine();
                            string enterText = "Choose a password: ";
                            string password = "";
                            do
                            {
                                password = CheckPassword(enterText);
                            } while (password.Length < 8);
                            enterText = "Re-enter password: ";
                            string confirm_password = "";
                            {
                                confirm_password = CheckPassword(enterText);
                            } while (confirm_password.Length < 8) ;
                            if (password == confirm_password)
                            {
                                hashBytes = sha_hash.ComputeHash(Encoding.UTF8.GetBytes(password + username));
                                hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                                response = $"{version}\r\nregister\r\n{hash}\r\n{username}\r\n{hash}";
                                responseData = Encoding.Unicode.GetBytes(response);
                                stream.Write(responseData, 0, responseData.Length);
                                bytesRead = 0;
                                buffer = new byte[1024];
                                Thread.Sleep(500);
                                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    string dataReceived = Encoding.Unicode.GetString(buffer, 0, bytesRead);
                                    string[] content = dataReceived.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                                    if (content[1] == "outdated")
                                    {
                                        Console.WriteLine("This clients version is outdated!");
                                        Console.WriteLine("Please upgrade to " + content[0]);
                                    }
                                    else if (content[1] == "register")
                                    {
                                        switch (content[2])
                                        {
                                            case "0":
                                                Console.WriteLine("User already exists!");
                                                Console.WriteLine("Please choose another username!");
                                                Console.ReadKey();
                                                break;
                                            case "1":
                                                Console.WriteLine("Sucessfully registered!");
                                                Console.ReadKey();
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        stream.Close();
                                        client.Close();
                                        Console.WriteLine("Connection error: Connection terminated by client due to corrupted message!");
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("Passwords do not match!");
                                Console.ReadKey();
                            }
                            break;
                        case "verify":
                            username = "";
                            do
                            {
                                Console.Write("Enter your username: ");
                                username = Console.ReadLine().ToLower();
                            } while (username == "");
                            enterText = "Enter your password: ";
                            password = "";
                            do
                            {
                                password = CheckPassword(enterText);
                            } while (password.Length < 8);

                            hashBytes = sha_hash.ComputeHash(Encoding.UTF8.GetBytes(password + username));
                            hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                            response = $"{version}\r\nverify\r\n{hash}\r\n{username}\r\n{hash}";
                            responseData = Encoding.Unicode.GetBytes(response);
                            stream.Write(responseData, 0, responseData.Length);
                            bytesRead = 0;
                            buffer = new byte[1024];
                            Thread.Sleep(500);
                            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                string dataReceived = Encoding.Unicode.GetString(buffer, 0, bytesRead);
                                string[] content = dataReceived.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                                if (content[1] == "outdated")
                                {
                                    Console.WriteLine("This clients version is outdated!");
                                    Console.WriteLine("Please upgrade to " + content[0]);
                                }
                                else if (content[1] == "verify")
                                {
                                    switch (content[2])
                                    {
                                        case "0":
                                            Console.WriteLine("User not found!");
                                            Console.WriteLine("Please register with the register command or see help for more information!");
                                            Console.ReadKey();
                                            break;
                                        case "1":
                                            Console.WriteLine("Password incorrect!");
                                            Console.WriteLine("You received a 20 seconds cooldown");
                                            hash = "00";
                                            Thread.Sleep(20000);
                                            Console.ReadKey();
                                            break;
                                        case "2":
                                            Console.WriteLine("This session is now verified!");
                                            Console.ReadKey();
                                            break;
                                    }
                                    break;
                                }
                                else
                                {
                                    stream.Close();
                                    client.Close();
                                    Console.WriteLine("Connection error: Connection terminated by client due to corrupted message!");
                                    break;
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:");
                Console.WriteLine(ex.Message);
            }
            Console.ReadKey();
        }
        static string CheckPassword(string EnterText)
        {
            string EnteredVal = "";
            try
            {
                Console.Write(EnterText);
                EnteredVal = "";
                do
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                    {
                        EnteredVal += key.KeyChar;
                        Console.Write("*");
                    }
                    else
                    {
                        if (key.Key == ConsoleKey.Backspace && EnteredVal.Length > 0)
                        {
                            EnteredVal = EnteredVal.Substring(0, (EnteredVal.Length - 1));
                            Console.Write("\b \b");
                        }
                        else if (key.Key == ConsoleKey.Enter)
                        {
                            if (string.IsNullOrWhiteSpace(EnteredVal))
                            {
                                Console.WriteLine("");
                                Console.WriteLine("Empty value not allowed.");
                                CheckPassword(EnterText);
                                break;
                            }
                            else
                            {
                                Console.WriteLine("");
                                break;
                            }
                        }
                    }
                } while (true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return EnteredVal;
        }
    }
}
