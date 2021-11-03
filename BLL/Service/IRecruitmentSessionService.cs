using BLL.Dto;
using BLL.Dto.RecruitmentSession;
using System.Collections.Generic;

namespace BLL.Service
{
    public interface IRecruitmentSessionService
    {
        BaseResponse<RecruitmentSessionResponse> CreateRecruitmentSession(string token, RecruitmentSessionRequest recruitmentSessionRequest);

        BaseResponse<RecruitmentSessionResponse> GetRecruitmentSessionById(string id);

        BaseResponse<IEnumerable<RecruitmentSessionResponse>> GetRecruitmentSessionByProjectId(string projectId);

        BaseResponse<IEnumerable<RecruitmentSessionResponse>> GetAllRecruitmentSession();

        BaseResponse<RecruitmentSessionResponse> UpdateRecruitmentSession(string token, string id, RecruitmentSessionRequest recruitmentSessionRequest);

        BaseResponse<RecruitmentSessionResponse> DeleteRecruitmentSession(string token, string id);

        double GetTotalAppliesBySessionId(string sessionId);

    }
}
