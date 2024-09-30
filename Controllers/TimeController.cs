namespace Playground.Controllers;

using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Playground.Contracts;
using Playground.Models;

[ApiController]
[Route("[controller]")]
public class TimeController : ControllerBase
{
    private readonly IBus bus;

    public TimeController(IBus bus)
    {
        this.bus = bus;
    }

    [HttpGet]
    public async Task<TimeModel> Get()
    {
        return new TimeModel()
        {
            Time = (await bus.Request<TimeContract, TimeContract>(new TimeContract())).Message.Time,
        };
    }
}