using System.Threading.Tasks;
using NUnit.Framework;
using TurnerStarterApi.Core.Features.Common;
using TurnerStarterApi.Core.Features.Users;
using TurnerStarterApi.Tests.Core.Builders;
using TurnerStarterApi.Tests.Unit.Extensions;

namespace TurnerStarterApi.Tests.Unit.Features.Common
{
    [TestFixture]
    public class GetByIdRequestTests : UnitTest
    {
        [Test]
        public async Task NonExistentEntity_ReturnsError()
        {
            // Act
            var result = await Mediator.HandleAsync(new GetByIdRequest<User, UserGetDto>
            {
                Id = 0
            });

            // Assert
            result.HasError("User with ID 0 not found.", "Id");
        }

        [Test]
        public async Task DeletedEntity_ReturnsError()
        {
            // Arrange
            var user = await UserBuilder.Instance()
                .WithName("Baz", "Qux")
                .AsDeleted()
                .PersistAndBuild(DataContext);

            // Act
            var result = await Mediator.HandleAsync(new GetByIdRequest<User, UserGetDto>
            {
                Id = user.Id
            });

            // Assert
            result.HasError($"User with ID {user.Id} not found.", "Id");
        }

        [Test]
        public async Task FilteredOutEntity_ReturnsError()
        {
            // Arrange
            var user = await UserBuilder.Instance()
                .WithName("Baz", "Qux")
                .PersistAndBuild(DataContext);

            // Act
            var result = await Mediator.HandleAsync(new GetByIdRequest<User, UserGetDto>
            {
                Id = user.Id,
                Filter = x => x.FirstName != "Baz"
            });

            // Assert
            result.HasError($"User with ID {user.Id} not found.", "Id");
        }

        [Test]
        public async Task ValidRequest_ReturnsObject()
        {
            // Arrange
            var user = await UserBuilder.Instance()
                .WithName("Baz", "Qux")
                .PersistAndBuild(DataContext);

            // Act
            var result = await Mediator.HandleAsync(new GetByIdRequest<User, UserGetDto>
            {
                Id = user.Id
            });

            // Assert
            result.HasNoErrors();
            Assert.AreEqual("Baz Qux", result.Data.FullName);
        }
    }
}
