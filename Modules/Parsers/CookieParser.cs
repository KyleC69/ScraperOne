// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// CookieParser.csCookieParser.cs032320233:30 AM


using System.Collections;
using System.Net;

namespace ScraperOne.Modules.Parsers;

public static class CookieParser
{
    public static CookieCollection GetAllCookiesFromHeader(string strHeader, string strHost)
    {
        CookieCollection cc = new();
        if (strHeader.Length > 0)
        {
            var al = ConvertCookieHeaderToArrayList(strHeader);
            cc = ConvertCookieArraysToCookieCollection(al, strHost);
        }

        return cc;
    }


    private static CookieCollection ConvertCookieArraysToCookieCollection(ArrayList al, string strHost)
    {
        CookieCollection cc = new();
        var alcount = al.Count;
        string strEachCook;
        string[] strEachCookParts;
        for (var i = 0; i < alcount; i++)
        {
            strEachCook = al[i].ToString();
            strEachCookParts = strEachCook.Split(';');
            var intEachCookPartsCount = strEachCookParts.Length;
            string[] nameValuePairTemp;
            Cookie cookTemp = new();
            for (var j = 0; j < intEachCookPartsCount; j++)
            {
                if (j == 0)
                {
                    var strCNameAndCValue = strEachCookParts[j];
                    if (strCNameAndCValue.Length > 0)
                    {
                        var firstEqual = strCNameAndCValue.IndexOf("=");
                        var firstName = strCNameAndCValue[..firstEqual];
                        var allValue = strCNameAndCValue[(firstEqual + 1)..];
                        cookTemp.Name = firstName;
                        cookTemp.Value = allValue;
                    }

                    continue;
                }

                string strPNameAndPValue;
                if (strEachCookParts[j].Contains("path", StringComparison.OrdinalIgnoreCase))
                {
                    strPNameAndPValue = strEachCookParts[j];
                    if (strPNameAndPValue.Length > 0)
                    {
                        nameValuePairTemp = strPNameAndPValue.Split('=');
                        cookTemp.Path = nameValuePairTemp[1].Length > 0 ? nameValuePairTemp[1] : "/";
                    }

                    continue;
                }

                if (strEachCookParts[j].Contains("domain", StringComparison.OrdinalIgnoreCase))
                {
                    strPNameAndPValue = strEachCookParts[j];
                    if (strPNameAndPValue.Length > 0)
                    {
                        nameValuePairTemp = strPNameAndPValue.Split('=');
                        cookTemp.Domain = nameValuePairTemp[1].Length > 0 ? nameValuePairTemp[1] : strHost;
                    }
                }
            }

            if (cookTemp.Path.Length == 0) cookTemp.Path = "/";
            if (cookTemp.Domain.Length == 0) cookTemp.Domain = strHost;
            cc.Add(cookTemp);
        }

        return cc;
    }


    private static ArrayList ConvertCookieHeaderToArrayList(string strCookHeader)
    {
        strCookHeader = strCookHeader.Replace("\r", "").Replace("\n", "");
        var strCookieParts = strCookHeader.Split(',');
        ArrayList al = new();
        var i = 0;
        var n = strCookieParts.Length;
        while (i < n)
        {
            if (strCookieParts[i].IndexOf("expires=", StringComparison.OrdinalIgnoreCase) > 0)
            {
                _ = al.Add(strCookieParts[i] + "," + strCookieParts[i + 1]);
                i++;
            }
            else if (i + 1 < n && (!strCookieParts[i + 1].Contains('=') ||
                                   (strCookieParts[i + 1].Count(x => x == '=') == 1 && strCookieParts[i + 1]
                                       .IndexOf("expires=", StringComparison.OrdinalIgnoreCase) > 0)))
            {
                strCookieParts[i + 1] = strCookieParts[i] + strCookieParts[i + 1];
            }
            else
            {
                _ = al.Add(strCookieParts[i]);
            }

            i++;
        }

        return al;
    }
}