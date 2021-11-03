using System.Collections.Generic;

namespace BLL.Constant
{
    public class ResultCode
    {
        public static readonly int ERROR_CODE = -1;

        public static readonly int SUCCESS_CODE = 0; //success

        public static readonly int USER_NOT_FOUND_CODE = 101; //user not found

        public static readonly int USER_INACTIVE_CODE = 102; //user inactive

        public static readonly int USER_UNAUTHORIZED_CODE = 103; //unauthorized user

        public static readonly int USER_BLOCKED_CODE = 104; //blocked user

        public static readonly int SQL_ERROR_CODE = 201; //sql error

        public static readonly int MESSAGE_NOT_SENT_CODE = 202; //message not sent

        public static readonly int WRONG_OTP_INPUTTED_CODE = 203; //wrong otp inputted

        public static readonly int UPLOAD_FILE_FIREBASE_ERROR_CODE = 204; //cannot upload file to Firebase Storage

        public static readonly int INVALID_NAME_CODE = 205; //invalid name

        public static readonly int INVALID_EMAIL_CODE = 206; //invalid email

        public static readonly int INVALID_PHONE_CODE = 207; //invalid phone

        public static readonly int INVALID_ADDRESS_CODE = 208; //invalid address

        public static readonly int INVALID_FORETIME_CODE = 209; //invalid foretime

        public static readonly int INVALID_STARTTIME_ENDTIME_CODE = 210; //invalid starttime endtime

        public static readonly int INVALID_DONATION_TARGET_CODE = 211; //invalid donation target

        public static readonly int EMPTY_LOCATION_CODE = 212; //location is empty

        public static readonly int EMPTY_CATEGORY_CODE = 213; //category is empty

        public static readonly int INVALID_DATE_IN_PROJECT_PERIOD_CODE = 214; //invalid date in project period

        public static readonly int INVALID_TIME_FOR_UPDATE_CODE = 215; //invalid time for update

        public static readonly int ORG_NOT_FOUND_CODE = 301; //organization not found

        public static readonly int UNVERIFIED_CREATE_ORGANIZATION_CODE = 302; //unverified create organization

        public static readonly int UNVERIFIED_UPDATE_ORGANIZATION_CODE = 303; //unverified update organization

        public static readonly int UNVERIFIED_DELETE_ORGANIZATION_CODE = 304; //unverified delete organization

        public static readonly int DELETED_ORGANIZATION_CODE = 305; //deleted organization

        public static readonly int PROJECT_NOT_FOUND_CODE = 401; //project not found

        public static readonly int UNVERIFIED_CREATE_PROJECT_CODE = 402; //unverified create project

        public static readonly int UNVERIFIED_UPDATE_PROJECT_CODE = 403; //unverified update project

        public static readonly int UNVERIFIED_DELETE_PROJECT_CODE = 404; //unverified delete project

        public static readonly int DELETED_PROJECT_CODE = 405; //deleted project

        public static readonly int DONATION_SESSION_NOT_FOUND_CODE = 501; //donation session not found

        public static readonly int UNVERIFIED_CREATE_DONATION_SESSION_CODE = 502; //unverified create donation session

        public static readonly int UNVERIFIED_UPDATE_DONATION_SESSION_CODE = 503; //unverified update donation session

        public static readonly int UNVERIFIED_DELETE_DONATION_SESSION_CODE = 504; //unverified delete donation session
        
        public static readonly int DELETED_DONATION_SESSION_CODE = 505; //deleted donation session

        public static readonly int RECRUITMENT_SESSION_NOT_FOUND_CODE = 601; //recruitment session not found

        public static readonly int UNVERIFIED_CREATE_RECRUITMENT_SESSION_CODE = 602; //unverified create recruitment session

        public static readonly int UNVERIFIED_UPDATE_RECRUITMENT_SESSION_CODE = 603; //unverified update recruitment session

        public static readonly int UNVERIFIED_DELETE_RECRUITMENT_SESSION_CODE = 604; //unverified update recruitment session

        public static readonly int DELETED_RECRUITMENT_SESSION_CODE = 605; //deleted recruitment session

