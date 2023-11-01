// LFInteractive LLC. 2021-2024
﻿using Microsoft.AspNetCore.Mvc;
using WebAPITemplate.Data;

namespace WebAPITemplate.Controllers;

[Route("/api")]
[ApiController]
public class ApplicationController : ControllerBase
{
    [HttpGet()]
    public IActionResult Get()
    {
        return Ok(ApplicationData.GenerateApplicationData());
    }
}