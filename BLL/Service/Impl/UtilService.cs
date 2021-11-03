using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BLL.Service.Impl
{
    public class UtilService : IUtilService
    {
        public string changeToInternationalPhoneNumber(string phone)
        {
            return phone.StartsWith("0") ? "+84" + phone.Substring(1) : phone;
        }

        public string changeToVietnamPhoneNumber(string phone)
        {
            return phone.StartsWith("+84") ? "0" + phone.Substring(3) : phone;
        }

        public string GenerateOTP()
        {
            string[] numericMap = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };

            string otp = String.Empty;

            Random rand = new Random();

            IEnumerable<int> otpInt = Enumerable.Range(1, 6).Select(x =>
                rand.Next(0, numericMap.Length));

            foreach (int i in otpInt)
            {
                otp += i.ToString();
            }
            return otp;
        }

        public bool IsNullOrEmpty<T>(IEnumerable<T> list)
        {
            return !(list?.Any()).GetValueOrDefault();
        }

        public string Create16DigitString()
        {
            Random RNG = new Random();

            var builder = new StringBuilder();

            while (builder.Length < 16)
            {
                builder.Append(RNG.Next(10).ToString());
            }
            return builder.ToString();
        }

        public string Create16Alphanumeric()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[16];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new String(stringChars);
        }

    }
}
