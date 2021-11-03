using BLL.Dto.Donate;
using DAL.Model;

namespace BLL.Profile
{
    public class DonateProfile : AutoMapper.Profile
    {
        public DonateProfile()
        {
            CreateMap<DonateRequest, DonateDetail>();

            CreateMap<DonateDetail, DonateResultResponse>();
        }
    }
}
