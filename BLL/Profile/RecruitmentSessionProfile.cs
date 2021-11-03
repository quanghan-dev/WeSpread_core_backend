using AutoMapper;
using BLL.Dto.RecruitmentSession;
using DAL.Model;

namespace BLL.Profile
{
    public class RecruitmentSessionProfile : AutoMapper.Profile
    {
        public RecruitmentSessionProfile()
        {
            CreateMap<RecruitmentSessionRequest, RecruitmentSession>();

            CreateMap<RecruitmentSession, RecruitmentSessionResponse>();
        }
    }
}
