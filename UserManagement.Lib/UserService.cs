using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using UserManagement.Lib.Models;
using System.Net;

namespace UserManagement.Lib
{
    public class UserService
    {
        private readonly CloudTable _strorageUserTable;
        private readonly TableContinuationToken _token = null;
        private readonly ILogger<UserService> _logger;
        private readonly IConfiguration _config;
        public UserService(ILogger<UserService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;

            string connection = _config.GetSection("AzureStorage:TableConnectionString").Value;
            string table = _config.GetSection("AzureStorage:UserTable").Value;
            //string connection = "DefaultEndpointsProtocol=https;AccountName=storagepocfunction;AccountKey=DoubleTt7LcEs0ZOBHPm/Eb2xqYfMTY6kwxMdlHCRFem7iGLFpJJcBkGp/r6xxjs/YUYqOelCbAcf3onSBYYIwV79A0yQ==;EndpointSuffix=core.windows.net";
            //string table = "TUser";
            if (string.IsNullOrEmpty(connection) || string.IsNullOrEmpty(table)) throw new Exception("connection details was not found in configuration file");

            CloudStorageAccount storageAccount;
            storageAccount = CloudStorageAccount.Parse(connection);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            _strorageUserTable = tableClient.GetTableReference(table);
            //var operationResults = _strorageUserTable.CreateIfNotExists();
            var operationResults = _strorageUserTable.CreateIfNotExistsAsync().Result;
            if (operationResults)
                _logger.LogDebug("Table have been created.");
            else
                _logger.LogDebug("Table already exist");
        }

        private async Task<bool> UserEmailExistsAsync(string email)
        {
            try
            {
                TableQuery<Users> query = new TableQuery<Users>()
                                            .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, email));

