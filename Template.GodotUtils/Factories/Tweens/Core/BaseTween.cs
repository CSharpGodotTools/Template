using Godot;
using System;
using System.Threading.Tasks;
using static Godot.Tween;

namespace GodotUtils;

/// <summary>
/// Base class for tween builders.
/// </summary>
/// <typeparam name="TSelf">Concrete tween builder type returned by fluent APIs.</typeparam>
public abstract class BaseTween<TSelf> where TSelf : BaseTween<TSelf>
{
    protected abstract TSelf Self { get; }

    /// <summary>
    /// The node the tween operates on.
    /// </summary>
    protected readonly Node _node;

    /// <summary>
    /// The underlying tween instance.
    /// </summary>
    protected Tween _tween;

    /// <summary>
    /// The active property tweener used for transition and ease settings.
    /// </summary>
    protected PropertyTweener? _tweener;

    /// <summary>
    /// Creates a tween bound to the provided node.
    /// </summary>
    /// <param name="node">Node that owns the tween and receives property updates.</param>
    public BaseTween(Node node)
    {
        _node = node;

        // Ensure the Tween is fresh when re-creating it.
        Kill();
        _tween = node.CreateTween();

        // Keep physics-driven objects in sync.
        _tween.SetProcessMode(TweenProcessMode.Physics);
    }

    /// <summary>
    /// Tweens a property to the target value over the given duration.
    /// </summary>
    /// <param name="property">Property path to animate.</param>
    /// <param name="finalValue">Target value.</param>
    /// <param name="duration">Tween duration in seconds.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public virtual TSelf Property(string property, Variant finalValue, double duration)
    {
        _tweener = _tween
            .TweenProperty(_node, property, finalValue, duration)
            .SetTrans(TransitionType.Sine);

        return Self;
    }

    /// <summary>
    /// Sets the tween processing mode.
    /// </summary>
    /// <param name="mode">Tween processing mode.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public TSelf SetProcessMode(TweenProcessMode mode)
    {
        _tween = _tween.SetProcessMode(mode);
        return Self;
    }

    /// <summary>
    /// Sets the tween to loop the specified number of times.
    /// </summary>
    /// <param name="loops">Loop count; 0 means infinite looping.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public TSelf Loop(int loops = 0)
    {
        _tween = _tween.SetLoops(loops);
        return Self;
    }

    /// <summary>
    /// Awaits the tween finish signal.
    /// </summary>
    /// <returns>Task that completes when the tween emits its finished signal.</returns>
    public async Task FinishedAsync()
    {
        await _node.ToSignal(_tween, Tween.SignalName.Finished);
    }

    /// <summary>
    /// Makes the next tweener run in parallel with the previous one.
    /// </summary>
    /// <returns>Current tween builder for chaining.</returns>
    public TSelf Parallel()
    {
        _tween = _tween.Parallel();
        return Self;
    }

    /// <summary>
    /// Sets whether appended tweeners run in parallel.
    /// </summary>
    /// <param name="parallel">True to run newly appended tweeners in parallel.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public TSelf SetParallel(bool parallel = true)
    {
        _tween = _tween.SetParallel(parallel);
        return Self;
    }

    /// <summary>
    /// Registers a callback when the tween reaches this point.
    /// </summary>
    /// <param name="callback">Callback to invoke.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public TSelf Then(Action callback)
    {
        _tween.TweenCallback(Callable.From(callback));
        return Self;
    }

    /// <summary>
    /// Inserts a delay before the next tween step.
    /// </summary>
    /// <param name="seconds">Delay duration in seconds.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public TSelf Delay(double seconds)
    {
        _tween.TweenCallback(Callable.From(() => { /* Empty Action */ })).SetDelay(seconds);
        return Self;
    }

    /// <summary>
    /// Registers a callback when the tween finishes.
    /// </summary>
    /// <param name="callback">Callback invoked when tween finishes.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public TSelf Finished(Action callback)
    {
        _tween.Finished += callback;
        return Self;
    }

    /// <summary>
    /// Stops the tween if it is looping.
    /// </summary>
    /// <returns>Current tween builder for chaining.</returns>
    public TSelf Stop()
    {
        _tween.Stop();
        return Self;
    }

    /// <summary>
    /// Pauses the tween.
    /// </summary>
    /// <returns>Current tween builder for chaining.</returns>
    public TSelf Pause()
    {
        _tween.Pause();
        return Self;
    }

    /// <summary>
    /// Resumes a paused tween.
    /// </summary>
    /// <returns>Current tween builder for chaining.</returns>
    public TSelf Resume()
    {
        _tween.Play();
        return Self;
    }

    /// <summary>
    /// Sets the transition type for the current property tweener.
    /// </summary>
    /// <param name="transType">Transition curve type.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public TSelf SetTrans(TransitionType transType)
    {
        return UpdateTweener(nameof(SetTrans), () => _tweener!.SetTrans(transType));
    }

