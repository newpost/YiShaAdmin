﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YiSha.Business.Cache;
using YiSha.Business.OrganizationManage;
using YiSha.Entity.OrganizationManage;
using YiSha.Entity.SystemManage;
using YiSha.Enum.SystemManage;
using YiSha.Model.Result;
using YiSha.Service.SystemManage;
using YiSha.Util.Extension;
using YiSha.Util.Model;
using YiSha.Web.Code;

namespace YiSha.Business.SystemManage
{
    public class MenuAuthorizeBLL
    {
        private MenuAuthorizeService menuAuthorizeService = new MenuAuthorizeService();

        private UserBLL userBLL = new UserBLL();

        private MenuAuthorizeCache menuAuthorizeCache = new MenuAuthorizeCache();
        private MenuCache menuCache = new MenuCache();

        #region 获取数据
        public async Task<List<MenuAuthorizeInfo>> GetAuthorizeList(OperatorInfo user)
        {
            List<MenuAuthorizeInfo> authorizeInfoList = new List<MenuAuthorizeInfo>();

            List<MenuAuthorizeEntity> authorizeList = new List<MenuAuthorizeEntity>();
            List<MenuAuthorizeEntity> userAuthorizeList = null;
            List<MenuAuthorizeEntity> roleAuthorizeList = null;

            var menuAuthorizeCacheList = await menuAuthorizeCache.GetList();

            // 用户
            userAuthorizeList = menuAuthorizeCacheList.Where(p => p.AuthorizeId == user.UserId && p.AuthorizeType == AuthorizeTypeEnum.User.ParseToInt()).ToList();

            // 角色
            if (!string.IsNullOrEmpty(user.RoleIds))
            {
                List<long> roleIdList = user.RoleIds.Split(',').Select(p => long.Parse(p)).ToList();
                roleAuthorizeList = menuAuthorizeCacheList.Where(p => roleIdList.Contains(p.AuthorizeId.Value) && p.AuthorizeType == AuthorizeTypeEnum.Role.ParseToInt()).ToList();
            }

            // 排除重复的记录
            if (userAuthorizeList.Count > 0)
            {
                authorizeList.AddRange(userAuthorizeList);
                roleAuthorizeList = roleAuthorizeList.Where(p => !userAuthorizeList.Select(u => u.AuthorizeId).Contains(p.AuthorizeId)).ToList();
            }
            if (roleAuthorizeList != null && roleAuthorizeList.Count > 0)
            {
                authorizeList.AddRange(roleAuthorizeList);
            }

            List<MenuEntity> menuList = await menuCache.GetList();
            foreach (MenuAuthorizeEntity authorize in authorizeList)
            {
                authorizeInfoList.Add(new MenuAuthorizeInfo
                {
                    MenuId = authorize.MenuId,
                    AuthorizeId = authorize.AuthorizeId,
                    AuthorizeType = authorize.AuthorizeType,
                    Authorize = menuList.Where(t => t.Id == authorize.MenuId).Select(t => t.Authorize).FirstOrDefault()
                });
            }
            return authorizeInfoList;
        }
        #endregion
    }
}
