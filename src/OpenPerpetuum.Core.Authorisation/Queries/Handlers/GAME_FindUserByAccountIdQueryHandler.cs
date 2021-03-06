﻿using OpenPerpetuum.Core.Authorisation.DatabaseResults;
using OpenPerpetuum.Core.Authorisation.Models;
using OpenPerpetuum.Core.DataServices.Database;
using OpenPerpetuum.Core.DataServices.Database.Interfaces;
using OpenPerpetuum.Core.Foundation.Processing;
using System;
using System.Linq;

namespace OpenPerpetuum.Core.Authorisation.Queries.Handlers
{
    internal class GAME_FindUserByAccountIdQueryHandler : IQueryHandler<GAME_FindUserByAccountIdQuery, UserModel>
    {
        private readonly IDatabaseProvider gameDatabase;

        public GAME_FindUserByAccountIdQueryHandler(IDataContext dataContext)
        {
            gameDatabase = dataContext.GetDataContext("Game");
        }

        public UserModel Handle(GAME_FindUserByAccountIdQuery query)
        {
            IResult<UserResult> result = gameDatabase.ExecuteProcedure<UserResult>(
                "API.FindUserByAccountId",
                new DbParameters
                {
                    { "AccountId", query.AccountId }
                });

            result.ValidateResult();

            if (result.Data.Users == null || result.Data.Users.Count == 0)
                return UserModel.Default;

            UserData ud = result.Data.Users.Single();
            TimeSpan utcOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);

            return new UserModel
            {
                AccessLevel = ud.AccessLevel,
                AccountId = ud.AccountId,
                BanDate = new DateTimeOffset(ud.BanDate, utcOffset),
                BanExpires = new DateTimeOffset(ud.BanDate.AddMinutes(ud.BanLengthMinutes), utcOffset),
                BanLength = TimeSpan.FromMinutes(ud.BanLengthMinutes),
                CreationDate = new DateTimeOffset(ud.CreationDate, utcOffset),
                Email = ud.Email,
                EmailConfirmed = ud.EmailConfirmed,
                FirstName = ud.FirstName,
                IsActive = ud.IsActive,
                LastLoggedIn = new DateTimeOffset(ud.LastLoggedIn, utcOffset),
                LastName = ud.LastName,
                TotalTimeOnline = TimeSpan.FromMinutes(ud.TotalMinutesOnline)
            };
        }
    }
}
