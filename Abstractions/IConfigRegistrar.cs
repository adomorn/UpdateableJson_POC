using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Abstractions
{
    public interface IConfigRegistrar
    {
        void Register(WebApplicationBuilder collection);
        string[] Configure();
    }
}