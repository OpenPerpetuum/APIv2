using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OpenPerpetuum.Core.Extensions
{
    /// <summary>
    /// Series of helpers for easing use of Enums
    /// </summary>
    public static class EnumHelpers
    {
        /// <summary>
        /// Checks if an Enum is nullable
        /// </summary>
        /// <param name="t">Enum type</param>
        /// <returns>Returns a boolean indicating whether or not the Enum is nullable</returns>
        public static bool IsNullableEnum(Type t)
        {
            Type u = Nullable.GetUnderlyingType(t);
            return (u != null) && u.IsEnum;
        }

        /// <summary>
        /// Checks if an enum field has the Description attribute and if so, returns the description value
        /// </summary>
        /// <typeparam name="T">Generic enum type to check</typeparam>
        /// <param name="value">Actual enum value</param>
        /// <returns>Returns description value or null if undefined</returns>
        public static string GetDescriptionValue<T>(T value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());

            if (!(fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false) is DescriptionAttribute[] descriptionAttributes))
                return string.Empty;

            return (descriptionAttributes.Length > 0) ? descriptionAttributes[0].Description : value.ToString();
        }
    }

    /// <summary>
    /// Enum helpers that can be associated with an explicit type
    /// </summary>
    /// <typeparam name="T">Enum type</typeparam>
    public static class EnumHelpers<T>
    {
        /// <summary>
        /// Retrieves the field value of an enum based on the "DisplayAttribute" name value
        /// </summary>
        /// <param name="displayName">Display name associated with the field value you want to parse</param>
        /// <returns>Enum field value</returns>
        public static T GetValueFromName(string displayName)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException("Type must be an enum to use this function");

            foreach (var field in type.GetFields())
            {

                if ((Attribute.GetCustomAttribute(field, typeof(DisplayAttribute)) is DisplayAttribute attribute && attribute.Name == displayName) || field.Name == displayName)
                    return (T)field.GetValue(null);
            }

            throw new ArgumentOutOfRangeException("Unable to find name associated with enum");
        }

        /// <summary>
        /// Checks if an enum field has the Description attribute and if so, returns the description value
        /// </summary>
        /// <param name="value">Actual enum value</param>
        /// <returns>Returns description value or null if undefined</returns>
        public static string GetDisplayNameValue(T value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());

            if (!(fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false) is DisplayAttribute[] displayAttribute))
                return string.Empty;

            return (displayAttribute.Length > 0) ? displayAttribute[0].Name : value.ToString();
        }

        /// <summary>
        /// Returns a list of Name/Description key pairs for UI presentation
        /// </summary>
        /// <returns>Dictionary or descriptions</returns>
        public static IDictionary<string, string> GetDisplayDescriptions()
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException("Type must be an enum to use this function");

            var fieldInfo = type.GetFields();
            Dictionary<string, string> descriptors = new Dictionary<string, string>();
            foreach (var field in fieldInfo)
            {
                if (field.CustomAttributes.Count() > 0)
                {
                    if (!(field.GetCustomAttributes(typeof(DisplayAttribute), false) is DisplayAttribute[] displayAttribute) || displayAttribute.Length < 1)
                        continue;
                    else
                        descriptors.Add(displayAttribute[0].Name, displayAttribute[0].Description);
                }
            }

            return descriptors;
        }
    }
}
