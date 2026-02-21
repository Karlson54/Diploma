using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using TimeTracker.Core.Mappings;
using TimeTracker.Core.Services.Audit;
using TimeTracker.Data.UnitOfWork;

namespace TimeTracker.Tests.Services.TestBase;

public abstract class ServiceTestBase
{
    protected readonly Mock<IUnitOfWork> UnitOfWorkMock;
    protected readonly Mock<IAuditService> AuditServiceMock;
    protected readonly IMapper Mapper;

    protected ServiceTestBase()
    {
        UnitOfWorkMock = new Mock<IUnitOfWork>();
        AuditServiceMock = new Mock<IAuditService>();

        // Регистрируем все профили проекта
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AuthMappingProfile>();
            cfg.AddProfile<UserMappingProfile>();
            cfg.AddProfile<RoleMappingProfile>();
            cfg.AddProfile<DictionaryMappingProfile>();
            cfg.AddProfile<TimeEntryMappingProfile>();
            cfg.AddProfile<AuditMappingProfile>();
        });
        Mapper = config.CreateMapper();

        // AuditService — мокаем все методы чтобы не падало
        AuditServiceMock.Setup(x => x.LogCreateAsync(
                It.IsAny<string>(), It.IsAny<long>(), It.IsAny<object>(),
                It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        AuditServiceMock.Setup(x => x.LogUpdateAsync(
                It.IsAny<string>(), It.IsAny<long>(), It.IsAny<object>(), It.IsAny<object>(),
                It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        AuditServiceMock.Setup(x => x.LogDeleteAsync(
                It.IsAny<string>(), It.IsAny<long>(), It.IsAny<object>(),
                It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        AuditServiceMock.Setup(x => x.LogLoginAsync(
                It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        AuditServiceMock.Setup(x => x.LogRegistrationAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<long?>()))
            .Returns(Task.CompletedTask);
    }

    protected static Mock<ILogger<T>> CreateLogger<T>() => new();
}