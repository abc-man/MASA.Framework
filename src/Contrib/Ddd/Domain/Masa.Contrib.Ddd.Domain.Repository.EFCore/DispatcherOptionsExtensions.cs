// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace Masa.Contrib.Ddd.Domain.Repository.EFCore;

public static class DispatcherOptionsExtensions
{
    public static IDispatcherOptions UseRepository<TDbContext>(
        this IDispatcherOptions options)
        where TDbContext : DbContext, IMasaDbContext
    {
        if (options.Services == null)
            throw new ArgumentNullException(nameof(options.Services));

        if (options.Services.Any(service => service.ImplementationType == typeof(RepositoryProvider)))
            return options;

        options.Services.AddSingleton<RepositoryProvider>();

        if (options.Services.All(service => service.ServiceType != typeof(IUnitOfWork)))
            throw new Exception("Please add UoW first.");

        options.Services.TryAddRepository<TDbContext>(options.Assemblies);
        MasaApp.TrySetServiceCollection(options.Services);
        return options;
    }

    private sealed class RepositoryProvider
    {

    }
}