    /// <summary>
    /// Sets the ease type for the current property tweener.
    /// </summary>
    /// <param name="easeType">Ease function type.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public TSelf SetEase(EaseType easeType)
    {
        return UpdateTweener(nameof(SetEase), () => _tweener!.SetEase(easeType));
    }

    /// <summary>
    /// Sets the transition type to Linear.
    /// </summary>
    /// <returns>Current tween builder for chaining.</returns>
    public TSelf TransLinear() => SetTrans(TransitionType.Linear);

    /// <summary>
    /// Sets the transition type to Back.
    /// </summary>
    /// <returns>Current tween builder for chaining.</returns>
    public TSelf TransBack() => SetTrans(TransitionType.Back);

    /// <summary>
    /// Sets the transition type to Sine.
    /// </summary>
    /// <returns>Current tween builder for chaining.</returns>
    public TSelf TransSine() => SetTrans(TransitionType.Sine);

    /// <summary>
    /// Sets the transition type to Bounce.
    /// </summary>
    /// <returns>Current tween builder for chaining.</returns>
    public TSelf TransBounce() => SetTrans(TransitionType.Bounce);

    /// <summary>
    /// Sets the transition type to Circ.
    /// </summary>
    /// <returns>Current tween builder for chaining.</returns>
    public TSelf TransCirc() => SetTrans(TransitionType.Circ);

    /// <summary>
    /// Sets the transition type to Cubic.
    /// </summary>
    /// <returns>Current tween builder for chaining.</returns>
    public TSelf TransCubic() => SetTrans(TransitionType.Cubic);

    /// <summary>
    /// Sets the transition type to Elastic.
    /// </summary>
    /// <returns>Current tween builder for chaining.</returns>
    public TSelf TransElastic() => SetTrans(TransitionType.Elastic);

    /// <summary>
    /// Sets the transition type to Expo.
    /// </summary>
    /// <returns>Current tween builder for chaining.</returns>
    public TSelf TransExpo() => SetTrans(TransitionType.Expo);

    /// <summary>
    /// Sets the transition type to Quad.
    /// </summary>
    /// <returns>Current tween builder for chaining.</returns>
    public TSelf TransQuad() => SetTrans(TransitionType.Quad);

    /// <summary>
    /// Sets the transition type to Quart.
    /// </summary>
    /// <returns>Current tween builder for chaining.</returns>
    public TSelf TransQuart() => SetTrans(TransitionType.Quart);

    /// <summary>
    /// Sets the transition type to Quint.
    /// </summary>
    /// <returns>Current tween builder for chaining.</returns>
    public TSelf TransQuint() => SetTrans(TransitionType.Quint);

    /// <summary>
    /// Sets the transition type to Spring.
    /// </summary>
    /// <returns>Current tween builder for chaining.</returns>
    public TSelf TransSpring() => SetTrans(TransitionType.Spring);

    /// <summary>
    /// Sets the ease type to In.
    /// </summary>
    /// <returns>Current tween builder for chaining.</returns>
    public TSelf EaseIn() => SetEase(EaseType.In);

    /// <summary>
    /// Sets the ease type to Out.
    /// </summary>
    /// <returns>Current tween builder for chaining.</returns>
    public TSelf EaseOut() => SetEase(EaseType.Out);

    /// <summary>
    /// Sets the ease type to InOut.
    /// </summary>
    /// <returns>Current tween builder for chaining.</returns>
    public TSelf EaseInOut() => SetEase(EaseType.InOut);

    /// <summary>
    /// Sets the ease type to OutIn.
    /// </summary>
    /// <returns>Current tween builder for chaining.</returns>
    public TSelf EaseOutIn() => SetEase(EaseType.OutIn);

    /// <summary>
    /// Returns true if the tween is running.
    /// </summary>
    /// <returns>True when tween is currently running.</returns>
    public bool IsRunning()
    {
        return _tween.IsRunning();
    }

    /// <summary>
    /// Kills the tween.
    /// </summary>
    /// <returns>Current tween builder for chaining.</returns>
    public TSelf Kill()
    {
        _tween?.Kill();
        return Self;
    }

    /// <summary>
    /// Executes an action on the current tweener.
    /// </summary>
    /// <param name="methodName">Method name used in error messages.</param>
    /// <param name="action">Action to execute on the current tweener.</param>
    /// <returns>Current tween builder for chaining.</returns>
    private TSelf UpdateTweener(string methodName, Action action)
    {
        // Mutating methods require an active tweener created by Animate.
        if (_tweener == null)
        {
            throw new InvalidOperationException($"Cannot call {methodName}() because no tweener has been set with tween.Animate(...)");
        }

        action();
        return Self;
    }
}
