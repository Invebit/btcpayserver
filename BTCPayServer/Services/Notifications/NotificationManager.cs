﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BTCPayServer.Data;
using BTCPayServer.Models.NotificationViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace BTCPayServer.Services.Notifications
{
    public class NotificationManager
    {
        private readonly ApplicationDbContextFactory _factory;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _memoryCache;

        public NotificationManager(ApplicationDbContextFactory factory, UserManager<ApplicationUser> userManager, IMemoryCache memoryCache)
        {
            _factory = factory;
            _userManager = userManager;
            _memoryCache = memoryCache;
        }

        private const int _cacheExpiryMs = 5000;
        public NotificationSummaryViewModel GetSummaryNotifications(ClaimsPrincipal user)
        {
            var userId = _userManager.GetUserId(user);

            if (_memoryCache.TryGetValue<NotificationSummaryViewModel>(userId, out var obj))
                return obj;

            var resp = FetchNotificationsFromDb(userId);
            _memoryCache.Set(userId, resp, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMilliseconds(_cacheExpiryMs)));

            return resp;
        }

        private NotificationSummaryViewModel FetchNotificationsFromDb(string userId)
        {
            var resp = new NotificationSummaryViewModel();
            using (var _db = _factory.CreateContext())
            {
                resp.UnseenCount = _db.Notifications
                    .Where(a => a.ApplicationUserId == userId && !a.Seen)
                    .Count();

                if (resp.UnseenCount > 0)
                {
                    try
                    {
                        resp.Last5 = _db.Notifications
                            .Where(a => a.ApplicationUserId == userId && !a.Seen)
                            .OrderByDescending(a => a.Created)
                            .Take(5)
                            .Select(a => a.ToViewModel())
                            .ToList();
                    }
                    catch (System.IO.InvalidDataException)
                    {
                        // invalid notifications that are not pkuzipable, burn them all
                        var notif = _db.Notifications.Where(a => a.ApplicationUserId == userId);
                        _db.Notifications.RemoveRange(notif);
                        _db.SaveChanges();

                        resp.UnseenCount = 0;
                        resp.Last5 = new List<NotificationViewModel>();
                    }
                }
                else
                {
                    resp.Last5 = new List<NotificationViewModel>();
                }
            }

            return resp;
        }
    }

    public class NotificationSummaryViewModel
    {
        public int UnseenCount { get; set; }
        public List<NotificationViewModel> Last5 { get; set; }
    }
}
