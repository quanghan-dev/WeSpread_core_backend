using BLL.Dto.DonationSession;
using DAL.Model;

namespace BLL.Profile
{
    public class DonationSessionProfile : AutoMapper.Profile
    {
        public DonationSessionProfile()
        {
            CreateMap<DonationSessionRequest, DonationSession>();

            CreateMap<DonationSession, DonationSessionResponse>();

        }
    }
}
