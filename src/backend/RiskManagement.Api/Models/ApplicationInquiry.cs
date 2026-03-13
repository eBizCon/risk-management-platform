using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RiskManagement.Api.Models;

[Table("application_inquiries")]
public class ApplicationInquiry
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("application_id")]
    public int ApplicationId { get; set; }

    [Column("inquiry_text")]
    public string InquiryText { get; set; } = string.Empty;

    [Column("status")]
    public string Status { get; set; } = "open"; // "open" or "answered"

    [Column("processor_email")]
    public string ProcessorEmail { get; set; } = string.Empty;

    [Column("response_text")]
    public string? ResponseText { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("responded_at")]
    public DateTime? RespondedAt { get; set; }
}
