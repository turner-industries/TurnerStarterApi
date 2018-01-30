using System;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Turner.Infrastructure.Mediator;
using TurnerStarterApi.Core.Features.Common;
using TurnerStarterApi.Core.Features.Users;
using TurnerStarterApi.Tests.Core.Builders;
using TurnerStarterApi.Tests.Unit.Extensions;

namespace TurnerStarterApi.Tests.Unit.Features.Common
{
    [TestFixture]
    public class EditRequestHandlerTests : UnitTest
    {
        private static Action<User> _beforeSaveAction;

        [SetUp]
        public void SetUp()
        {
            _beforeSaveAction = user => { };
        }

        [Test]
        public async Task ValidationFailure_ReturnsError()
        {
            // Act
            var result = await Mediator.HandleAsync(new EditTestRequest
            {
                LastName = "Bar",
                Role = Roles.Admin
            });

            // Assert
            result.HasError("'First Name' should not be empty.", "FirstName");
        }

        [Test]
        public async Task NonExistentEntity_ReturnsError()
        {
            // Act
            var result = await Mediator.HandleAsync(new EditTestRequest
            {
                FirstName = "Foo",
                LastName = "Bar",
                Role = Roles.Admin
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
            var result = await Mediator.HandleAsync(new EditTestRequest
            {
                Id = user.Id,
                FirstName = "Foo",
                LastName = "Bar",
                Role = Roles.Admin
            });

            // Assert
            result.HasError($"User with ID {user.Id} not found.", "Id");
        }

        [Test]
        public async Task ValidObject_MapsAndUpdatesEntity()
        {
            // Arrange
            var user = await UserBuilder.Instance()
                .WithName("Baz", "Qux")
                .PersistAndBuild(DataContext);

            user.CreatedDate = DateTime.UtcNow.AddDays(-2);
            user.ModifiedDate = DateTime.UtcNow.AddDays(-5);
            user.ModifiedByUserId = 5;

            // Act
            var result = await Mediator.HandleAsync(new EditTestRequest
            {
                Id = user.Id,
                FirstName = "Foo",
                LastName = "Bar",
                Role = Roles.Admin
            });

            // Assert
            var entity = await DataContext.Set<User>().FirstAsync(x => x.Id == user.Id);
            Assert.AreEqual(RequestingUserId, entity.ModifiedByUserId);

            Assert.AreEqual("Foo", result.Data.FirstName);
            Assert.AreEqual("Bar", result.Data.LastName);
            Assert.AreEqual(DateTime.UtcNow.Date.AddDays(-2), result.Data.CreatedDate.Date);
            Assert.AreEqual(DateTime.UtcNow.Date, result.Data.ModifiedDate.Date);
            Assert.AreEqual(RequestingUserId, result.Data.CreatedByUserId);
            Assert.AreEqual(RequestingUserId, result.Data.ModifiedByUserId);
            Assert.IsFalse(result.Data.IsDeleted);
            Assert.IsNull(result.Data.DeletedDate);
            Assert.AreEqual(Roles.Admin, result.Data.Role);
        }

        [Test]
        public async Task BeforeSaveMethod_ModifiesObject()
        {
            // Arrange
            var user = await UserBuilder.Instance()
                .WithName("Baz", "Qux")
                .PersistAndBuild(DataContext);

            _beforeSaveAction = item =>
            {
                item.Role = Roles.User;
            };

            // Act
            var result = await Mediator.HandleAsync(new EditTestRequest
            {
                Id = user.Id,
                FirstName = "Foo",
                LastName = "Bar",
                Role = Roles.Admin
            });

            // Assert
            var entity = await DataContext.Set<User>().FirstAsync(x => x.Id == result.Data.Id);
            Assert.AreEqual(Roles.User, result.Data.Role);
            Assert.AreEqual(Roles.User, entity.Role);
        }

        public class EditTestRequest : UserDto, IRequest<UserGetDto>, IEditRequest
        {
            public int Id { get; set; }
        }

        public class EditTestRequestHandler : EditRequestHandler<EditTestRequest, User, UserGetDto>
        {
            public EditTestRequestHandler(DbContext dataContext) : base(dataContext)
            {
            }

            protected override void BeforeSave(EditTestRequest request, User entity)
            {
                _beforeSaveAction(entity);
                base.BeforeSave(request, entity);
            }
        }

        public class EditTestRequestMapping : Profile
        {
            public EditTestRequestMapping()
            {
                CreateMap<EditTestRequest, User>();
            }
        }

        public class EditTestRequestValidator : AbstractValidator<EditTestRequest>
        {
            public EditTestRequestValidator(IValidator<UserDto> baseValidator)
            {
                Include(baseValidator);
            }
        }
    }
}
