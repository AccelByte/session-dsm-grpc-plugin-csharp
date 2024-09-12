// Copyright (c) 2024 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections.Generic;

using CommandLine;

using AccelByte.Sdk.Core;
using AccelByte.Sdk.Api;
using AccelByte.Sdk.Core.Util;
using AccelByte.Sdk.Api.Session.Model;
using AccelByte.PluginArch.SessionDsm.Demo.Client.Model;
using AccelByte.Sdk.Tests.Mod.Scenario;
using System.Threading;


namespace AccelByte.PluginArch.SessionDsm.Demo.Client
{
    internal class Program
    {
        static int Main(string[] args)
        {
            int exitCode = 0;
            Parser.Default.ParseArguments<ApplicationConfig>(args)
                .WithParsed((config) =>
                {
                    config.FinalizeConfigurations();
                    Console.WriteLine($"\tBaseUrl: {config.BaseUrl}");
                    Console.WriteLine($"\tClientId: {config.ClientId}");
                    if (config.GrpcServerUrl != "")
                        Console.WriteLine($"\tGrpc Target: {config.GrpcServerUrl}");
                    else if (config.ExtendAppName != "")
                        Console.WriteLine($"\tExtend App: {config.ExtendAppName}");
                    else
                    {
                        Console.WriteLine($"\tNO GRPC TARGET SERVER");
                        exitCode = 2;
                        return;
                    }

                    var sdk = AccelByteSDK.Builder
                        .SetConfigRepository(config)
                        .UseDefaultHttpClient()
                        .UseDefaultTokenRepository()
                        .EnableLog()
                        .Build();

                    try
                    {
                        Console.Write("SDK login... ");
                        sdk.LoginClient();
                        Console.WriteLine("[OK]");

                        var initTemplateData = new ApimodelsCreateConfigurationTemplateRequest()
                        {
                            Name = $"csharp-extend-test-{Helper.GenerateRandomId(8)}",
                            MinPlayers = 1,
                            MaxPlayers = 2,
                            MaxActiveSessions = -1,
                            Joinability = "OPEN",
                            InviteTimeout = 60,
                            InactiveTimeout = 60,
                            AutoJoin = true,
                            Type = "DS",
                            DsSource = "custom",
                            DsManualSetReady = false,
                            RequestedRegions = new() { "us-west-2" }
                        };

                        if (config.ExtendAppName != "")
                            initTemplateData.AppName = config.ExtendAppName;
                        else if (config.GrpcServerUrl != "")
                            initTemplateData.CustomURLGRPC = config.GrpcServerUrl;

                        Console.Write("Create session template... ");
                        var cTemplate = sdk.Session.ConfigurationTemplate.AdminCreateConfigurationTemplateV1Op
                            .Execute(initTemplateData, sdk.Namespace);
                        Console.WriteLine("[OK]");

                        try
                        {
                            Console.Write("Create new player... ");
                            var player = new NewTestPlayer(sdk, true);
                            Console.WriteLine("[OK]");
                            try
                            {
                                player.Run((playerSdk, thisPlayer) =>
                                {
                                    var newSession = playerSdk.Session.GameSession.CreateGameSessionOp
                                        .Execute(new ApimodelsCreateGameSessionRequest()
                                        {
                                            ConfigurationName = initTemplateData.Name                                            
                                        }, playerSdk.Namespace);
                                    if (newSession == null)
                                        throw new Exception("New game session is null");

                                    //periodically checking session data

                                    bool isDsAvailable = false;
                                    for (int i = 0; i < config.DsCheckCount; i++)
                                    {
                                        var sessionData = playerSdk.Session.GameSession.GetGameSessionOp
                                            .Execute(playerSdk.Namespace, newSession.Id!);
                                        if (sessionData != null && sessionData.DSInformation != null)
                                        {
                                            if ((sessionData.DSInformation.StatusV2! == "AVAILABLE")
                                                && (sessionData.DSInformation.Server != null))
                                            {
                                                isDsAvailable = true;
                                                var serverInfo = sessionData.DSInformation.Server;
                                                Console.WriteLine("\tDS is AVAILABLE");
                                                Console.WriteLine("\t{0}:{1}", serverInfo.Ip, serverInfo.Port);
                                                break;
                                            }
                                        }

                                        Thread.Sleep(config.DsWaitingInterval);
                                    }

                                    playerSdk.Session.GameSession.DeleteGameSessionOp
                                        .Execute(playerSdk.Namespace, newSession.Id!);

                                    if (!isDsAvailable)
                                        throw new Exception("no DS is available after waiting for a while.");
                                });                                
                            }
                            catch (Exception x)
                            {
                                Console.WriteLine($"[PLAYER RUN] Exception: {x.Message}");
                                exitCode = 1;
                            }
                            finally
                            {
                                Console.Write("Delete player... ");
                                player.Logout();
                                Console.WriteLine("[OK]");
                            }
                        }
                        catch (Exception x)
                        {
                            Console.WriteLine($"[CREATE PLAYER] Exception: {x.Message}");
                            exitCode = 1;
                        }
                        finally
                        {
                            Console.Write("Delete session template... ");
                            sdk.Session.ConfigurationTemplate.AdminDeleteConfigurationTemplateV1Op
                                .Execute(initTemplateData.Name, sdk.Namespace);
                            Console.WriteLine("[OK]");
                        }
                    }
                    catch (Exception x)
                    {
                        Console.WriteLine($"[SDK] Exception: {x.Message}");
                        exitCode = 1;
                    }
                    finally
                    {
                        sdk.Logout();
                    }
                })
                .WithNotParsed((errors) =>
                {
                    Console.WriteLine("Invalid argument(s)");
                    foreach (var error in errors)
                        Console.WriteLine($"\t{error}");
                    exitCode = 2;
                });

            return exitCode;
        }
    }
}