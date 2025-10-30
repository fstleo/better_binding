<h1 align="center"> Better Binding </h1>
This package allows to make bindings between reactive properties and Unity components without any runtime reflection calls.

## Install

#### UPM
Open the package manager window (menu: Window > Package Manager)<br/>
Select "Add package from git URL...", fill in the pop-up with the following link:<br/>
https://github.com/fstleo/better_binding.git?path=Package#1.0.0<br/>

## Usage

1. [Create Bindings classes](#Bindings)
2. [Create Contract class](#Contract)
3. [Setup Game Objects](#Binder) 
4. [Initialization](#Initialization)

### Bindings

First, you need to create your binding classes.
There are three types of bindings: [PropertyBinding](#PropertyBinding),  [CommandBinding](#CommandBinding), [TwoWayBinding](#TwoWayBinding). You can find more examples in the Samples project. 

#### ```PropertyBinding```
PropertyBinding passes value from reactive property to component.

```C#
[Serializable]
public class TextBinding : PropertyBinding<string>
{
    [SerializeField]
    private TextMeshProUGUI _text;
    
    public override void OnNext(string? value)
    {
        if (_text != null)
        {
            _text.text = value;
        }
    }
}
```

#### ```CommandBinding``` 
CommandBinding passes value from component to reactive property.
```C#
[Serializable]
public class ButtonBinding : CommandBinding<Unit>
{
    [SerializeField]
    private Button _button;

    protected override void Subscribe()
    {
        _button.onClick.AddListener(Execute);
    }

    protected override void Unsubscribe()
    {
        _button.onClick.RemoveListener(Execute);
    }

    private void Execute()
    {
        Execute(Unit.Default);
    }
}
```

#### ```TwoWayBinding```
TwoWayBinding passes value both ways from and to component.
```C#
[Serializable]
public class ToggleBinding : TwoWayBinding<bool>
{
    [SerializeField] 
    private Toggle _toggle;
    
    protected override void Subscribe()
    {
        _toggle.onValueChanged.AddListener(Execute);
    }

    protected override void Unsubscribe()
    {
        _toggle.onValueChanged.RemoveListener(Execute);
    }

    public override void OnNext(bool value)
    {
        _toggle.SetIsOnWithoutNotify(value);
    }
}
```

### Contract

Second, create a class with the reactive properties. The only restriction is the class should be partial to allow code generation works. 
<b>NOTE</b> If contract class implements ```IDisposable``` interface, ```DisposeInternal()``` method should be called.

```C#
public partial class ExampleContract
{
    public Property<Color> ColorProperty { get; } = new(Color.white);
    public Property<string> TextProperty { get; } = new();
    public Property<bool> IsSomethingEnabled { get; } = new();
    public Property<Unit> DoSomethingCommand { get; } = new();
}
```

### Binder

After creating all the needed bindings and a contract class.
- Add <b>Binder</b> component to your game object.
- Click <b>Set contract type</b> button and select your Contract class name
- Click on blue <b>+</b> button to add a new binding
- Setup bindings (it may be several bindings per property or none at all)

### Initialization
Create your contract object and call <b>Bind</b> on your <b>Binder</b> object.
Call <b>Unbind</b> after you done, and <b>dispose</b> the contract object.

```C#
public class Entry : MonoBehaviour
{
    [SerializeField]
    private Binder _binder;
    
    private ExampleContract _viewModel;
    
    private void Awake()
    {
        _viewModel = new ExampleContract();
        _binder.Bind(_viewModel);
    }

    private void OnDestroy()
    {
        _binder.Unbind();
        _viewModel.Dispose();
    }
}
```

## Potential issues

- If you rename your property, you have to recreate all the bindings

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/Y8Y81NMIZ7)
