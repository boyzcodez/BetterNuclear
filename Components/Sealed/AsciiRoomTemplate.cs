using Godot;
using System;
using System.Collections.Generic;

public sealed class AsciiRoomTemplate
{
    public readonly string Name;
    public readonly Vector2I Size;

    // Local coordinates
    public readonly HashSet<Vector2I> Floor = new();
    public readonly HashSet<Vector2I> Solid = new(); // '#'
    public readonly List<Door> Doors = new();

    public Vector2I? SpawnLocal; // optional 'S'

    public readonly struct Door
    {
        public readonly Vector2I Pos;
        public readonly Vector2I Dir;
        public Door(Vector2I pos, Vector2I dir) { Pos = pos; Dir = dir; }
    }

    public AsciiRoomTemplate(string name, Vector2I size)
    {
        Name = name;
        Size = size;
    }

    public IEnumerable<Vector2I> OccupiedLocal()
    {
        foreach (var p in Floor) yield return p;
        foreach (var p in Solid) yield return p;
    }

    public static AsciiRoomTemplate FromTextFile(string path)
    {
        using var f = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (f == null)
            throw new Exception($"Could not open room file: {path}");

        // Read lines (preserve leading spaces)
        var lines = new List<string>();
        while (!f.EofReached())
        {
            string line = f.GetLine();
            // allow comment lines
            if (line.StartsWith(";")) continue;
            lines.Add(line.Replace("\r", ""));
        }

        int h = lines.Count;
        int w = 0;
        for (int i = 0; i < h; i++)
            w = Math.Max(w, lines[i].Length);

        var t = new AsciiRoomTemplate(System.IO.Path.GetFileNameWithoutExtension(path), new Vector2I(w, h));

        for (int y = 0; y < h; y++)
        {
            string line = lines[y];
            for (int x = 0; x < w; x++)
            {
                char c = x < line.Length ? line[x] : ' ';
                Vector2I p = new Vector2I(x, y);

                switch (c)
                {
                    case '.':
                        t.Floor.Add(p);
                        break;

                    case '#':
                        t.Solid.Add(p);
                        break;

                    case 'S':
                        t.Floor.Add(p);
                        t.SpawnLocal ??= p;
                        break;

                    case '^':
                        t.Floor.Add(p);
                        t.Doors.Add(new Door(p, Vector2I.Up));
                        break;
                    case 'v':
                        t.Floor.Add(p);
                        t.Doors.Add(new Door(p, Vector2I.Down));
                        break;
                    case '<':
                        t.Floor.Add(p);
                        t.Doors.Add(new Door(p, Vector2I.Left));
                        break;
                    case '>':
                        t.Floor.Add(p);
                        t.Doors.Add(new Door(p, Vector2I.Right));
                        break;

                    default:
                        // space or any other char = ignored (outside room)
                        break;
                }
            }
        }

        return t;
    }

    public AsciiRoomTemplate RotatedCW(int turns)
    {
        turns = ((turns % 4) + 4) % 4;
        if (turns == 0) return this;

        AsciiRoomTemplate cur = this;
        for (int i = 0; i < turns; i++)
            cur = cur.RotateCWOnce();
        return cur;
    }

    private AsciiRoomTemplate RotateCWOnce()
    {
        // (x,y) -> (h-1-y, x)
        int w = Size.X;
        int h = Size.Y;

        var t = new AsciiRoomTemplate(Name + "_rot", new Vector2I(h, w));

        foreach (var p in Floor)
            t.Floor.Add(new Vector2I(h - 1 - p.Y, p.X));

        foreach (var p in Solid)
            t.Solid.Add(new Vector2I(h - 1 - p.Y, p.X));

        foreach (var d in Doors)
        {
            Vector2I rp = new Vector2I(h - 1 - d.Pos.Y, d.Pos.X);
            Vector2I rd = new Vector2I(-d.Dir.Y, d.Dir.X); // rotate dir CW
            t.Doors.Add(new Door(rp, rd));
        }

        if (SpawnLocal.HasValue)
        {
            var s = SpawnLocal.Value;
            t.SpawnLocal = new Vector2I(h - 1 - s.Y, s.X);
        }

        return t;
    }
}
