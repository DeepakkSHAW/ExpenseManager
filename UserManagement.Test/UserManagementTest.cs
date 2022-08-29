namespace UserManagement.Test
{
    public class UserManagementTest
    {
        private readonly Users _userA = new Users(UserType.Admin, "deepak.shaw@gmail.com") { Name = "Deepak", Surname = "Shaw", DOB = DateOnly.FromDateTime(DateTime.Now), Password = "PassWord@123", PhoneNumber = "046-001-2002" };
        private readonly Users _userB = new Users(UserType.Admin, "rupam.shaw@gmail.com") { Name = "Rupam", Surname = "Shaw", DOB = DateOnly.FromDateTime(DateTime.Now.AddYears(-10)), Password = "PassWord@ABC", PhoneNumber = "001-002-3003" };

        private readonly UserDto _apiUserOne = new UserDto() { EmailID = "deepak.shaw@gmail.com", Name = "Deepak", Surname = "Shaw", DOB = DateOnly.FromDateTime(DateTime.Now), Password = "PassWord@123", PhoneNumber = "046-001-2002" };
        private readonly UserDto _apiUserTwo = new UserDto() { EmailID = "rupam.shaw@gmail.com", Name = "Rupam", Surname = "Shaw", DOB = DateOnly.FromDateTime(DateTime.Now.AddYears(-10)), Password = "PassWord@ABC", PhoneNumber = "001-002-3003" };

        public static IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder()
               .AddJsonFile("appsettings.test.json")
                //.AddEnvironmentVariables()
                .Build();
            return config;
        }

        [Fact]
        public async Task NewUser_ShouldAddNewUser()
        {
            var config = InitConfiguration();
            // Arrange
            ILogger<UserService> dummyLogger = new NullLogger<UserService>();
            var userMgmt = new UserService(dummyLogger, config);
            // Act
            var oNewUser = await userMgmt.NewUserAsync(_apiUserOne);
            // Assert
            Assert.True(oNewUser.status);

            //var v = await userMgmt.GetUserDetails("deepak");

            //var name = v.returnMessage;
            //var code = v.returnCode;
            //var o = v.returnObject;
            //var status = v.status;

            //Assert.Equal(10, code);
        }
        [Fact]
        public async Task GetUser_Details()
        {
            var config = InitConfiguration();
            ILogger<UserService> dummyLogger = new NullLogger<UserService>();
            var userMgmt = new UserService(dummyLogger, config);
            var oGetUser = await userMgmt.GetUserDetailsAsync(_apiUserOne.EmailID);
            Assert.True(oGetUser.status);
        }
        [Fact]
        public async Task UpdateUser_ShouldUpdate()
        {
            var config = InitConfiguration();
            ILogger<UserService> dummyLogger = new NullLogger<UserService>();
            var userMgmt = new UserService(dummyLogger, config);
            _userB.PartitionKey = _userA.PartitionKey;
            _userB.RowKey = _userA.RowKey;
            _userB.ETag = "*";
            var oGetUser = await userMgmt.UpdateUserAsync(_userB);
            Assert.True(oGetUser.status);
        }
        [Fact]
        public async Task DeleteUser_ShouldDelete()
        {
            var config = InitConfiguration();
            ILogger<UserService> dummyLogger = new NullLogger<UserService>();
            var userMgmt = new UserService(dummyLogger, config);

            var isEnumParsed = Enum.TryParse(_userA.PartitionKey, true, out UserType parsedEnumValue);
            Console.WriteLine(isEnumParsed ? parsedEnumValue.ToString() : "Not Parsed");

            var oGetUser = await userMgmt.DeleteUserAsync((UserType)parsedEnumValue, _userA.RowKey);
            Assert.True(oGetUser.status);
        }
        [Fact]
        public async Task Admin_ShouldAdminUser()
        {
            var config = InitConfiguration();
            ILogger<UserService> dummyLogger = new NullLogger<UserService>();
            var userMgmt = new UserService(dummyLogger, config);
            var vResult = await userMgmt.GetUserTypeAsync(_userA.RowKey);
            Assert.Equal("Admin", vResult.returnMessage);

            var uservalidation = await userMgmt.UserVerificationAsync(_userA.RowKey, "PassWord@ABC");
            Assert.True(uservalidation.status);
        }
    }
}