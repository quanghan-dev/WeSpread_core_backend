using BLL.Dto;
using BLL.Dto.Donate;
using BLL.Dto.Member;
using System.Collections.Generic;

namespace BLL.Service
{
    public interface IDonateService
    {
        BaseResponse<DonateLinkResponse> CreateLinkDonate(DonateRequest request, string token);

        BaseResponse<IEnumerable<DonateResultResponse>> HistoryDonate(string token);

        bool CheckRemainingTarget(string sessionId, long amount);

        double GetTotalAmountDonatedBySessionId(string sessionId);

        double GetTotalDonationsBySessionId(string sessionId);

        BaseResponse<IEnumerable<MemberDonate>> GetMemberDonateBySession(string sessionId);
    }
}
