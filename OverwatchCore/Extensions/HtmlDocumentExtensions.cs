using System;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace OverwatchCore.Extensions
{
    internal static class HtmlDocumentExtensions
    {
        internal static ushort? GetUShort(this IHtmlDocument doc, string selector, Func<IElement, string> editSelectorFindings = null, Func<string, string> editTextFindings = null)
        {
            var ele = doc.QuerySelector(selector);
            
            if (ele == null) return null;

            string str = null;
            if (editSelectorFindings != null)
                str = editSelectorFindings(ele);

            if (str == null) return null;

            if (editTextFindings != null)
                str = editTextFindings(str);

            if (str == null) return null;

            if (ushort.TryParse(str, out ushort parsedstr))
                return parsedstr;
            
            return null;
        }

        internal static bool? GetBool(this IHtmlDocument doc, string selector, string strToCheckAgainst, Func<IElement, string> editSelectorFindings = null, Func<string, string> editTextFindings = null)
        {
            var ele = doc.QuerySelector(selector);
            
            if (ele == null) return null;

            string str = null;
            if (editSelectorFindings != null)
                str = editSelectorFindings(ele);

            if (str == null) return null;

            if (editTextFindings != null)
                str = editTextFindings(str);

            if (str == null) return null;

            return str == strToCheckAgainst;
        }

        internal static string GetString(this IHtmlDocument doc, string selector, string attribute, Func<string, string> editTextFindings = null)
        {
            // var ele = doc.QuerySelector(selector);
            
            // if (ele == null) return null;

            // string str = null;
            // if (editSelectorFindings != null)
            //     str = editSelectorFindings(ele);

            // if (str == null) return null;

            // if (editTextFindings != null)
            //     str = editTextFindings(str);

            // return str;

            var str = doc.QuerySelector(selector)?.GetAttribute(attribute);

            if (str == null) return null;

            if (editTextFindings != null)
                str = editTextFindings(str);

            return str;
        }
    }
}
