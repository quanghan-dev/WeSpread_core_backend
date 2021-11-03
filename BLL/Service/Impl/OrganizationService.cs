using AutoMapper;
using BLL.Constant;
using BLL.Dto;
using BLL.Dto.Exception;
using BLL.Dto.Location;
using BLL.Dto.Organization;
using DAL.Model;
using DAL.UnifOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace BLL.Service.Impl
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _distributedCache;
        private readonly IPersistentLoginService _persistentLoginService;
        private readonly IUploadFirebaseService _uploadFirebaseService;
        private readonly IValidateDataService _validateDataService;
        private readonly IUtilService _utilService;
        private readonly ILocationService _locationService;
        private const string TYPE = "Organization";
        private const string ORG_ADMIN = "ORG_ADMIN";

        public OrganizationService(IUnitOfWork unitOfWork, ILogger logger, IMapper mapper,
            IDistributedCache distributedCache, IPersistentLoginService persistentLoginService,
            IUploadFirebaseService uploadFirebaseService, IValidateDataService validateDataService,
            IUtilService utilService, ILocationService locationService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
            _distributedCache = distributedCache;
            _persistentLoginService = persistentLoginService;
            _uploadFirebaseService = uploadFirebaseService;
            _validateDataService = validateDataService;
            _utilService = utilService;
            _locationService = locationService;
        }

        //Create Org
        public BaseResponse<OrganizationResponse> Create(OrganizationRequest organizationCreate, string token,
            IFormFile logo, List<IFormFile> covers)
        {
            //validate token
            string userId = _persistentLoginService.GetUserIDByToken(token);
            if (String.IsNullOrEmpty(userId))
            {
                _logger.Error($"User with token {token} is unauthorized.");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<OrganizationResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE)
                });
            }

            //validate name
            if (!_validateDataService.IsValidName(organizationCreate.OrgName, null))
            {
                _logger.Error($"{organizationCreate.OrgName} is invalid name.");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<OrganizationResponse>
                {
                    ResultCode = ResultCode.INVALID_NAME_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_NAME_CODE)
                });
            }

            if (organizationCreate.EngName != null)
            {
                if (!_validateDataService.IsValidName(organizationCreate.EngName, null))
                {
                    _logger.Error($"{organizationCreate.EngName} is invalid name.");

                    throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<OrganizationResponse>
                    {
                        ResultCode = ResultCode.INVALID_NAME_CODE,
                        ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_NAME_CODE)
                    });
                }
            }

            //validate foretime
            if (!_validateDataService.IsValidForetime(organizationCreate.FoundingDate))
            {
                _logger.Error($"{organizationCreate.FoundingDate} is invalid date.");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<OrganizationResponse>
                {
                    ResultCode = ResultCode.INVALID_FORETIME_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_FORETIME_CODE)
                });
            }


            //upload file to Firebase Storage
            string coverUrl = _uploadFirebaseService
                .UploadFilesToFirebase(covers, TYPE, organizationCreate.OrgName, "cover").Result;

            string logoUrl = _uploadFirebaseService
                .UploadFileToFirebase(logo, TYPE, organizationCreate.OrgName, "logo").Result;

            if (coverUrl is null || logoUrl is null)
            {
                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<OrganizationResponse>
                {
                    ResultCode = ResultCode.UPLOAD_FILE_FIREBASE_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.UPLOAD_FILE_FIREBASE_ERROR_CODE)
                });
            }

            //map org and store to db
            Organization orgModel = _mapper.Map<Organization>(organizationCreate);
            string orgID = "Org_" + ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString();

            ICollection<LocationResponse> location;

            try
            {
                orgModel.Name = organizationCreate.OrgName;
                orgModel.Id = orgID;
                orgModel.CreatedBy = userId;
                orgModel.CreatedAt = DateTime.Now;
                orgModel.IsActive = false;
                orgModel.UpdatedAt = orgModel.CreatedAt;
                orgModel.Cover = coverUrl;
                orgModel.Logo = logoUrl;
                orgModel.CreationCode = ResultCode.UNVERIFIED_CREATE_ORGANIZATION_CODE;
                orgModel.CreationMessage = ResultCode.GetMessage(ResultCode.UNVERIFIED_CREATE_ORGANIZATION_CODE);

                location = _locationService.CreateLocation(organizationCreate.Location);
                CreateItemCategory(organizationCreate.Category, orgID);
                CreateItemLocation(location, orgID);

                SetRoleOfOrg(userId, orgID, ORG_ADMIN);

                _unitOfWork.GetRepository<Organization>().Add(orgModel);

                _unitOfWork.Commit();
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<OrganizationResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }

            OrganizationResponse organizationCreateResponse = _mapper.Map<OrganizationResponse>(orgModel);
            organizationCreateResponse.Location = location;

            //store orgInfo to Redis
            string cacheKey = organizationCreateResponse.Id;
            string cacheValue = _distributedCache.GetString(cacheKey);

            if (!string.IsNullOrEmpty(cacheValue))
            {
                _distributedCache.Remove(cacheKey);
            }

            _distributedCache.SetString(cacheKey, JsonConvert.SerializeObject(organizationCreateResponse).ToString());

            return new BaseResponse<OrganizationResponse>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = organizationCreateResponse
            };
        }

        //Get all Orgs
        public BaseResponse<IEnumerable<OrganizationResponse>> GetAll()
        {
            IEnumerable<Organization> orgs = null;
            try
            {
                orgs = _unitOfWork.GetRepository<Organization>()
                    .GetAll();
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<OrganizationResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }

            List<OrganizationResponse> responses = _mapper.Map<List<OrganizationResponse>>(orgs);

            //get location
            foreach(OrganizationResponse response in responses)
            {
                response.Location = _locationService.GetLocationByOrgId(response.Id);
            }

            responses.Sort((x, y) => x.CreatedAt.CompareTo(y.CreatedAt));

            return new BaseResponse<IEnumerable<OrganizationResponse>>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = responses
            };
        }

        public BaseResponse<OrganizationResponse> GetById(string id)
        {
            string cacheKey = id;
            Organization org;
            if (_distributedCache.GetString(cacheKey) != null)
            {
                return new BaseResponse<OrganizationResponse>
                {
                    ResultCode = ResultCode.SUCCESS_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                    Data = JsonConvert.DeserializeObject<OrganizationResponse>(_distributedCache.GetString(cacheKey))
                };
            }
            else
            {
                try
                {
                    org = _unitOfWork.GetRepository<Organization>().Get(id);
                }
                catch (Exception e)
                {
                    _logger.Error(e.Message);

                    throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<OrganizationResponse>
                    {
                        ResultCode = ResultCode.ORG_NOT_FOUND_CODE,
                        ResultMessage = ResultCode.GetMessage(ResultCode.ORG_NOT_FOUND_CODE)
                    });
                }
            }

            OrganizationResponse response = _mapper.Map<OrganizationResponse>(org);
            response.Location = _locationService.GetLocationByOrgId(response.Id);

            return new BaseResponse<OrganizationResponse>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = response
            };
        }

        public BaseResponse<OrganizationResponse> UpdateById(string id, OrganizationRequest organizationUpdate,
            string token, IFormFile logo, List<IFormFile> covers)
        {
            //validate token
            string userId = _persistentLoginService.GetUserIDByToken(token);
            if (String.IsNullOrEmpty(userId))
            {
                _logger.Error($"User with token {token} is unauthorized.");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<OrganizationResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE)
                });
            }

            //valid role user
            if (!CheckUserRoleByOrgId(userId, id, ORG_ADMIN))
            {
                _logger.Error($"User {userId} does not have permission to update org {id}");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<OrganizationResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE)
                });
            }

            //validate Org Id
            Organization orgModel;
            try
            {
                orgModel = _unitOfWork.GetRepository<Organization>().Get(id);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<OrganizationResponse>
                {
                    ResultCode = ResultCode.ORG_NOT_FOUND_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.ORG_NOT_FOUND_CODE)
                });
            }

            //validate name
            if (!_validateDataService.IsValidName(organizationUpdate.OrgName, null))
            {
                _logger.Error($"{organizationUpdate.OrgName} is invalid name.");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<OrganizationResponse>
                {
                    ResultCode = ResultCode.INVALID_NAME_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_NAME_CODE)
                });
            }

            if (organizationUpdate.EngName != null)
            {
                if (!_validateDataService.IsValidName(organizationUpdate.EngName, null))
                {
                    _logger.Error($"{organizationUpdate.EngName} is invalid name.");

                    throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<OrganizationResponse>
                    {
                        ResultCode = ResultCode.INVALID_NAME_CODE,
                        ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_NAME_CODE)
                    });
                }
            }
            if (!_validateDataService.IsValidForetime(organizationUpdate.FoundingDate))
            {
                _logger.Error($"{organizationUpdate.FoundingDate} is invalid date.");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<OrganizationResponse>
                {
                    ResultCode = ResultCode.INVALID_FORETIME_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_FORETIME_CODE)
                });
            }


            //upload file to Firebase Storage
            string coverUrl = _uploadFirebaseService
                .UploadFilesToFirebase(covers, TYPE, organizationUpdate.OrgName, "cover").Result;

            string logoUrl = _uploadFirebaseService
                .UploadFileToFirebase(logo, TYPE, organizationUpdate.OrgName, "logo").Result;

            if (coverUrl is null || logoUrl is null)
            {
                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<OrganizationResponse>
                {
                    ResultCode = ResultCode.UPLOAD_FILE_FIREBASE_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.UPLOAD_FILE_FIREBASE_ERROR_CODE)
                });
            }

            //map org and store to db
            orgModel = _mapper.Map(organizationUpdate, orgModel);
            orgModel.Name = organizationUpdate.OrgName;
            orgModel.UpdatedAt = DateTime.Now;
            orgModel.Logo = logoUrl;
            orgModel.Cover = coverUrl;
            orgModel.CreationCode = ResultCode.UNVERIFIED_UPDATE_ORGANIZATION_CODE;
            orgModel.CreationMessage = ResultCode.GetMessage(ResultCode.UNVERIFIED_UPDATE_ORGANIZATION_CODE);
            orgModel.Checker = null;
            orgModel.CheckerNavigation = null;

            ICollection<LocationResponse> location = _locationService.CreateLocation(organizationUpdate.Location);

            UpdateItemCategory(organizationUpdate.Category, id);
            UpdateItemLocation(location, id);

            try
            {
                _unitOfWork.GetRepository<Organization>().Update(orgModel);

                _unitOfWork.Commit();

            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<OrganizationResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }

            //create response
            OrganizationResponse organizationResponse;
            organizationResponse = _mapper.Map<OrganizationResponse>(orgModel);

            //store org to Redis
            string cacheKey = id;
            string cacheValue = _distributedCache.GetString(cacheKey);

            if (!string.IsNullOrEmpty(cacheValue))
            {
                _distributedCache.Remove(cacheKey);
            }

            _distributedCache.SetString(cacheKey,
                JsonConvert.SerializeObject(organizationResponse).ToString());

            return new BaseResponse<OrganizationResponse>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = organizationResponse
            };
        }

        public void CreateItemLocation(ICollection<LocationResponse> locations, string orgId)
        {
            foreach (LocationResponse location in locations)
            {
                try
                {
                    ItemLocation item = new ItemLocation
                    {
                        Id = _utilService.Create16DigitString(),
                        OrgId = orgId,
                        LocationId = location.Id,
                    };

                    _unitOfWork.GetRepository<ItemLocation>().Add(item);

                }
                catch (Exception e)
                {
                    _logger.Error(e.Message);

                    throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ItemLocation>
                    {
                        ResultCode = ResultCode.SQL_ERROR_CODE,
                        ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                    });
                }
            }
        }

        public void UpdateItemLocation(ICollection<LocationResponse> locations, string orgId)
        {
            try
            {
                IEnumerable<ItemLocation> itemsModel = _unitOfWork.GetRepository<ItemLocation>()
                    .GetAll()
                    .Where(item => item.OrgId.Equals(orgId));

                foreach (ItemLocation item in itemsModel)
                {
                    if (!locations.Where(location => location.Id == item.LocationId).Any())
                    {
                        _unitOfWork.GetRepository<ItemLocation>().Delete(item);
                        itemsModel = itemsModel.Where(x => x.LocationId != item.LocationId);
                    }
                }

                foreach (LocationResponse location in locations)
                {
                    //if itemsModel is not contains ItemCity with @param cityCode
                    if (_utilService.IsNullOrEmpty<ItemLocation>
                        (itemsModel.Where(x => x.LocationId.Equals(location.Id))))
                    {
                        ItemLocation item = new ItemLocation
                        {
                            Id = _utilService.Create16DigitString(),
                            OrgId = orgId,
                            LocationId = location.Id,
                        };

                        _unitOfWork.GetRepository<ItemLocation>().Add(item);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ItemLocation>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }
        }

        public void CreateItemCategory(ICollection<string> categoryIds, string orgId)
        {
            if (_utilService.IsNullOrEmpty<string>(categoryIds))
            {
                _logger.Warning($"Org with id: {orgId} cannot create without any category");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ItemCategory>
                {
                    ResultCode = ResultCode.EMPTY_CATEGORY_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.EMPTY_CATEGORY_CODE)
                });
            }

            if (categoryIds.Count > 0)
            {
                foreach (string categoryId in categoryIds)
                {
                    try
                    {
                        ItemCategory item = new ItemCategory
                        {
                            Id = _utilService.Create16DigitString(),
                            OrgId = orgId,
                            CategoryId = categoryId,
                        };

                        _unitOfWork.GetRepository<ItemCategory>().Add(item);
                    }
                    catch (Exception e)
                    {
                        _logger.Error("[OrgService.createItemCategory] " + e.Message);

                        throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ItemCategory>
                        {
                            ResultCode = ResultCode.SQL_ERROR_CODE,
                            ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                        });
                    }
                }
            }
        }

        public void UpdateItemCategory(ICollection<string> categoryIds, string orgId)
        {
            if (_utilService.IsNullOrEmpty<string>(categoryIds))
            {
                _logger.Warning($"Org with id: {orgId} cannot update without any category");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ItemCategory>
                {
                    ResultCode = ResultCode.EMPTY_CATEGORY_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.EMPTY_CATEGORY_CODE)
                });
            }

            if (categoryIds.Count > 0)
            {
                try
                {
                    IEnumerable<ItemCategory> itemsModel = _unitOfWork.GetRepository<ItemCategory>()
                        .GetAll()
                        .Where(item => item.OrgId.Equals(orgId));

                    foreach (ItemCategory item in itemsModel)
                    {
                        if (!categoryIds.Contains(item.CategoryId))
                        {
                            _unitOfWork.GetRepository<ItemCategory>().Delete(item);
                            itemsModel = itemsModel.Where(x => x.CategoryId != item.CategoryId);
                        }
                    }

                    foreach (string categoryId in categoryIds)
                    {
                        //if itemsModel is not contains ItemCategory with @param categoryId
                        if (_utilService.IsNullOrEmpty<ItemCategory>
                            (itemsModel.Where(x => x.CategoryId.Equals(categoryId))))
                        {
                            ItemCategory item = new ItemCategory
                            {
                                Id = _utilService.Create16DigitString(),
                                OrgId = orgId,
                                CategoryId = categoryId,
                            };

                            _unitOfWork.GetRepository<ItemCategory>().Add(item);
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e.Message);

                    throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ItemCategory>
                    {
                        ResultCode = ResultCode.SQL_ERROR_CODE,
                        ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                    });
                }
            }
        }

        public BaseResponse<IEnumerable<OrganizationResponse>> GetByCategory(string categoryId)
        {
            IEnumerable<Organization> orgs = null;
            try
            {
                using (var context = new WeSpreadCoreContext())
                {
                    orgs = context.Organizations.FromSqlRaw(
                        "SELECT * FROM dbo.Organization WHERE Id IN " +
                        "( SELECT Organization.Id FROM dbo.Organization INNER JOIN dbo.ItemCategory ON " +
                        $"ItemCategory.OrgId = Organization.Id AND CategoryId = '{categoryId}')")
                        .ToList();
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<OrganizationResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }

            List<OrganizationResponse> orgResponses = _mapper.Map
                <List<OrganizationResponse>>(orgs);

            if (!orgResponses.Any())
            {
                return new BaseResponse<IEnumerable<OrganizationResponse>>
                {
                    ResultCode = ResultCode.ORG_NOT_FOUND_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.ORG_NOT_FOUND_CODE),
                    Data = default
                };
            }

            foreach(OrganizationResponse response in orgResponses)
            {
                response.Location = _locationService.GetLocationByOrgId(response.Id);
            }

            orgResponses.Sort((x, y) => x.CreatedAt.CompareTo(y.CreatedAt));

            return new BaseResponse<IEnumerable<OrganizationResponse>>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = orgResponses
            };
        }

        public BaseResponse<IEnumerable<OrganizationResponse>> GetByLocation(string locationId)
        {
            IEnumerable<Organization> orgs = null;
            try
            {
                using (var context = new WeSpreadCoreContext())
                {
                    orgs = context.Organizations.FromSqlRaw(
                        "SELECT * FROM dbo.Organization WHERE Id IN " +
                        "( SELECT Organization.Id FROM dbo.Organization INNER JOIN dbo.ItemLocation ON " +
                        $"ItemLocation.OrgId = Organization.Id AND ItemLocation.LocationId = '{locationId}')")
                        .ToList();
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<OrganizationResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }

            List<OrganizationResponse> orgResponses = _mapper.Map
                <List<OrganizationResponse>>(orgs);

            if (!orgResponses.Any())
            {
                return new BaseResponse<IEnumerable<OrganizationResponse>>
                {
                    ResultCode = ResultCode.ORG_NOT_FOUND_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.ORG_NOT_FOUND_CODE),
                    Data = default
                };
            }

            foreach (OrganizationResponse response in orgResponses)
            {
                response.Location = _locationService.GetLocationByOrgId(response.Id);
            }

            orgResponses.Sort((x, y) => x.CreatedAt.CompareTo(y.CreatedAt));


            return new BaseResponse<IEnumerable<OrganizationResponse>>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = orgResponses
            };
        }

        public BaseResponse<OrganizationResponse> DeleteById(string id, string token)
        {
            //validate token
            string userId = _persistentLoginService.GetUserIDByToken(token);
            if (String.IsNullOrEmpty(userId))
            {
                _logger.Error($"User with token {token} is unauthorized.");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<OrganizationResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE)
                });
            }

            //valid role user
            if (!CheckUserRoleByOrgId(userId, id, ORG_ADMIN))
            {
                _logger.Error($"User {userId} does not have permission to delete org {id}");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<OrganizationResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE)
                });
            }

            //validate Org Id
            Organization orgModel;
            try
            {
                orgModel = _unitOfWork.GetRepository<Organization>().Get(id);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<OrganizationResponse>
                {
                    ResultCode = ResultCode.ORG_NOT_FOUND_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.ORG_NOT_FOUND_CODE)
                });
            }

            //delete org
            if (!orgModel.IsActive && orgModel.CreationCode == ResultCode.UNVERIFIED_CREATE_ORGANIZATION_CODE)
            {
                orgModel.CreationCode = ResultCode.DELETED_ORGANIZATION_CODE;
                orgModel.CreationMessage = ResultCode.GetMessage(ResultCode.DELETED_ORGANIZATION_CODE);
            }
            else
            {
                orgModel.IsActive = false;
                orgModel.Checker = null;
                orgModel.CheckerNavigation = null;
                orgModel.UpdatedAt = DateTime.Now;
                orgModel.CreationCode = ResultCode.UNVERIFIED_DELETE_ORGANIZATION_CODE;
                orgModel.CreationMessage = ResultCode.GetMessage(ResultCode.UNVERIFIED_DELETE_ORGANIZATION_CODE);
            }

            try
            {
                _unitOfWork.GetRepository<Organization>().Update(orgModel);

                _unitOfWork.Commit();
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<OrganizationResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }

            //create response
            OrganizationResponse organizationResponse = _mapper.Map<OrganizationResponse>(orgModel);

            //store org to Redis
            string cacheKey = id;
            string cacheValue = _distributedCache.GetString(cacheKey);

            if (!string.IsNullOrEmpty(cacheValue))
            {
                _distributedCache.Remove(cacheKey);
            }

            _distributedCache.SetString(cacheKey,
                JsonConvert.SerializeObject(organizationResponse).ToString());

            return new BaseResponse<OrganizationResponse>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = organizationResponse
            };
        }

        public void SetRoleOfOrg(string userId, string orgId, string roleId)
        {
            UserOrganization userOrganization = new UserOrganization
            {
                Id = _utilService.Create16DigitString(),
                OrganizationId = orgId,
                UserId = userId,
                RoleId = roleId
            };
            try
            {
                _unitOfWork.GetRepository<UserOrganization>().Add(userOrganization);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message + $"\nCannot set role {roleId} in Org {orgId} for User {userId}");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<UserOrganization>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }

            List<UserOrganization> userOrganizations = new List<UserOrganization>();
            userOrganizations.Add(userOrganization);

            string cacheKey = userId + "_ROLE";

            _distributedCache.SetString(cacheKey,
                JsonConvert.SerializeObject(userOrganizations).ToString());
        }

        public bool CheckUserRoleByOrgId(string userId, string orgId, string roleId)
        {
            List<UserOrganization> userOrganizations;

            string cacheKey = userId + "_ROLE";
            string cacheValue = _distributedCache.GetString(cacheKey);

            if (!string.IsNullOrEmpty(cacheValue))
            {
                userOrganizations = JsonConvert.DeserializeObject<List<UserOrganization>>(cacheValue);

                if (userOrganizations.Any(uo => uo.OrganizationId.Equals(orgId) && uo.RoleId.Equals(roleId)))
                    return true;
            }
            try
            {
                using (var context = new WeSpreadCoreContext())
                {
                    List<UserOrganization> users = context.UserOrganizations.Where(uo =>
                    uo.OrganizationId.Equals(orgId) && uo.RoleId.Equals(roleId)).ToList();

                    if (!_utilService.IsNullOrEmpty(users))
                    {
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<UserOrganization>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }
            return false;
        }

        public OrganizationResponse GetByIdAndUser(string userId, string orgId, string roleId)
        {
            try
            {
                using (var context = new WeSpreadCoreContext())
                {
                    //Organization org = context.Organizations.FromSqlRaw(
                    //    "IF EXISTS (SELECT * FROM UserOrganization " +
                    //    $"WHERE UserId = '{userId}' AND OrganizationId = '{orgId}' AND RoleId = '{roleId}') " +
                    //    $"SELECT * FROM Organization WHERE Id = '{orgId}'").ToList().First();

                    var orgs = from uo in context.UserOrganizations
                               where (uo.UserId == userId && uo.OrganizationId == orgId && uo.RoleId == roleId)
                               join o in context.Organizations
                               on uo.OrganizationId equals o.Id
                               select new
                               {
                                   o.Id,
                                   o.Name,
                                   o.EngName,
                                   o.Logo,
                                   o.Cover,
                                   o.Type,
                                   o.Description,
                                   o.Mission,
                                   o.Vision,
                                   o.Achievement,
                                   o.FoundingDate,
                                   o.IsActive,
                                   o.CreatedBy,
                                   o.CreatedAt,
                                   o.UpdatedAt,
                                   o.Config,
                                   o.CreationCode,
                                   o.CreationMessage,
                                   o.TaxCode
                               };

                    foreach (var org in orgs)
                    {
                        return new OrganizationResponse
                        {
                            Id = org.Id,
                            Name = org.Name,
                            EngName = org.EngName,
                            Logo = org.Logo,
                            Cover = org.Cover,
                            Type = org.Type,
                            Description = org.Description,
                            Mission = org.Mission,
                            Vision = org.Vision,
                            Achievement = org.Achievement,
                            IsActive = org.IsActive,
                            FoundingDate = org.FoundingDate,
                            Config = org.Config,
                            CreationCode = org.CreationCode,
                            CreationMessage = org.CreationMessage,
                            TaxCode = org.TaxCode
                        };
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<UserOrganization>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }
            return null;
        }
    }
}
