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
using System.Net;
using System.Net.Http.Json;

namespace Boxty.Tests;

public class SmtpConfigUiTests : BunitContext, IAsyncLifetime
{
    private readonly Mock<ICrudService<SmtpConfigDto>> crud = new();
    private readonly SettingsHttpHandler httpHandler = new();

    public SmtpConfigUiTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddSingleton(crud.Object);
        Services.AddSingleton(Mock.Of<IAuthHelperService>());
        Services.AddSingleton<IValidator<SmtpConfigDto>>(new SmtpConfigValidator());
        Services.AddSingleton(new HttpClient(httpHandler) { BaseAddress = new Uri("https://example.test/") });
        Render<MudPopoverProvider>();
    }

    public Task InitializeAsync() => Task.CompletedTask;
    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    [Fact]
    public void LoadsSettingsAndRendersInlineCreateFormWhenNoneExist()
    {
        var component = Render<SmtpSettings>();
        var form = component.FindComponent<CrudForm<SmtpConfigDto>>().Instance;

        Assert.Single(component.FindComponents<SmtpConfigForm>());
        Assert.False(form.IsModal);
        Assert.Equal(FormModeEnum.Create, form.FormMode);
        Assert.Same(component.Instance.Model, form.Model);
        Assert.True(form.OnAdd.HasDelegate);
        Assert.False(form.OnUpdate.HasDelegate);
        Assert.False(form.OnDelete.HasDelegate);
        Assert.Empty(component.FindComponents<CrudGrid<SmtpConfigDto>>());
        Assert.Empty(component.FindComponents<MudDialog>());
        Assert.Equal("/api/smtpconfig/getall", httpHandler.RequestUri?.AbsolutePath);
        crud.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task LoadsExistingSettingsAndSavesAnUpdate()
    {
        var model = SmtpDefaultsTests.ValidDto();
        model.Id = Guid.NewGuid();
        httpHandler.Response = Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new[] { model })
        });
        var component = Render<SmtpSettings>();
        var form = component.FindComponent<CrudForm<SmtpConfigDto>>().Instance;

        Assert.Equal(FormModeEnum.Edit, form.FormMode);
        Assert.Equal(model.Id, form.Model.Id);
        Assert.Equal(model.SmtpHost, component.FindComponents<MudTextField<string>>()
            .Single(field => field.Instance.Label == "SMTP Host").Instance.Value);
        Assert.Equal(model.SenderAddress, form.Model.SenderAddress);
        Assert.Equal(model.SmtpPort, form.Model.SmtpPort);
        Assert.Equal(model.SmtpUsername, form.Model.SmtpUsername);
        Assert.Equal(model.SmtpPassword, form.Model.SmtpPassword);
        Assert.Equal(model.EnableEmailSending, form.Model.EnableEmailSending);

        await Button(component, "Save").ClickAsync(new());

        crud.Verify(service => service.UpdateItem(form.Model, It.IsAny<CancellationToken>()), Times.Once());
        crud.Verify(service => service.AddItem(It.IsAny<SmtpConfigDto>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public void HidesFormUntilSettingsHaveLoaded()
    {
        var pending = new TaskCompletionSource<HttpResponseMessage>();
        httpHandler.Response = pending.Task;
        var component = Render<SmtpSettings>();

        Assert.Single(component.FindComponents<MudProgressCircular>());
        Assert.Empty(component.FindComponents<CrudForm<SmtpConfigDto>>());

        pending.SetResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(Array.Empty<SmtpConfigDto>()) });

        component.WaitForAssertion(() =>
        {
            Assert.Single(component.FindComponents<CrudForm<SmtpConfigDto>>());
            Assert.Empty(component.FindComponents<MudProgressCircular>());
        });
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public void LoadingFailureShowsErrorWithoutBlankForm(HttpStatusCode statusCode)
    {
        httpHandler.Response = Task.FromResult(new HttpResponseMessage(statusCode));
        var component = Render<SmtpSettings>();

        Assert.Empty(component.FindComponents<CrudForm<SmtpConfigDto>>());
        Assert.Equal(MudBlazor.Severity.Error, component.FindComponent<MudAlert>().Instance.Severity);
        crud.VerifyNoOtherCalls();
    }

    [Fact]
    public void NoContentResponseAllowsCreatingSettings()
    {
        httpHandler.Response = Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
        var component = Render<SmtpSettings>();

        Assert.Equal(FormModeEnum.Create, component.FindComponent<CrudForm<SmtpConfigDto>>().Instance.FormMode);
    }

    [Fact]
    public void MultipleConfigurationsShowErrorRatherThanEditingAnArbitraryRecord()
    {
        httpHandler.Response = Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new[] { SmtpDefaultsTests.ValidDto(), SmtpDefaultsTests.ValidDto() })
        });
        var component = Render<SmtpSettings>();

        Assert.Empty(component.FindComponents<CrudForm<SmtpConfigDto>>());
        Assert.Contains("Multiple SMTP configurations", component.Markup);
    }

    [Fact]
    public void InvalidResponseShowsErrorWithoutBlankForm()
    {
        httpHandler.Response = Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("not-json")
        });
        var component = Render<SmtpSettings>();

        Assert.Empty(component.FindComponents<CrudForm<SmtpConfigDto>>());
        Assert.Contains("invalid SMTP settings response", component.Markup);
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
        Assert.Equal(FormModeEnum.Edit, component.FindComponent<CrudForm<SmtpConfigDto>>().Instance.FormMode);
        Assert.Null(httpHandler.RequestUri);

        await Button(component, "Save").ClickAsync(new());
        crud.Verify(service => service.UpdateItem(model, It.IsAny<CancellationToken>()), Times.Once());
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

    private sealed class SettingsHttpHandler : HttpMessageHandler
    {
        public Task<HttpResponseMessage> Response { get; set; } = Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(Array.Empty<SmtpConfigDto>()) });
        public Uri? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            return Response;
        }
    }
}
