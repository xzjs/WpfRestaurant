using System;
using System.Net;
using System.Windows;

namespace WpfRestaurant
{
    public class MyApp
    {
        public static long TableId = 0;
        public static int TableType = 0;
        public static string Http = null;
        public static Config Config;
        public static Infomation Infomation;

        /// <summary>
        /// 下载图片
        /// </summary>
        /// <param name="downloadPath">下载路径</param>
        /// <param name="name">文件名</param>
        /// <returns></returns>
        public static string Download_Img(string downloadPath, string name)
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(downloadPath + name, name);
                    return name;
                }
            }
            catch (Exception exception)
            {
                //MessageBox.Show("下载" + name + "出错");
                Console.WriteLine(exception.Message);
                return "menu.png";
            }
        }
    }
}