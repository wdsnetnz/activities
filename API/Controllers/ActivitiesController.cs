using System;
using Application.Features.Commands;
using Application.Features.Queries;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace API.Controllers;

public class ActivitiesController : BaseApiController
{
     
    [HttpGet]
    public async Task<ActionResult<List<Activity>>> GetActivities()
    {
        var activities = await Mediator.Send(new GetActivityList.Query());
        return Ok(activities);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Activity>> GetActivity(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest("Id cannot be null or empty");
        }

        return await Mediator.Send(new GetActivityDetails.Query { Id = id });       
    }

    [HttpPost]
    public async Task<ActionResult<string>> CreateActivity(Activity activity)
    {
        if (activity == null)
        {
            return BadRequest("Activity cannot be null");
        }

        var command = new CreateActivity.Command
        {
            Activity = activity
        };

        return await Mediator.Send(command);       
    }

    [HttpPut]
    public async Task<IActionResult> EditActivity(Activity activity)
    {
        if (activity == null)
        {
            return BadRequest("Activity cannot be null");
        }

        var command = new EditActivity.Command
        {
            Activity = activity
        };

        await Mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteActivity(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest("Id cannot be null or empty");
        }

        var command = new DeleteActivity.Command
        {
            Id = id
        };

        await Mediator.Send(command);
        return Ok();
    }
}