        public static readonly int UNVERIFIED_REGISTRATION_FORM_CODE = 701; //unverified registration form

        public static readonly int CANCEL_REGISTRATION_FORM_CODE = 702; //cancel registration form

        public static readonly int REJECT_REGISTRATION_FORM_CODE = 703; //reject registration form

        public static readonly int UNVERIFIED_DONATE_DETAIL_CODE = 801; // unverified donate detail code

        public static readonly int OVER_TARGET_DONATE_DETAIL_CODE = 802; // over target donate detail code

        public static readonly int INVALID_DONATE_DETAIL_AMOUNT_CODE = 803; // invalid donate detail amount code

        public static readonly int LOCATION_EXISTS_CODE = 901; // location already exists

        public static readonly int MOMO_IPN_SIGNATURE_NOT_MATCH_CODE = 1001; // MoMo IPN's signature not match


        // -------------------------------------------------------------------------------------------
        public static readonly string ERROR_MESSAGE = "Lỗi hệ thống"; // Default errror message

        public static readonly string SUCCESS_MESSAGE = "Thành công";

        public static readonly string USER_NOT_FOUND_MESSAGE = "Không tìm thấy người dùng"; //user not found

        public static readonly string USER_INACTIVE_MESSAGE = "Người dùng chưa được kích hoạt"; //user inactive

        public static readonly string USER_UNAUTHORIZED_MESSAGE = "Bạn không có quyền thực hiện yêu cầu này"; //unauthorized user

        public static readonly string USER_BLOCKED_MESSAGE = "Tài khoản của bạn đã bị vô hiệu hoá"; //blocked user

        public static readonly string SQL_ERROR_MESSAGE = "Không kết nối được dữ liệu"; //sql error

        public static readonly string ORG_NOT_FOUND_MESSAGE = "Không tìm thấy tổ chức"; //org notfound

        public static readonly string MESSAGE_NOT_SEND_MESSAGE = "Không thể gửi tin nhắn OTP"; //message not send

        public static readonly string WRONG_OTP_INPUTTED_MESSAGE = "Nhập sai OTP"; //wrong otp inputted

        public static readonly string UPLOAD_FILE_FIREBASE_ERROR_MESSAGE = "Không thể tải được hình ảnh của bạn"; //cannot upload file to Firebase Storage

        public static readonly string INVALID_NAME_MESSAGE = "Vui lòng nhập đúng định dạng của tên (ví dụ: Nguyễn Văn A)"; //invalid name

        public static readonly string INVALID_EMAIL_MESSAGE = "Vui lòng nhập đúng định dạng email (ví dụ: abc@abc.abc)"; //invalid email

        public static readonly string INVALID_PHONE_MESSAGE = "Vui lòng nhập đúng định dạng số điện thoại (ví dụ: 0123456789)"; //invalid phone

        public static readonly string INVALID_ADDRESS_MESSAGE = "Vui lòng nhập đúng định dạng địa chỉ (ví dụ: đường Quang Trung, quận Gò Vấp, TP.HCM)"; //invalid address

        public static readonly string INVALID_FORETIME_MESSAGE = "Vui lòng nhập ngày trước ngày hiện tại"; //invalid foretime

        public static readonly string INVALID_STARTTIME_ENDTIME_MESSAGE = "Thời điểm bắt đầu phải sớm hơn thời điểm kết thúc tối thiểu 15 ngày"; //invalid starttime endtime

        public static readonly string UNVERIFIED_CREATE_ORGANIZATION_MESSAGE = "Tổ chức được tạo mới chưa được xác thực"; //unverified create organization

        public static readonly string UNVERIFIED_UPDATE_ORGANIZATION_MESSAGE = "Tổ chức được cập nhật chưa được xác thực"; //unverified update organization

        public static readonly string UNVERIFIED_DELETE_ORGANIZATION_MESSAGE = "Xoá tổ chức chưa được xác thực"; //unverified delete organization

        public static readonly string DELETED_ORGANIZATION_MESSAGE = "Tổ chức đã bị xoá"; //deleted organization

