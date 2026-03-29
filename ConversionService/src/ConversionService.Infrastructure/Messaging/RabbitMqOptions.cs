using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionService.Infrastructure.Messaging
{
    public sealed class RabbitMqOptions
    {
        public const string SectionName = "RabbitMq";

        public string Host { get; set; } = "localhost";

        public int Port { get; set; } = 5672;

        public string Username { get; set; } = "guest";

        public string Password { get; set; } = "guest";

        public string VirtualHost { get; set; } = "/";

        public string QueueName { get; set; } = "report-requests";
    }
}