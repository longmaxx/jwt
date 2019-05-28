using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using JWTConsoleApp.CommandLine.Utility;
using System.Collections;
using System.IO;

namespace JWTConsoleApp
{
    class Program
    {
        String secret;

        static void Main(string[] args)
        {
            Arguments CommandLine = new Arguments(args);
            String secret = "";
            String HttpMethod = "";
            String BaseUrl = "";
            String Url = "";
            bool HasQshParam = false;
            bool HasUrlParams = false;

            Dictionary<string, object> payload = new Dictionary<string, object>();


            Uri u = new Uri("https://yandex.ru/abc/1/users?id=12&p=dedpihto");
            string u2 = u.ToString();

            if (CommandLine.Parameters.Count == 0)
            {
                Console.WriteLine("Command line:");
                Console.WriteLine("/secret:\"\" /live:  [/verb:\"\" /url:\"\" /baseurl:\"\"] [<other payload parameters>]");
                Console.WriteLine("");
                Console.WriteLine("Parameters are added to payload as is. But the tool has some supported parameters:");
                Console.WriteLine("/secret:   - your secret key. This is required parameter.");
                Console.WriteLine("/accesskey:   - your access key. Added to payload as is.");
                Console.WriteLine("/live:   - token live time in milliseconds. Generates claims 'iat, exp' in payload.");
                Console.WriteLine("/verb:   - Http method. Needed to generate QSH payload parameter. GET, POST, etc.");
                Console.WriteLine("/baseurl:   - Base API url. Needed to generate QSH payload parameter.");
                Console.WriteLine("/url:   - Full html query string. Needed to generate QSH payload parameter.");
                Console.WriteLine("");
                Console.WriteLine("example: /secretkey:\"flkjds02340ewjl\" /live:1000 /sub:\"vasuliy@mail.com\" /iss:\"KHDFSF32039kdfjlslk3l21jlk\"");
                return;
            }

            foreach (DictionaryEntry de in CommandLine.Parameters)
            {
                String curKey = de.Key.ToString();
                String curValue = de.Value.ToString();
                if (curKey == "secret")
                {
                    secret = curValue;
                }
                else if (curKey == "accesskey")
                {
                    payload.Add("iss", curValue);
                }
                else if (curKey == "live")
                {
                    DateTimeOffset t_now = new DateTimeOffset(DateTime.Now);
                    long nowMSeconds = t_now.ToUnixTimeMilliseconds();
                    payload.Add("iat", nowMSeconds);
                    payload.Add("exp", nowMSeconds +  Convert.ToInt64(curValue));
                }
                else if (curKey == "verb")
                {
                    HttpMethod = curValue;
                    HasUrlParams = true;
                }
                else if (curKey == "url")
                {
                    Url = curValue;
                    HasUrlParams = true;
                }
                else if (curKey == "baseurl")
                {
                    BaseUrl = curValue;
                    HasUrlParams = true;
                }
                else if (curKey == "qsh")
                {
                    HasQshParam = true;
                    payload.Add(curKey, curValue);
                }
                else
                {
                    payload.Add(curKey, curValue);
                }
            }
            if (HasUrlParams && HasQshParam)
            {
                TextWriter ErrWriter = Console.Error;
                ErrWriter.WriteLine("Both '/qsh:' and url parameters (/verb:, /url:, /baseurl:) are detected. Please use only one way to set QSH payload parameter.");
                Environment.Exit(2);
            }
            // Calculate QSH here
            if (HttpMethod!="" && Url!="" && BaseUrl != "")
            {
                URICanonicalizer uc = new URICanonicalizer(HttpMethod, BaseUrl, Url);
                payload["qsh"] = uc.QueryStringHash();
            }
            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);
            var token = encoder.Encode(payload, secret);
            Console.WriteLine(token);
            //=====================================
        }
    }
}

