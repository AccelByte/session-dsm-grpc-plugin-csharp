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
using Google.Cloud.Compute.V1;
using System.Threading;
using AccelByte.PluginArch.SessionDsm.Demo.Server.Model;
using Google.Apis.Auth.OAuth2;

namespace AccelByte.PluginArch.SessionDsm.Demo.Server.Services
{
    public class SessionDsmGcpService : AccelByte.Session.SessionDsm.SessionDsm.SessionDsmBase
    {
        private readonly ILogger<SessionDsmGcpService> _Logger;

        private readonly IAccelByteServiceProvider _ABClientProvider;

        private InstancesClient _GInstanceClient;

        private readonly Dictionary<string, string> _AwsToGcpRegionMap = new()
        {
            { "us-east-1",      "us-east1" },
            { "us-east-2",      "us-east4" },
            { "us-west-1",      "us-west1" },
            { "us-west-2",      "us-west2" },
            { "ca-central-1",   "northamerica-northeast1" },
            { "sa-east-1",      "southamerica-east1" },
            { "eu-central-1",   "europe-west3" },
            { "eu-west-1",      "europe-west1" },
            { "eu-west-2",      "europe-west2" },
            { "eu-west-3",      "europe-west9" },
            { "eu-north-1",     "europe-north1" },
            { "me-south-1",     "me-west1" },
            { "af-south-1",     "africa-north1" },
            { "ap-east-1",      "asia-east2" },
            { "ap-south-1",     "asia-south1" },
            { "ap-northeast-3", "asia-northeast2" },
            { "ap-northeast-2", "asia-northeast3" },
            { "ap-southeast-1", "asia-southeast1" },
            { "ap-southeast-2", "australia-southeast1" },
            { "ap-northeast-1", "asia-northeast1" }
        };

        private readonly Dictionary<string, List<string>> _GcpZones = new()
        {
            { "us-east1",                   new() {"us-east1-b", "us-east1-c", "us-east1-d"} },
            { "us-east4",                   new() {"us-east4-a", "us-east4-b", "us-east4-c"} },
            { "us-west1",                   new() {"us-west1-a", "us-west1-b", "us-west1-c"} },
            { "us-west2",                   new() { "us-west2-a", "us-west2-b", "us-west2-c"} },
            { "northamerica-northeast1",    new() { "northamerica-northeast1-a", "northamerica-northeast1-b", "northamerica-northeast1-c"} },
            { "southamerica-east1",         new() { "southamerica-east1-a", "southamerica-east1-b", "southamerica-east1-c"} },
            { "europe-west3",               new() { "europe-west3-a", "europe-west3-b", "europe-west3-c"} },
            { "europe-west1",               new() { "europe-west1-b", "europe-west1-c", "europe-west1-d"} },
            { "europe-west2",               new() { "europe-west2-a", "europe-west2-b", "europe-west2-c"} },
            { "europe-west9",               new() { "europe-west9-a", "europe-west9-b", "europe-west9-c"} },
            { "europe-north1",              new() { "europe-north1-a", "europe-north1-b", "europe-north1-c"} },
            { "me-west1",                   new() { "me-west1-a", "me-west1-b", "me-west1-c"} },
            { "africa-north1",              new() { "africa-north1-a", "africa-north1-b", "africa-north1-c"} },
            { "asia-east2",                 new() { "asia-east2-a", "asia-east2-b", "asia-east2-c"} },
            { "asia-south1",                new() { "asia-south1-a", "asia-south1-b", "asia-south1-c"} },
            { "asia-northeast2",            new() { "asia-northeast2-a", "asia-northeast2-b", "asia-northeast2-c"} },
            { "asia-northeast3",            new() { "asia-northeast3-a", "asia-northeast3-b", "asia-northeast3-c"} },
            { "asia-southeast1",            new() { "asia-southeast1-a", "asia-southeast1-b", "asia-southeast1-c"} },
            { "australia-southeast1",       new() { "australia-southeast1-a", "australia-southeast1-b", "australia-southeast1-c"} },
            { "asia-northeast1",            new() { "asia-northeast1-a", "asia-northeast1-b", "asia-northeast1-c"} }
        };

