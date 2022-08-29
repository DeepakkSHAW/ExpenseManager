//--Delete this file : only used for quick unit testing purpose--//
using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ExpenseManager.API
{
    public class EnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema model, SchemaFilterContext context)
        {
            if (context.Type.IsEnum)
            {
                model.Enum.Clear();
                Enum.GetNames(context.Type)
                    .ToList()
                    .ForEach(n => model.Enum.Add(new OpenApiString(n)));
            }
        }
    }


    public class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        private const string Format = "yyyy-MM-dd";

        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateOnly.ParseExact(reader.GetString()!, Format, CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
        }
    }

    public class Author
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly DOB { get; set; }
    }
    public interface IAuthorRepository
    {
        public List<Author> GetAuthors();
        public Author GetAuthor(int id);
        public string GetConfigurationInfo();
    }
    public class AuthorRepository : IAuthorRepository
    {
        private readonly List<Author> _authors;
        private readonly ILogger<AuthorRepository> _logger;
        private readonly IConfiguration _config;
        public AuthorRepository(ILogger<AuthorRepository> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;

            _authors = new List<Author>
            {
                new Author
                {
                    Id = 1,
                    FirstName = "Deepak",
                    LastName = "Shaw",
                    DOB = new DateOnly(2022, 1, 1)
                },
                new Author
                {
                    Id = 2,
                    FirstName = "Manas",
                    LastName = "Sardar",
                    DOB = new DateOnly(2022, 1, 1)
                },
                new Author
                {
                    Id = 3,
                    FirstName = "Ranjula",
                    LastName = "K",
                    DOB = new DateOnly(2022, 1, 1)
                },
                new Author
                {
                    Id = 4,
                    FirstName = "Simon",
                    LastName = "Chessume",
                    DOB = new DateOnly(2022, 1, 1)
                }
            };
        }
        public string GetConfigurationInfo()
        {
            _logger.LogInformation($"Application Info {_config.GetSection("ApplicationInfo:Application").Value}");
            return _config.GetSection("ApplicationInfo:Application").Value;
        }
        public List<Author> GetAuthors()
        {
            _logger.LogInformation("Log started");
            return _authors;
        }
        public Author GetAuthor(int id)
        {
            _logger.LogInformation("Log started");
            return _authors.Find(x => x.Id == id);
        }
    }

}
