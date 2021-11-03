using BLL.Dto.Payment.MoMo.IPN;

namespace BLL.Profile
{
    public class MomoProfile : AutoMapper.Profile
    {
        public MomoProfile()
        {
            CreateMap<MoMoIPNRequest, MoMoIPNResponse>();
        }
    }
}
