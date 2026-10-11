using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RR.Dynamics.Retail.Payment.NBE.Contract
{
    public enum EftErrorSeverity
    {
        Info,
        Warning,
        Error,
        Critical,
    }

    [XmlRoot("Error")]
    public class EftError
    {
        public EftErrorSeverity Severity { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string ExtendedInfo { get; set; }
        public string ContainerId { get; set; }
        public string FieldId { get; set; }

        public EftError() { }
        public EftError(EftErrorSeverity severity, string errorMessage)
        {
            Severity = severity;
            ErrorMessage = errorMessage;
        }
        public EftError(EftErrorSeverity severity, string errorMessage, string containerId)
        {
            Severity = severity;
            ErrorMessage = errorMessage;
            ContainerId = containerId;
        }
        public EftError(EftErrorSeverity severity, string errorMessage, string containerId, string fieldId)
        {
            Severity = severity;
            ErrorMessage = errorMessage;
            ContainerId = containerId;
            FieldId = fieldId;
        }
    }

    public class EftErrorsXml
    {
        [XmlArray("Errors")]
        [XmlArrayItem("Error", typeof(EftError))]
        public EftError[] Errors { get; set; }
    }

    public class EftErrorList
    {
        private List<EftError> _errors;
        internal EftErrorList()
        {
            _errors = new List<EftError>();
        }

        public void Add(EftError error)
        {
            if (error != null) _errors.Add(error);
        }

        public void AddWarning(string errorMessage, string extendedInfo, string containerId, string fieldId)
        {
            Add(EftErrorSeverity.Warning, errorMessage, containerId, fieldId, extendedInfo);
        }

        public void Add(EftErrorSeverity severity, string errorMessage)
        {
            EftError error = new EftError();
            error.Severity = severity;
            error.ErrorMessage = errorMessage;
            _errors.Add(error);
        }

        public void Add(EftErrorSeverity severity, string errorMessage, string containerId)
        {
            EftError error = new EftError();
            error.Severity = severity;
            error.ErrorMessage = errorMessage;
            error.ContainerId = containerId;
            _errors.Add(error);
        }

        private void Add(EftErrorSeverity severity, string errorMessage, string containerId, string fieldId, string extendedInfo)
        {
            EftError error = new EftError();
            error.Severity = severity;
            error.ErrorMessage = errorMessage;
            error.ContainerId = containerId;
            error.FieldId = fieldId;
            error.ExtendedInfo = extendedInfo;
            _errors.Add(error);
        }

        public bool Any(bool critical)
        {
            if (critical)
                return _errors.FirstOrDefault(e => e.Severity == EftErrorSeverity.Critical) != null;
            else
                return _errors.FirstOrDefault(e => e.Severity == EftErrorSeverity.Critical || e.Severity == EftErrorSeverity.Error) != null;
        }

        public EftError Last(bool critical)
        {
            EftError error = _errors.LastOrDefault(e => e.Severity == EftErrorSeverity.Critical);
            if (error == null && !critical)
                error = _errors.LastOrDefault(e => e.Severity == EftErrorSeverity.Error);
            return error;
        }

        public EftError[] Select()
        {
            EftError[] errors = _errors.Where(e => e.Severity == EftErrorSeverity.Error || e.Severity == EftErrorSeverity.Critical).ToArray();
            if (errors.Length > 0)
                return errors;
            else
                return null;
        }

        public override string ToString()
        {
            if (_errors.Count > 0)
            {
                EftErrorsXml list = new EftErrorsXml();
                list.Errors = _errors.ToArray();
                using (StringWriter buffer = new StringWriter(CultureInfo.InvariantCulture))
                {
                    XmlSerializer formatter = new XmlSerializer(typeof(EftErrorsXml));
                    formatter.Serialize(buffer, list);
                    return buffer.ToString();
                }
            }
            return null;
        }
    }
}
