using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AI_Campus_Assistant.Models
{
    public class Fee
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string FeeId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string Program { get; set; } = string.Empty;

        public int SemesterNo { get; set; }

        public double TuitionFee { get; set; } = 25000;
        public double LibraryFee { get; set; } = 1000;
        public double TransportFee { get; set; } = 3000;
        public double HostelFee { get; set; } = 8000;

        public bool HasTransport { get; set; }
        public bool HasHostel { get; set; }

        public string Status { get; set; } = "Unpaid";

        public DateTime? PaidAt { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? PaymentProofPath { get; set; }
        public string? PaymentProofName { get; set; }
        public DateTime? ProofSubmittedAt { get; set; }

        [BsonIgnore]
        public double TotalFee =>
            TuitionFee +
            LibraryFee +
            (HasTransport ? TransportFee : 0) +
            (HasHostel ? HostelFee : 0);
    }
}