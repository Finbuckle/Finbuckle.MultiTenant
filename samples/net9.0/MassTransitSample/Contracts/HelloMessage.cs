using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassTransitSample.Contracts
{
    public record HelloMessage
    {
        public string Text { get; init; }
    }
}
