// Copyright (c) 2023-2024 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using Microsoft.Extensions.Configuration;

using AccelByte.Sdk.Core.Logging;
using AccelByte.Sdk.Core.Repository;

namespace AccelByte.PluginArch.SessionDsm.Demo.Server
{
    public class AppSettingConfigRepository : IConfigRepository
    {
        public string BaseUrl { get; set; } = "";

        public string ClientId { get; set; } = "";

        public string ClientSecret { get; set; } = "";

        public string AppName { get; set; } = "";

        public string TraceIdVersion { get; set; } = "";

        public string Namespace { get; set; } = "";

        public bool EnableTraceId { get; set; } = false;

        public bool EnableUserAgentInfo { get; set; } = false;

        public string ResourceName { get; set; } = "";

        public IHttpLogger? Logger { get; set; } = null;


        public string GCP_ServiceAccountFile { get; set; } = "";

        public string GCP_ProjectId { get; set; } = "";

        public string GCP_MachineType { get; set; } = "";

        public string GCP_NetworkName { get; set; } = "";

        public string GCP_RepositoryName { get; set; } = "";

        public int GCP_Retry { get; set; } = 3;

        public int GCP_WaitInterval { get; set; } = 1; //in seconds

        public int GCP_ImageOpenPort { get; set; } = 8080;


        public string AWS_AccessKeyId { get; set; } = "";

        public string AWS_SecretAccessKey { get; set; } = "";

        public string AWS_Region { get; set; } = "";


        public void ReadEnvironmentVariables()
        {
            string? abBaseUrl = Environment.GetEnvironmentVariable("AB_BASE_URL");
            if ((abBaseUrl != null) && (abBaseUrl.Trim() != ""))
                BaseUrl = abBaseUrl.Trim();

            string? abClientId = Environment.GetEnvironmentVariable("AB_CLIENT_ID");
            if ((abClientId != null) && (abClientId.Trim() != ""))
                ClientId = abClientId.Trim();

            string? abClientSecret = Environment.GetEnvironmentVariable("AB_CLIENT_SECRET");
            if ((abClientSecret != null) && (abClientSecret.Trim() != ""))
                ClientSecret = abClientSecret.Trim();

            string? abNamespace = Environment.GetEnvironmentVariable("AB_NAMESPACE");
            if ((abNamespace != null) && (abNamespace.Trim() != ""))
                Namespace = abNamespace.Trim();

            string? appResourceName = Environment.GetEnvironmentVariable("APP_RESOURCE_NAME");
            if (appResourceName == null)
                appResourceName = "SESSIONDSMGRPCSERVICE";
            ResourceName = appResourceName;

            string? gcpServiceAccountFile = Environment.GetEnvironmentVariable("GCP_SERVICE_ACCOUNT_FILE");
            if ((gcpServiceAccountFile != null) && (gcpServiceAccountFile.Trim() != ""))
                GCP_ServiceAccountFile = gcpServiceAccountFile.Trim();

            string? gcpProjectId = Environment.GetEnvironmentVariable("GCP_PROJECT_ID");
            if ((gcpProjectId != null) && (gcpProjectId.Trim() != ""))
                GCP_ProjectId = gcpProjectId.Trim();

            string? gcpMachineType = Environment.GetEnvironmentVariable("GCP_MACHINE_TYPE");
            if ((gcpMachineType != null) && (gcpMachineType.Trim() != ""))
                GCP_MachineType = gcpMachineType.Trim();

            if (GCP_MachineType == "")
                GCP_MachineType = "e2-micro";

            string? gcpNetwork = Environment.GetEnvironmentVariable("GCP_NETWORK");
            if ((gcpNetwork != null) && (gcpNetwork.Trim() != ""))
                GCP_NetworkName = gcpNetwork.Trim();

            if (GCP_NetworkName == "")
                GCP_NetworkName = "public";

            string? gcpRepo = Environment.GetEnvironmentVariable("GCP_REPOSITORY");
            if ((gcpRepo != null) && (gcpRepo.Trim() != ""))
                GCP_RepositoryName = gcpRepo.Trim();

            string? gcpRetryStr = Environment.GetEnvironmentVariable("GCP_RETRY");
            if ((gcpRetryStr != null) && (gcpRetryStr.Trim() != ""))
                GCP_Retry = int.Parse(gcpRetryStr.Trim());

            string? gcpWaitInterval = Environment.GetEnvironmentVariable("GCP_WAIT_GET_IP");
            if ((gcpWaitInterval != null) && (gcpWaitInterval.Trim() != ""))
                GCP_WaitInterval = int.Parse(gcpWaitInterval.Trim());

            string? gcpOpenPort = Environment.GetEnvironmentVariable("GCP_IMAGE_OPEN_PORT");
            if ((gcpOpenPort != null) && (gcpOpenPort.Trim() != ""))
                GCP_ImageOpenPort = int.Parse(gcpOpenPort.Trim());


            string? awsAccessKeyId = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
            if ((awsAccessKeyId != null) && (awsAccessKeyId.Trim() != ""))
                AWS_AccessKeyId = awsAccessKeyId.Trim();

            string? awsSecret = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
            if ((awsSecret != null) && (awsSecret.Trim() != ""))
                AWS_SecretAccessKey = awsSecret.Trim();

            string? awsRegion = Environment.GetEnvironmentVariable("GAMELIFT_REGION");
            if (awsRegion == null)
                awsRegion = Environment.GetEnvironmentVariable("AWS_REGION");
            if ((awsRegion != null) && (awsRegion.Trim() != ""))
                AWS_Region = awsRegion.Trim();
        }
    }
}
