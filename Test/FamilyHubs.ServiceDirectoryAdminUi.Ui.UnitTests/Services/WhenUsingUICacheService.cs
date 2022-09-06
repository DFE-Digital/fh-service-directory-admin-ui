using FamilyHubs.ServiceDirectory.Shared.Models.Api;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FluentAssertions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.UnitTests.Services;

public class WhenUsingUICacheService : BaseClientService
{
    [Fact]
    public async Task ThenGetUICacheById()
    {
        //Arrange
        var uiCache = new UICacheDto() { Id = "Id", Value = "Value" };
        var json = JsonConvert.SerializeObject(uiCache);
        var mockClient = GetMockClient(json);
        UICacheService uiCacheService = new(mockClient);

        //Act
        var result = await uiCacheService.GetUICacheById("Id");

        //Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(uiCache);
    }

    [Fact]
    public async Task ThenCreateUICache()
    {
        //Arrange
        var uiCache = new UICacheDto() { Id = "Id", Value = "Value" };
        var mockClient = GetMockClient(uiCache.Id);
        UICacheService uiCacheService = new(mockClient);

        //Act
        var result = await uiCacheService.CreateUICache(uiCache);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(uiCache.Id);
    }

    [Fact]
    public async Task ThenUpdateUICache()
    {
        //Arrange
        var uiCache = new UICacheDto() { Id = "Id", Value = "Value1" };
        var mockClient = GetMockClient(uiCache.Id);
        UICacheService uiCacheService = new(mockClient);

        //Act
        var result = await uiCacheService.UpdateUICache(uiCache);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(uiCache.Id);
    }
}
