namespace otor.msixhero.ui.Domain
{
    public enum ValueResetType
    {
        /// <summary>
        /// Soft reset just only sets the value back the original and updates the dirty flag accordingly, but leaves the touched flag intact.
        /// </summary>
        Soft,

        /// <summary>
        /// Hard reset sets everything back to the original, including dirty and touched flags which will be <c>False</c>.
        /// </summary>
        Hard
    }
}