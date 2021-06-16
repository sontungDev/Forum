using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LIB
{
    public class Assitant
    {
        private static string key = "AuAgCuFe";
        private static Assitant instance;
        private Assitant() {  }
        
        public static Assitant Instance
        {
            get
            {
                if (instance == null)
                    instance = new Assitant();
                return instance;
            }
        }
        // mã hóa chuỗi
        public string EncodeF64(string text)
        {
            if (String.IsNullOrEmpty(text)) return "";
            text += key;
            var text_bytes = Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(text_bytes);
        }
        // giải mã chuỗi
        public string DecodeF64(string text)
        {
            if (String.IsNullOrEmpty(text)) return "";
            var base64 = Convert.FromBase64String(text);
            var result = Encoding.UTF8.GetString(base64);
            result = result.Substring(0, result.Length - key.Length);
            return result;
        }

        // xin id tự động
        public string GetAutoID(string current_id, string prefix)
        {
            string day = DateTime.Now.Day.ToString("00");
            string month = DateTime.Now.Month.ToString("00");
            string year = DateTime.Now.Year.ToString().Substring(2, 2);

            string date = day + month + year;
            string num = "0001";

            if (!String.IsNullOrEmpty(current_id))
            {
                string date_now = current_id.Substring(3, 6);

                if (date.Equals(date_now))
                {
                    num = current_id.Substring(9);
                    int new_num = int.Parse(num);
                    new_num = new_num + 1;
                    num = new_num.ToString("0000");
                }
            }
            return prefix + date + num;
        }
    }
}
