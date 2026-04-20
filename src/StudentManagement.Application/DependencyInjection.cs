using Microsoft.Extensions.DependencyInjection;
using StudentManagement.Application.Interfaces;
using StudentManagement.Application.Services;

namespace StudentManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<HumanInTheLoopEngine>();

        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IExamService, ExamService>();
        services.AddScoped<IPrivacyService, PrivacyService>();

        return services;
    }
}
