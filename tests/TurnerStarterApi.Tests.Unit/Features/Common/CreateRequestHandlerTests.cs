using System;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Turner.Infrastructure.Mediator;
using TurnerStarterApi.Core.Features.Common;
using TurnerStarterApi.Core.Features.Users;
using TurnerStarterApi.Tests.Unit.Extensions;

namespace TurnerStarterApi.Tests.Unit.Features.Common
{
    [TestFixture]
    public class CreateRequestHandlerTests : UnitTest
    {
        private static Action<User> _beforeSaveAction;
        private static Action<UserGetDto> _beforeReturnAction;

        [SetUp]
        public void SetUp()
        {
            _beforeSaveAction = user => { };
            _beforeReturnAction = userDto => { };
        }

        [Test]
        public async Task ValidationFailure_ReturnsError()
        {
            // Act
            var result = await Mediator.HandleAsync(new CreateTestRequest
            {
                LastName = "Bar",
                Role = Roles.Admin
            });

            // Assert
            result.HasError("'First Name' should not be empty.", "FirstName");
        }

        [Test]
        public async Task ValidObject_MapsAndAddsNewEntity()
        {
            // Act
            var result = await Mediator.HandleAsync(new CreateTestRequest
            {
                FirstName = "Foo",
                LastName = "Bar",
                Role = Roles.Admin
            });

            // Assert
            result.HasNoErrors();
            var entity = await DataContext.Set<User>().FirstAsync(x => x.Id == result.Data.Id);
            Assert.AreEqual(RequestingUserId, entity.CreatedByUserId);

            Assert.AreEqual("Foo", result.Data.FirstName);
            Assert.AreEqual("Bar", result.Data.LastName);
            Assert.AreEqual(DateTime.UtcNow.Date, result.Data.CreatedDate.Date);
            Assert.AreEqual(DateTime.UtcNow.Date, result.Data.ModifiedDate.Date);
            Assert.AreEqual(RequestingUserId, result.Data.CreatedByUserId);
            Assert.AreEqual(RequestingUserId, result.Data.ModifiedByUserId);
            Assert.IsFalse(result.Data.IsDeleted);
            Assert.IsNull(result.Data.DeletedDate);
            Assert.AreEqual(Roles.Admin, result.Data.Role);
        }

        [Test]
        public async Task BeforeSave_ModifiesObject()
        {
            // Arrange
            _beforeSaveAction = user =>
            {
                user.Role = Roles.User;
            };

            // Act
            var result = await Mediator.HandleAsync(new CreateTestRequest
            {
                FirstName = "Foo",
                LastName = "Bar",
                Role = Roles.Admin
            });

            // Assert
            var entity = await DataContext.Set<User>().FirstAsync(x => x.Id == result.Data.Id);
            Assert.AreEqual(Roles.User, result.Data.Role);
            Assert.AreEqual(Roles.User, entity.Role);
        }

        [Test]
        public async Task BeforeReturnMethod_ModifiesDto()
        {
            // Arrange
            _beforeReturnAction = userDto =>
            {
                userDto.LastName = "Baz";
            };

            // Act
            var result = await Mediator.HandleAsync(new CreateTestRequest
            {
                FirstName = "Foo",
                LastName = "Bar"
            });

            // Assert
            var entity = await DataContext.Set<User>().FirstAsync(x => x.Id == result.Data.Id);
            Assert.AreEqual("Baz", result.Data.LastName);
            Assert.AreEqual("Bar", entity.LastName);
        }

        public class CreateTestRequest : UserDto, IRequest<UserGetDto>
        {
        }

        public class CreateTestRequestHandler : CreateRequestHandler<CreateTestRequest, User, UserGetDto>
        {
            public CreateTestRequestHandler(DbContext dataContext) : base(dataContext)
            {
            }

            protected override Task BeforeSave(User entity)
            {
                _beforeSaveAction(entity);
                return base.BeforeSave(entity);
            }

            protected override Task BeforeReturn(UserGetDto dto)
            {
                _beforeReturnAction(dto);
                return base.BeforeReturn(dto);
            }
        }

        public class CreateTestRequestMapping : Profile
        {
            public CreateTestRequestMapping()
            {
                CreateMap<CreateTestRequest, User>();
            }
        }

        public class CreateTestRequestValidator : AbstractValidator<CreateTestRequest>
        {
            public CreateTestRequestValidator(IValidator<UserDto> baseValidator)
            {
                Include(baseValidator);
            }
        }
    }
}
