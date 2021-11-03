using BLL.Dto.Payment.MoMo.CaptureWallet;
using BLL.Dto.Payment.MoMo.IPN;

namespace BLL.Service
{
    public interface IMoMoService
    {
        MoMoCaptureWalletResponse CreateCaptureWallet(MoMoCaptureWalletRequest requestData);
        MoMoIPNResponse ProcessIPN(MoMoIPNRequest momoIPNRequest);
        void SendMomoPaymentResponseToClient(MoMoIPNRequest momoIPNRequest);
    }
}
