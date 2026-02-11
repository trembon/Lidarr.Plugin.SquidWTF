using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.SquidWTF
{
    public class QobuzSettingsValidator : AbstractValidator<QobuzSettings>
    {
        public QobuzSettingsValidator()
        {
            RuleFor(x => x.BaseUrl).IsValidUrl();
        }
    }

    public class QobuzSettings : IProviderConfig
    {
        private static readonly QobuzSettingsValidator Validator = new();

        [FieldDefinition(1, Label = "SquidWTF Qobuz API URL", HelpText = "Base API URL for SquidWTF Qobuz.")]
        public string BaseUrl { get; set; } = "";

        [FieldDefinition(2, Label = "Download folder", HelpText = "Root folder the downloads will be placed.")]
        public string DownloadPath { get; set; } = "";

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
