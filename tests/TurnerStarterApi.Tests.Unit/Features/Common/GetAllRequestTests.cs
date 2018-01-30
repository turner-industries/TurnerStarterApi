using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using TurnerStarterApi.Core.Features.Common;
using TurnerStarterApi.Core.Features.Users;
using TurnerStarterApi.Tests.Core.Builders;
using TurnerStarterApi.Tests.Unit.Extensions;

namespace TurnerStarterApi.Tests.Unit.Features.Common
{
    [TestFixture]
    public class GetAllRequestTests : UnitTest
    {
        [Test]
        public async Task ReturnsAllActiveNonFilteredEntities()
        {
            // Arrange
            await UserBuilder.Instance()
                .WithName("Baz", "Qux")
                .PersistAndBuild(DataContext);

            // Act
            var result = await Mediator.HandleAsync(new GetAllRequest<User, UserGetDto>());

            // Assert
            result.HasNoErrors();
            Assert.AreEqual(2, result.Data.Count);
            Assert.Contains("Baz Qux", result.Data.Select(x => x.FullName).ToList());
        }

        [Test]
        public async Task DeletedEntity_NotIncludedInResults()
        {
            // Arrange
            await UserBuilder.Instance()
                .WithName("Baz", "Qux")
                .AsDeleted()
                .PersistAndBuild(DataContext);

            await UserBuilder.Instance()
                .WithName("Foo", "Bar")
                .PersistAndBuild(DataContext);

            // Act
            var result = await Mediator.HandleAsync(new GetAllRequest<User, UserGetDto>());

            // Assert
            result.HasNoErrors();
            Assert.AreEqual(2, result.Data.Count);
            Assert.IsTrue(result.Data.Select(x => x.FullName).Contains("Foo Bar"));
            Assert.IsFalse(result.Data.Select(x => x.FullName).Contains("Baz Qux"));
        }

        [Test]
        public async Task FilteredOutEntity_NotIncludedInResults()
        {
            // Arrange
            await UserBuilder.Instance()
                .WithName("Baz", "Qux")
                .AsDeleted()
                .PersistAndBuild(DataContext);

            await UserBuilder.Instance()
                .WithName("Foo", "Bar")
                .PersistAndBuild(DataContext);

            // Act
            var result = await Mediator.HandleAsync(new GetAllRequest<User, UserGetDto>
            {
                Filter = x => x.FirstName != "Baz"
            });

            // Assert
            result.HasNoErrors();
            Assert.AreEqual(2, result.Data.Count);
            Assert.IsTrue(result.Data.Select(x => x.FullName).Contains("Foo Bar"));
            Assert.IsFalse(result.Data.Select(x => x.FullName).Contains("Baz Qux"));
        }

        [Test]
        public async Task SortEntitiesAscending_ReturnsAscendingList()
        {
            // Arrange
            await UserBuilder.Instance()
                .WithName("Foo", "Bar")
                .PersistAndBuild(DataContext);

            await UserBuilder.Instance()
                .WithName("Baz", "Qux")
                .PersistAndBuild(DataContext);

            // Act
            var result = await Mediator.HandleAsync(new GetAllRequest<User, UserGetDto>
            {
                SortBy = x => x.FullName
            });

            // Assert
            result.HasNoErrors();
            Assert.AreEqual(3, result.Data.Count);
            Assert.That(result.Data[0].FullName == "Baz Qux");
            Assert.That(result.Data[1].FullName == "Foo Bar");
            Assert.That(result.Data[2].FullName == "Test Admin");
        }

        [Test]
        public async Task SortEntitiesDescending_ReturnsDescendingList()
        {
            // Arrange
            await UserBuilder.Instance()
                .WithName("Foo", "Bar")
                .PersistAndBuild(DataContext);

            await UserBuilder.Instance()
                .WithName("Baz", "Qux")
                .PersistAndBuild(DataContext);

            // Act
            var result = await Mediator.HandleAsync(new GetAllRequest<User, UserGetDto>
            {
                SortByDescending = x => x.FullName
            });

            // Assert
            result.HasNoErrors();
            Assert.AreEqual(3, result.Data.Count);
            Assert.That(result.Data[2].FullName == "Baz Qux");
            Assert.That(result.Data[1].FullName == "Foo Bar");
            Assert.That(result.Data[0].FullName == "Test Admin");
        }
    }
}
