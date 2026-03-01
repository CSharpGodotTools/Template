Tweening has never been so easy!!! 🦄
```cs
// Node tween
Tweens.Animate(colorRect)
    .Position(new Vector(100, 300), 2.5);

// Node2D tween
Tweens.Animate(playerSprite)
    .Position(new Vector2(100, 300), 2.5)
    .Property("position", Vector2.Zero, 1.0);

// Control tween with parallel
Tweens.Animate(colorRect)
    .Parallel().Scale(Vector2.One * 2, 2)
    .Parallel().Color(Colors.Green, 2)
    .Rotation(Mathf.Pi, 2)
    .Then(() => GD.Print("Finished!"));

// Tween specific properties
Tween.Animate(colorRect, "color")
    .PropertyTo(Colors.Red,   0.5).TransExpo().EaseIn()
    .PropertyTo(Colors.Green, 0.5).TransQuad().EaseOut()
    .PropertyTo(Colors.Blue,  0.5).TransSpring().EaseInOut()
    .Loop();
```

> [!TIP]
> Prefer strongly typed names over strings? Instead of typing for example `"scale"` do `Control.PropertyName.Scale`

![TweensPreview](https://github.com/user-attachments/assets/38b352bc-a8f6-49f9-ad9a-d0e85a55e081)