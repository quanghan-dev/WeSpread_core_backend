using System.Collections.Generic;

namespace BLL.Service
{
    public interface IUtilService
    {
        string GenerateOTP();

        string changeToInternationalPhoneNumber(string phone);

        string changeToVietnamPhoneNumber(string phone);

        bool IsNullOrEmpty<T>(IEnumerable<T> list);

        string Create16DigitString();

        string Create16Alphanumeric();
    }
}
