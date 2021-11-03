using AutoMapper;
using BLL.Constant;
using BLL.Dto;
using BLL.Dto.Donate;
using BLL.Dto.Exception;
using BLL.Dto.Payment.MoMo.CaptureWallet;
using BLL.Dto.Payment.MoMo.IPN;
using BLL.SignalRHub;
using DAL.Model;
using DAL.UnifOfWork;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace BLL.Service.Impl
{
    public class MoMoService : IMoMoService
    {

        private HttpWebRequest request;
        private HttpWebResponse response;
        private readonly ILogger _logger;
        private readonly ISecurityService _securityService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _distributedCache;
        private readonly IUnitOfWork _unitOfWork;

        public MoMoService(ILogger logger, ISecurityService securityService, IConfiguration configuration,
            IMapper mapper, IDistributedCache distributedCache,
            IUnitOfWork unitOfWork)
        {

            _logger = logger;
            _securityService = securityService;
            _configuration = configuration;
            _mapper = mapper;
            _distributedCache = distributedCache;
            _unitOfWork = unitOfWork;
        }

        public MoMoCaptureWalletResponse CreateCaptureWallet(MoMoCaptureWalletRequest requestData)
        {
            MoMoCaptureWalletResponse result;
            //try
            //{
                // Convert object to json string
                string jsonData = JsonConvert.SerializeObject(requestData);

                // Encoding to UTF8 before pass params
                byte[] byteData = Encoding.UTF8.GetBytes(jsonData);

                request = (HttpWebRequest)WebRequest.Create(Endpoint.MOMO_TEST + Endpoint.MOMO_CREATE_PAYMENT);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = byteData.Length;
                request.Timeout = (int)TimeUnit.TIMEOUT_20_SEC;
                request.ReadWriteTimeout = (int)TimeUnit.TIMEOUT_20_SEC;

                _logger.Information($"[MoMoCaptureWallet] Start request with data: {jsonData}");

                // Open request stream to send data
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(byteData, 0, byteData.Length);

                // Must close stream after write data
                requestStream.Close();

                // Open response
                response = (HttpWebResponse)request.GetResponse();

                // Open response stream to receive data
                Stream responseStream = response.GetResponseStream();

                string responseString;
                using (StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8))
                {
                    responseString = streamReader.ReadToEnd();
                }

                // Must close response and its stream after receive data
                response.Close();
                responseStream.Close();

                _logger.Information($"[MoMoCaptureWallet] End request with data: {responseString}");

                result = JsonConvert.DeserializeObject<MoMoCaptureWalletResponse>(responseString);

                return result;
            //} catch (Exception ex)
            //{
            //    _logger.Error(ex.Message);

            //    throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<string>
            //    {
            //        ResultCode = ResultCode.ERROR_CODE,
            //        ResultMessage = ResultCode.GetMessage(ResultCode.ERROR_CODE),
            //        Data = null
            //    });
            //}
        }

        public MoMoIPNResponse ProcessIPN(MoMoIPNRequest momoIPNRequest)
        {
            // Validate signature
            List<string> ignoreFields = new List<string>() { "signature", "partnerName", "storeId", "lang" };
            string rawData = _securityService.GetRawDataSignature(momoIPNRequest, ignoreFields);
            rawData = "accessKey=" + _configuration.GetValue<string>("MoMo:AccessKey") + "&" + rawData;

            string merchantSignature = _securityService.SignHmacSHA256(rawData, _configuration.GetValue<string>("MoMo:SecretKey"));

            _logger.Information($"[MoMo IPN] MoMo - Merchant signature: {momoIPNRequest.signature} - {merchantSignature}");

            if (!merchantSignature.Equals(momoIPNRequest.signature))
            {
                _logger.Error("[MoMoIPN] Signature not match!");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<MoMoIPNResponse>
                {
                    ResultCode = ResultCode.MOMO_IPN_SIGNATURE_NOT_MATCH_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.MOMO_IPN_SIGNATURE_NOT_MATCH_CODE)
                });
            }

            // update data donate and response to client
            SendMomoPaymentResponseToClient(momoIPNRequest);

            //response to MoMo
            MoMoIPNResponse momoIPNResponse = _mapper.Map<MoMoIPNResponse>(momoIPNRequest);
            momoIPNResponse.signature = _securityService.GetRawDataSignature(momoIPNResponse, ignoreFields);

            return momoIPNResponse;
        }

        public void SendMomoPaymentResponseToClient(MoMoIPNRequest momoIPNRequest)
        {
            DonateDetail donateDetail;
            try
            {
                donateDetail = _unitOfWork.GetRepository<DonateDetail>().Get(momoIPNRequest.orderId);

                donateDetail.TransId = momoIPNRequest.transId.ToString();
                donateDetail.UpdatedAt = DateTime.Now;
                donateDetail.ResultCode = momoIPNRequest.resultCode;
                donateDetail.ResultMessage = ResultCode.GetMessage(donateDetail.ResultCode);

                _unitOfWork.GetRepository<DonateDetail>().Update(donateDetail);
                _unitOfWork.Commit();
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<MoMoIPNResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }

            ////get connectionId
            //string cacheKey = donateDetail.Id;
            //string connectionId = _distributedCache.GetString(cacheKey).ToString();

            ////response to client
            //DonateResultResponse donateResultResponse = _mapper.Map<DonateResultResponse>(donateDetail);

            //SignalRHubService signalRHubService = new SignalRHubService();

            //await signalRHubService.SendPaymentResponseToClient(new BaseResponse<DonateResultResponse>
            //{
            //    ResultCode = ResultCode.SUCCESS_CODE,
            //    ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
            //    Data = donateResultResponse

            //}, connectionId);
        }
    }
}
