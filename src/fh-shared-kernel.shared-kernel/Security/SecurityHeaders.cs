using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace FamilyHubs.SharedKernel.Security;

[Obsolete("Please swap to using the version in FamilyHubs.SharedKernel.Razor.Security instead.")]
public static class SecurityHeaders
{
    /// <summary>
    /// nuget documentation
    /// https://github.com/andrewlock/NetEscapades.AspNetCore.SecurityHeaders
    /// csp introduction
    /// https://scotthelme.co.uk/content-security-policy-an-introduction/
    /// google analytics tag manager required csp
    /// https://developers.google.com/tag-platform/tag-manager/web/csp
    /// jquery csp
    /// https://content-security-policy.com/examples/jquery/
    ///
    /// Note: we _may_ need the other google domains from the das ga doc,
    /// but there were no violations reported without them, so we leave them out for now
    ///
    /// Allowing unsafe-inline scripts
    /// ------------------------------
    /// Google's nonce-aware tag manager code has an issue with custom html tags (which we use).
    /// https://stackoverflow.com/questions/65100704/gtm-not-propagating-nonce-to-custom-html-tags
    /// https://dev.to/matijamrkaic/using-google-tag-manager-with-a-content-security-policy-9ai
    ///
    /// We tried the given solution (above), but the last piece of the puzzle to make it work,
    /// would involve self hosting a modified version of google's gtm.js script.
    ///
    /// In gtm.js, where it's creating customScripts, we'd have to change...
    /// var n = C.createElement("script");
    /// to
    /// var n=C.createElement("script");n.nonce=[get nonce from containing script block];
    ///
    /// The problems with self hosting a modified gtm.js are (from https://stackoverflow.com/questions/45615612/is-it-possible-to-host-google-tag-managers-script-locally)
    /// * we wouldn't automatically pick up any new tags or triggers that Steve added
    /// * we would need a version of the script that worked across all browsers and versions (and wouldn't have a browser optimised version)
    /// * we wouldn't pick up new versions of the script
    /// For these reasons, the only way to get the campaign tracking working, is to open up the CSP to allow unsafe-inline scripts.
    /// This will make our site less secure, but is a trade-off between security and tracking functionality.
    /// </summary>
    public static IApplicationBuilder UseAppSecurityHeaders(this WebApplication app)
    {
#pragma warning disable S1075
#pragma warning disable S125
        app.UseSecurityHeaders(policies =>
            policies.AddDefaultSecurityHeaders()
                .AddContentSecurityPolicy(builder =>
                {
                    builder.AddUpgradeInsecureRequests();

                    var defaultSrc = builder.AddDefaultSrc()
                        .Self();

                    var connectSrc = builder.AddConnectSrc()
                        .Self()
                        .From(new[]
                        {
                            "https://*.google-analytics.com",
                            /* application insights*/ "https://dc.services.visualstudio.com/v2/track", "rt.services.visualstudio.com/v2/track"
                        });

                    builder.AddFontSrc()
                        .Self();

                    builder.AddObjectSrc()
                        .None();

                    builder.AddFormAction()
                        .Self();

                    builder.AddImgSrc()
                        .OverHttps()
                        .Self();

                    var scriptSrc = builder.AddScriptSrc()
                        .Self()
                        .From(new[]
                        {
                            "https://*.google-analytics.com/",
                            "https://*.analytics.google.com",
                            "https://*.googletagmanager.com"
                        })
                        // this is needed for GTM
                        .UnsafeInline();
                    // if we wanted the nonce back, we'd add `.WithNonce();` here

                    builder.AddStyleSrc()
                        .Self()
                        .StrictDynamic()
                        .UnsafeInline();

                    builder.AddMediaSrc()
                        .None();

                    builder.AddFrameAncestors()
                        .None();

                    builder.AddBaseUri()
                        .Self();

                    if (app.Environment.IsDevelopment())
                    {
                        // open up for browserlink
                        defaultSrc.From(new[] { "http://localhost:*", "ws://localhost:*" });

                        scriptSrc.From("http://localhost:*");

                        connectSrc.From(new[] { "http://localhost:*", "https://localhost:*", "ws://localhost:*", "wss://localhost:*" });
                    }
                })
                .AddPermissionsPolicy(builder =>
                {
                    builder.AddAccelerometer()
                        .None();
                    builder.AddAmbientLightSensor()
                        .None();
                    builder.AddAutoplay()
                        .None();
                    builder.AddCamera()
                        .None();
                    builder.AddEncryptedMedia()
                        .None();
                    builder.AddFullscreen()
                        .None();
                    builder.AddGeolocation()
                        .None();
                    builder.AddGyroscope()
                        .None();
                    builder.AddMagnetometer()
                        .None();
                    builder.AddMicrophone()
                        .None();
                    builder.AddMidi()
                        .None();
                    builder.AddPayment()
                        .None();
                    builder.AddPictureInPicture()
                        .None();
                    builder.AddSpeaker()
                        .None();
                    // don't need it yet, but we probably will when we enable js filtering, and we don't want to set a trap
                    builder.AddSyncXHR()
                        .Self();
                    builder.AddUsb()
                        .None();
                    builder.AddVR()
                        .None();
                }
                )
                .AddCustomHeader("X-Permitted-Cross-Domain-Policies", "none")
                .AddXssProtectionDisabled());
#pragma warning restore S125
#pragma warning restore S1075

        return app;
    }
}
