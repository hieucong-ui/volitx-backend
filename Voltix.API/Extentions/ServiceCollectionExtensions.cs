using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Voltix.Application.IService;
using Voltix.Application.IServices;
using Voltix.Application.Mappings;
using Voltix.Application.Service;
using Voltix.Application.Services;
using Voltix.Application.Validations;
using Voltix.Domain.Entities;
using Voltix.Infrastructure.Client;
using Voltix.Infrastructure.Context;
using Voltix.Infrastructure.IClient;
using Voltix.Infrastructure.IRepository;
using Voltix.Infrastructure.Repository;

namespace Voltix.API.Extentions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterService(this IServiceCollection services)
        {
            // Register Application Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IRedisService, RedisService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ICustomerOrderService, CustomerOrderService>();
            services.AddScoped<IEContractService, EContractService>();
            services.AddScoped<IGHNService, GHNService>();
            services.AddScoped<IElectricVehicleColorService, ElectricVehicleColorService>();
            services.AddScoped<IElectricVehicleModelService, ElectricVehicleModelService>();
            services.AddScoped<IElectricVehicleVersionService, ElectricVehicleVersionService>();
            services.AddScoped<IElectricVehicleService, ElectricVehicleService>();
            services.AddScoped<IBookingEVService, BookingEVService>();
            services.AddScoped<IEVCInventoryService, EVCInventoryService>();
            services.AddScoped<IWarehouseService, WarehouseService>();
            services.AddScoped<IEContractTemplateService, EContractTemplateService>();
            services.AddScoped<IPromotionService, PromotionService>();
            services.AddScoped<IS3Service, S3Service>();
            services.AddScoped<IEVCService, EVCService>();
            services.AddScoped<IQuoteService, QuoteService>();
            services.AddScoped<IDealerService, DealerService>();
            services.AddScoped<IEVTemplateService, EVTemplateService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IDealerFeedbackService, DealerFeedbackService>();
            services.AddScoped<ICustomerFeedbackService, CustomerFeedbackService>();
            services.AddScoped<IDealerTierService, DealerTierService>();
            services.AddScoped<IDealerDebtService, DealerDebtService>();
            services.AddScoped<IVehicleDeliveryService, VehicleDeliveryService>();
            services.AddScoped<IDealerDebtTransactionService, DealerDebtTransactionService>();
            services.AddScoped<IDashBoardService,DashBoardService>();
            services.AddScoped<IDealerForecastService, DealerForecastService>();
            services.AddScoped<ILogService, LogService>();
            services.AddScoped<IDealerConfigurationService, DealerConfigurationService>();

            // Register Infrastructure Repositories
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IVnptEContractClient, VnptEContractClient>();
            services.AddScoped<IGHNClient, GHNClient>();

            // Register Identity
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Register Fluent Validation
            services.AddValidatorsFromAssemblyContaining<RegisterCustomerValidation>();
            services.AddValidatorsFromAssemblyContaining<LoginUserValidation>();
            services.AddValidatorsFromAssemblyContaining<ForgotPasswordValidation>();
            services.AddValidatorsFromAssemblyContaining<ResetPasswordValidation>();

            services.AddFluentValidationAutoValidation();

            // Configure token lifespan for email confirmation
            services.Configure<DataProtectionTokenProviderOptions>(opt =>
                opt.TokenLifespan = TimeSpan.FromHours(1)
            );


            services.AddAutoMapper(cfg => { }, typeof(AutoMappingProfile));

            // Remove InvalidModelState, keep fluent validation
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors
                                .Where(e => !e.ErrorMessage.StartsWith("The ") || !e.ErrorMessage.EndsWith(" field is required."))
                                .Select(e => e.ErrorMessage)
                                .Where(msg => !string.IsNullOrEmpty(msg))
                                .ToArray()
                        )
                        .Where(kvp => kvp.Value.Length > 0)
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                    var result = new
                    {
                        type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                        title = "One or more validation errors occurred.",
                        status = 400,
                        errors = errors,
                        traceId = context.HttpContext.TraceIdentifier
                    };

                    return new BadRequestObjectResult(result);
                };
            });

            return services;
        }
    }
}
