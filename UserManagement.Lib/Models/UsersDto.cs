using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using UserManagement.Lib.Helper;

namespace UserManagement.Lib.Models
{
    public class UserDto
    {
        public UserDto() { }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string EmailID { get; set; }
        public string PhoneNumber { get; set; }

        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly DOB { get; set; }
        public string Password { get; set; }
    }
}
