#nullable enable

namespace BetterBinding.Runtime.Bindings
{
    public abstract class CommandBinding<T> : Binding<Property<T>>
    {
        private Property<T>? _command;

        public override void Bind(Property<T>? command)
        {
            if (command == null)
            {
                return;
            }
         
            Unbind();
            _command = command;
            Subscribe();
        }
        
        public override void Unbind()
        {
            if (_command == null)
            {
                return;
            }
            
            Unsubscribe();
            _command = null;
        }
        
        protected void Execute(T? value)
        {
            _command?.Execute(value);
        }
        
        protected abstract void Subscribe();
        protected abstract void Unsubscribe();
    }
}