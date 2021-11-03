using BLL.Constant;
using BLL.Dto;
using BLL.Dto.Exception;
using DAL.Model;
using DAL.UnifOfWork;
using System;
using System.Linq;
using System.Net;

namespace BLL.Service.Impl
{
    public class PersistentLoginService : IPersistentLoginService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;

        public PersistentLoginService(IUnitOfWork unitOfWork, ILogger logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public string GetUserIDByToken(string token)
        {
            try
            {
                PersistentLogin persistentLogin = _unitOfWork
                    .GetRepository<PersistentLogin>()
                    .GetAll()
                    .FirstOrDefault(pl => pl.Token.Equals(token) && 
                    pl.ExpirationDate >= DateTime.Now);

                if(persistentLogin != null)
                {
                    return persistentLogin.UserId;
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                return null;
            }

            return null;
        }

        public void StoreUserToken(string token, string userID)
        {
            try
            {
                PersistentLogin persitentLogin = _unitOfWork.GetRepository<PersistentLogin>().Get(userID);

                if (persitentLogin != null)
                {
                    persitentLogin.ExpirationDate = DateTime.Now.AddDays(TimeUnit.THIRTY_DAYS);
                    persitentLogin.Token = token;
                    persitentLogin.LastLogin = DateTime.Now;

                    _unitOfWork.GetRepository<PersistentLogin>().Update(persitentLogin);
                }
                else
                    _unitOfWork.GetRepository<PersistentLogin>().Add(new PersistentLogin
                    {
                        UserId = userID,
                        ExpirationDate = DateTime.Now.AddDays(TimeUnit.THIRTY_DAYS),
                        Token = token,
                        LastLogin = DateTime.Now
                    });

            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<PersistentLogin>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }

        }
    }
}
