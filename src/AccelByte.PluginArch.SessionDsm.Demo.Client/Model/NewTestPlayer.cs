// Copyright (c) 2024 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AccelByte.Sdk.Core;
using AccelByte.Sdk.Core.Repository;
using AccelByte.Sdk.Api;
using AccelByte.Sdk.Api.Iam.Model;
using AccelByte.Sdk.Core.Util;

namespace AccelByte.Sdk.Tests.Mod.Scenario
{
    public class NewTestPlayer
    {
        private string _UserName = String.Empty;

        private readonly bool _DeleteOnLogout;

        private readonly DefaultTokenRepository _TokenRepo;

        private readonly AccelByteSDK _AdminSdk;

        private readonly AccelByteSDK _Sdk;

        public string AccessToken
        {
            get => _TokenRepo.Token;
        }

        public string UserId { get; private set; } = String.Empty;

        public static string GenerateRandomNumber(int length)
        {
            Random random = new Random();
            string chars = "0123456789";
            return new String(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Create an ITestPlayer object by creating a new user via IAM.
        /// </summary>
        /// <param name="adminSdk">AccelByteSDK object with necessary access and permission to create a user.</param>
        /// <param name="deleteOnLogout">Set true to delete the created user on logout.</param>
        public NewTestPlayer(AccelByteSDK adminSdk, bool deleteOnLogout)
        {
            _DeleteOnLogout = deleteOnLogout;
            _AdminSdk = adminSdk;

            _UserName = ("csharpsdk_" + Helper.GenerateRandomId(8));
            string user_password = Helper.GenerateRandomPassword(10);
            string user_email = $"{_UserName}@dummy.com";

            AccountCreateUserRequestV4 newUser = new AccountCreateUserRequestV4()
            {
                AuthType = "EMAILPASSWD",
                EmailAddress = user_email,
                Password = user_password,
                DisplayName = $"Server SDK Test Player {GenerateRandomNumber(3)}",
                Username = _UserName,
                Country = "ID",
                DateOfBirth = "1995-01-10",
                UniqueDisplayName = _UserName
            };

            AccountCreateUserResponseV4? cuResp = _AdminSdk.Iam.UsersV4.PublicCreateUserV4Op
                .Execute(newUser, _AdminSdk.Namespace);
            if (cuResp != null)
                UserId = cuResp.UserId!;

            _TokenRepo = new DefaultTokenRepository();
            _Sdk = AccelByteSDK.Builder
                .SetConfigRepository(_AdminSdk.Configuration.ConfigRepository)
                .UseDefaultHttpClient()
                .SetTokenRepository(_TokenRepo)
                .EnableLog()
                .Build();

            _Sdk.LoginUser(user_email, user_password);
        }

        public void Login() { }

        public void Logout()
        {
            _Sdk.Logout();
            if (_DeleteOnLogout)
                _AdminSdk.Iam.Users.AdminDeleteUserInformationV3Op.Execute(_AdminSdk.Namespace, UserId);
        }

        public void Run(Action<AccelByteSDK, NewTestPlayer> action)
        {
            action.Invoke(_Sdk, this);
        }
    }
}