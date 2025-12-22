using Godot;
using System;

public static class AnimatedSpriteBuilder
{
    public static void BuildAnimation(
        AnimatedSprite2D sprite,
        Animation data
    )
    {
        if (sprite == null || data == null || data.SpriteSheet == null)
            return;

        if (sprite.SpriteFrames == null)
            sprite.SpriteFrames = new SpriteFrames();

        SpriteFrames frames = sprite.SpriteFrames;

        if (frames.HasAnimation(data.Name))
            frames.RemoveAnimation(data.Name);

        frames.AddAnimation(data.Name);
        frames.SetAnimationSpeed(data.Name, data.FrameRate);
        frames.SetAnimationLoop(data.Name, data.Loops);

        int cell = data.CellSize;
        int row = data.WhichRow;

        // Determine how many frames to add
        int frameCount = data.FrameCount > 0
            ? Mathf.Min(data.FrameCount, data.Horizontal)
            : data.Horizontal;

        for (int col = 0; col < frameCount; col++)
        {
            Rect2 region = new Rect2(
                col * cell,
                row * cell,
                cell,
                cell
            );

            AtlasTexture atlas = new AtlasTexture
            {
                Atlas = data.SpriteSheet,
                Region = region
            };

            frames.AddFrame(data.Name, atlas);
        }

        sprite.Animation = data.Name;
        sprite.Play();
    }
}
