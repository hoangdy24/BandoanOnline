﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace QlW_BandoanOnline.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                          .GetMember(enumValue.ToString())
                          .First()
                          .GetCustomAttribute<DisplayAttribute>()
                          ?.GetName() ?? enumValue.ToString();
        }
    }
}