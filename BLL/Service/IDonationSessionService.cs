using BLL.Dto;
using BLL.Dto.DonationSession;
using System.Collections.Generic;

namespace BLL.Service
{
    public interface IDonationSessionService
    {
        BaseResponse<DonationSessionResponse> CreateDonationSession(string token, DonationSessionRequest donationSessionRequest);

        BaseResponse<DonationSessionResponse> GetDonationSessionById(string id);

        BaseResponse<IEnumerable<DonationSessionResponse>> GetDonationSession();

        BaseResponse<IEnumerable<DonationSessionResponse>> GetDonationSessionByProjectId(string projectId);

        BaseResponse<DonationSessionResponse> UpdateDonationSession(string id, string token, DonationSessionRequest donationSessionRequest);

        BaseResponse<DonationSessionResponse> DeleteDonationSession(string id, string token);

    }
}
