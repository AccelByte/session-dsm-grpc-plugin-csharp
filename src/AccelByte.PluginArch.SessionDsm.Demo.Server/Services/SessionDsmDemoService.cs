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
using AccelByte.PluginArch.SessionDsm.Demo.Server.Model;

namespace AccelByte.PluginArch.SessionDsm.Demo.Server.Services
{
    public class SessionDsmDemoService: AccelByte.Session.SessionDsm.SessionDsm.SessionDsmBase
    {
        private readonly ILogger<SessionDsmDemoService> _Logger;

        public SessionDsmDemoService(ILogger<SessionDsmDemoService> logger)
        {
            _Logger = logger;
        }

        public override Task<ResponseCreateGameSession> CreateGameSession(RequestCreateGameSession request, ServerCallContext context)
        {
            if (request.RequestedRegion.Count == 0)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Please provide requested region."));

            string selectedRegion = request.RequestedRegion[0];

            return Task.FromResult(new ResponseCreateGameSession()
            {
                SessionId = request.SessionId,
                Namespace = request.Namespace,
                Deployment = request.Deployment,
                SessionData = request.SessionData,
                Status = ServerStatusType.Ready,
                Ip = "10.10.10.11",
                Port = 8080,
                ServerId = $"demo-local-{request.SessionId}",
                Source = ServerServiceType.Demo,                
                Region = selectedRegion,
                ClientVersion = request.ClientVersion,
                GameMode = request.GameMode,
                CreatedRegion = selectedRegion
            });
        }

        public override Task<ResponseTerminateGameSession> TerminateGameSession(RequestTerminateGameSession request, ServerCallContext context)
        {
            return Task.FromResult(new ResponseTerminateGameSession()
            {
                Namespace = request.Namespace,
                Reason = "",
                SessionId = request.SessionId,
                Success = true
            });
        }
    }
}
