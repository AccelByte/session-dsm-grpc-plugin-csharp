// Copyright (c) 2024 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandLine;

using AccelByte.Sdk.Core.Repository;
using AccelByte.Sdk.Core.Logging;
using AccelByte.Sdk.Core.Util;

namespace AccelByte.PluginArch.SessionDsm.Demo.Client.Model
{
    public class ApplicationConfig : IConfigRepository
    {
        [Option('b', "baseurl", Required = false, HelpText = "AGS base URL", Default = "")]
        public string BaseUrl { get; set; } = "";

        [Option('c', "client", Required = false, HelpText = "AGS client id", Default = "")]
        public string ClientId { get; set; } = "";

        [Option('s', "secret", Required = false, HelpText = "AGS client secret", Default = "")]
        public string ClientSecret { get; set; } = "";

        public string AppName { get; set; } = "CustomSessionDsmDemoClient";

        public string TraceIdVersion { get; set; } = "";

        [Option('n', "namespace", Required = false, HelpText = "AGS namespace", Default = "")]
        public string Namespace { get; set; } = "";

        public bool EnableTraceId { get; set; } = false;

        public bool EnableUserAgentInfo { get; set; } = false;

        public IHttpLogger? Logger { get; set; } = null;

        [Option('g', "grpc-target", Required = false, HelpText = "Grpc plugin target server url.", Default = "")]
        public string GrpcServerUrl { get; set; } = "";

        [Option('e', "extend-app", Required = false, HelpText = "Extend app name for grpc plugin.", Default = "")]
        public string ExtendAppName { get; set; } = "";

        [Option('w', "ds-wait-interval", Required = false, HelpText = "Wait interval (in ms) between session check for DS status", Default = 500)]
        public int DsWaitingInterval { get; set; } = 500;

        [Option('o', "ds-check-count", Required = false, HelpText = "How many times app need to check session data for DS status", Default = 10)]
        public int DsCheckCount { get; set; } = 10;

        protected string ReplaceWithEnvironmentVariableIfExists(string pValue, string evKey)
        {
            string? temp = Environment.GetEnvironmentVariable(evKey);
            if ((pValue == "") && (temp != null))
                return temp.Trim();
            else
                return pValue;
        }

        public void FinalizeConfigurations()
        {
            BaseUrl = ReplaceWithEnvironmentVariableIfExists(BaseUrl, "AB_BASE_URL");
            ClientId = ReplaceWithEnvironmentVariableIfExists(ClientId, "AB_CLIENT_ID");
            ClientSecret = ReplaceWithEnvironmentVariableIfExists(ClientSecret, "AB_CLIENT_SECRET");
            Namespace = ReplaceWithEnvironmentVariableIfExists(Namespace, "AB_NAMESPACE");

            string? dsWaitingIntervalVar = Environment.GetEnvironmentVariable("DS_WAITING_INTERVAL");
            if (dsWaitingIntervalVar != null)
                DsWaitingInterval = int.Parse(dsWaitingIntervalVar);

            string? dsCheckCountVar = Environment.GetEnvironmentVariable("DS_CHECK_COUNT");
            if (dsCheckCountVar != null)
                DsCheckCount = int.Parse(dsCheckCountVar);
        }
    }
}
