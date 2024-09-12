// Copyright (c) 2024 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using Grpc.Core;
using AccelByte.Session.SessionDsm;

namespace AccelByte.PluginArch.SessionDsm.Demo.Server.Services
{
    public class SessionDsmGameLiftService : AccelByte.Session.SessionDsm.SessionDsm.SessionDsmBase
    {
        private readonly ILogger<SessionDsmGameLiftService> _Logger;

        public SessionDsmGameLiftService(ILogger<SessionDsmGameLiftService> logger)
        {
            _Logger = logger;
        }

        public override Task<ResponseCreateGameSession> CreateGameSession(RequestCreateGameSession request, ServerCallContext context)
        {
            return base.CreateGameSession(request, context);
        }

        public override Task<ResponseTerminateGameSession> TerminateGameSession(RequestTerminateGameSession request, ServerCallContext context)
        {
            return base.TerminateGameSession(request, context);
        }
    }
}
