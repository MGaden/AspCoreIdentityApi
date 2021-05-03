using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CoreIdentity.API.Helpers
{
    public class JsonHelper
    {
        public static JObject ReadJsonFile(string fileName)
        {
            try
            {
                var myJsonString = File.ReadAllText(fileName);
                return JObject.Parse(myJsonString);
            }
            catch (Exception)
            {
                return null;
            }
            
        }
    }
}
