using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Security.Cryptography;
namespace JWTConsoleApp
{
    class URICanonicalizer
    {
        protected   String Method;
                    Uri Url;
                    Uri BaseUrl;

                    String CanonicalVerb;
                    String CanonicalUrl;
                    String CanonicalQuery;


        public URICanonicalizer(String Method, String BaseUrl, String AbsoluteUrl)
        {
            this.Method = Method;
            this.Url = new Uri(AbsoluteUrl);
            this.BaseUrl = new Uri(BaseUrl);

            CanonicalizeVerb();
            CanonicalizeUrl();
            CanonicalizeQuery();
        }

        public String MakeCanonicalUrl()
        {
            String sCanonical = CanonicalVerb;
            if (CanonicalUrl != "")
                sCanonical += "&" + CanonicalUrl;
            if (CanonicalQuery != "")
                sCanonical += "&" + CanonicalQuery;
            return sCanonical;
        }
        public String QueryStringHash()
        {
            var crypt = new SHA256Managed();
            var hash = new System.Text.StringBuilder();
            byte[] cryptedBytes =  crypt.ComputeHash(Encoding.UTF8.GetBytes(MakeCanonicalUrl()));
            foreach (byte theByte in cryptedBytes)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }

        protected void CanonicalizeVerb()
        {
            CanonicalVerb = Method.ToUpper();
        }
        protected void  CanonicalizeUrl()
        {
            String res = "";
            // trim base url
            String absPath = Url.AbsolutePath;
            if (absPath.StartsWith(BaseUrl.AbsolutePath))
                res = absPath.TrimStart(BaseUrl.AbsolutePath.ToCharArray());
            //starts with /
            if (!res.StartsWith("/"))
                res = "/" + res;
            if (res.Length >1 )
            {
                res.TrimEnd('/');
            }
            //no &  letter
            //res = res.Replace("&", "%26");
            res = HttpUtility.UrlPathEncode(res);
            res = res.Replace("&", "%26");
            // throw new Exception("Not implemented");
            this.CanonicalUrl = res;
        }

        protected void CanonicalizeQuery()
        {
            String queryString = Url.Query.TrimStart('?');
            if (queryString == "")
            {
                //this.CanonicalQuery = "";
                return;
            }
                
            List<String> lParams = queryString.Split('&').ToList();
            Dictionary<String, String[]> dic = new Dictionary<String, String[]>(lParams.Count);
            foreach(String sParam in lParams)
            {
                String[] paramArr = sParam.Split('=');
                String paramName = paramArr[0];
                String paramValue = paramArr[1];

                String[] valArray;
                if (!dic.Keys.Contains(paramName))
                {
                    valArray = new String[1];
                    valArray[0] = paramValue;
                    dic.Add(paramName, valArray);
                }
                else
                {
                    String[] ExistingValues;
                    dic.TryGetValue(paramName, out ExistingValues);

                    
                    var newValArray = new String[ExistingValues.Length + 1];
                    ExistingValues.CopyTo(newValArray, 0);
                    newValArray[newValArray.Length - 1] = paramValue;
                    dic[paramName] = newValArray.OrderBy(key => key).ToArray();
                }
            }
            SortedDictionary<String, String[]> sDic = new SortedDictionary<String,String[]>(dic);
            //ignore jwt
            sDic.Remove("jwt");
            //urlencode keys
            //urlencode values
            //sort keys
            //group same params by ,
            List<String> lOutQuery = new List<String>();
            foreach(KeyValuePair<String, String[]> par in sDic)
            {
                String sOutQuery = "";
                sOutQuery += HttpUtility.UrlPathEncode(par.Key);
                sOutQuery += "=";
                sOutQuery += HttpUtility.UrlPathEncode(String.Join(",", par.Value));
                lOutQuery.Add(sOutQuery);
            }
            this.CanonicalQuery =  String.Join<String>("&", lOutQuery);
        }
    }
}
