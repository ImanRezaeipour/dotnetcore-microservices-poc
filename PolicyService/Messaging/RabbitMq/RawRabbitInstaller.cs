﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using PolicyService.Messaging.RabbitMq.Outbox;
using RawRabbit.Configuration;
using RawRabbit.Configuration.Exchange;
using RawRabbit.DependencyInjection.ServiceCollection;
using RawRabbit.Instantiation;

namespace PolicyService.Messaging.RabbitMq;

public static class RawRabbitInstaller
{
    public static IServiceCollection AddRabbitListeners(this IServiceCollection services)
    {
        var host = "localhost";
        if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true") host = "rabbit";
        services.AddRawRabbit(new RawRabbitOptions
        {
            ClientConfiguration = new RawRabbitConfiguration
            {
                Username = "guest",
                Password = "guest",
                VirtualHost = "/",
                Port = 5672,
                Hostnames = new List<string> { host },
                RequestTimeout = TimeSpan.FromSeconds(10),
                PublishConfirmTimeout = TimeSpan.FromSeconds(1),
                RecoveryInterval = TimeSpan.FromSeconds(1),
                PersistentDeliveryMode = true,
                AutoCloseConnection = true,
                AutomaticRecovery = true,
                TopologyRecovery = true,
                Exchange = new GeneralExchangeConfiguration
                {
                    Durable = true,
                    AutoDelete = false,
                    Type = ExchangeType.Topic
                },
                Queue = new GeneralQueueConfiguration
                {
                    Durable = true,
                    AutoDelete = false,
                    Exclusive = false
                }
            }
        });

        services.AddScoped<IEventPublisher, OutboxEventPublisher>();
        services.AddSingleton<Outbox.Outbox>();
        services.AddHostedService<OutboxSendingService>();
        return services;
    }
}