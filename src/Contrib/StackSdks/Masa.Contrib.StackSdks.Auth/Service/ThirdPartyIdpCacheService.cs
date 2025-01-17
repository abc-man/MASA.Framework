// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace Masa.Contrib.StackSdks.Auth.Service;

public class ThirdPartyIdpCacheService : IThirdPartyIdpCacheService
{
    readonly IMultilevelCacheClient _memoryCacheClient;

    public ThirdPartyIdpCacheService(IMultilevelCacheClient memoryCacheClient)
    {
        _memoryCacheClient = memoryCacheClient;
    }

    public async Task<List<ThirdPartyIdpModel>> GetAllAsync()
    {
        var thirdPartyIdps = await _memoryCacheClient.GetAsync<List<ThirdPartyIdpModel>>(CacheKeyConsts.GETALLTHIRDPARTYIDP);
        return thirdPartyIdps ?? new();
    }
}
