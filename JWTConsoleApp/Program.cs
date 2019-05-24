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

namespace JWTConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Arguments CommandLine = new Arguments(args);
            string secret = "";
            Dictionary<string, object> payload = new Dictionary<string, object>();


            Uri u = new Uri("https://yandex.ru/abc/1/users?id=12&p=dedpihto");
            string u2 = u.ToString();

            if (CommandLine.Parameters.Count == 0)
            {
                Console.WriteLine("Command line:");
                Console.WriteLine("/secretkey:\"\" /live:  <other payload parameters>");
                Console.WriteLine("");
                Console.WriteLine("Parameters are added to payload as is. But the tool has some supported parameters:");
                Console.WriteLine("/secretkey:   - your secret key. This is required parameter.");
                Console.WriteLine("/accesskey:   - your access key. Added to payload as is.");
                Console.WriteLine("/live:   - token live time in seconds. Generates claims 'iat, exp' in payload.");
                Console.WriteLine("");
                Console.WriteLine("example: /secretkey:\"flkjds02340ewjl\" /live:1000 /sub:\"vasuliy@mail.com\" /iss:\"KHDFSF32039kdfjlslk3l21jlk\"");
                return;
            }

            foreach (DictionaryEntry de in CommandLine.Parameters)
            {
                if (de.Key.ToString() == "secretkey")
                {
                    secret = de.Value.ToString();
                }
                else if (de.Key.ToString() == "accesskey")
                {
                    payload.Add("iss", de.Value.ToString());
                }
                else if (de.Key.ToString() == "live")
                {
                    DateTimeOffset t_now = new DateTimeOffset(DateTime.Now);
                    long nowSeconds = t_now.ToUnixTimeSeconds();
                    payload.Add("iat", nowSeconds);
                    payload.Add("exp", nowSeconds +  Convert.ToInt64(de.Value));
                }
                else
                {
                    payload.Add(de.Key.ToString(),de.Value);
                }
            }

            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            var token = encoder.Encode(payload, secret);
            Console.WriteLine(token);
        }
    }
}

