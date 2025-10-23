#nullable enable

namespace BetterBinding.Runtime
{
    public abstract class PropertyBinding<T> : BaseBinding<Property<T>>
    {
        private Property<T>? _property;
        public override void Bind(Property<T>? value)
        {
            if (value == null)
            {
                return;
            }
            
            _property = value;
            _property.Changed += SetValue;
            SetValue(_property.Value);
        }

        public override void Unbind()
        {
            if (_property != null)
            {
                _property.Changed -= SetValue;
            }
        }
        
        protected abstract void SetValue(T? value);
    }
}