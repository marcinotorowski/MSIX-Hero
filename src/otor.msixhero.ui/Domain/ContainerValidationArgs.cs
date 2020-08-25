namespace Otor.MsixHero.Ui.Domain
{
    public class ContainerValidationArgs
    {
        public ContainerValidationArgs()
        {
            this.SetValid();
        }

        public ContainerValidationArgs(string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
            {
                this.SetValid();
            }
            else
            {
                this.SetError(errorMessage);
            }
        }

        public void SetError(string message)
        {
            this.IsValid = false;
            this.ValidationMessage = message;
        }

        public void SetValid()
        {
            this.IsValid = true;
            this.ValidationMessage = null;
        }

        public bool IsValid { get; private set; }

        public string ValidationMessage { get; private set; }
    }
}