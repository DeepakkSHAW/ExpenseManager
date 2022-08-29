// See https://aka.ms/new-console-template for more information
using Microsoft.OData.Edm;
using UserManagement.Lib;
using UserManagement.Lib.Models;

Console.WriteLine("-----------Expense Manager--------------");

var o = new UserService(null, null);
var user = new UserDto{EmailID = "dummy@gmail.com", Name = "Deepak", Surname = "Shaw", DOB = DateOnly.FromDateTime(DateTime.Now), Password = "PassWord@123" , PhoneNumber = "046-001-2002" };

var v = await o.NewUserAsync(user);

