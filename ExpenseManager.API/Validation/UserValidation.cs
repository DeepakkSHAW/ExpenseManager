using UserManagement.Lib.Models;
using FluentValidation;
using System.Text.RegularExpressions;

namespace ExpenseManager.API.Validation
{
    public class UserValidation : AbstractValidator<UserDto>
    {
        protected bool CheckAge(DateOnly date)
        {
            int currentYear = DateTime.Now.Year;
            int dobYear = date.Year;

            //if (dobYear <= currentYear && dobYear > (currentYear - 120))
            if(date <= DateOnly.FromDateTime(DateTime.Now) && dobYear > (currentYear - 120))
            {
                return true;
            }

            return false;
        }
        public UserValidation()
        {
            RuleFor(x => x.EmailID).NotEmpty().EmailAddress();
            RuleFor(x => x.Name).Length(4, 20);
            RuleFor(x => x.Surname).Length(0, 20);
            RuleFor(p => p.Password).NotEmpty().WithMessage("Your password cannot be empty")
                    .MinimumLength(8).WithMessage("Your password length must be at least 8.")
                    .MaximumLength(16).WithMessage("Your password length must not exceed 16.")
                    .Matches(@"[A-Z]+").WithMessage("Your password must contain at least one uppercase letter.")
                    .Matches(@"[a-z]+").WithMessage("Your password must contain at least one lowercase letter.")
                    .Matches(@"[0-9]+").WithMessage("Your password must contain at least one number.")
                    .Matches(@"[\!\?\*\.]+").WithMessage("Your password must contain at least one (!? *.).");

            RuleFor(p => p.PhoneNumber)
               .NotEmpty()
               .NotNull().WithMessage("Phone Number is required.")
               .MinimumLength(10).WithMessage("PhoneNumber must not be less than 10 characters.")
               .MaximumLength(20).WithMessage("PhoneNumber must not exceed 50 characters.")
               .Matches(new Regex(@"((\(\d{3}\) ?)|(\d{3}-))?\d{3}-\d{4}")).WithMessage("PhoneNumber not valid, Valid formate is: 000-000-0000");

            RuleFor(x => x.DOB)
                .Must(CheckAge).WithMessage("Invalid user Date of Birth")
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Now.AddYears(-16))).WithMessage("User must have age at least 16 years.")
                ;
        }
    }
}
