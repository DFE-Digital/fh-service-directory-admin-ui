﻿using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Areas.MyAccount.Pages;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.UnitTests.Areas.MyAccount
{
    public class ChangeNameTests
    {
        private readonly Mock<IIdamClient> _mockIdamClient;
        
        public ChangeNameTests()
        {
            _mockIdamClient = new Mock<IIdamClient>();
        }

        [Fact]
        public void OnGet_Valid_SetFullNameFromCache()
        {
            //  Arrange           
            const string expectedName = "test name";

            var claims = new List<Claim> { new(FamilyHubsClaimTypes.FullName, expectedName) };
            var mockHttpContext = TestHelper.GetHttpContext(claims);

            var sut = new ChangeNameModel(_mockIdamClient.Object)
            {
                PageContext = { HttpContext = mockHttpContext.Object }
            };

            //  Act
            sut.OnGet();

            //  Assert
            Assert.Equal(expectedName, sut.FullName);
        }

        [Fact]
        public async Task OnPost_Valid_UpdateAccount()
        {
            //  Arrange
            var claims = new List<Claim> { 
                new(FamilyHubsClaimTypes.FullName, "oldName") ,
                new(FamilyHubsClaimTypes.AccountId, "1")
            };
            var mockHttpContext = TestHelper.GetHttpContext(claims);

            _mockIdamClient.Setup(x => x.UpdateAccountSelfService(It.IsAny<UpdateAccountSelfServiceDto>(), It.IsAny<CancellationToken>()));
            var sut = new ChangeNameModel(_mockIdamClient.Object)
            {
                PageContext = { HttpContext = mockHttpContext.Object },
                FullName = "newName"
            };

            //  Act
            var result = await sut.OnPost(CancellationToken.None);

            //  Assert
            _mockIdamClient.Verify(x=>x.UpdateAccountSelfService(It.IsAny<UpdateAccountSelfServiceDto>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal("ChangeNameConfirmation", ((RedirectToPageResult)result).PageName);
        }

        [Fact]
        public async Task OnPost_NotValid_NoNameHasValidationError()
        {
            //  Arrange
            var claims = new List<Claim> {
                new(FamilyHubsClaimTypes.FullName, "oldName") ,
                new(FamilyHubsClaimTypes.AccountId, "1")
            };
            var mockHttpContext = TestHelper.GetHttpContext(claims);

            _mockIdamClient.Setup(x => x.UpdateAccount(It.IsAny<UpdateAccountDto>(), It.IsAny<CancellationToken>()));
            var sut = new ChangeNameModel(_mockIdamClient.Object)
            {
                PageContext = { HttpContext = mockHttpContext.Object },                
            };

            //  Act
            var result = await sut.OnPost(CancellationToken.None);

            //  Assert
            Assert.True(sut.Errors.HasErrors);            
        }
    }
}
