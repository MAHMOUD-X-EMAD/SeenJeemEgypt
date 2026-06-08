using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SeenJeemGame.Application.Categories;
using SeenJeemGame.Infrastructure.Persistence;
using SeenJeemGame.Infrastructure.Services.Categories;
using SeenJeemGame.Application.Questions;
using SeenJeemGame.Infrastructure.Services.Questions;
using SeenJeemGame.Application.Games;
using SeenJeemGame.Infrastructure.Services.Games;
namespace SeenJeemGame.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<IGameSetupService, GameSetupService>();
        services.AddScoped<IGamePlayService, GamePlayService>();


        return services;
    }
}