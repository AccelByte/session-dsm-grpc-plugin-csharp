// Copyright (c) 2022-2024 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using AccelByte.Sdk.Core;

namespace AccelByte.PluginArch.SessionDsm.Demo.Server
{
    public interface IAccelByteServiceProvider
    {
        AccelByteSDK Sdk { get; }

        AppSettingConfigRepository Config { get; }
    }
}