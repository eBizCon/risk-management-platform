using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RiskManagement.Api.Models;
using RiskManagement.Api.Services;

namespace RiskManagement.Api.Data;

public class ApplicationRepository
{
    public const int PageSize = 10;

    private readonly ApplicationDbContext _context;
    private readonly ScoringService _scoringService;

    public ApplicationRepository(ApplicationDbContext context, ScoringService scoringService)
    {
        _context = context;
        _scoringService = scoringService;
    }

    public async Task<Application> CreateApplication(Application data)
    {
        var scoring = _scoringService.CalculateScore(
            data.Income, data.FixedCosts, data.DesiredRate,
            data.EmploymentStatus, data.HasPaymentDefault);

        data.Score = scoring.Score;
        data.TrafficLight = scoring.TrafficLight;
        data.ScoringReasons = JsonSerializer.Serialize(scoring.Reasons);
        data.CreatedAt = DateTime.UtcNow.ToString("o");

        _context.Applications.Add(data);
        await _context.SaveChangesAsync();

        return data;
    }

    public async Task<Application?> UpdateApplication(int id, ApplicationUpdateDto updateData)
    {
        var existing = await _context.Applications.FindAsync(id);
        if (existing == null || existing.Status != "draft")
        {
            return null;
        }

        existing.Name = updateData.Name;
        existing.Income = updateData.Income;
        existing.FixedCosts = updateData.FixedCosts;
        existing.DesiredRate = updateData.DesiredRate;
        existing.EmploymentStatus = updateData.EmploymentStatus;
        existing.HasPaymentDefault = updateData.HasPaymentDefault;

        var scoring = _scoringService.CalculateScore(
            existing.Income, existing.FixedCosts, existing.DesiredRate,
            existing.EmploymentStatus, existing.HasPaymentDefault);

        existing.Score = scoring.Score;
        existing.TrafficLight = scoring.TrafficLight;
        existing.ScoringReasons = JsonSerializer.Serialize(scoring.Reasons);

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<Application?> SubmitApplication(int id)
    {
        var existing = await _context.Applications.FindAsync(id);
        if (existing == null || existing.Status != "draft")
        {
            return null;
        }

        var scoring = _scoringService.CalculateScore(
            existing.Income, existing.FixedCosts, existing.DesiredRate,
            existing.EmploymentStatus, existing.HasPaymentDefault);

        existing.Status = "submitted";
        existing.SubmittedAt = DateTime.UtcNow.ToString("o");
        existing.Score = scoring.Score;
        existing.TrafficLight = scoring.TrafficLight;
        existing.ScoringReasons = JsonSerializer.Serialize(scoring.Reasons);

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<Application?> ProcessApplication(int id, string decision, string? comment)
    {
        var existing = await _context.Applications.FindAsync(id);
        if (existing == null || (existing.Status != "submitted" && existing.Status != "resubmitted"))
        {
            return null;
        }

        existing.Status = decision;
        existing.ProcessorComment = string.IsNullOrEmpty(comment) ? null : comment;
        existing.ProcessedAt = DateTime.UtcNow.ToString("o");

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<Application?> GetApplicationById(int id)
    {
        return await _context.Applications.FindAsync(id);
    }

    public async Task<List<Application>> GetApplicationsByUser(string userEmail, string? status = null)
    {
        var query = _context.Applications.Where(a => a.CreatedBy == userEmail);

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(a => a.Status == status);
        }

        return await query.ToListAsync();
    }

    public async Task<List<Application>> GetApplicationsByStatus(string status)
    {
        return await _context.Applications.Where(a => a.Status == status).ToListAsync();
    }

    public async Task<List<Application>> GetAllApplications()
    {
        return await _context.Applications.ToListAsync();
    }

    public async Task<(List<Application> Items, int TotalCount)> GetProcessorApplicationsPaginated(
        string? status, int page, int pageSize)
    {
        var query = _context.Applications.AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(a => a.Status == status);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<ProcessorStats> GetProcessorApplicationStats()
    {
        var total = await _context.Applications.CountAsync();
        var submitted = await _context.Applications.CountAsync(a => a.Status == "submitted");
        var approved = await _context.Applications.CountAsync(a => a.Status == "approved");
        var rejected = await _context.Applications.CountAsync(a => a.Status == "rejected");

        return new ProcessorStats
        {
            Total = total,
            Submitted = submitted,
            Approved = approved,
            Rejected = rejected
        };
    }

    public async Task<bool> DeleteApplication(int id)
    {
        var existing = await _context.Applications.FindAsync(id);
        if (existing == null || existing.Status != "draft")
        {
            return false;
        }

        _context.Applications.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<DashboardStats> GetDashboardStats(string? userEmail = null)
    {
        var query = _context.Applications.AsQueryable();
        if (!string.IsNullOrEmpty(userEmail))
        {
            query = query.Where(a => a.CreatedBy == userEmail);
        }

        var draft = await query.CountAsync(a => a.Status == "draft");
        var submitted = await query.CountAsync(a =>
            a.Status == "submitted" || a.Status == "needs_information" || a.Status == "resubmitted");
        var approved = await query.CountAsync(a => a.Status == "approved");
        var rejected = await query.CountAsync(a => a.Status == "rejected");

        return new DashboardStats
        {
            Draft = draft,
            Submitted = submitted,
            Approved = approved,
            Rejected = rejected
        };
    }

    public async Task<List<Application>> GetApplicationsForExport(string? status = null)
    {
        var query = _context.Applications.AsQueryable();
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(a => a.Status == status);
        }
        return await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
    }

    public async Task<Application?> UpdateApplicationStatus(int id, string status)
    {
        var existing = await _context.Applications.FindAsync(id);
        if (existing == null) return null;

        existing.Status = status;
        await _context.SaveChangesAsync();
        return existing;
    }

    // Inquiry methods

    public async Task<List<ApplicationInquiry>> GetApplicationInquiries(int applicationId)
    {
        return await _context.ApplicationInquiries
            .Where(i => i.ApplicationId == applicationId)
            .OrderBy(i => i.CreatedAt)
            .ThenBy(i => i.Id)
            .ToListAsync();
    }

    public async Task<ApplicationInquiry?> GetLatestOpenInquiry(int applicationId)
    {
        return await _context.ApplicationInquiries
            .Where(i => i.ApplicationId == applicationId && i.Status == "open")
            .OrderByDescending(i => i.CreatedAt)
            .ThenByDescending(i => i.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<ApplicationInquiry> CreateApplicationInquiry(int applicationId, string inquiryText, string processorEmail)
    {
        var inquiry = new ApplicationInquiry
        {
            ApplicationId = applicationId,
            InquiryText = inquiryText,
            ProcessorEmail = processorEmail,
            Status = "open",
            CreatedAt = DateTime.UtcNow
        };

        _context.ApplicationInquiries.Add(inquiry);
        await _context.SaveChangesAsync();
        return inquiry;
    }

    public async Task<ApplicationInquiry?> AnswerApplicationInquiry(int inquiryId, string responseText)
    {
        var inquiry = await _context.ApplicationInquiries.FindAsync(inquiryId);
        if (inquiry == null) return null;

        inquiry.ResponseText = responseText;
        inquiry.RespondedAt = DateTime.UtcNow;
        inquiry.Status = "answered";

        await _context.SaveChangesAsync();
        return inquiry;
    }
}
