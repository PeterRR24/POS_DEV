using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

using Microsoft.Dynamics.Commerce.Runtime;

using RR.Commerce.Runtime.Printing;
using RR.Commerce.Contracts.Reporting;

namespace RR.Commerce.Runtime.Reporting
{
    [XmlRoot("root")]
    public class LabelPrinterJob
    {
        [XmlArray("Labels")]
        [XmlArrayItem("Label", typeof(LabelPrinterTask))]

        public List<LabelPrinterTask> Labels { get; set; }
    }

    [XmlRoot("root")]
    public class FormItemLabelTask
    {
        [XmlArray("ItemLabels")]
        [XmlArrayItem("ItemLabel", typeof(FormItemLabel))]
        public List<FormItemLabel> ItemLabels { get; set; }
    }

    //[XmlRoot("root")]
    //public class PrrLabelProductTask
    //{
    //    [XmlArray("LabelProducts")]
    //    [XmlArrayItem("LabelProduct", typeof(PrrLabelProduct))]
    //    public List<PrrLabelProduct> LabelProducts { get; set; }
    //}

    public interface ILabelVariableResolver
    {
        int NoOfLabels { get; }
        object Resolve(string name);
    }

    public static class LabelPrinterFormat
    {
        private const string NumberOfLabels = "NumberOfLabels";

        public static string ToXml(List<LabelPrinterTask> labels)
        {
            ThrowIf.Null(labels, "labels");

            LabelPrinterJob job = new LabelPrinterJob();
            job.Labels = labels;

            using (StringWriter buffer = new StringWriter())
            {
                XmlSerializer formatter = new XmlSerializer(typeof(LabelPrinterJob));
                formatter.Serialize(buffer, job);
                return buffer.ToString();
            }
        }

        public static string SerializeItems(IEnumerable<IFormItemLabel> iObjs)
        {
            //List<FormItemLabel> items = new List<FormItemLabel>();
            //foreach (IFormItemLabel obj in objs)
            //{
            //    items.Add((FormItemLabel)obj);
            //}
            List<FormItemLabel> items = iObjs.Cast<FormItemLabel>().ToList();
            FormItemLabelTask labelTask = new FormItemLabelTask()
            {
                ItemLabels = items
            };

            using (StringWriter buffer = new StringWriter())
            {
                XmlSerializer formatter = new XmlSerializer(typeof(FormItemLabelTask));
                formatter.Serialize(buffer, labelTask);
                return buffer.ToString();
            }
        }

        //public static string SerializeProductDataSet(object obj)
        //{
        //    List<PrrLabelProduct> plps = (List<PrrLabelProduct>)obj;
        //    PrrLabelProductTask task = new PrrLabelProductTask();
        //    task.LabelProducts = plps;

        //    using (StringWriter buffer = new StringWriter())
        //    {
        //        XmlSerializer formatter = new XmlSerializer(typeof(PrrLabelProductTask));
        //        formatter.Serialize(buffer, task);
        //        return buffer.ToString();
        //    }
        //}

        public static LabelPrinterTask Format(string template, ILabelVariableResolver lvr)
        {
            if (String.IsNullOrEmpty(template))
                return null;

            ThrowIf.Null<ILabelVariableResolver>(lvr, "lvr");

            int numberOfLabels = lvr.NoOfLabels;
            if (numberOfLabels <= 0)
                return null;

            string[] tokens = template.Split('{');  // template.Split('('); // DOBBIES used curly bracket in template we need to fix this using back-office configuration.
            StringBuilder resolvedTokens = new StringBuilder(); ;
            LabelPrinterFormatToken lptt;
            string resolvedToken;
            object variable;
            string lastToken = null;
            bool escaped = false;
            foreach (string token in tokens)
            {
                if (lastToken != null)
                {
                    lptt = escaped ? null : GetToken(token);
                    if (lptt != null)
                    {
                        if (String.Compare(lptt.Name, NumberOfLabels, StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            variable = numberOfLabels;
                            numberOfLabels = 1;
                        }
                        else
                            variable = lvr.Resolve(lptt.Name);

                        if (variable != null)
                            resolvedToken = String.Format(lptt.Format, variable) + lptt.Text;
                        else
                            resolvedToken = lptt.Text;
                    }
                    else
                        resolvedToken = "{" + token;    // resolvedToken = "(" + token; // DOBBIES used curly bracket in template we need to fix this using back-office configuration.
                }
                else
                    resolvedToken = token;

                escaped = resolvedToken.EndsWith(@"\");
                if (escaped)
                    resolvedTokens.Append(resolvedToken.Substring(0, resolvedToken.Length - 1));
                else
                    resolvedTokens.Append(resolvedToken);

                lastToken = token;
            }

            return new LabelPrinterTask { Quantity = numberOfLabels, Text = resolvedTokens.ToString() };
        }

        private static LabelPrinterFormatToken GetToken(string token)
        {
            int endBracket = token.IndexOf('}');    // token.IndexOf(')'); // DOBBIES used curly bracket in template we need to fix this using back-office configuration.
            if (endBracket < 0)
                return null;

            string expr = token.Substring(0, endBracket + 1);
            string text = token.Substring(endBracket + 1);

            // { index[,alignment][ :formatString] }
            int nameSeparator = expr.IndexOf(',');
            if (nameSeparator < 0)
                nameSeparator = expr.IndexOf(':');
            if (nameSeparator < 0)
                nameSeparator = expr.Length - 1;

            string name = expr.Substring(0, nameSeparator);
            string format = "{0" + expr.Substring(nameSeparator);
            // string format = "(0" + expr.Substring(nameSeparator); // DOBBIES used curly bracket in template we need to fix this using back-office configuration.

            return new LabelPrinterFormatToken(name, format, text);
        }

        internal class LabelPrinterFormatToken
        {
            private string _name;
            private string _text;
            private string _format;

            public LabelPrinterFormatToken(string name, string format, string text)
            {
                _name = name;
                _text = text;
                _format = format;
                // _format = format.Replace('(', '{').Replace(')', '}'); // DOBBIES used curly bracket in template we need to fix this using back-office configuration.
            }

            public string Name { get { return _name; } }
            public string Text { get { return _text; } }
            public string Format { get { return _format; } }
        }
    }
}
