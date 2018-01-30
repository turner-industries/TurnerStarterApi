using System;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurnerStarterApi.Core.Data;

namespace TurnerStarterApi.Core.Features.Users
{
    public class User : UserGetDto
    {
        public User CreatedByUser { get; set; }

        public User ModifiedByUser { get; set; }

        public User DeletedByUser { get; set; }
    }

    public class UserGetDto : UserDto, IEntity
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public int? CreatedByUserId { get; set; }

        public int? ModifiedByUserId { get; set; }

        public int? DeletedByUserId { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ModifiedDate { get; set; }

        public DateTime? DeletedDate { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }

    public class UserDto
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Role { get; set; }
    }

    public static class Roles
    {
        public const string User = nameof(User);

        public const string Admin = nameof(Admin);
    }

    public class UserDtoValidator : AbstractValidator<UserDto>
    {
        public UserDtoValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .Length(0, 100);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .Length(0, 100);
        }
    }

    public class UserEntityConfiguration : EntityTypeConfiguration<User>
    {
        public override void Map(EntityTypeBuilder<User> builder)
        {
            builder.HasOne(x => x.CreatedByUser);
            builder.HasOne(x => x.ModifiedByUser);
            builder.HasOne(x => x.DeletedByUser);
        }
    }

    public class UserMapping : Profile
    {
        public UserMapping()
        {
            CreateMap<User, UserGetDto>();
        }
    }
}