        public SessionDsmGcpService(IAccelByteServiceProvider abClientProvider, ILogger<SessionDsmGcpService> logger)
        {
            _Logger = logger;
            _ABClientProvider = abClientProvider;

            var gcBuilder = new InstancesClientBuilder();
            gcBuilder.GoogleCredential = GoogleCredential.FromFile(_ABClientProvider.Config.GCP_ServiceAccountFile);

            _GInstanceClient = gcBuilder.Build();
        }

        public async Task<(bool, string)> TerminateInstanceAsync(string instanceName, string zone)
        {
            string projectId = _ABClientProvider.Config.GCP_ProjectId;

            var deleteOp = await _GInstanceClient.DeleteAsync(new DeleteInstanceRequest()
            {
                Project = projectId,
                Zone = zone,
                Instance = instanceName
            });

            if (deleteOp.IsFaulted)
            {
                if (deleteOp.Exception.Message.Contains("Error 404"))
                    return (true, deleteOp.Exception.Message);
                else
                {
                    _Logger.LogError($"Could not terminate GCP instance. {deleteOp.Exception.Message}");
                    return (false, deleteOp.Exception.Message);
                }
            }

            return (true, deleteOp.Result.StatusMessage);
        }

        public override async Task<ResponseCreateGameSession> CreateGameSession(RequestCreateGameSession request, ServerCallContext context)
        {
            if (request.RequestedRegion.Count == 0)
                throw new RpcException(new Grpc.Core.Status(StatusCode.InvalidArgument, "Please provide requested region."));

            string selectedRegion = request.RequestedRegion[0];
            string projectId = _ABClientProvider.Config.GCP_ProjectId;
            string machineType = _ABClientProvider.Config.GCP_MachineType;
            string networkName = _ABClientProvider.Config.GCP_NetworkName;
            string repositoryName = _ABClientProvider.Config.GCP_RepositoryName;

            //translate
            string gcpRegion;
            if (_AwsToGcpRegionMap.ContainsKey(selectedRegion))
                gcpRegion = _AwsToGcpRegionMap[selectedRegion];
            else
                throw new Exception($"Unknown AWS region: {selectedRegion}");

            string gcpZone;
            if (_GcpZones.ContainsKey(gcpRegion))
            {
                List<string> zones = _GcpZones[gcpRegion];
                gcpZone = zones[(new Random()).Next(0, zones.Count)];
            }
            else
                throw new Exception($"Unknown GCP region: {gcpRegion}");

            string instanceName = $"{request.Namespace}-{request.SessionId}";

            var tags = new Tags();
            tags.Items.Add("http-server");
            tags.Items.Add("https-server");

            var metaData = new Google.Cloud.Compute.V1.Metadata();
            metaData.Items.Add(new Items()
            {
                Key = "gce-container-declaration",
                Value = $"spec:\n  containers:\n  - name: {instanceName}\n    image: {repositoryName}/{request.Deployment}\n    env:\n    - name: SESSION_ID\n      value: {instanceName}\n    securityContext:\n      privileged: true\n    stdin: true\n    tty: true\n  restartPolicy: Never\n# This container declaration format is not public API and may change without notice. Please\n# use gcloud command-line tool or Google Cloud Console to run Containers on Google Compute Engine."
            });

            var instanceSpec = new Instance()
            {
                Name = instanceName,
                MachineType = $"zones/{gcpZone}/machineTypes/{machineType}",
                ShieldedInstanceConfig = new ShieldedInstanceConfig()
                {
                    EnableIntegrityMonitoring = true,
                    EnableSecureBoot = true,
                    EnableVtpm = true
                },
                ReservationAffinity = new ReservationAffinity()
                {
                    ConsumeReservationType = "ANY_RESERVATION"
                },
                ConfidentialInstanceConfig = new ConfidentialInstanceConfig()
                {
                    EnableConfidentialCompute = false
                },
                Tags = tags,
                Metadata = metaData
            };

            instanceSpec.Disks.Add(new AttachedDisk()
            {
                AutoDelete = true,
                Boot = true,
                DeviceName = $"{instanceSpec.Name}-disk",
                InitializeParams = new AttachedDiskInitializeParams()
                {
                    DiskSizeGb = 10,
                    DiskType = $"projects/{projectId}/zones/{gcpZone}/diskTypes/pd-balanced",
                    SourceImage = $"projects/cos-cloud/global/images/cos-stable-113-18244-85-5"
                },
                Mode = "READ_WRITE",
                Type = "PERSISTENT"
            });

            var newNetworkIntf = new NetworkInterface()
            {
                StackType = "IPV4_ONLY",
                Subnetwork = $"projects/{projectId}/regions/{gcpRegion}/subnetworks/{networkName}"
            };
            newNetworkIntf.AccessConfigs.Add(new AccessConfig()
            {
                Name = "External NAT",
                NetworkTier = "PREMIUM"
            });
            instanceSpec.NetworkInterfaces.Add(newNetworkIntf);            


            var createInstanceOp = await _GInstanceClient.InsertAsync(new InsertInstanceRequest()
            {
                Project = projectId,
                Zone = gcpZone,                
                InstanceResource = instanceSpec,                
            });
            if (createInstanceOp.IsFaulted)
                throw new Exception($"Failed to create instance. {createInstanceOp.Exception.Message}");

            try
            {
                var instanceInfo = await _GInstanceClient.GetAsync(new GetInstanceRequest()
                {
                    Project = projectId,
                    Zone = gcpZone,
                    Instance = instanceName
                });

                bool instanceReady = false;
                int checkRetry = 0;
                while (checkRetry <= _ABClientProvider.Config.GCP_Retry)
                {
                    if (instanceInfo.Status != "RUNNING")
                    {
                        checkRetry++;
                        Thread.Sleep(_ABClientProvider.Config.GCP_WaitInterval * 1000);

                        instanceInfo = await _GInstanceClient.GetAsync(new GetInstanceRequest()
                        {
                            Project = projectId,
                            Zone = gcpZone,
                            Instance = instanceName
                        });
                    }
                    else
                    {
                        instanceReady = true;
                        break;
                    }
                }

                if (instanceReady)
                {
                    string externalIp = "";
                    foreach (var netIntf in instanceInfo.NetworkInterfaces)
                    {
                        if (netIntf.AccessConfigs.Count > 0)
                        {
                            externalIp = netIntf.AccessConfigs[0].NatIP;
                            break;
                        }
                    }

                    return await Task.FromResult(new ResponseCreateGameSession()
                    {
                        SessionId = request.SessionId,
                        Namespace = request.Namespace,
                        Deployment = request.Deployment,
                        SessionData = request.SessionData,
                        Status = ServerStatusType.Ready,
                        Ip = externalIp,
                        Port = _ABClientProvider.Config.GCP_ImageOpenPort,
                        ServerId = instanceName,
                        Source = ServerServiceType.GCP,
                        Region = selectedRegion,
                        ClientVersion = request.ClientVersion,
                        GameMode = request.GameMode,
                        CreatedRegion = gcpZone
                    });
                }
                else
                {
                    var b = await TerminateInstanceAsync(instanceName, projectId);
                    if (b.Item1)
                        throw new Exception("Instance creation process failed");                    
                    else
                        throw new Exception("Instance creation process isn't finish and failed to delete it.");
                }
            }            
            catch (Exception x)
            {
                _Logger.LogError($"CreateGameSession error: {x.Message}");
                throw;
            }
        }

        public override async Task<ResponseTerminateGameSession> TerminateGameSession(RequestTerminateGameSession request, ServerCallContext context)
        {
            string instanceName = $"{request.Namespace}-{request.SessionId}";
            var b = await TerminateInstanceAsync(instanceName, request.Zone);
            if (!b.Item1)
                throw new Exception($"Could not delete GCP instance {instanceName}");

            return await Task.FromResult(new ResponseTerminateGameSession()
            {
                Namespace = request.Namespace,
                Reason = b.Item2,
                SessionId = request.SessionId,
                Success = true
            });
        }
    }
}
