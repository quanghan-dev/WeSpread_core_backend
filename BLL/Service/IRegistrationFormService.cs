using BLL.Dto;
using BLL.Dto.Member;
using BLL.Dto.RegistrationForm;
using System.Collections.Generic;

namespace BLL.Service
{
    public interface IRegistrationFormService
    {
        public BaseResponse<RegistrationFormResponse> CreateRegistrationForm(RegistrationFormRequest registrationFormRequest, string token);

        public BaseResponse<RegistrationFormResponse> CancelRegistrationForm(string id, string token);

        public BaseResponse<IEnumerable<RegistrationFormResponse>> GetRegistrationFormByUser(string token);

        public BaseResponse<IEnumerable<RegistrationFormResponse>> GetRegistrationFormBySessionIdAndOrgAdmin(string sessonId, string token);

        public bool CheckOrgRoleByRecruitmentSession(string recruitmentSessionId, string userId);

        public BaseResponse<RegistrationFormResponse> ApproveForm(string formId, string token);

        public BaseResponse<RegistrationFormResponse> RejectForm(string formId, string token);

        public BaseResponse<IEnumerable<MemberApply>> GetMemberApplyBySession(string id);
    }
}
