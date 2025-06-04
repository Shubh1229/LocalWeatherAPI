using Grpc.Core;
using Grpc.LocalWeatherAPI;
using LocalWeatherAPI.DBService;
using static Grpc.LocalWeatherAPI.LocalWeatherService;

namespace LocalWeatherAPI.GrpcServices
{
    public class LocalWeatherGrpcServiceImpl : LocalWeatherServiceBase
    {
        private readonly ILogger<LocalWeatherGrpcServiceImpl> logger;
        private readonly LocalWeatherDBService dbService;

        public LocalWeatherGrpcServiceImpl(ILogger<LocalWeatherGrpcServiceImpl> logger, LocalWeatherDBService dbService)
        {
            this.logger = logger;
            this.dbService = dbService;
        }
        public async override Task<DataInfo> GetData(DateTimeStamp stamp, ServerCallContext context)
        {
            DateTime dtStamp = DateTime.Parse(stamp.Datetime).ToUniversalTime();
            var result = await dbService.GetData(dtStamp);
            return new DataInfo
            {
                Temp = result.Temp,
                Humidity = result.Humidity,
                Pressure = result.Pressure,
                Stamp = result.Stamp
            };
        }
        public async override Task<DataInfoRange> GetDataRange(DateTimeStampRange range, ServerCallContext context)
        {
            DateTime start = DateTime.Parse(range.Startdatetime).ToUniversalTime();
            DateTime end = DateTime.Parse(range.Enddatetime).ToUniversalTime();
            var result = await dbService.GetDataRange(start, end);
            DataInfoRange data = new DataInfoRange { };
            foreach (var d in result)
            {
                data.Datainfo.Add(new DataInfo
                {
                    Temp = d.Temp,
                    Humidity = d.Humidity,
                    Pressure = d.Pressure,
                    Stamp = d.Stamp
                });
            }
            return data;
        }
    }
}