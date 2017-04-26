using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WpfRestaurant
{
    public class TableItem
    {
        public long Id { get; set; }
        public Table Table { get; set; }

        public string Time { get; set; }

        public string No { get; set; }

        public decimal Cost { get; set; }

        public int Counts { get; set; }

        public Order Order { get; set; }

        /// <summary>
        ///     设置桌位状态
        /// </summary>
        /// <param name="status">0：空闲；1、预订；2；已下单</param>
        /// <param name="tableId">桌子id</param>
        public static void SetTableStatus(int status, long tableId)
        {
            try
            {
                using (var db = new restaurantEntities())
                {
                    var t = db.Table.Find(tableId);
                    t.Status = status;
                    db.SaveChanges();
                    using (var client = new WebClient())
                    {
                        var values = new NameValueCollection
                        {
                            ["deskId"] = t.DeskID.ToString(),
                            ["status"] = status.ToString()
                        };
                        try
                        {
                            if (MyApp.Http == null)
                                throw new Exception("未设置HTTP");
                            var response = client.UploadValues("http://" + MyApp.Http + "/restClient/setDeskStatus.nd",
                                values);

                            var responseString = Encoding.Default.GetString(response);
                            var jo = JObject.Parse(responseString);
                            if ((string) jo["errorFlag"] != "false")
                                throw new Exception("设置服务器桌位失败");
                        }
                        catch (WebException webException)
                        {
                            var parameter = JsonConvert.SerializeObject(new Dictionary<string, string>
                            {
                                ["deskId"] = t.DeskID.ToString(),
                                ["status"] = status.ToString()
                            }, Formatting.Indented);
                            var queue = new Queue
                            {
                                Url = "http://" + MyApp.Http + "/restClient/setDeskStatus.nd",
                                Type = "POST",
                                Time = DateTime.Now,
                                Parameter = parameter
                            };
                            db.Queue.Add(queue);
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }
}