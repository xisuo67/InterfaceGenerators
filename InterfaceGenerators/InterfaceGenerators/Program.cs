using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace InterfaceGenerators
{
    class Program
    {
        private static List<string> CKeys = new List<string>();
        static void Main(string[] args)
        {
            Console.WriteLine("请输入swagger接口地址(http://192.168.0.133:6001/swagger/v1/swagger.json)：");
            string url = Console.ReadLine().ToLower();
            while (true)
            {
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    Console.WriteLine("请输入正确的接口地址(http://192.168.0.133:6001/swagger/v1/swagger.json)：");
                    url = Console.ReadLine().ToLower();
                }
                else
                {
                    break;
                }
            }
            Console.WriteLine("开始从IMS中获取接口描述");
            string desc = GetInterfaceDesc(url).Replace("$ref", "refDef");
            Console.WriteLine("获取接口描述已完成，开始生成接口文档");
            CreateApiDoc(desc);

            Console.WriteLine("接口文档已经完成到【" + AppDomain.CurrentDomain.BaseDirectory + "API说明手册.docx" + "】路径下");
            Console.Read();
        }
        public static void CreateApiDoc(string apiDesc)
        {
            SwaggerDocument data = JsonConvert.DeserializeObject<SwaggerDocument>(apiDesc);
            if (data!=null)
            {
                Dictionary<string, dynamic> dict = new Dictionary<string, dynamic>();
                foreach (var key in data.definitions.Keys)
                {
                    InitParamsDefinedCache(data.definitions[key].properties, "#/definitions/" + key, dict, data.definitions);
                }
                var dictController = data.ControllerDesc == null ? new Dictionary<string, string>() : data.ControllerDesc;

                string curHeader = string.Empty, preHeader = string.Empty;
            }
        }
        private static Dictionary<string, dynamic> InitParamsDefinedCache(IDictionary<string, Schema> item, string defkey, Dictionary<string, dynamic> dictResult, IDictionary<string, Schema> allitem)
        {
            if (dictResult == null)
            {
                dictResult = new Dictionary<string, dynamic>();
            }
            else if (dictResult.ContainsKey(defkey))
            {
                return dictResult[defkey];
            }

            if (CKeys.Contains(defkey))
            {
                return null;
            }
            CKeys.Add(defkey);
            if (defkey == "#/definitions/Citms.Utility.ApiResult[System.Collections.Generic.Dictionary[System.String,Citms.PIS.Model.Capture.CaptureTemplateDetail]]")
            {

            }
            Dictionary<string, dynamic> dictItem = new Dictionary<string, dynamic>();
            if (item != null && item.Keys.Count > 0)
            {
                foreach (var key in item.Keys)
                {
                    var prop = item[key];
                    if (prop.refDef == "#/definitions/System.Object")
                    {
                        dictItem[key] = new object();
                    }
                    else if (prop.type == "object")
                    {
                        if (prop.items == null)
                        {
                            continue;
                        }
                        dynamic r = null;
                        if (dictResult.TryGetValue(prop.items.refDef, out r))
                        {
                            dictItem[key] = r;
                        }
                        else
                        {
                            dictItem[key] = InitParamsDefinedCache(allitem[prop.items.refDef.Substring("#/definitions/".Length)].properties, prop.items.refDef, dictResult, allitem);
                        }
                    }
                    else if (prop.type == "array")
                    {
                        var list = new List<dynamic>();
                        if (prop.items == null)
                        {
                            continue;
                        }
                        if (!string.IsNullOrEmpty(prop.items.type))
                        {
                            if (prop.items.type == "string")
                            {
                                if (prop.items.format == "date-time")
                                {
                                    list.Add(DateTime.Now);
                                }
                                else
                                {
                                    list.Add("string");
                                }
                            }
                            else if (prop.items.type == "boolean")
                            {
                                list.Add(true);
                            }
                            else if (prop.items.type == "integer")
                            {
                                list.Add(0);
                            }
                            else if (prop.items.type == "number")
                            {
                                list.Add(0.0);
                            }
                        }
                        else
                        {
                            dynamic r = null;
                            if (dictResult.TryGetValue(prop.items.refDef, out r))
                            {
                                dictItem[key] = r;
                            }
                            else
                            {
                                list.Add(InitParamsDefinedCache(allitem[prop.items.refDef.Substring("#/definitions/".Length)].properties, prop.items.refDef, dictResult, allitem));
                            }

                        }
                        dictItem[key] = list;
                    }
                    else if (prop.type == "string")
                    {
                        if (prop.format == "date-time")
                        {
                            dictItem[key] = DateTime.Now;
                        }
                        else
                        {
                            dictItem[key] = "string";
                        }
                    }
                    else if (prop.type == "boolean")
                    {
                        dictItem[key] = true;
                    }
                    else if (prop.type == "integer")
                    {
                        dictItem[key] = 0;
                    }
                    else if (prop.type == "number")
                    {
                        dictItem[key] = 0.0;
                    }
                    else
                    {
                        //Console.WriteLine(prop.type);
                    }
                }
            }
            dictResult[defkey] = dictItem;
            return dictItem;
        }
        /// <summary>
        /// 获取接口描述
        /// </summary>
        /// <param name="url">站点地址</param>
        /// <returns>SwaggerDocument描述</returns>
        public static string GetInterfaceDesc(string url)
        {
            HttpWebRequest req = null;
            HttpWebResponse res = null;
            try
            {
                req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "GET";
                req.Timeout = 200000;
                res = (HttpWebResponse)req.GetResponse();
                using (StreamReader sr = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                {
                    return sr.ReadToEnd();
                }
            }
            catch (WebException exception)
            {
                Console.WriteLine("获取IMS接口描述异常:" + exception.ToString());
                throw exception;
            }
            finally
            {
                if (res != null)
                {
                    res.Close();
                    res = null;
                }
                if (req != null)
                {
                    req.Abort();
                    req = null;
                }
            }
        }
    }
}
