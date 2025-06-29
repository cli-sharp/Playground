﻿var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost";
});
builder.Services.AddHybridCache();

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