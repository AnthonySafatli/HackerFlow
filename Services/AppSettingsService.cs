using HackerFlow.Data;
using HackerFlow.Models;
using Microsoft.EntityFrameworkCore;

namespace HackerFlow.Services;

public class AppSettingsService : IAppSettingsService
{
    private readonly IConfiguration _config;
    private readonly HackerFlowContext _context;

    public AppSettingsService(IConfiguration config, HackerFlowContext context)
    {
        _config = config;
        _context = context;
    }

    public async Task<string> GetSetting(string settingName) 
    {
        var setting = await _context.AppSettings.FirstOrDefaultAsync(x => x.Name == settingName);

        if (setting != null)
            return setting.Value;

        string? defaultSetting = _config[$"Settings:{settingName}"] 
            ?? throw new ArgumentException($"Setting {settingName} is not a valid setting!");

        var defaultSettingObj = new AppSetting
        {
            Name = settingName,
            Value = defaultSetting
        };

        _context.Add(defaultSettingObj);
        await _context.SaveChangesAsync();

        return defaultSetting;
    }

    public async Task SetSetting(string settingName, string settingValue)
    {
        var setting = await _context.AppSettings.FirstOrDefaultAsync(x => x.Name == settingName);

        if (setting != null)
            setting.Value = settingValue;
        else
        {
            string? defaultSetting = _config[$"Settings:{settingName}"] 
                ?? throw new ArgumentException($"Setting {settingName} is not a valid setting!");

            setting = new AppSetting
            {
                Name = settingName,
                Value = settingValue
            };

            _context.Add(setting);
        }
        
        await _context.SaveChangesAsync();
    }
}

public interface IAppSettingsService
{
    Task<string> GetSetting(string settingName);
    Task SetSetting(string settingName, string settingValue);
}