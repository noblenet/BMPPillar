using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;

namespace PseudoKlient
{
    public class HttpServer
    {
        public static string HttpStart(string prefix, string fileName)
        {
            Console.WriteLine("Er i httpstart\n" + prefix + "\n" + fileName);
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return "nej";
            }
            var listener = new HttpListener();
            listener.Prefixes.Add(prefix);
            try
            {
                listener.Start();
            }
            catch (HttpListenerException httpEx)
            {
                Console.WriteLine("Der er allerede en proces der lytter til denne port.\n" + httpEx + "\nTryk på en tast for at fortsætte.");
                Console.ReadLine();
            }

            Console.WriteLine("starter while");

            DateTime timeout = DateTime.Now.AddSeconds(2);
            int dateBreak = 0;
            //while (true)
            do
            {
                Console.WriteLine("er i while");
                HttpListenerContext context = listener.GetContext(); // blokerer mens den venter, det sutter jo helt vildt!!! BeginGetContext er asynkron og skal bruges i stedet. http://msdn.microsoft.com/en-us/library/system.net.httplistener.begingetcontext.aspx
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                response.SendChunked = true;
                string requestType = context.Request.HttpMethod.ToString(CultureInfo.InvariantCulture);
                Console.WriteLine("requestType: " + requestType);

                bool breakWhile = false;
                switch (requestType)
                {
                    case "POST":
                        try
                        {
                            Console.WriteLine("Er i case : POST");
                            response.StatusCode = 200;
                            var writer = new StreamWriter(response.OutputStream);

                            Stream output = File.OpenWrite(@"C:\" + request.QueryString["FileName"]);
                            Stream input = request.InputStream;
                            {
                                input.CopyTo(output);
                            }
                            writer.WriteLine("HTTP/1.1 200 OK");
                            writer.Close();
                            output.Close();
                            input.Close();
                            response.Close();
                            breakWhile = true;
                            break;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("exception: " + e);
                            breakWhile = true;
                            break;
                        }
                    case "GET":
                        try
                        {
                            Console.WriteLine("Er i case: GET");
                            //response.ContentLength64 = File.ReadAllBytes(@"D:\testFiler\" + FileName).Length;
                            response.ContentLength64 = File.ReadAllBytes(@"c:\testFiler\" + fileName).Length;
                            Console.WriteLine("1");
                            //FileStream filestream = new FileStream(@"D:\testFiler\" + FileName, FileMode.Open);
                            var filestream = new FileStream(@"C:\testFiler\" + fileName, FileMode.Open);
                            Console.WriteLine("2");
                            filestream.CopyTo(response.OutputStream);
                            filestream.Dispose();
                            Console.WriteLine("3");
                            Console.WriteLine("Er på vej ud af case: GET");
                            //listener.Stop();
                            breakWhile = true;
                            break;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("exception: " + e);
                            breakWhile = true;
                            break;
                        }
                    default:
                        Console.WriteLine("Er i default)");
                        breakWhile = true;
                        break;
                }
                dateBreak = DateTime.Compare(DateTime.Now, timeout);
                if (breakWhile) break;
            } while (dateBreak <= 0);
            Console.WriteLine("Før sleep");
            Thread.Sleep(1000);
            listener.Stop();
            listener.Close();
            Console.WriteLine("Efter while");
            return "modtaget";
        }
    }
}