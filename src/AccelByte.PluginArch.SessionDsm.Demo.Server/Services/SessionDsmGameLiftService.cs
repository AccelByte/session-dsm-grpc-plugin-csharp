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
using Amazon.GameLift;
using Amazon.GameLift.Model;
using Amazon.Runtime;
using Amazon;
using AccelByte.PluginArch.SessionDsm.Demo.Server.Model;

namespace AccelByte.PluginArch.SessionDsm.Demo.Server.Services
{
    public class SessionDsmGameLiftService : AccelByte.Session.SessionDsm.SessionDsm.SessionDsmBase
    {
        private readonly ILogger<SessionDsmGameLiftService> _Logger;

        private readonly IAccelByteServiceProvider _ABClientProvider;

        private readonly IAmazonGameLift _GameLiftClient;

        public SessionDsmGameLiftService(IAccelByteServiceProvider abClientProvider, ILogger<SessionDsmGameLiftService> logger)
        {
            _Logger = logger;
            _ABClientProvider = abClientProvider;

            var credentials = new BasicAWSCredentials(_ABClientProvider.Config.AWS_AccessKeyId, _ABClientProvider.Config.AWS_SecretAccessKey);
            var awsRegion = RegionEndpoint.GetBySystemName(_ABClientProvider.Config.AWS_Region);
            _GameLiftClient = new AmazonGameLiftClient(credentials, awsRegion);
        }

        public override async Task<ResponseCreateGameSession> CreateGameSession(RequestCreateGameSession request, ServerCallContext context)
        {
            if (request.RequestedRegion.Count == 0)
                throw new RpcException(new Grpc.Core.Status(StatusCode.InvalidArgument, "Please provide requested region."));
            string selectedRegion = request.RequestedRegion[0];

            var cgsRequest = new CreateGameSessionRequest()
            {
                AliasId = request.Deployment,
                GameSessionData = request.SessionData,
                IdempotencyToken = request.SessionId,
                MaximumPlayerSessionCount = (int)request.MaximumPlayer,
                Location = selectedRegion
            };

            var cgsResponse = await _GameLiftClient.CreateGameSessionAsync(cgsRequest);
            if (cgsResponse == null)
                throw new Exception("CreateGameSession response is NULL");

            return await Task.FromResult(new ResponseCreateGameSession()
            {
                SessionId = request.SessionId,
                Namespace = request.Namespace,
                Deployment = cgsResponse.GameSession.FleetId,
                SessionData = request.SessionData,
                Status = ServerStatusType.Ready,
                Ip = cgsResponse.GameSession.IpAddress,
                Port = cgsResponse.GameSession.Port,
                ServerId = cgsResponse.GameSession.GameSessionId,
                Source = ServerServiceType.GameLift,
                Region = selectedRegion,
                ClientVersion = request.ClientVersion,
                GameMode = request.GameMode,
                CreatedRegion = cgsResponse.GameSession.Location
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
