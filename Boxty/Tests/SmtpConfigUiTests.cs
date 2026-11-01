using Boxty.ClientApp.Pages.Components;
using Boxty.ClientApp.Pages.Components.Forms;
using Boxty.ClientBase.Components;
using Boxty.ClientBase.Enums;
using Boxty.ClientBase.Services;
using Boxty.SharedApp.DTOs.Settings;
using Boxty.SharedApp.Validators;
using Bunit;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;

namespace Boxty.Tests;

public class SmtpConfigUiTests : BunitContext, IAsyncLifetime
{
    private readonly Mock<ICrudService<SmtpConfigDto>> crud = new();

    public SmtpConfigUiTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddSingleton(crud.Object);
        Services.AddSingleton(Mock.Of<IAuthHelperService>());
        Services.AddSingleton<IValidator<SmtpConfigDto>>(new SmtpConfigValidator());
        Render<MudPopoverProvider>();
    }

    public Task InitializeAsync() => Task.CompletedTask;
    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    [Fact]
    public void RendersOnlyInlineCrudFormWithoutLoadingOrCallbacks()
    {
        var component = Render<SmtpSettings>();
        var form = component.FindComponent<CrudForm<SmtpConfigDto>>().Instance;

        Assert.Single(component.FindComponents<SmtpConfigForm>());
        Assert.False(form.IsModal);
        Assert.Equal(FormModeEnum.Create, form.FormMode);
        Assert.Same(component.Instance.Model, form.Model);
        Assert.False(form.OnAdd.HasDelegate);
        Assert.False(form.OnUpdate.HasDelegate);
        Assert.False(form.OnDelete.HasDelegate);
        Assert.Empty(component.FindComponents<CrudGrid<SmtpConfigDto>>());
        Assert.Empty(component.FindComponents<MudDialog>());
        crud.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CrudFormCreatesSuppliedNewModel()
    {
        var model = SmtpDefaultsTests.ValidDto();
        var id = Guid.NewGuid();
        crud.Setup(service => service.AddItem(model, It.IsAny<CancellationToken>())).ReturnsAsync(id);
        var component = Render<SmtpSettings>(parameters => parameters.Add(settings => settings.Model, model));

        await Button(component, "Save").ClickAsync(new());

        Assert.Equal(id, model.Id);
        crud.Verify(service => service.AddItem(model, It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task CrudFormUpdatesSuppliedExistingModelInEditMode()
    {
        var model = SmtpDefaultsTests.ValidDto();
        model.Id = Guid.NewGuid();
        var component = Render<SmtpSettings>(parameters => parameters
            .Add(settings => settings.Model, model)
            .Add(settings => settings.FormMode, FormModeEnum.Edit));

        await Button(component, "Save").ClickAsync(new());

        crud.Verify(service => service.UpdateItem(model, It.IsAny<CancellationToken>()), Times.Once());
        crud.Verify(service => service.AddItem(It.IsAny<SmtpConfigDto>(), It.IsAny<CancellationToken>()), Times.Never());
        Assert.NotNull(Button(component, "Delete"));
    }

    [Fact]
    public async Task CrudFormValidatesBeforeSaving()
    {
        var component = Render<SmtpSettings>();

        await Button(component, "Save").ClickAsync(new());

        crud.Verify(service => service.AddItem(It.IsAny<SmtpConfigDto>(), It.IsAny<CancellationToken>()), Times.Never());
        Assert.Contains(Services.GetRequiredService<ISnackbar>().ShownSnackbars,
            snackbar => snackbar.Severity == MudBlazor.Severity.Error);
    }

    [Fact]
    public async Task EnableEmailSendingCheckboxUpdatesSuppliedModel()
    {
        var model = SmtpDefaultsTests.ValidDto();
        var component = Render<SmtpSettings>(parameters => parameters.Add(settings => settings.Model, model));
        var checkbox = component.FindComponent<MudCheckBox<bool>>();

        Assert.True(checkbox.Instance.Value);
        await checkbox.InvokeAsync(() => checkbox.Instance.ValueChanged.InvokeAsync(false));
        Assert.False(model.EnableEmailSending);
        Assert.Equal(InputType.Password, component.FindComponents<MudTextField<string>>()
            .Single(field => field.Instance.Label == "SMTP Password").Instance.InputType);
    }

    private static AngleSharp.Dom.IElement Button(IRenderedComponent<SmtpSettings> component, string text) =>
        component.FindAll("button").Single(button => button.TextContent.Trim() == text);
}
