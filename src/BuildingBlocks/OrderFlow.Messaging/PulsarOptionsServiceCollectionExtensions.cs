using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OrderFlow.Messaging;

public static class PulsarOptionsServiceCollectionExtensions
{
    public static IServiceCollection AddPulsarOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<PulsarOptions>()
            .Bind(
                configuration.GetSection(
                    PulsarOptions.SectionName))
            .Validate(
                options => Uri.TryCreate(
                    options.ServiceUrl,
                    UriKind.Absolute,
                    out _),
                "Pulsar ServiceUrl is missing or invalid.")
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.Topic),
                "Pulsar Topic is required.")
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.SubscriptionName),
                "Pulsar SubscriptionName is required.")
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.DeadLetterTopic),
                "Pulsar DeadLetterTopic is required.")
            .Validate(
                options =>
                    options.MaxDeliveryAttempts >= 1,
                "Pulsar MaxDeliveryAttempts must be at least 1.")
            .Validate(
                options =>
                    options.RedeliveryDelaySeconds >= 0,
                "Pulsar RedeliveryDelaySeconds cannot be negative.")
            .ValidateOnStart();

        return services;
    }
}