        public static readonly string PROJECT_NOT_FOUND_MESSAGE = "Không tìm thấy dự án"; //project not found

        public static readonly string UNVERIFIED_CREATE_PROJECT_MESSAGE = "Dự án được tạo mới chưa được xác thực"; //unverified create project

        public static readonly string UNVERIFIED_UPDATE_PROJECT_MESSAGE = "Dự án được cập nhật chưa được xác thực"; //unverified update project

        public static readonly string DELETED_PROJECT_MESSAGE = "Dự án đã bị xoá"; //deleted project

        public static readonly string UNVERIFIED_DELETE_PROJECT_MESSAGE = "Dự án bị xoá chưa được xác thực"; //unverified delete project

        public static readonly string EMPTY_LOCATION_MESSAGE = "Vui lòng chọn vị trí"; //location is empty

        public static readonly string EMPTY_CATEGORY_MESSAGE = "Vui lòng chọn loại tổ chức (dự án)"; //category is empty

        public static readonly string DONATION_SESSION_NOT_FOUND_MESSAGE = "Không tìm thấy đợt quyên góp"; //donation session not found

        public static readonly string UNVERIFIED_CREATE_DONATION_SESSION_MESSAGE = "Đợt quyên góp được tạo mới chưa được xác thực"; //unverified create donation session

        public static readonly string UNVERIFIED_UPDATE_DONATION_SESSION_MESSAGE = "Đợt quyên góp được cập nhật chưa được xác thực"; //unverified update donation session

        public static readonly string UNVERIFIED_DELETE_DONATION_SESSION_MESSAGE = "Đợt quyên góp bị xoá chưa được xác thực"; //unverified update donation session

        public static readonly string DELETED_DONATION_SESSION_MESSAGE = "Đợt quyên góp đã bị xoá"; //unverified delete project

        public static readonly string INVALID_DONATION_TARGET_MESSAGE = "Mục tiêu tối thiểu là 10.000.000đ, tối đa là 1.000.000.000đ"; //invalid donation target

        public static readonly string RECRUITMENT_SESSION_NOT_FOUND_MESSAGE = "Không tìm thấy đợt tuyển thành viên"; //recruitment session not found

        public static readonly string UNVERIFIED_CREATE_RECRUITMENT_SESSION_MESSAGE = "Đợt tuyển thành viên được tạo mới chưa được xác thực"; //unverified create recruitment session

        public static readonly string UNVERIFIED_UPDATE_RECRUITMENT_SESSION_MESSAGE = "Đợt tuyển thành viên được cập nhật chưa được xác thực"; //unverified update recruitment session

        public static readonly string UNVERIFIED_DELETE_RECRUITMENT_SESSION_MESSAGE = "Đợt tuyển thành viên bị xoá chưa được xác thực"; //unverified update recruitment session

        public static readonly string DELETED_RECRUITMENT_SESSION_MESSAGE = "Đợt tuyển thành viên đã bị xoá"; //unverified delete project

        public static readonly string INVALID_DATE_IN_PROJECT_PERIOD_MESSAGE = "Thời gian đợt quyên góp, tuyển thành viên phải nằm trong khoảng thời gian dự án hoạt động"; //invalid date in project period

        public static readonly string INVALID_TIME_FOR_UPDATE_MESSAGE = "Dự án chỉ được chỉnh sửa 7 ngày trước khi bắt đầu"; //invalid time for update

        public static readonly string MOMO_IPN_SIGNATURE_NOT_MATCH_MESSAGE = "Chữ kí yêu cầu không hợp lệ"; // MoMo IPN's signature not match

        public static readonly string UNVERIFIED_REGISTRATION_FORM_MESSAGE = "Yêu cầu đăng ký thành viên của bạn đang chờ xét duyệt"; //unverified registration form

        public static readonly string CANCEL_REGISTRATION_FORM_MESSAGE = "Yêu cầu đăng ký thành viên của bạn đã được huỷ"; //cancel registration form

        public static readonly string REJECT_REGISTRATION_FORM_MESSAGE = "Yêu cầu đăng ký thành viên đã bị từ chối"; //reject registration form

