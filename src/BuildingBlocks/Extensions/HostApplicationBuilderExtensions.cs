using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Extensions;

public static partial class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddConfigurationOptions<T>(
        this IHostApplicationBuilder builder,
        string? key = null,
        Action<T>? configurator = null
    )
        where T : class
    {
        var optionBuilder = builder.Services.AddOptions<T>().BindConfiguration(key ?? typeof(T).Name);

        if (configurator is not null)
        {
            optionBuilder = optionBuilder.Configure(configurator);
        }

        builder.Services.AddSingleton(x => x.GetRequiredService<IOptions<T>>().Value);

        return builder;
    }

    public static IHostApplicationBuilder AddValidationOptions<T>(
        this IHostApplicationBuilder builder,
        Action<T>? configurator = null
    )
        where T : class
    {
        var key = typeof(T).Name;

        return AddValidatedOptions(builder, key, RequiredConfigurationValidator.Validate, configurator);
    }

    public static IHostApplicationBuilder AddValidationOptions<T>(
        this IHostApplicationBuilder builder,
        string? key = null,
        Action<T>? configurator = null
    )
        where T : class
    {
        return AddValidatedOptions(
            builder,
            key ?? typeof(T).Name,
            RequiredConfigurationValidator.Validate,
            configurator
        );
    }

    public static IHostApplicationBuilder AddValidatedOptions<T>(
        this IHostApplicationBuilder builder,
        string? key = null,
        Func<T, bool>? validator = null,
        Action<T>? configurator = null
    )
        where T : class
    {
        validator ??= RequiredConfigurationValidator.Validate;

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options
        // https://thecodeblogger.com/2021/04/21/options-pattern-in-net-ioptions-ioptionssnapshot-ioptionsmonitor/
        // https://code-maze.com/aspnet-configuration-options/
        // https://code-maze.com/aspnet-configuration-options-validation/
        // https://dotnetdocs.ir/Post/42/difference-between-ioptions-ioptionssnapshot-and-ioptionsmonitor
        // https://andrewlock.net/adding-validation-to-strongly-typed-configuration-objects-in-dotnet-6/

        var optionBuilder = builder.Services.AddOptions<T>().BindConfiguration(key ?? typeof(T).Name);

        if (configurator is not null)
        {
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/#configure-options-with-a-delegate
            optionBuilder = optionBuilder.Configure(configurator);
        }

        optionBuilder.Validate(validator);

        // IOptions itself registered as singleton
        builder.Services.AddSingleton(x => x.GetRequiredService<IOptions<T>>().Value);

        return builder;
    }

    public static class RequiredConfigurationValidator
    {
        public static bool Validate<T>(T arg)
            where T : class
        {
            var requiredProperties = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => Attribute.IsDefined(x, typeof(RequiredMemberAttribute)));

            foreach (var requiredProperty in requiredProperties)
            {
                var propertyValue = requiredProperty.GetValue(arg);
                if (propertyValue is null)
                {
                    throw new System.Exception($"Required property '{requiredProperty.Name}' was null");
                }
            }

            return true;
        }
    }
}
