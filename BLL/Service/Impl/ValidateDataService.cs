using BLL.Constant;
using BLL.Dto;
using System;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace BLL.Service.Impl
{
    public class ValidateDataService : IValidateDataService
    {
        public bool IsValidAddress(string address)
        {
            if (!String.IsNullOrEmpty(address))
            {
                Regex regex = new Regex(@"^[a-zA-Z0-9ÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚĂĐĨŨƠàáâãèéêìíòóôõùúăđĩũơƯĂẠẢẤẦẨẪẬẮẰẲẴẶẸẺẼỀỀỂẾưăạảấầẩẫậắằẳẵặẹẻẽềềểếỄỆỈỊỌỎỐỒỔỖỘỚỜỞỠỢỤỦỨỪễệỉịọỏốồổỗộớờởỡợụủứừỬỮỰỲỴÝỶỸửữựỳỵỷỹ\s\W|_]+$");
                return regex.IsMatch(address);
            }
            return false;
        }

        public bool IsValidForetime(DateTime? date)
        {
            if(date.HasValue)
            {
                if (date >= DateTime.Now)
                    return false;
            }
            return true;
        }

        public bool IsValidEmail(string email)
        {
            if (!String.IsNullOrEmpty(email))
            {
                try
                {
                    MailAddress m = new MailAddress(email);
                    return true;
                }
                catch (FormatException)
                {
                    return false;
                }
            }
            return false;
        }

        public bool IsValidName(string name, string obj)
        {
            if (!String.IsNullOrEmpty(name))
            {
                if(obj != null)
                {
                    Regex regex = new Regex(@"^[a-zA-ZÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚĂĐĨŨƠàáâãèéêìíòóôõùúăđĩũơƯĂẠẢẤẦẨẪẬẮẰẲẴẶẸẺẼỀỀỂẾưăạảấầẩẫậắằẳẵặẹẻẽềềểếỄỆỈỊỌỎỐỒỔỖỘỚỜỞỠỢỤỦỨỪễệỉịọỏốồổỗộớờởỡợụủứừỬỮỰỲỴÝỶỸửữựỳỵỷỹ\s\W|_]+$");
                    return regex.IsMatch(name);
                }
                else
                {
                    Regex regex = new Regex(@"^[a-zA-Z0-9ÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚĂĐĨŨƠàáâãèéêìíòóôõùúăđĩũơƯĂẠẢẤẦẨẪẬẮẰẲẴẶẸẺẼỀỀỂẾưăạảấầẩẫậắằẳẵặẹẻẽềềểếỄỆỈỊỌỎỐỒỔỖỘỚỜỞỠỢỤỦỨỪễệỉịọỏốồổỗộớờởỡợụủứừỬỮỰỲỴÝỶỸửữựỳỵỷỹ\s\W|_]+$");
                    return regex.IsMatch(name);
                }
                
            }
            return false;
        }

        public bool IsValidPhone(string phone)
        {
            if (!String.IsNullOrEmpty(phone))
            {
                Regex regex = new Regex(@"^(?:\+?1[-. ]?)?\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$");
                return regex.IsMatch(phone);
            }
            return false;
        }

        public bool Is15DaysBefore(DateTime startDate, DateTime endDate)
        {
            if (startDate.AddDays(15) > endDate)
                return false;
            return true;
        }

        public bool IsValidDonationTarget(double target)
        {
            if (target < CurrencyUnit.TEN_MILLION || target > CurrencyUnit.ONE_BILLION)
                return false;
            return true;
        }

        public bool IsValidDateInProjectPeriod(Period projectPeriod, DateTime startDate, DateTime endDate)
        {
            if(startDate >= projectPeriod.StartDate && endDate <= projectPeriod.EndDate)
            {
                return true;
            }
            return false;
        }

        public bool Is7DaysBeforeStartDate(DateTime date, DateTime startDate)
        {
            if(date.AddDays(7) <= startDate)
            {
                return true;
            }
            return false;
        }

        public bool IsValidForetime(DateTime date)
        {
            throw new NotImplementedException();
        }

        public bool IsValidDonateAmount(long amount)
        {
            if (amount > CurrencyUnit.TWENTY_MILLION || amount < CurrencyUnit.ONE_THOUSAND)
            {
                return false;
            }
            return true;
        }
    }
}
