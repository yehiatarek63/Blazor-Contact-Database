using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Contacts.Client;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using SoloX.BlazorJsonLocalization;
using Contacts.Client.Extensions;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("ServerAPI",
      client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
  .CreateClient("ServerAPI"));

builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Auth0", options.ProviderOptions);
    options.ProviderOptions.ResponseType = "code";
    options.ProviderOptions.AdditionalProviderParameters.Add("audience", builder.Configuration["Auth0:Audience"]);
});

builder.Services.AddScoped(typeof(AccountClaimsPrincipalFactory<RemoteUserAccount>),
  typeof(CustomAccountFactory));

builder.Services.AddJsonLocalization(
    builder => builder.UseEmbeddedJson(
        options => options.ResourcesPath = "Resources"));

var host = builder.Build();
await host.SetDefaultCulture();
await builder.Build().RunAsync();