        public static readonly string UNVERIFIED_DONATE_DETAIL_MESSAGE = "Yêu cầu đang được xử lý"; // unverified donate detail code

        public static readonly string OVER_TARGET_DONATE_DETAIL_MESSAGE = "Số tiền quyên góp vượt quá số tiền cần quyên góp còn lại"; // over target donate detail code

        public static readonly string INVALID_DONATE_DETAIL_AMOUNT_MESSAGE = "Số tiền quyên góp tối thiểu là 1.000đ, tối đa là 20.000.000đ"; // invalid donate detail amount code

        public static readonly string LOCATION_EXISTS_MESSAGE = "Vị trí này đã tồn tại"; // location already exists

        public static IDictionary<int, string> CodeMessageMap = new Dictionary<int, string>();


        static ResultCode()
        {
            // Error

            // Success
            CodeMessageMap.Add(SUCCESS_CODE, SUCCESS_MESSAGE);

            // User
            CodeMessageMap.Add(USER_NOT_FOUND_CODE, USER_NOT_FOUND_MESSAGE);
            CodeMessageMap.Add(USER_INACTIVE_CODE, USER_INACTIVE_MESSAGE);
            CodeMessageMap.Add(USER_UNAUTHORIZED_CODE, USER_UNAUTHORIZED_MESSAGE);

            // Server, validate data
            CodeMessageMap.Add(SQL_ERROR_CODE, SQL_ERROR_MESSAGE);
            CodeMessageMap.Add(MESSAGE_NOT_SENT_CODE, MESSAGE_NOT_SEND_MESSAGE);
            CodeMessageMap.Add(WRONG_OTP_INPUTTED_CODE, WRONG_OTP_INPUTTED_MESSAGE);
            CodeMessageMap.Add(UPLOAD_FILE_FIREBASE_ERROR_CODE, UPLOAD_FILE_FIREBASE_ERROR_MESSAGE);
            CodeMessageMap.Add(INVALID_NAME_CODE, INVALID_NAME_MESSAGE);
            CodeMessageMap.Add(INVALID_EMAIL_CODE, INVALID_EMAIL_MESSAGE);
            CodeMessageMap.Add(INVALID_PHONE_CODE, INVALID_PHONE_MESSAGE);
            CodeMessageMap.Add(INVALID_ADDRESS_CODE, INVALID_ADDRESS_MESSAGE);
            CodeMessageMap.Add(INVALID_FORETIME_CODE, INVALID_FORETIME_MESSAGE);
            CodeMessageMap.Add(INVALID_STARTTIME_ENDTIME_CODE, INVALID_STARTTIME_ENDTIME_MESSAGE);
            CodeMessageMap.Add(INVALID_DATE_IN_PROJECT_PERIOD_CODE, INVALID_DATE_IN_PROJECT_PERIOD_MESSAGE);
            CodeMessageMap.Add(INVALID_TIME_FOR_UPDATE_CODE, INVALID_TIME_FOR_UPDATE_MESSAGE);

            // Organization
            CodeMessageMap.Add(ORG_NOT_FOUND_CODE, ORG_NOT_FOUND_MESSAGE);
            CodeMessageMap.Add(UNVERIFIED_UPDATE_ORGANIZATION_CODE, UNVERIFIED_UPDATE_ORGANIZATION_MESSAGE);
            CodeMessageMap.Add(UNVERIFIED_CREATE_ORGANIZATION_CODE, UNVERIFIED_CREATE_ORGANIZATION_MESSAGE);
            CodeMessageMap.Add(UNVERIFIED_DELETE_ORGANIZATION_CODE, UNVERIFIED_DELETE_ORGANIZATION_MESSAGE);
            CodeMessageMap.Add(DELETED_ORGANIZATION_CODE, DELETED_ORGANIZATION_MESSAGE);
            

            // Project
            CodeMessageMap.Add(PROJECT_NOT_FOUND_CODE, PROJECT_NOT_FOUND_MESSAGE);
            CodeMessageMap.Add(UNVERIFIED_CREATE_PROJECT_CODE, UNVERIFIED_CREATE_PROJECT_MESSAGE);
            CodeMessageMap.Add(UNVERIFIED_UPDATE_PROJECT_CODE, UNVERIFIED_UPDATE_PROJECT_MESSAGE);
            CodeMessageMap.Add(DELETED_PROJECT_CODE, DELETED_PROJECT_MESSAGE);
            CodeMessageMap.Add(UNVERIFIED_DELETE_PROJECT_CODE, UNVERIFIED_DELETE_PROJECT_MESSAGE);


            // City, category
            CodeMessageMap.Add(EMPTY_LOCATION_CODE, EMPTY_LOCATION_MESSAGE);
            CodeMessageMap.Add(EMPTY_CATEGORY_CODE, EMPTY_CATEGORY_MESSAGE);

            // Donation session
            CodeMessageMap.Add(DONATION_SESSION_NOT_FOUND_CODE, DONATION_SESSION_NOT_FOUND_MESSAGE);
            CodeMessageMap.Add(UNVERIFIED_CREATE_DONATION_SESSION_CODE, UNVERIFIED_CREATE_DONATION_SESSION_MESSAGE);
            CodeMessageMap.Add(UNVERIFIED_UPDATE_DONATION_SESSION_CODE, UNVERIFIED_UPDATE_DONATION_SESSION_MESSAGE);
            CodeMessageMap.Add(UNVERIFIED_DELETE_DONATION_SESSION_CODE, UNVERIFIED_DELETE_DONATION_SESSION_MESSAGE);
            CodeMessageMap.Add(INVALID_DONATION_TARGET_CODE, INVALID_DONATION_TARGET_MESSAGE);
            CodeMessageMap.Add(DELETED_DONATION_SESSION_CODE, DELETED_DONATION_SESSION_MESSAGE);

            // Recruitment session
            CodeMessageMap.Add(RECRUITMENT_SESSION_NOT_FOUND_CODE, RECRUITMENT_SESSION_NOT_FOUND_MESSAGE);
            CodeMessageMap.Add(UNVERIFIED_CREATE_RECRUITMENT_SESSION_CODE, UNVERIFIED_CREATE_RECRUITMENT_SESSION_MESSAGE);
            CodeMessageMap.Add(UNVERIFIED_UPDATE_RECRUITMENT_SESSION_CODE, UNVERIFIED_UPDATE_RECRUITMENT_SESSION_MESSAGE);
            CodeMessageMap.Add(UNVERIFIED_DELETE_RECRUITMENT_SESSION_CODE, UNVERIFIED_DELETE_RECRUITMENT_SESSION_MESSAGE);
            CodeMessageMap.Add(DELETED_RECRUITMENT_SESSION_CODE, DELETED_RECRUITMENT_SESSION_MESSAGE);

            //registration form
            CodeMessageMap.Add(UNVERIFIED_REGISTRATION_FORM_CODE, UNVERIFIED_REGISTRATION_FORM_MESSAGE);
            CodeMessageMap.Add(CANCEL_REGISTRATION_FORM_CODE, CANCEL_REGISTRATION_FORM_MESSAGE);
            CodeMessageMap.Add(REJECT_REGISTRATION_FORM_CODE, REJECT_REGISTRATION_FORM_MESSAGE);

            //donate detail
            CodeMessageMap.Add(UNVERIFIED_DONATE_DETAIL_CODE, UNVERIFIED_DONATE_DETAIL_MESSAGE);
            CodeMessageMap.Add(OVER_TARGET_DONATE_DETAIL_CODE, OVER_TARGET_DONATE_DETAIL_MESSAGE);
            CodeMessageMap.Add(INVALID_DONATE_DETAIL_AMOUNT_CODE, INVALID_DONATE_DETAIL_AMOUNT_MESSAGE);

            //location
            CodeMessageMap.Add(LOCATION_EXISTS_CODE, LOCATION_EXISTS_MESSAGE);
            
        }

        public static string GetMessage(int code)
        {
            string message;
            if (!CodeMessageMap.TryGetValue(code, out message)) return ERROR_MESSAGE;

            return message;
        }
    }
}
