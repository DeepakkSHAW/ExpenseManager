using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using UserManagement.Lib.Helper;

namespace UserManagement.Lib.Models
{
    public enum UserType { Admin, User, Guest }

    public class Users : TableEntity
    {
        public Users() { }
        public Users(UserType userType, string userEmail)
        {
            PartitionKey = userType.ToString();
            RowKey = userEmail;
        }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Password { get; set; }

        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly DOB { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; } = false;
    }


}
