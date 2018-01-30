using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TurnerStarterApi.Core.Features.Common;
using TurnerStarterApi.Core.Features.Users;
using TurnerStarterApi.Tests.Core.Builders;
using TurnerStarterApi.Tests.Unit.Extensions;

namespace TurnerStarterApi.Tests.Unit.Features.Common
{
    [TestFixture]
    public class DeleteByIdRequestTests : UnitTest
    {
        [Test]
        public async Task NonExistentEntity_ReturnsError()
        {
            // Act
            var result = await Mediator.HandleAsync(new DeleteByIdRequest<User>
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
            var result = await Mediator.HandleAsync(new DeleteByIdRequest<User>
            {
                Id = user.Id
            });

            // Assert
            result.HasError($"User with ID {user.Id} not found.", "Id");
        }

        [Test]
        public async Task FilterApplied_ReturnsErrorWhenNotFound()
        {
            // Arrange
            var user = await UserBuilder.Instance()
                .WithName("Baz", "Qux")
                .PersistAndBuild(DataContext);

            // Act
            var result = await Mediator.HandleAsync(new DeleteByIdRequest<User>
            {
                Id = user.Id,
                Filter = x => x.FirstName != "Baz"
            });

            // Assert
            result.HasError($"User with ID {user.Id} not found.", "Id");
        }

        [Test]
        public async Task ValidObject_SetsDeletedFlags()
        {
            // Arrange
            var user = await UserBuilder.Instance()
                .WithName("Baz", "Qux")
                .PersistAndBuild(DataContext);

            user.CreatedDate = DateTime.UtcNow.AddDays(-2);
            user.ModifiedDate = DateTime.UtcNow.AddDays(-5);
            user.ModifiedByUserId = 5;

            // Act
            await Mediator.HandleAsync(new DeleteByIdRequest<User>
            {
                Id = user.Id
            });

            // Assert
            var entity = await DataContext.Set<User>().FirstAsync(x => x.Id == user.Id);
            Assert.IsTrue(entity.IsDeleted);
            Assert.AreEqual(RequestingUserId, entity.DeletedByUserId);
            Assert.AreEqual(DateTime.UtcNow.Date, entity.DeletedDate.GetValueOrDefault().Date);
            Assert.AreEqual(5, entity.ModifiedByUserId);
            Assert.AreEqual(DateTime.UtcNow.Date.AddDays(-2), entity.CreatedDate.Date);
            Assert.AreEqual(DateTime.UtcNow.Date.AddDays(-5), entity.ModifiedDate.Date);
        }

        [Test]
        public async Task DependentEntities_SetsDeletedFlags()
        {
            // Arrange
            var user = await UserBuilder.Instance()
                .WithName("Baz", "Qux")
                .PersistAndBuild(DataContext);

            user.CreatedDate = DateTime.UtcNow.AddDays(-2);
            user.ModifiedDate = DateTime.UtcNow.AddDays(-5);
            user.ModifiedByUserId = 5;

            // Act
            await Mediator.HandleAsync(new DeleteByIdRequest<User>
            {
                Id = user.Id,
                DependenciesToDelete = new Dictionary<Type, Func<User, int>>
                {
                    {typeof(User), x => x.CreatedByUserId.GetValueOrDefault()}
                }
            });

            // Assert
            var dependency = await DataContext.Set<User>().FirstAsync(x => x.Id == RequestingUserId);
            Assert.IsTrue(dependency.IsDeleted);
            Assert.AreEqual(RequestingUserId, dependency.DeletedByUserId);
            Assert.AreEqual(DateTime.UtcNow.Date, dependency.DeletedDate.GetValueOrDefault().Date);
        }
    }
}
