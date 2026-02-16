using Godot;
using System;
using System.Threading.Tasks;
using static Godot.Tween;

namespace GodotUtils;

/// <summary>
/// Base class for tween builders.
/// </summary>
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
    protected PropertyTweener _tweener;

    /// <summary>
    /// Creates a tween bound to the provided node.
    /// </summary>
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
    public TSelf SetProcessMode(TweenProcessMode mode)
    {
        _tween = _tween.SetProcessMode(mode);
        return Self;
    }

    /// <summary>
    /// Sets the tween to loop the specified number of times.
    /// </summary>
    public TSelf Loop(int loops = 0)
    {
        _tween = _tween.SetLoops(loops);
        return Self;
    }

    /// <summary>
    /// Awaits the tween finish signal.
    /// </summary>
    public async Task FinishedAsync()
    {
        await _node.ToSignal(_tween, Tween.SignalName.Finished);
    }

    /// <summary>
    /// Makes the next tweener run in parallel with the previous one.
    /// </summary>
    public TSelf Parallel()
    {
        _tween = _tween.Parallel();
        return Self;
    }

    /// <summary>
    /// Sets whether appended tweeners run in parallel.
    /// </summary>
    public TSelf SetParallel(bool parallel = true)
    {
        _tween = _tween.SetParallel(parallel);
        return Self;
    }

    /// <summary>
    /// Registers a callback when the tween reaches this point.
    /// </summary>
    public TSelf Then(Action callback)
    {
        _tween.TweenCallback(Callable.From(callback));
        return Self;
    }

    /// <summary>
    /// Inserts a delay before the next tween step.
    /// </summary>
    public TSelf Delay(double seconds)
    {
        _tween.TweenCallback(Callable.From(() => { /* Empty Action */ })).SetDelay(seconds);
        return Self;
    }

    /// <summary>
    /// Registers a callback when the tween finishes.
    /// </summary>
    public TSelf Finished(Action callback)
    {
        _tween.Finished += callback;
        return Self;
    }

    /// <summary>
    /// Stops the tween if it is looping.
    /// </summary>
    public TSelf Stop()
    {
        _tween.Stop();
        return Self;
    }

    /// <summary>
    /// Pauses the tween.
    /// </summary>
    public TSelf Pause()
    {
        _tween.Pause();
        return Self;
    }

    /// <summary>
    /// Resumes a paused tween.
    /// </summary>
    public TSelf Resume()
    {
        _tween.Play();
        return Self;
    }

    /// <summary>
    /// Sets the transition type for the current property tweener.
    /// </summary>
    public TSelf SetTrans(TransitionType transType)
    {
        return UpdateTweener(nameof(SetTrans), () => _tweener.SetTrans(transType));
    }

    /// <summary>
    /// Sets the ease type for the current property tweener.
    /// </summary>
    public TSelf SetEase(EaseType easeType)
    {
        return UpdateTweener(nameof(SetEase), () => _tweener.SetEase(easeType));
    }

    /// <summary>
    /// Sets the transition type to Linear.
    /// </summary>
    public TSelf TransLinear() => SetTrans(TransitionType.Linear);

    /// <summary>
    /// Sets the transition type to Back.
    /// </summary>
    public TSelf TransBack() => SetTrans(TransitionType.Back);

    /// <summary>
    /// Sets the transition type to Sine.
    /// </summary>
    public TSelf TransSine() => SetTrans(TransitionType.Sine);

    /// <summary>
    /// Sets the transition type to Bounce.
    /// </summary>
    public TSelf TransBounce() => SetTrans(TransitionType.Bounce);

    /// <summary>
    /// Sets the transition type to Circ.
    /// </summary>
    public TSelf TransCirc() => SetTrans(TransitionType.Circ);

    /// <summary>
    /// Sets the transition type to Cubic.
    /// </summary>
    public TSelf TransCubic() => SetTrans(TransitionType.Cubic);

    /// <summary>
    /// Sets the transition type to Elastic.
    /// </summary>
    public TSelf TransElastic() => SetTrans(TransitionType.Elastic);

    /// <summary>
    /// Sets the transition type to Expo.
    /// </summary>
    public TSelf TransExpo() => SetTrans(TransitionType.Expo);

    /// <summary>
    /// Sets the transition type to Quad.
    /// </summary>
    public TSelf TransQuad() => SetTrans(TransitionType.Quad);

    /// <summary>
    /// Sets the transition type to Quart.
    /// </summary>
    public TSelf TransQuart() => SetTrans(TransitionType.Quart);

    /// <summary>
    /// Sets the transition type to Quint.
    /// </summary>
    public TSelf TransQuint() => SetTrans(TransitionType.Quint);

    /// <summary>
    /// Sets the transition type to Spring.
    /// </summary>
    public TSelf TransSpring() => SetTrans(TransitionType.Spring);

    /// <summary>
    /// Sets the ease type to In.
    /// </summary>
    public TSelf EaseIn() => SetEase(EaseType.In);

    /// <summary>
    /// Sets the ease type to Out.
    /// </summary>
    public TSelf EaseOut() => SetEase(EaseType.Out);

    /// <summary>
    /// Sets the ease type to InOut.
    /// </summary>
    public TSelf EaseInOut() => SetEase(EaseType.InOut);

    /// <summary>
    /// Sets the ease type to OutIn.
    /// </summary>
    public TSelf EaseOutIn() => SetEase(EaseType.OutIn);

    /// <summary>
    /// Returns true if the tween is running.
    /// </summary>
    public bool IsRunning()
    {
        return _tween.IsRunning();
    }

    /// <summary>
    /// Kills the tween.
    /// </summary>
    public TSelf Kill()
    {
        _tween?.Kill();
        return Self;
    }

    /// <summary>
    /// Executes an action on the current tweener.
    /// </summary>
    private TSelf UpdateTweener(string methodName, Action action)
    {
        if (_tweener == null)
        {
            throw new Exception($"Cannot call {methodName}() because no tweener has been set with tween.Animate(...)");
        }

        action();
        return Self;
    }
}
