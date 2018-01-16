namespace Settings.Binding
{
    /// <summary>
    /// Interface to allow unspecified collections of bindable items in property panels - BindableBase is generic, and requires the implementation of its generic parameters.
    /// </summary>
    interface IBindable
    {
        void SetTarget(object target);
    }
}