                var userResults = await _strorageUserTable.ExecuteQuerySegmentedAsync(query, _token);
                return userResults.Results.Count == 0 ? false : true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred {ex.Message}");
                throw;
            }
        }

        public async Task<(string returnMessage, object returnObject, HttpStatusCode returnCode, bool status)>
            GetUserDetailsAsync(string email)
        {
            var method = MethodBase.GetCurrentMethod()?.Name;
            _logger.LogDebug($"{method}:: Started - {DateTime.UtcNow.ToLocalTime()}");

            Users noUser = new Users();
            var vReturn = ("", noUser, HttpStatusCode.BadRequest, false);
            //var r = System.Net.HttpStatusCode.NotFound;
            try
            {
                //Check whether User already Exist
                TableOperation toFindUser = TableOperation.Retrieve<Users>(UserType.User.ToString(), email);
                TableResult trUsertExist = await _strorageUserTable.ExecuteAsync(toFindUser);
                if (trUsertExist.Result != null)
                    vReturn = ("User found", (Users)trUsertExist.Result, HttpStatusCode.Accepted, true);
                else
                    vReturn = ("User doesn't exist", noUser, HttpStatusCode.BadRequest, false);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred {ex.Message}");
                vReturn = (ex.Message, noUser, HttpStatusCode.InternalServerError, false);
            }

            _logger.LogDebug($"{method}:: Ended - {DateTime.UtcNow.ToLocalTime()}");
            return vReturn;
        }
        
        public async Task<(string returnMessage, object returnObject, HttpStatusCode returnCode, bool status)>
            NewUserAsync(UserDto apiUser)
        {
            var method = MethodBase.GetCurrentMethod()?.Name;

            _logger.LogDebug($"{method}:: Started - {DateTime.UtcNow.ToLocalTime()}");
            Users noUser = new Users();
            var vReturn = ("", noUser, HttpStatusCode.BadRequest, false);
            try
            {
                var userEntity = new Users(UserType.User, apiUser.EmailID) { Name = apiUser.Name, Surname = apiUser.Surname, DOB = apiUser.DOB, Password = apiUser.Password, PhoneNumber = apiUser.PhoneNumber };

                //Check whether User already Exist
                //TableOperation toFindUser = TableOperation.Retrieve<Users>(userEntity.PartitionKey, userEntity.RowKey);
                //TableResult trUsertExist = await _strorageUserTable.ExecuteAsync(toFindUser);
                //if (trUsertExist.Result == null)
                if (!UserEmailExistsAsync(userEntity.RowKey).Result)
                {
                    TableOperation insertOperation = TableOperation.Insert(userEntity);
                    TableResult result = await _strorageUserTable.ExecuteAsync(insertOperation);
                    if (result.Result != null)
                        vReturn = ("User created successfully", (Users)result.Result, HttpStatusCode.Created, true);
                }
                else
                    vReturn = ("User already exist", (Users)userEntity, HttpStatusCode.Conflict, false);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred {ex.Message}");
                vReturn = (ex.Message, noUser, HttpStatusCode.InternalServerError, false);
            }
            _logger.LogDebug($"{method}:: Ended - {DateTime.UtcNow.ToLocalTime()}");
            return vReturn;
        }

        public async Task<(string returnMessage, object returnObject, HttpStatusCode returnCode, bool status)>
            UpdateUserAsync(UserDto apiUser, string email)
        {
            var method = MethodBase.GetCurrentMethod()?.Name;
            _logger.LogDebug($"{method}:: Started - {DateTime.UtcNow.ToLocalTime()}");
            Users noUser = new Users();
            var vReturn = ("", noUser, HttpStatusCode.BadRequest, false);
            try
            {
                //Check whether User already Exist
                TableOperation toFindUser = TableOperation.Retrieve<Users>(UserType.User.ToString(), email);
                TableResult trUsertExist = await _strorageUserTable.ExecuteAsync(toFindUser);
                if (trUsertExist.Result != null)
                {
                    var UsersEntity = new Users { Name = apiUser.Name, Surname = apiUser.Surname, DOB = apiUser.DOB, PhoneNumber = apiUser.PhoneNumber, Password = apiUser.Password };
                    TableOperation insertOrMergeOperation = TableOperation.Merge(UsersEntity);
                    TableResult result = await _strorageUserTable.ExecuteAsync(insertOrMergeOperation);
                    if (result.Result != null)
                        vReturn = ("User updated successfully", (Users)result.Result, HttpStatusCode.Accepted, true);
                }
                else
                    vReturn = ("User doesn't exist", noUser, HttpStatusCode.BadRequest, false);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred {ex.Message}");
                vReturn = (ex.Message, noUser, HttpStatusCode.InternalServerError, false);
            }

            _logger.LogInformation($"{method}:: Ended - {DateTime.UtcNow.ToLocalTime()}");
            return vReturn;
        }

        public async Task<(string returnMessage, object returnObject, HttpStatusCode returnCode, bool status)>
            DeleteUserAsync(string email)
        {
            var method = MethodBase.GetCurrentMethod()?.Name;
            _logger.LogDebug($"{method}:: Started - {DateTime.UtcNow.ToLocalTime()}");

            Users noUser = new Users();
            var vReturn = ("", noUser, HttpStatusCode.BadRequest, false);
            try
            {
                //Check whether User already Exist
                TableOperation toFindUser = TableOperation.Retrieve<Users>(UserType.User.ToString(), email);
                TableResult trUsertExist = await _strorageUserTable.ExecuteAsync(toFindUser);
                if (trUsertExist.Result != null)
                {
                    TableOperation deleteOperation = TableOperation.Delete((Users)trUsertExist.Result);
                    TableResult result = await _strorageUserTable.ExecuteAsync(deleteOperation);
                    if (result.Result != null) vReturn = ("User deleted", (Users)trUsertExist.Result, HttpStatusCode.NoContent, true);
                }
                else
                    vReturn = ("User doesn't exist", noUser, HttpStatusCode.NotFound, false);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred {ex.Message}");
                vReturn = (ex.Message, noUser, HttpStatusCode.InternalServerError, false);
            }

            _logger.LogDebug($"{method}:: Ended - {DateTime.UtcNow.ToLocalTime()}");
            return vReturn;
        }

        
        public async Task<(string returnMessage, object returnObject, int returnCode, bool status)>
            GetUserTypeAsync(string email)
        {
            var method = MethodBase.GetCurrentMethod()?.Name;
            _logger.LogDebug($"{method}:: Started - {DateTime.UtcNow.ToLocalTime()}");

            Users noUser = new Users();
            var vReturn = ("", noUser, 0, false);
            try
            {
                //await Task.Delay(1);
                TableQuery<Users> query = new TableQuery<Users>()
                            .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, email));
                var userResults = await _strorageUserTable.ExecuteQuerySegmentedAsync(query, _token);
                if (userResults.Results == null)
                    vReturn = ("User not found", noUser, 404, true);
                else
                    vReturn = (userResults.Results[0].PartitionKey, userResults.Results[0], 200, true);

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred {ex.Message}");
                vReturn = (ex.Message, noUser, 500, false);
            }
            return vReturn;
        }

        public async Task<(string returnMessage, object returnObject, int returnCode, bool status)>
            UserVerificationAsync(string email, string password)
        {
            var method = MethodBase.GetCurrentMethod()?.Name;
            _logger.LogDebug($"{method}:: Started - {DateTime.UtcNow.ToLocalTime()}");

            Users noUser = new Users();
            var vReturn = ("", noUser, 0, false);
            try
            {
                //await Task.Delay(1);
                TableQuery<Users> query = new TableQuery<Users>()
                                            .Where(TableQuery.CombineFilters(
                                                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, email),
                                                TableOperators.And,
                                                TableQuery.GenerateFilterCondition("Password", QueryComparisons.Equal, password)));

                var userResults = await _strorageUserTable.ExecuteQuerySegmentedAsync(query, _token);
                if (userResults.Results == null)
                    vReturn = ("User not found", noUser, 404, false);
                else
                    vReturn = (userResults.Results[0].RowKey, userResults.Results[0], 200, true);


            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred {ex.Message}");
                vReturn = (ex.Message, noUser, 500, false);
            }
            return vReturn;
        }

    }
}