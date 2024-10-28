using Geotab.Checkmate;
using Geotab.Checkmate.ObjectModel;
using Geotab.Checkmate.ObjectModel.Tachograph;
using Nelibur.ObjectMapper;

var builder = WebApplication.CreateBuilder(args);

var api = new API(
    builder.Configuration.GetValue<string>("GeoTabUser"),
    builder.Configuration.GetValue<string>("GeoTabPassword"),
    null,
    builder.Configuration.GetValue<string>("GeoTabDb"));

var drivers = (await api.CallAsync<IEnumerable<Geotab.Checkmate.ObjectModel.User>>(
    "Get",
    typeof(Geotab.Checkmate.ObjectModel.User),
    new
    {
        fromVersion = 0,
    })).
    Select(user => user as Driver).
    Where(driver => string.IsNullOrWhiteSpace(driver?.Keys?.FirstOrDefault()?.SerialNumber) is false);

foreach (var driver in drivers)
{
    var activities = (await api.CallAsync<IEnumerable<TachographDriverActivity>>(
        "Get",
        typeof(TachographDriverActivity),
        new
        {
            search = new TachographDriverActivitySearch()
            {
                UserSearch = new UserSearch(driver.Id),
                Type = "STREAM",
                FromDate = DateTime.UtcNow.AddDays(-14),
                ToDate = DateTime.UtcNow,
                Extrapolate = true,
            },
        })).
        Where(activity => string.IsNullOrWhiteSpace(activity.Activity) is false &&
            activity.Activity != "UNKNOWN").
        Where(activity => activity.Device?.Id is not null);

    foreach (var activity in activities)
    {
        var device = (await api.CallAsync<IEnumerable<Device>>(
            "Get",
            typeof(Device),
            new
            {
                search = new DeviceSearch(activity.Device.Id),
            })).
            First();

        TinyMapper.Bind<Go9, Vehicle>();
        TinyMapper.Bind<Go9B, Vehicle>();
        var vehicle = TinyMapper.Map<Vehicle>(device);

        var vin = vehicle.VehicleIdentificationNumber;
        var timeStamp = activity.DateTime;
        var activityActivity = activity.Activity;
        var slot = activity.Slot;
        var driverCardNumber = driver.Keys.FirstOrDefault()?.SerialNumber;
    }
}

builder.Services.AddDbContextPool<UserContext>(opt =>
    opt.UseNpgsql("Host=localhost; Username=postgres; Password=postgres"));

builder.Services.AddSingleton<ConnectionMultiplexer>(provider => ConnectionMultiplexer.Connect("localhost"));

builder.Services.AddMassTransit(configurator =>
{
    configurator.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost");
        cfg.ConfigureEndpoints(context);
    });

    configurator.AddConsumers(Assembly.GetExecutingAssembly());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();