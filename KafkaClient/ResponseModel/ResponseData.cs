using System;

namespace KafkaMessagesExtractor.ResponseModel
{
    public class ResponseData
    {
        public Guid CommonId { get; set; }
        public int? UserId { get; set; }
        public int ProductVersion { get; set; }
        public string SimpleProduct { get; set; }
        public string BaseProduct { get; set; }
        public string CodeValue { get; set; }
        public string AccessType { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public bool IsSlave { get; set; }
        public int[] SlaveIds { get; set; }
        public DateTime CreateDate { get; set; }
        public object UserXssId { get; set; }
        public int LicenseStatus { get; set; }
        public string Owner { get; set; }
        public ResponseVersionModel Version { get; set; }
        public Guid? ProductVersionId { get; set; }
        public object PaidAccessStatusId { get; set; }
    }
}