using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class ConfigurationController : ControllerBase
{
    private readonly IConfigurationService _configService;

    public ConfigurationController(IConfigurationService configService)
    {
        _configService = configService;
    }

    [HttpGet]
    public async Task<IEnumerable<ConfigurationModel>> Get()
    {
        return await _configService.GetAllConfigurationsAsync();
    }

    [HttpGet("{key}")]
    public async Task<ActionResult<ConfigurationModel>> GetByKey(string key)
    {
        var config = await _configService.GetConfigurationByKeyAsync(key);
        if (config == null)
        {
            return NotFound();
        }
        return config;
    }
}