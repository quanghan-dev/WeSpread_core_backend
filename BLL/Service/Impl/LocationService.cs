using AutoMapper;
using BLL.Constant;
using BLL.Dto;
using BLL.Dto.Exception;
using BLL.Dto.Location;
using DAL.Model;
using DAL.UnifOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace BLL.Service.Impl
{
    public class LocationService : ILocationService
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUtilService _utilService;

        public LocationService(ILogger logger, IUnitOfWork unitOfWork, IMapper mapper,
            IUtilService utilService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _utilService = utilService;
        }

        public bool CheckLocationExist(string locationName)
        {
            try
            {
                List<Location> locationModels = _unitOfWork.GetRepository<Location>().GetAll().ToList();
                if(locationModels.Any(x => x.Name == locationName))
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.Error("[CheckLocationExist]: " + e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<LocationResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }
            return false;
        }

        public ICollection<LocationResponse> CreateLocation(ICollection<LocationRequest> locationRequest)
        {
            //check empty
            if (_utilService.IsNullOrEmpty(locationRequest))
            {
                _logger.Warning($"No location was sent to server.");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ItemLocation>
                {
                    ResultCode = ResultCode.EMPTY_LOCATION_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.EMPTY_LOCATION_CODE)
                });
            }

            //check exist
            foreach(LocationRequest location in locationRequest)
            {
                if (CheckLocationExist(location.Name))
                {
                    locationRequest.Remove(location);
                }
            }

            List<Location> locations = _mapper.Map<List<Location>>(locationRequest);

            //store to db and check exist
            try
            {
                List<Location> locationModels = _unitOfWork.GetRepository<Location>().GetAll().ToList();

                foreach (Location location in locations)
                {
                    if(locationModels.Any(x => x.Name == location.Name))
                    {
                        throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ItemLocation>
                        {
                            ResultCode = ResultCode.LOCATION_EXISTS_CODE,
                            ResultMessage = ResultCode.GetMessage(ResultCode.LOCATION_EXISTS_CODE)
                        });
                    }

                    location.Id = _utilService.Create16Alphanumeric();

                    _unitOfWork.GetRepository<Location>().Add(location);
                }
            }
            catch (Exception e)
            {
                _logger.Error("[CreateLocation]: " + e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<LocationResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }

            return _mapper.Map<ICollection<LocationResponse>>(locations);
        }

        public ICollection<LocationResponse> GetLocationByOrgId(string orgId)
        {
            ICollection<LocationResponse> responses = new List<LocationResponse>();

            try
            {
                using(var context = new WeSpreadCoreContext())
                {
                    var locations = from l in context.Locations
                                   join il in context.ItemLocations
                                   on l.Id equals il.LocationId
                                   where il.OrgId == orgId
                                   select new { 
                                       Id = l.Id,
                                       Name = l.Name,
                                       Address = l.Address,
                                       Latitude = l.Latitude,
                                       Longtitude = l.Longitude
                                   };

                    foreach(var location in locations)
                    {
                        responses.Add(new LocationResponse
                        {
                            Id = location.Id,
                            Address = location.Address,
                            Name = location.Name,
                            Latitude = location.Latitude,
                            Longitude = location.Longtitude
                        });
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error("[GetLocationByOrgId]: " + e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<LocationResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }

            return responses;
        }

        public ICollection<LocationResponse> GetLocationByProjectId(string projectId)
        {
            ICollection<LocationResponse> responses = new List<LocationResponse>();

            try
            {
                using (var context = new WeSpreadCoreContext())
                {
                    var locations = from l in context.Locations
                                    join il in context.ItemLocations
                                    on l.Id equals il.LocationId
                                    where il.ProjectId == projectId
                                    select new
                                    {
                                        Id = l.Id,
                                        Name = l.Name,
                                        Address = l.Address,
                                        Latitude = l.Latitude,
                                        Longtitude = l.Longitude
                                    };

                    foreach (var location in locations)
                    {
                        responses.Add(new LocationResponse
                        {
                            Id = location.Id,
                            Address = location.Address,
                            Name = location.Name,
                            Latitude = location.Latitude,
                            Longitude = location.Longtitude
                        });
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error("[GetLocationByOrgId]: " + e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<LocationResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }

            return responses;
        }
    }
}
